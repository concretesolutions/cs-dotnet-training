module.exports = {

	executeAsync: (options) => {

		let latest = options.migrationDAO.getLatestVersion();
		
		return new Promise((res, rej) => {

			options.migrationDAO.getCurrentVersionAsync()
				.then((version) => {

					if (version == latest) {
						console.log("Schema Updated");
						res();
						return;
					}

					//installing schema version
					options.migrationDAO.migrateSchemaVersionAsync(version, latest)
						.then(() => {
							console.log("Migrate Schema Version - OK");

							//installing identity
							options.migrationDAO.migrateIdentityAsync(version, latest)
								.then(() => {
									console.log('Migrate Identity - OK');

									//creating test result
									options.migrationDAO.migrateTestResultAsync(version, latest)
										.then(() => {
											console.log('Migrate Test Result - OK');

											//creating users
											options.migrationDAO.migrateDbmsUsersAsync(version, latest)
												.then(() => {
													console.log('Migrate DBMS Users - OK');
													res();
												})
												.catch(rej);
										})
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