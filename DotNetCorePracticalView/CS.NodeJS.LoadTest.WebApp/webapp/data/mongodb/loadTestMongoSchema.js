module.exports = {

	identity: {
		collection: 'Identity',
		identityNameProp: 'identityName',
		identityNameIndex: 'PK_IdentityName'
	},

	schemaVersion: {
		collection: 'SchemaVersion',
		versionProp: 'version'
	},

	testResult: {
		collection: 'TestResult',
		testResultTestIndex: 'IX_TestResult_Test',
		testIdProp: 'test.testId',
		languageProp: 'test.language'
	}
}