namespace ASPNetLambdaProxy.Config
{
	using AWSDeveloperTraining.Practices.Config;

	interface IASPNetLambdaProxyConfigSection
    {
		IASPNetLambdaProxyAWSConfigSection AWS { get; }
	}
}
