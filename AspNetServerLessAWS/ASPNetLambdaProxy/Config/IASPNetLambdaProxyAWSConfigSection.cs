namespace ASPNetLambdaProxy.Config
{
	using AWSDeveloperTraining.Practices.Config;

	public interface IASPNetLambdaProxyAWSConfigSection : IAWSConfigSection
	{
		IAWSLambdaConfigSection Lambda { get; }
	}
}
