namespace AWSDeveloperTraining.Practices.Config
{
	public interface IAWSConfigSection
	{
		IAWSCredentialsConfigSection Credentials { get; }

		string DefaultRegion { get; }
	}
}
