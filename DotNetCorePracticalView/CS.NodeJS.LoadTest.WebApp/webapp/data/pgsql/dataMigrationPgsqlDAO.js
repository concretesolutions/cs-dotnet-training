let pg = require('pg');
let schema = require('../loadTestSchema.js');
let appSettings = require('../../config/appsettings.js');

module.exports = {

	createAsync: (pgsqlConfig) => {

		let dao = {
			_isConnOpen: false,
			_currentDatabase: '',
			_dbConfig: pgsqlConfig ? pgsqlConfig : appSettings.databaseConfig.pgsql,
			_client: null,
			_retryTimeout: null
		};

		dao._closeConnection = () => {

			dao._client.end();
			dao._isConnOpen = false;
			dao._currentDatabase = '';
			dao._client = null;
		};

		dao._openConnectionAsync = (config) => {

			let retryCount = 0;
			let retryTimeout = null;


			if (!dao._client) {
				dao._client = new pg.Client(config);
			}

			return new Promise((res, rej) => {

				let connectCallback = (err) => {

					if (err) {

						if (retryCount == 2) {
							rej(err);
							return;
						}

						retryCount++;
						console.warn('DataMigration connection error. try:' + retryCount + ' starting retry.')

						retryTimeout = setTimeout(() => {

							dao._closeConnection();
							dao._client = new pg.Client(config);
							dao._client.connect(connectCallback);
						}, 10000);

						return;
					}

					if (retryCount > 0) {
						console.log("DataMigration connection success");
					}

					if (retryTimeout) {
						clearTimeout(retryTimeout);
					}

					dao._isConnOpen = true;
					dao._currentDatabase = config.database;
					res();
				};

				dao._client.connect(connectCallback);
			});
		};

		dao._changeDatabaseAsync = (db) => {

			let config = {
				user: dao._dbConfig.rootConn.user,
				password: dao._dbConfig.rootConn.password,
				host: dao._dbConfig.rootConn.host,
				port: dao._dbConfig.rootConn.port,
				database: db
			};

			if (!dao._isConnOpen) {
				return dao._openConnectionAsync(config);
			}

			//change database if needed
			if (dao._currentDatabase != config.database) {

				dao._closeConnection();
				return dao._openConnectionAsync(config);
			}

			return new Promise((res, rej) => { res(); });
		};

		dao._createDatabaseAsync = () => {

			//check if app database exists
			let selectDbCmd = {
				text: 'select 1 as "db" from pg_database where datname = $1',
				values: [dao._dbConfig.conn.database]
			};

			let createDbCmd = {};
			createDbCmd.text = 'create database "' + dao._dbConfig.conn.database + '"';

			createDbCmd.text += ' with template=\'template0\' encoding=\'UTF8\' tablespace=pg_default';
			createDbCmd.text += ' lc_collate=\'' + dao._dbConfig.collate + '\'';

			createDbCmd.text += ' lc_ctype=\'' + dao._dbConfig.ctype + '\' connection limit= -1; '
			createDbCmd.values = [];

			return new Promise((res, rej) => {

				dao._client.query(selectDbCmd)
					.then((result) => {

						if (result.rows.length == 0) {

							dao._client.query(createDbCmd)
								.then((result_2) => { res(); })
								.catch(rej)

							return;
						}
						res();
					})
					.catch(rej);
			});
		};

		dao._createSchemaVersionAsync = () => {

			let cmd = {};
			cmd.text = 'create table if not exists "' + schema.schemaVersion.table + '"';
			cmd.text += ' ("' + schema.schemaVersion.versionColumn + '" char(5) not null);';

			return new Promise((res, rej) => {

				dao._client.query(cmd)
					.then(() => { res(); })
					.catch(rej);
			});
		};

		dao._validateMigrationOperation = (operation, fromVersion, toVersion) => {

			if (fromVersion || toVersion != dao.getLatestVersion()) {
				throw new Error(operation + ' from version "' + fromVersion + '"' + ' to version "' + toVersion + '" not implemented.');
			}

			if (fromVersion == toVersion) {
				console.log(operation + " - Updated");
				return false;
			}

			return true;
		};

		dao.getCurrentVersionAsync = () => {

			return new Promise((res, rej) => {

				let query = {};
				query.text = 'select "' + schema.schemaVersion.versionColumn + '"';
				query.text += ' from "' + schema.schemaVersion.table + '"';

				dao._client.query(query)
					.then((result) => {

						if (result.rows && result.rows[0]) {
							res(result.rows[0][schema.schemaVersion.versionColumn]);
						}
						else {
							res(null);
						}
					})
					.catch(rej);
			});
		};

		dao.migrateDbmsUsersAsync = (fromVersion, toVersion) => {

			if (!dao._validateMigrationOperation('Migrate DBMS Users', fromVersion, toVersion)) {
				return new Promise((res, rej) => { res(); });
			}

			//create user
			let cmd = {};
			cmd.text = 'create user ' + dao._dbConfig.conn.user;
			cmd.text += ' with encrypted password \'' + dao._dbConfig.conn.password + '\';';

			//grant access to connect
			cmd.text += ' grant connect, temporary on database ' + dao._dbConfig.conn.database
			cmd.text += ' TO ' + dao._dbConfig.conn.user + ';';

			//grant access to tables
			cmd.text += ' grant select, insert, update, delete, truncate, references, trigger';
			cmd.text += ' on all tables in schema public to ' + dao._dbConfig.conn.user + ';';

			return new Promise((res, rej) => {

				dao._client.query(cmd)
					.then(() => { res(); })
					.catch(rej);
			});
		};

		dao.migrateIdentityAsync = (fromVersion, toVersion) => {

			if (!dao._validateMigrationOperation('Migrate Identity', fromVersion, toVersion)) {
				return new Promise((res, rej) => { res(); });
			}

			//create table identity
			let cmd = {}
			cmd.text = 'create table "' + schema.identity.table + '"';
			cmd.text += ' ("' + schema.identity.identityNameColumn + '" varchar(100) not null';
			cmd.text += ' constraint ' + schema.identity.IdentityNameIndex + ' primary key,';
			cmd.text += ' "' + schema.identity.dataColumn + '" json not null);';

			return new Promise((res, rej) => {

				dao._client.query(cmd)
					.then(() => { res(); })
					.catch(rej);
			});
		};

		dao.migrateSchemaVersionAsync = (fromVersion, toVersion) => {

			if (!dao._validateMigrationOperation('Migrate Schema Version', fromVersion, toVersion)) {
				return new Promise((res, rej) => { });
			}

			//update version
			let cmd = {};
			cmd.text = 'update "' + schema.schemaVersion.table + '"';

			cmd.text += ' set "' + schema.schemaVersion.versionColumn + '" = $1;';
			cmd.values = [toVersion];

			return new Promise((res, rej) => {

				dao._client.query(cmd)
					.then((result) => {

						//if there is no record insert one
						if (result.rowCount == 0) {

							cmd.text = 'insert into "' + schema.schemaVersion.table + '"'
							cmd.text += ' ("' + schema.schemaVersion.versionColumn + '") values($1);'

							dao._client.query(cmd)
								.then(() => { res(); })
								.catch(rej);
							return;
						}
						res();
					})
					.catch(rej);
			});
		};

		dao.disposeAsync = () => {

			dao._closeConnection();
			return new Promise((res, rej) => { res(); });
		};

		dao.getLatestVersion = () => { return '1.0.0'; }

		dao.migrateTestResultAsync = (fromVersion, toVersion) => {

			if (!dao._validateMigrationOperation('Migrate Test Result', fromVersion, toVersion)) {
				return new Promise((res, rej) => { res(); });
			}

			//create table TestResult
			let cmd = {};
			cmd.text = 'create table "' + schema.testResult.table + '"';
			cmd.text += ' ("' + schema.testResult.testIdColumn + '" varchar(10) not null,';
			cmd.text += ' "' + schema.testResult.languageColumn + '" varchar(50) null,';
			cmd.text += ' "' + schema.testResult.dataColumn + '" json not null,';

			cmd.text += ' constraint "' + schema.testResult.testResultTestIndex + '"';
			cmd.text += ' unique ("' + schema.testResult.testIdColumn + '","' + schema.testResult.languageColumn + '"));';

			return new Promise((res, rej) => {

				dao._client.query(cmd)
					.then(() => { res(); })
					.catch(rej);
			});
		};

		return new Promise((res, rej) => {

			dao._openConnectionAsync(dao._dbConfig.rootConn)
				.then(() => {

					dao._createDatabaseAsync()
						.then(() => {

							dao._changeDatabaseAsync(dao._dbConfig.conn.database)
								.then(() => {

									dao._createSchemaVersionAsync()
										.then(() => { res(dao); })
										.catch(rej);
								})
								.catch(rej);
						})
						.catch(rej);
				})
				.catch(rej);
		});
	}
}