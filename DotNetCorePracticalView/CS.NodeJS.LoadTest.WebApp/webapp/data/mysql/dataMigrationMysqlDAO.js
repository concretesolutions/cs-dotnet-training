let mysql = require('mysql');
let schema = require('../loadTestSchema.js');
let appSettings = require('../../config/appsettings.js');

module.exports = {

	createAsync: (mysqlConfig) => {

		let dao = {
			_dbConfig: mysqlConfig ? mysqlConfig : appSettings.databaseConfig.mysql,
			_client: null,
		};

		dao._closeConnectionAsync = () => {

			if (!dao._client) {
				return new Promise((res, rej) => { res(); });
			}

			return new Promise((res, rej) => {

				dao._client.end((err) => {

					if (err) {
						rej(err);
						return;
					}

					dao._client = null;
					res();
				});
			});
		};

		dao._openConnectionAsync = () => {

			let retryCount = 0;
			let retryTimeout = null;

			return new Promise((res, rej) => {

				let connectCallback = (err) => {

					if (err) {

						if (retryCount == 2) {
							rej(err);
							return;
						}

						console.warn('Data Migration connect fail. try: ' + (retryCount + 1) + ' starting retry..');
						retryCount++;

						retryTimeout = setTimeout(() => {

							dao._client.destroy();
							dao._client = mysql.createConnection(dao._dbConfig.rootConn);
							dao._client.connect(connectCallback);
						}, 10000);

						return;
					}

					if (retryCount > 0) {
						console.info('Data Migartion connect success.');
					}

					if (retryTimeout) {
						clearTimeout(retryTimeout);
					}

					res();
				}

				dao._client = mysql.createConnection(dao._dbConfig.rootConn);
				dao._client.connect(connectCallback);
			});
		};

		dao._createDatabaseAsync = () => {

			let sql = 'create database if not exists `' + dao._dbConfig.conn.database + '`';
			sql += ' character set=\'' + dao._dbConfig.ctype + '\'';
			sql += ' collate=\'' + dao._dbConfig.collate + '\';';

			let mysqlCmd = mysql.format(sql, []);

			return new Promise((res, rej) => {

				dao._openConnectionAsync()
					.then(() => {

						dao._client.query(mysqlCmd, (err, result) => {

							if (err) {
								rej(err)
								return;
							}

							res();
						});
					})
					.catch(rej);
			});
		};

		dao._createSchemaVersionAsync = () => {

			let sql = 'use ' + dao._dbConfig.conn.database + ';';
			sql += ' create table if not exists `' + schema.schemaVersion.table + '`';
			sql += ' (`' + schema.schemaVersion.versionColumn + '` char(10) not null);';

			let mysqlCmd = mysql.format(sql);

			return new Promise((res, rej) => {

				dao._openConnectionAsync()
					.then(() => {

						dao._client.query(mysqlCmd, (err, result) => {

							if (err) {
								rej(err);
								return;
							}

							res();
						});
					})
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
		}

		dao._updateSchemaVersionASync = (toVersion) => {

			let sql = 'update `' + schema.schemaVersion.table + '`';
			sql += ' set `' + schema.schemaVersion.versionColumn + '`=?;'

			let mysqlCmd = mysql.format(sql, [toVersion]);

			return new Promise((res, rej) => {

				dao._client.query(mysqlCmd, (err, result) => {

					if (err) {
						rej(err);
						return;
					}

					res(result.affectedRows);
				})
			});
		};

		dao._insertSchemaVersionAsync = (toVersion) => {

			let sql = 'insert into `' + schema.schemaVersion.table + '`';
			sql += ' (`' + schema.schemaVersion.versionColumn + '`)';
			sql += ' values (?)';

			let mysqlCmd = mysql.format(sql, [toVersion]);

			return new Promise((res, rej) => {

				dao._client.query(mysqlCmd, (err, result) => {

					if (err) {
						rej(err);
						return;
					}

					res();
				});
			});
		};

		dao.disposeAsync = () => {
			return dao._closeConnectionAsync();
		};

		dao.getCurrentVersionAsync = () => {

			let sql = 'select `' + schema.schemaVersion.versionColumn + '`';
			sql += ' from `' + schema.schemaVersion.table + '`;'

			let mysqlCmd = mysql.format(sql);

			return new Promise((res, rej) => {

				dao._client.query(mysqlCmd, (err, rows) => {

					if (err) {
						rej(err);
						return;
					}

					if (rows && rows[0]) {
						res(rows[0][schema.schemaVersion.versionColumn]);
						return;
					}

					res(null);
				});
			});
		};

		dao.migrateSchemaVersionAsync = (fromVersion, toVersion) => {

			if (!dao._validateMigrationOperation('Migrate Schema Version', fromVersion, toVersion)) {
				return new Promise((res, rej) => { res(); });
			}

			return new Promise((res, rej) => {

				dao._updateSchemaVersionASync(toVersion)
					.then((affected) => {

						if (!affected || affected < 1) {

							dao._insertSchemaVersionAsync(toVersion)
								.then(res)
								.catch(rej);

							return;
						}

						res();
					})
					.catch(rej);
			});
		}

		dao.migrateIdentityAsync = (fromVersion, toVersion) => {

			if (!dao._validateMigrationOperation('Migrate Identity', fromVersion, toVersion)) {
				return new Promise((res, rej) => { res(); });
			}

			let sql = 'create table `' + schema.identity.table + '`';
			sql += ' (`' + schema.identity.identityNameColumn + '` varchar(100) not null,';
			sql += ' `' + schema.identity.dataColumn + '` json not null);';

			sql += ' alter table `' + schema.identity.table + '`';
			sql += ' add constraint ' + schema.identity.IdentityNameIndex + ' primary key';
			sql += ' using btree (`' + schema.identity.identityNameColumn + '` asc);';

			let mysqlCmd = mysql.format(sql);

			return new Promise((res, rej) => {

				dao._client.query(mysqlCmd, (err, result) => {

					if (err) {
						rej(err);
						return;
					}

					res();
				})
			});
		}

		dao.migrateDbmsUsersAsync = (fromVersion, toVersion) => {

			if (!dao._validateMigrationOperation('Migrate DBMS Users', fromVersion, toVersion)) {
				return new Promise((res, rej) => { res(); });
			}

			let sql = 'create user \'' + dao._dbConfig.conn.user + '\'';
			sql += ' identified by \'' + dao._dbConfig.conn.password + '\' password expire never;';

			sql += ' grant create temporary tables, create view, delete, execute, insert, select, update';
			sql += ' on `' + dao._dbConfig.conn.database + '`.* to \'' + dao._dbConfig.conn.user + '\'';

			let mysqlCmd = mysql.format(sql);

			return new Promise((res, rej) => {

				dao._client.query(mysqlCmd, (err, result) => {

					if (err) {
						rej(err);
						return;
					}

					res();
				});
			});
		};

		dao.migrateTestResultAsync = (fromVersion, toVersion) => {

			if (!dao._validateMigrationOperation('Migrate Test Result', fromVersion, toVersion)) {
				return new Promise((res, rej) => { res(); });
			}

			let sql = 'create table `' + schema.testResult.table + '`';
			sql += ' (`' + schema.testResult.testIdColumn + '` varchar(10) not null,';
			sql += ' `' + schema.testResult.languageColumn + '` varchar(50) null,';
			sql += ' `' + schema.testResult.dataColumn + '` json not null,';

			sql += ' constraint ' + schema.testResult.testResultTestIndex;
			sql += ' unique(`' + schema.testResult.testIdColumn + '`,`' + schema.testResult.languageColumn + '`));';

			let mysqlCmd = mysql.format(sql);

			return new Promise((res, rej) => {

				dao._client.query(mysqlCmd, (err, result) => {

					if (err) {
						rej(err);
						return;
					}

					res();
				})
			});
		}

		dao.getLatestVersion = () => { return '1.0.0'; };

		return new Promise((res, rej) => {

			dao._openConnectionAsync()
				.then(() => {

					dao._createDatabaseAsync()
						.then(() => {

							dao._createSchemaVersionAsync()
								.then(() => {

									res(dao);
								})
								.catch(rej);
						})
						.catch(rej);
				})
				.catch(rej);
		})
	}
}