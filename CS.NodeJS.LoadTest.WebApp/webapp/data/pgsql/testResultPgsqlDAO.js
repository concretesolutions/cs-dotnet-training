let schema = require('../loadTestSchema.js')

const _baseInsert = 'insert into "' + schema.testResult.table + '"' +
	' ("' + schema.testResult.testIdColumn + '", "' + schema.testResult.languageColumn + '", "' +
	schema.testResult.dataColumn + '")';

module.exports = {

	_insertSqlCommand: _baseInsert + ' values ($1, $2, $3);',
	_deleteAllSqlCommand: 'delete from "' + schema.testResult.table + '";',

	insertAsync: (testResult, connPool) => {

		let cmd = {
			text: module.exports._insertSqlCommand,
			values: [testResult.test.testId, testResult.test.language, testResult]
		};

		return new Promise((res, rej) => {

			connPool.connect()
				.then(cl => {

					cl.query(cmd, (err, result) => {

						cl.release();

						if (err) {
							rej(err);
						}
						else {
							res();
						}
					});
				})
				.catch(rej);
		});
	},

	insertRangeAsync: (testResultArray, connPool) => {

		if (!testResultArray || !testResultArray.length || testResultArray.length < 1) {

			return new Promise((res, rej) => { res(); });
		}

		let cmd = {
			text: _baseInsert + ' values',
			values: []
		};

		let paramIndex = 1;

		testResultArray.forEach((tr) => {

			if (paramIndex > 1) {
				cmd.text += ',';
			}

			cmd.text += ' ($' + paramIndex.toString() + ', $' + (paramIndex + 1).toString() +
				', $' + (paramIndex + 2).toString() + ')'

			cmd.values.push(tr.test.testId, tr.test.language, tr);
			paramIndex += 3;
		});

		cmd.text += ';';

		return new Promise((res, rej) => {

			connPool.connect()
				.then(cl => {

					cl.query(cmd, (err, result) => {

						cl.release();

						if (err) {
							rej(err);
						}
						else {
							res();
						}
					})
				})
				.catch(rej);
		});
	},

	deleteAllAsync: (connPool) => {

		let cmd = {
			text: _insertSqlCommand,
			values: []
		};

		return new Promise((res, rej) => {

			connPool.connect()
				.then(cl => {

					cl.query(cmd, (err, result) => {

						cl.release();

						if (err) {
							rej(err);
						}
						else {
							res(result.rowCount);
						}
					})
				})
				.catch(rej);
		});
	}
}