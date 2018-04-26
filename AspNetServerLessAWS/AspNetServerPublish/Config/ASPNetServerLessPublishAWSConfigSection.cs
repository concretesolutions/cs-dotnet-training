namespace AspNetServerPublish.Config
{
	using AWSDeveloperTraining.Practices.Config;
	using Microsoft.Extensions.Configuration;

	internal class ASPNetServerLessPublishAWSConfigSection : AWSConfigSection
	{
		private readonly string _accountIdOption;

		public ASPNetServerLessPublishAWSLambdaConfigSection Lambda { get; private set; }

		public AWSApiGatewayConfigSection ApiGateway { get; private set; }

		public string AccountId => ConfigRoot[_accountIdOption];

		internal ASPNetServerLessPublishAWSConfigSection(IConfigurationRoot configRoot, string parentPath) : base(configRoot, parentPath)
		{
			_accountIdOption = $"{SectionPath}:AccountId";
			ApiGateway = new AWSApiGatewayConfigSection(ConfigRoot, SectionPath);
			Lambda = new ASPNetServerLessPublishAWSLambdaConfigSection(ConfigRoot, SectionPath, AccountId);
		}
	}
}
