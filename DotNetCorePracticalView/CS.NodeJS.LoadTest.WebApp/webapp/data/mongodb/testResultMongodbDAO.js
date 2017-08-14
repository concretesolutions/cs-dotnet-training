let schema = require('./loadTestMongoSchema.js');

module.exports = {
	_insertOptions: { bypassDocumentValidation: false },

	insertAsync: (testResult, pool) => {

		return new Promise((res, rej) => {

			pool.getDbAsync()
				.then(db => {

					db.collection(schema.testResult.collection)
						.insertOne(testResult, module.exports._insertOptions, (err, result) => {

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

	insertRangeAsync: (testResultArray, pool) => {

		if (testResultArray.length < 1) {
			return new Promise((res, rej) => { res(); });
		}

		return new Promise((res, rej) => {

			pool.getDbAsync()
				.then(db => {

					db.collection(schema.testResult.collection)
						.insertMany(testResultArray, module.exports._insertOptions, (err, result) => {

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

	deleteAllAsync: (pool) => {

		return new Promise((res, rej) => {

			pool.getDbAsync()
				.then(db => {
					db.collection(schema.testResult.collection)
						.deleteMany({}, (err, result) => {

							if (err) {
								rej(err);
							}
							else {
								res(result.result.n);
							}
						});
				})
				.catch(rej);
		});
	}
}