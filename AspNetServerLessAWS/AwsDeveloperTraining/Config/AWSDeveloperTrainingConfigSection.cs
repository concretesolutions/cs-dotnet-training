namespace AwsDeveloperTraining.Config
{
	using AWSDeveloperTraining.Practices.Config;
	using Microsoft.Extensions.Configuration;

	internal class AWSDeveloperTrainingConfigSection : BaseConfigSection, IAWSDeveloperTrainingConfigSection
	{
		private const string _sectionPath = "AwsDeveloperTraining";
		private static readonly string _environmentSecretOption = $"{_sectionPath}:EnvironmentSecret";
		private static readonly string _credentialSaltOption = $"{_sectionPath}:CredentialSalt";

		public IAWSConfigSection AWS { get; private set; }

		public string EnvironmentSecret => GetSecret(_environmentSecretOption);

		public string CredentialSalt => GetSecret(_credentialSaltOption);

		internal AWSDeveloperTrainingConfigSection(IConfigurationRoot configRoot)
			: base(configRoot)
		{
			AWS = new AWSConfigSection(configRoot, _sectionPath);
		}
	}
}