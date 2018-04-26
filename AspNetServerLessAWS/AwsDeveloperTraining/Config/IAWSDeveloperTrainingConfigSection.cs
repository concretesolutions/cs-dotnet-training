namespace AwsDeveloperTraining.Config
{
	using AWSDeveloperTraining.Practices.Config;

	public interface IAWSDeveloperTrainingConfigSection
	{
		IAWSConfigSection AWS { get; }

		string EnvironmentSecret { get; }

		string CredentialSalt { get; }
	}
}