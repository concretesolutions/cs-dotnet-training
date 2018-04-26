namespace ASPNetLambdaProxy.Config
{
	using AWSDeveloperTraining.Practices.Config;
	using Microsoft.Extensions.Configuration;

	internal class ASPNetLambdaProxyConfigSection : BaseConfigSection, IASPNetLambdaProxyConfigSection
	{
		private const string SECTION_PATH = "ASPNetLambdaProxy";

		public IASPNetLambdaProxyAWSConfigSection AWS { get; private set; }

		public ASPNetLambdaProxyConfigSection(IConfigurationRoot configRoot) : base(configRoot)
		{
			AWS = new ASPNetLambdaProxyAWSConfigSection(configRoot, SECTION_PATH);
		}
	}
}
