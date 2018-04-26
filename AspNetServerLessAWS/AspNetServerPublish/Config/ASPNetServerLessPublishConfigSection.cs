namespace AspNetServerPublish.Config
{
	using Microsoft.Extensions.Configuration;
	using System.Collections.Generic;
	using System.Linq;

	internal class ASPNetServerLessPublishConfigSection
	{
		private const string SECTION_PATH = "AspNetServerlessPublish";
		private static readonly string PackagePathOption = $"{SECTION_PATH}:PackagePath";
		private readonly IConfigurationRoot _configRoot;

		public ASPNetServerLessPublishAWSConfigSection AWS { get; private set; }

		public string PackagePath => _configRoot[PackagePathOption];

		public string ASPNETCORE_ENVIRONMENT
		{
			get
			{
				var env = AWS.Lambda.EnvironmentVAriables.FirstOrDefault(e => "ASPNETCORE_ENVIRONMENT".Equals(e.Key));
				return env.Equals(default(KeyValuePair<string, string>)) ? null : env.Value;
			}
		}

		internal ASPNetServerLessPublishConfigSection(IConfigurationRoot configRoot)
		{
			_configRoot = configRoot;
			AWS = new ASPNetServerLessPublishAWSConfigSection(_configRoot, SECTION_PATH);
		}
	}
}
