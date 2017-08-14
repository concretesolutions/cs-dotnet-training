let appSettings = require('../config/appsettings.js');
let loadTestDBMS = require('./loadTestDBMS.js');

switch (appSettings.DBMS) {

	case loadTestDBMS.pgsql:
		module.exports = require('./pgsql/identityPgsqlDAO.js');
		break;

	case loadTestDBMS.mysql:
		module.exports = require('./mysql/identityMysqlDAO.js');
		break;

	case loadTestDBMS.mongodb:
		module.exports = require('./mongodb/identityMongodbDAO.js');
		break;

	default:
		throw new Error('Identity DAO For DBMS: ' + appSettings.DBMS + ' not implemented');
}