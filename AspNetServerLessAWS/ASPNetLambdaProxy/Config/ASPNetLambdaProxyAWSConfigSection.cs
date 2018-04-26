namespace ASPNetLambdaProxy.Config
{
	using AWSDeveloperTraining.Practices.Config;
	using Microsoft.Extensions.Configuration;

	internal class ASPNetLambdaProxyAWSConfigSection : AWSConfigSection, IASPNetLambdaProxyAWSConfigSection
	{
		public IAWSLambdaConfigSection Lambda { get; private set; }

		public ASPNetLambdaProxyAWSConfigSection(IConfigurationRoot configRoot, string parentPath) : base(configRoot, parentPath)
		{
			Lambda = new AWSLambdaConfigSection(ConfigRoot, SectionPath);
		}
	}
}
