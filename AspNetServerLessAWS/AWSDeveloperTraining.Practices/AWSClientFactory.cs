namespace AWSDeveloperTraining.Practices
{
	using Amazon;
	using Amazon.Lambda;
	using Amazon.Runtime;
	using Amazon.S3;
	using AWSDeveloperTraining.Practices.Config;
	using Microsoft.AspNetCore.Hosting;
	using System;

	public class AWSClientFactory : IAWSClientFactory
	{
		private readonly IHostingEnvironment _env;
		private readonly IAWSConfigSection _awsConfig;

		public AWSClientFactory(IAWSConfigSection awsConfig, IHostingEnvironment env)
		{
			_awsConfig = awsConfig ?? throw new ArgumentNullException(nameof(awsConfig));
			_env = env ?? throw new ArgumentNullException(nameof(env));
		}

		public IAmazonS3 CreateS3Client()
		{
			var region = RegionEndpoint.GetBySystemName(_awsConfig.DefaultRegion);

			if (_env.IsDevelopment())
			{
				return new AmazonS3Client
				(
					new BasicAWSCredentials(_awsConfig.Credentials.AccessKeyId, _awsConfig.Credentials.SecretAccessKey),
					region
				);
			}

			return new AmazonS3Client(region);
		}

		public IAmazonLambda CreateLambdaClient()
		{
			var region = RegionEndpoint.GetBySystemName(_awsConfig.DefaultRegion);

			if (_env.IsDevelopment())
			{
				return new AmazonLambdaClient
				(
					new BasicAWSCredentials(_awsConfig.Credentials.AccessKeyId, _awsConfig.Credentials.SecretAccessKey),
					region
				);
			}

			return new AmazonLambdaClient(region);
		}
	}
}
