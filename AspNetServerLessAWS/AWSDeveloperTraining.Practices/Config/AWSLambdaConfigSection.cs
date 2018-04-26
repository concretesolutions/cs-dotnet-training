namespace AWSDeveloperTraining.Practices.Config
{
	using Microsoft.Extensions.Configuration;
	using System;

	public class AWSLambdaConfigSection : BaseConfigSection, IAWSLambdaConfigSection
	{
		private readonly string _functionNameOption;

		protected string SectionPath { get; private set; }

		public string FunctionName => ConfigRoot[_functionNameOption];

		public AWSLambdaConfigSection(IConfigurationRoot configRoot, string parentPath) : base(configRoot)
		{
			if (string.IsNullOrWhiteSpace(parentPath))
			{
				throw new ArgumentNullException();
			}

			SectionPath = parentPath.EndsWith(":") ? parentPath + "Lambda" : $"{parentPath}:Lambda";
			_functionNameOption = $"{SectionPath}:FunctionName";
		}		
	}
}
