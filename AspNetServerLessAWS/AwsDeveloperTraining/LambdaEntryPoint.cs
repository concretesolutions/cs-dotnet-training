namespace AwsDeveloperTraining
{
	using Amazon.Lambda.AspNetCoreServer;
	using Microsoft.AspNetCore.Hosting;
	using System.IO;

	public class LambdaEntryPoint : APIGatewayProxyFunction
	{
		protected override void Init(IWebHostBuilder builder)
		{
			builder
				.UseContentRoot(Directory.GetCurrentDirectory())
				.UseStartup<Startup>()
				.UseApiGateway();
		}
	}
}
