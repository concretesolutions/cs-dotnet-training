let schema = require('../loadTestSchema.js')
let mysql = require('mysql');

const _baseInsert = 'insert into `' + schema.testResult.table + '`' +
	' (`' + schema.testResult.testIdColumn + '`, `' + schema.testResult.languageColumn + '`' +
	', `' + schema.testResult.dataColumn + '`)';

module.exports = {

	_insertSqlCommand: _baseInsert + ' values (?,?,?);',
	_deleteAllSqlCommand: 'delete from `' + schema.testResult.table + '`;',

	insertAsync: (testResult, connPool) => {

		let cmdArgs = [testResult.test.testId, testResult.test.language, testResult];
		let cmd = mysql.format(module.exports._insertSqlCommand, cmdArgs);

		return new Promise((res, rej) => {

			connPool.query(cmd, (err, result) => {

				if (err) {
					rej(err);
				}
				else {
					res();
				}
			});
		});
	},

	insertRangeAsync: (testResultArray, connPool) => {

		if (!testResultArray || !testResultArray.length || testResultArray.length < 1) {
			return new Promise((res, rej) => { res(); })
		}

		let sql = _baseInsert + ' values';
		let cmdArgs = [];
		let paramIndex = 0;

		testResultArray.forEach((tr) => {

			if (paramIndex > 0) {
				sql += ',';
			}

			sql += ' (?,?,?)';
			cmdArgs.push(tr.test.testId, tr.test.language, JSON.stringify(tr));
			paramIndex += 3;
		});

		sql += ';';
		let cmd = mysql.format(sql, cmdArgs);

		return new Promise((res, rej) => {

			connPool.query(cmd, (err, result) => {

				if (err) {
					rej(err);
				}
				else {
					res();
				}
			});
		});
	},

	deleteAllAsync: (connPool) => {

		let cmd = mysql.format(module.exports._deleteAllSqlCommand, []);

		return new Promise((res, rej) => {

			connPool.query(cmd, (err, result) => {

				if (err) {
					rej(err);
				}
				else {
					res(result.affectedRows);
				}
			});
		});
	}
}