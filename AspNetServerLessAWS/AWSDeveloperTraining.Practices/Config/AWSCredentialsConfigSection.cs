namespace AWSDeveloperTraining.Practices.Config
{
	using Microsoft.Extensions.Configuration;
	using System;

	public class AWSCredentialsConfigSection : BaseConfigSection, IAWSCredentialsConfigSection
	{
		private readonly string _keyIdOption;
		private readonly string _secretKeyOption;

		protected string SectionPath { get; private set; }

		public string AccessKeyId => GetSecret(_keyIdOption);

		public string SecretAccessKey => GetSecret(_secretKeyOption);

		public AWSCredentialsConfigSection(IConfigurationRoot configRoot, string parentPath) : base(configRoot)
		{
			if (string.IsNullOrWhiteSpace(parentPath))
			{
				throw new ArgumentException($"invalid {parentPath}");
			}

			SectionPath = parentPath.EndsWith(":") ? parentPath + "Credentials" : $"{parentPath}:Credentials";
			_keyIdOption = $"{SectionPath}:AccessKeyId";
			_secretKeyOption = $"{SectionPath}:SecretAccessKey";
		}
	}
}