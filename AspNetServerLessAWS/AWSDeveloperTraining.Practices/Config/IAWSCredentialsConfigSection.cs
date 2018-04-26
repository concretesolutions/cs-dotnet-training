namespace AWSDeveloperTraining.Practices.Config
{
	public interface IAWSCredentialsConfigSection
	{
		string AccessKeyId { get; }

		string SecretAccessKey { get; }
	}
}