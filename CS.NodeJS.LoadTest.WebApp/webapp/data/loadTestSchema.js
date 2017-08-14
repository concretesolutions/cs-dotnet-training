module.exports = {

	identity: {
		table: 'Identity',
		identityNameColumn: 'IdentityName',
		IdentityNameIndex: 'PK_IdentityName',
		dataColumn: 'Data'
	},

	schemaVersion: {
		table: 'SchemaVersion',
		versionColumn: 'Version'
	},

	testResult: {
		table: 'TestResult',
		testIdColumn: 'TestId',
		languageColumn: 'Language',
		dataColumn: 'Data',
		testResultTestIndex: 'UQ_TestResult'
	}
}