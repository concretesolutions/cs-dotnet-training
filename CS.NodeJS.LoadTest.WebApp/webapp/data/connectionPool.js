let pg = require('pg');
let mysql = require('mysql');

let mongodb = require('mongodb');
let loadTestDBMS = require('./loadTestDBMS.js');

module.exports = {

	_createPgsqlConnectionPool: (databaseConfig) => {

		let pool = new pg.Pool(databaseConfig.conn);

		pool.on('error', (err, client) => {
			console.error('idle client error', err.message, err.stack)
		});

		return pool;
	},

	_createMysqlConnectionPool: (databaseConfig) => {

		let pool = mysql.createPool(databaseConfig.conn);

		pool.on('error', (err, client) => {
			console.error('idle client error', err.message, err.stack)
		});

		return pool;
	},

	_createMongodbConnectionPoolAsync: (databaseConfig) => {
		return new Promise((res, rej) => {

			mongodb.MongoClient.connect(databaseConfig.conn)
				.then(db => {

					db.on('close', () => {
						module.exports._mongoPool._isClosed = true;
						module.exports._mongoPool.getDbAsync();

					})

					res(db);
				})
				.catch(rej);
		});
	},

	_mongoPool: {
		_config: null,
		_db: null,
		_isClosed: true,

		getDbAsync: () => {
			return new Promise((res, rej) => {

				if (module.exports._mongoPool._isClosed == true) {

					setTimeout(() => {

						if (module.exports._mongoPool._isClosed == true) {

							module.exports._createMongodbConnectionPoolAsync(module.exports._mongoPool._config)
								.then((db => {

									module.exports._mongoPool._db = db;
									module.exports._mongoPool._isClosed = false;
									res(module.exports._mongoPool._db);
								}))
								.catch(rej);
						}
						else {
							res(module.exports._mongoPool._db);
						}

					}, 10);
				}
				else {

					res(module.exports._mongoPool._db);
				}
			});
		}
	},

	createAsync: (args) => {

		switch (args.appSettings.DBMS) {

			case loadTestDBMS.pgsql:

				return new Promise((res, rej) => {
					res(module.exports._createPgsqlConnectionPool(args.appSettings.databaseConfig.pgsql));
				});
				break;

			case loadTestDBMS.mysql:

				return new Promise((res, rej) => {
					res(module.exports._createMysqlConnectionPool(args.appSettings.databaseConfig.mysql));
				});
				break;

			case loadTestDBMS.mongodb:

				return new Promise((res, rej) => {

					module.exports._createMongodbConnectionPoolAsync(args.appSettings.databaseConfig.mongodb)
						.then(db => {

							module.exports._mongoPool._config = args.appSettings.databaseConfig.mongodb;
							module.exports._mongoPool._isClosed = false;
							module.exports._mongoPool._db = db;
							res(module.exports._mongoPool);
						})
						.catch(rej);
				});
				break;

			default:
				throw new Error('Connection Pool for DBMS: ' + appSettings.DBMS + ' not implemented');
		}
	}
}