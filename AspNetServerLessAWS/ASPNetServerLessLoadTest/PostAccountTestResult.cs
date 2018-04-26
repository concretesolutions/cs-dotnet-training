namespace ASPNetServerLessLoadTest
{
	using AwsDeveloperTraining.ServiceModel;

	internal class PostAccountTestResult
	{
		public PostAccountResponseDTO RequestResponse { get; private set; }

		public long ElapsedMiliseconds { get; private set; }

		public long ProxyMiliseconds { get; private set; }

		internal PostAccountTestResult(PostAccountResponseDTO requestResponse, long elapsedMiliSenconds, long proxyMiliSeconds)
		{
			RequestResponse = requestResponse;
			ElapsedMiliseconds = elapsedMiliSenconds;
			ProxyMiliseconds = proxyMiliSeconds;
		}
	}
}
