namespace AWSDeveloperTraining.Practices.Config
{
	using AWSDeveloperTraining.Practices.Config;
	using Microsoft.Extensions.Configuration;
	using System;

	public class AWSConfigSection : BaseConfigSection, IAWSConfigSection
	{
		private readonly string _defaultRegionOption;

		protected string SectionPath { get; set; }

		public IAWSCredentialsConfigSection Credentials { get; private set; }

		public string DefaultRegion => ConfigRoot[_defaultRegionOption];

		public AWSConfigSection(IConfigurationRoot configRoot, string parentPath) : base(configRoot)
		{
			if (string.IsNullOrWhiteSpace(parentPath))
			{
				throw new ArgumentException($"invalid {nameof(parentPath)}");
			}

			SectionPath = parentPath.EndsWith(":") ? parentPath + "AWS" : $"{parentPath}:AWS";
			Credentials = new AWSCredentialsConfigSection(configRoot, SectionPath);

			_defaultRegionOption = $"{SectionPath}:DefaultRegion";
		}
	}
}
