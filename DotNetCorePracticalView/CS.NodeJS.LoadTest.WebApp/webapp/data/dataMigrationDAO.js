let mongoDAO = require('./mongodb/dataMigrationMongodbDAO.js');
let mysqlDAO = require('./mysql/dataMigrationMysqlDAO.js');
let pgsqlDAO = require('./pgsql/dataMigrationPgsqlDAO.js');
let loadTestDBMS = require('./loadTestDBMS.js');

module.exports = {
	createAsync: (options) => {

		switch (options.appSettings.DBMS) {

			case loadTestDBMS.pgsql:
				return pgsqlDAO.createAsync(options.appSettings.databaseConfig.pgsql)
				break;

			case loadTestDBMS.mysql:
				return mysqlDAO.createAsync(options.appSettings.databaseConfig.mysql)
				break;

			case loadTestDBMS.mongodb:
				return mongoDAO.createAsync(options.appSettings.databaseConfig.mongodb)
				break;

			default:
				throw new Error('Data Migration DAO For DBMS: ' + appSettings.DBMS + ' not implemented');
		}
	}
}