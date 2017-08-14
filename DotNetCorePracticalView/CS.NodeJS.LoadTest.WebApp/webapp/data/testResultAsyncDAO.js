let loadTestDBMS = require('./loadTestDBMS.js');
let appSettings = require('../config/appsettings.js');

switch (appSettings.DBMS) {

	case loadTestDBMS.mongodb:
		module.exports = require('./mongodb/testResultMongodbDAO.js');
		break;

	case loadTestDBMS.pgsql:
		module.exports = require('./pgsql/testResultPgsqlDAO.js');
		break;

	case loadTestDBMS.mysql:
		module.exports = require('./mysql/testResultMysqlDAO.js');
		break;

	default:
		throw new Error('Test Result Async DAO For DBMS: ' + appSettings.DBMS + ' not implemented');
}