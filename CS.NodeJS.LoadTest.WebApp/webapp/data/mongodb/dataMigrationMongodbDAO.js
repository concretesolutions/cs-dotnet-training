let mongodb = require('mongodb');
let schema = require('./loadTestMongoSchema.js');
let appSettings = require('../../config/appsettings.js');

module.exports = {

	createAsync: (mongodbConfig) => {

		let dao = {
			_dbConfig: mongodbConfig ? mongodbConfig : appSettings.databaseConfig.mongodb,
			_client: null,
		};

		dao.getLatestVersion = () => { return '1.0.0'; };

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

		dao._createSchemaVersionAsync = () => {

			return new Promise((res, rej) => {

				dao._client.createCollection(schema.schemaVersion.collection)
					.then((coll) => { res(); })
					.catch(rej);
			});
		};

		dao._openConnectionAsync = () => {

			let retryCount = 0;
			let retryTimeout = null;

			return new Promise((res, rej) => {

				let connectCallback = (err, db) => {

					if (err) {

						if (retryCount == 2) {
							rej(err);
							return;
						}

						console.warn('Data Migration connect fail. try: ' + (retryCount + 1) + ' starting retry..');
						retryCount++;

						retryTimeout = setTimeout(() => {
							mongodb.MongoClient.connect(dao._dbConfig.rootConn, connectCallback);
						}, 10000);

						return;
					}

					if (retryCount > 0) {
						console.info('Data Migration connect success.');
					}

					if (retryTimeout) {
						clearTimeout(retryTimeout);
					}

					dao._client = db;
					res();
				};

				mongodb.MongoClient.connect(dao._dbConfig.rootConn, connectCallback);
			});
		};

		dao.disposeAsync = () => {

			return new Promise((res, rej) => {
				dao._client.close(true)
					.then(() => {
						dao._client = null;
						res();
					})
					.catch(rej);
			});
		};

		dao.migrateIdentityAsync = (fromVersion, toVersion) => {

			if (!dao._validateMigrationOperation('Migrate Identity', fromVersion, toVersion)) {
				return new Promise((res, rej) => { res(); });
			}

			return new Promise((res, rej) => {

				//create Identity Collection;
				dao._client.createCollection(schema.identity.collection)
					.then(coll => {

						//create unique index in Identity.identityName;
						coll.createIndex(schema.identity.identityNameProp,
							{
								unique: 1,
								name: schema.identity.identityNameIndex
							})
							.then(indexName => { res(); })
							.catch(rej);
					})
					.catch(rej);
			});
		};

		dao.migrateSchemaVersionAsync = (fromVersion, toVersion) => {

			let cmdFind = { $set: {} };
			cmdFind.$set[schema.schemaVersion.versionProp] = toVersion;

			return new Promise((res, rej) => {

				dao._client.collection(schema.schemaVersion.collection)
					.findOneAndUpdate({}, cmdFind, { upsert: true })
					.then((result) => { res(); })
					.catch(rej);
			});
		};

		dao.migrateDbmsUsersAsync = (fromVersion, toVersion) => {

			return new Promise((res, rej) => {
				console.log('Migrate users does not apply to mongo migration.');
				res();
			});
		};

		dao.getCurrentVersionAsync = () => {

			let projection = {};
			projection[schema.schemaVersion.versionProp] = 1;

			return new Promise((res, rej) => {

				dao._client.collection(schema.schemaVersion.collection)
					.findOne({}, { fields: projection })
					.then(doc => {

						if (doc) {
							res(doc[schema.schemaVersion.versionProp])
							return;
						}

						res(null);
					})
					.catch(rej);
			});
		};

		dao.migrateTestResultAsync = (fromVersion, toVersion) => {

			if (!dao._validateMigrationOperation('Migrate Test Result', fromVersion, toVersion)) {
				return new Promise((res, rej) => { res(); });
			}

			return new Promise((res, rej) => {

				//create Test Result Collection
				dao._client.createCollection(schema.testResult.collection)
					.then((coll) => {

						//create testId index
						coll.createIndex([schema.testResult.testIdProp, schema.testResult.languagePro],
							{
								unique: 1,
								name: schema.testResult.testResultTestIndex
							})
							.then((indexName) => { res(); })
							.catch(rej);
					})
					.catch(rej);
			});
		};

		return new Promise((res, rej) => {

			dao._openConnectionAsync()
				.then(() => { res(dao); })
				.catch(rej);
		});
	}
}