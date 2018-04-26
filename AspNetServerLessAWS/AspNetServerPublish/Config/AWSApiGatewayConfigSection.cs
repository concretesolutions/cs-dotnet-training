using Microsoft.Extensions.Configuration;

namespace AspNetServerPublish.Config
{
	internal class AWSApiGatewayConfigSection
	{
		private readonly string _sectionPath;
		private readonly string _gatewayNameOption;

		private readonly string _stageNameOption;
		private readonly IConfigurationRoot _configRoot;

		public string GatewayName => _configRoot[_gatewayNameOption];

		public string StageName => _configRoot[_stageNameOption];

		internal AWSApiGatewayConfigSection(IConfigurationRoot configRoot, string parentPath)
		{
			_sectionPath = parentPath.EndsWith(":") ? parentPath + "ApiGateway" : $"{parentPath}:ApiGateway";
			_gatewayNameOption = $"{_sectionPath}:GatewayName";

			_stageNameOption = $"{_stageNameOption}:StageName";
			_configRoot = configRoot;
		}
	}
}
