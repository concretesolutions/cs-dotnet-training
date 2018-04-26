namespace AspNetServerPublish.Config
{
	using AWSDeveloperTraining.Practices.Config;
	using Microsoft.Extensions.Configuration;
	using System.Collections.Generic;
	using System.Linq;

	internal class ASPNetServerLessPublishAWSLambdaConfigSection : AWSLambdaConfigSection
	{
		private readonly string _functionMemoryOption;
		private readonly string _functionTimeoutSecondsOption;

		private readonly string _functionRoleOption;
		private readonly string _functionHandlerOption;

		private readonly string _environmentVariablesOption;
		private readonly string _permissionActionOption;

		private readonly string _permissionPrincipalOption;
		private readonly string _permissionStatementIdOption;

		private readonly string _accountId;
		private readonly IConfigurationRoot _configRoot;

		public int? FunctionMemory => int.TryParse(_configRoot[_functionMemoryOption], out int memory) ? memory : (int?)null;

		public int? FunctionTimeoutSeconds => int.TryParse(_configRoot[_functionTimeoutSecondsOption], out int timeout) ? timeout : (int?)null;

		public string FunctionRole => string.Format(_configRoot[_functionRoleOption], _accountId);

		public string FunctionHandler => _configRoot[_functionHandlerOption];

		public string PermissionAction => _configRoot[_permissionActionOption];

		public string PermissionPrincipal => _configRoot[_permissionPrincipalOption];

		public string PermissionStatementId => _configRoot[_permissionStatementIdOption];

		public IEnumerable<KeyValuePair<string, string>> EnvironmentVAriables => _configRoot
			.GetSection(_environmentVariablesOption)
			.GetChildren()
			.Select(c => new KeyValuePair<string, string>(c.Key, c.Value));

		internal ASPNetServerLessPublishAWSLambdaConfigSection(IConfigurationRoot configRoot, string parentPath, string accountId) : base(configRoot, parentPath)
		{
			_functionMemoryOption = $"{SectionPath}:FunctionMemory";
			_functionTimeoutSecondsOption = $"{SectionPath}:FunctionTimeoutSeconds";

			_functionRoleOption = $"{SectionPath}:FunctionRole";
			_functionHandlerOption = $"{SectionPath}:FunctionHandler";

			_environmentVariablesOption = $"{SectionPath}:EnvironmentVariables";
			_permissionActionOption = $"{SectionPath}:PermissionAction";

			_permissionPrincipalOption = $"{SectionPath}:PermissionPrincipal";
			_permissionStatementIdOption = $"{SectionPath}:PermissionStatementId";

			_accountId = accountId;
			_configRoot = configRoot;
		}
	}
}
