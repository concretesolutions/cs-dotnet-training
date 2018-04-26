

namespace AspNetServerPublish
{
	using Amazon;
	using Amazon.APIGateway;
	using Amazon.APIGateway.Model;
	using Amazon.Lambda;
	using Amazon.Lambda.Model;
	using Amazon.Runtime;
	using AspNetServerPublish.Config;
	using AWSPractices;
	using Microsoft.Extensions.Configuration;
	using Newtonsoft.Json;
	using System.IO;
	using System.Net;
	using System.Linq;
	using AspNetServerPublish.Extensions;
	using AWSDeveloperTraining.Practices.Config;

	class Program
	{
		private static void ExitDueUnexpectedResponse(object response)
		{
			System.Console.WriteLine("AWS Lambda returned an unexpected response:");
			System.Console.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));
			System.Environment.Exit(1);
		}

		private static ASPNetServerLessPublishConfigSection CreateConfiguration()
		{
			//loading config
			System.Console.WriteLine("loading configuration...");
			var rootDir = Directory.GetCurrentDirectory();

			var configBuilder = new ConfigurationBuilder()
				.AddJsonFile($"{rootDir}/appsettings.json", optional: true, reloadOnChange: true)
				.AddJsonFile($"{rootDir}/Secrets/secrets.json", optional: true, reloadOnChange: true);

			var configRoot = configBuilder.Build();
			var config = new ASPNetServerLessPublishConfigSection(configRoot);

			return config;
		}

		static void Main(string[] args)
		{
			var config = CreateConfiguration();
			var rootDir = Directory.GetCurrentDirectory();

			var awsRegion = RegionEndpoint.GetBySystemName(config.AWS.DefaultRegion);
			var awsCredential = new BasicAWSCredentials(config.AWS.Credentials.AccessKeyId, config.AWS.Credentials.SecretAccessKey);

			//checking package file
			System.Console.WriteLine("checking package file...");

			if (File.Exists(config.PackagePath) == false)
			{
				System.Console.WriteLine("package file does not exists.");
				System.Environment.Exit(1);
				return;
			}

			var appPackage = new AppPackage(config.PackagePath);
			var functionARN = "";

			//creating lambda function
			using (var lambdaClient = new AmazonLambdaClient(awsCredential, awsRegion))
			{
				//checking lambda function
				System.Console.WriteLine("checking if lambda function exists...");

				var getFunctionResponse = lambdaClient
					.ExecuteWithNoExceptionsAsync(c => { return c.GetFunctionConfigurationAsync(config.AWS.Lambda.FunctionName); })
					.Result;

				if (getFunctionResponse.HttpStatusCode != HttpStatusCode.OK && getFunctionResponse.HttpStatusCode != HttpStatusCode.NotFound)
				{
					ExitDueUnexpectedResponse(getFunctionResponse);
					return;
				}

				//function does not exists, create a new
				if (getFunctionResponse.HttpStatusCode == HttpStatusCode.NotFound)
				{
					System.Console.WriteLine("lambda function does not exists, creating function....");

					var createFunctionResponse = lambdaClient
						.ExecuteWithNoExceptionsAsync(c => { return c.CreateFunctionAsync(appPackage, config.AWS.Lambda, Runtime.Dotnetcore20); })
						.Result;

					//checking create function response
					if (createFunctionResponse.HttpStatusCode != HttpStatusCode.Created)
					{
						ExitDueUnexpectedResponse(createFunctionResponse);
						return;
					}

					System.Console.WriteLine("lambda function created with success.");
					functionARN = createFunctionResponse.FunctionArn;
				}
				else
				{
					functionARN = getFunctionResponse.FunctionArn;
				}

				//checking if is new package
				var hashFilePath = $"{rootDir}/package.hash";
				var isSamePackage = false;

				if (File.Exists(hashFilePath))
				{
					var currentHash = File.ReadAllText(hashFilePath);
					isSamePackage = appPackage.PackageSha256.Equals(currentHash);
				}

				if (getFunctionResponse.HttpStatusCode == HttpStatusCode.OK)
				{
					functionARN = getFunctionResponse.FunctionArn;

					if (isSamePackage == false)
					{

						System.Console.WriteLine("lambda function exists, updating lambda function code...");

						var updateCodeResponse = lambdaClient
							.ExecuteWithNoExceptionsAsync(c => { return c.UpdateFunctionCodeAsync(appPackage, config.AWS.Lambda); })
							.Result;

						if (updateCodeResponse.HttpStatusCode != HttpStatusCode.OK)
						{
							ExitDueUnexpectedResponse(updateCodeResponse);
							return;
						}

						System.Console.WriteLine("lambda function code updated with success.");
					}
					else
					{
						System.Console.WriteLine("lambda function exists and its code is updated.");
					}
				}

				if (isSamePackage == false)
				{
					File.WriteAllText(hashFilePath, appPackage.PackageSha256);
				}
			}

			using (var gatewayClient = new AmazonAPIGatewayClient(awsCredential, awsRegion))
			{
				//checking if api exists
				System.Console.WriteLine("checking existence of api gateway...");

				var listApiResponse = gatewayClient
					.ExecuteWithNoExceptionsAsync(c => { return c.GetRestApisAsync(config.AWS.ApiGateway.GatewayName); })
					.Result;

				if (listApiResponse.HttpStatusCode != HttpStatusCode.OK)
				{
					ExitDueUnexpectedResponse(listApiResponse);
					return;
				}

				//creating api
				var restApi = listApiResponse.Items.SingleOrDefault();

				if (restApi == null)
				{
					System.Console.WriteLine("api gateway does not exists, creating api gateway...");

					var createApiResponse = gatewayClient
						.ExecuteWithNoExceptionsAsync(c => { return c.CreateRestApiAsync(config.AWS.ApiGateway.GatewayName); })
						.Result;

					if (createApiResponse.HttpStatusCode != HttpStatusCode.Created)
					{
						ExitDueUnexpectedResponse(createApiResponse);
						return;
					}

					restApi = new RestApi
					{
						Id = createApiResponse.Id,
						Name = config.AWS.ApiGateway.GatewayName
					};
				}
				else
				{
					System.Console.WriteLine("api gateway - OK");
				}

				//checkig if resource exists
				System.Console.WriteLine("checking existence of api resource...");
				var listResourceRequest = new GetResourcesRequest() { RestApiId = restApi.Id, };

				var listResourceResponse = gatewayClient
					.ExecuteWithNoExceptionsAsync(c => { return c.GetResourcesAsync(listResourceRequest); })
					.Result;

				if (listResourceResponse.HttpStatusCode != HttpStatusCode.OK)
				{
					ExitDueUnexpectedResponse(listResourceResponse);
					return;
				}

				var rootResource = listResourceResponse.Items.GetRootResource() ?? throw new System.Exception($"root resource can't be found on api:{restApi.Id}");
				var proxyResource = listResourceResponse.Items.GetProxyResource();

				if (proxyResource == null)
				{
					//creating resource
					System.Console.WriteLine("api resource not found, creating api resource...");

					var createResourceResponse = gatewayClient
						.ExecuteWithNoExceptionsAsync(c => { return c.CreateProxyResourceAsync(restApi.Id, rootResource.Id); })
						.Result;

					if (createResourceResponse.HttpStatusCode != HttpStatusCode.Created)
					{
						ExitDueUnexpectedResponse(createResourceResponse);
						return;
					}

					proxyResource = createResourceResponse.MapToResource();
					System.Console.WriteLine("api resource created.");
				}
				else
				{
					System.Console.WriteLine("api resource - OK");
				}

				//checking if methods were created
				System.Console.WriteLine("checking if resource methods exists...");

				var getMethodRequest = new GetMethodRequest()
				{
					HttpMethod = AmazonModelExtensions.ANY_METHOD,
					ResourceId = proxyResource.Id,
					RestApiId = restApi.Id
				};

				var getMethodResponse = gatewayClient
					.ExecuteWithNoExceptionsAsync(c => { return c.GetMethodAsync(getMethodRequest); })
					.Result;

				if (getMethodResponse.HttpStatusCode != HttpStatusCode.OK && getMethodResponse.HttpStatusCode != HttpStatusCode.NotFound)
				{
					ExitDueUnexpectedResponse(getMethodResponse);
					return;
				}

				if (getMethodResponse.HttpStatusCode == HttpStatusCode.NotFound)
				{
					System.Console.WriteLine("resource methods does not exists, creating resource methods....");

					var putMethodResponse = gatewayClient.ExecuteWithNoExceptionsAsync
					(
						c =>
						{
							return c.PutProxyMethodAsync(proxyResource.Id, restApi.Id, AmazonModelExtensions.ANY_METHOD, "proxy");
						}
					)
					.Result;

					if (putMethodResponse.HttpStatusCode != HttpStatusCode.Created)
					{
						ExitDueUnexpectedResponse(putMethodResponse);
						return;
					}

					System.Console.WriteLine("resource method created with success");
				}
				else
				{
					System.Console.WriteLine("resource methods - OK");
				}

				//checking integration with lambda function
				System.Console.WriteLine("checking if integration with lambda exists...");

				var getIntegrationRequest = new GetIntegrationRequest()
				{
					HttpMethod = AmazonModelExtensions.ANY_METHOD,
					ResourceId = proxyResource.Id,
					RestApiId = restApi.Id
				};

				var getIntegrationResponse = gatewayClient
					.ExecuteWithNoExceptionsAsync(c => { return c.GetIntegrationAsync(getIntegrationRequest); })
					.Result;

				if (getIntegrationResponse.HttpStatusCode != HttpStatusCode.OK && getIntegrationResponse.HttpStatusCode != HttpStatusCode.NotFound)
				{
					ExitDueUnexpectedResponse(getIntegrationResponse);
					return;
				}

				if (getIntegrationResponse.HttpStatusCode == HttpStatusCode.NotFound)
				{
					System.Console.WriteLine("integration with lambda does not exists, creating integration...");

					var putIntegrationResponse = gatewayClient.ExecuteWithNoExceptionsAsync
					(
						c =>
						{
							return c.PutLambdaProxyIntegrationAsync
							(
								proxyResource.Id,
								restApi.Id,

								AmazonModelExtensions.ANY_METHOD,
								config.AWS.Lambda.FunctionTimeoutSeconds.Value * 1000,

								functionARN,
								config.AWS.DefaultRegion
							);
						}
					)
					.Result;

					if (putIntegrationResponse.HttpStatusCode != HttpStatusCode.Created)
					{
						ExitDueUnexpectedResponse(putIntegrationResponse);
						return;
					}

					System.Console.WriteLine("integration with lambda created with success");
				}
				else
				{
					System.Console.WriteLine("integration with lambda - OK");
				}

				//checking deployment
				System.Console.WriteLine("checking api gateway deployment...");
				var getDeployRequest = new GetDeploymentsRequest() { RestApiId = restApi.Id };

				var getDeployResponse = gatewayClient
					.ExecuteWithNoExceptionsAsync(c => { return c.GetDeploymentsAsync(getDeployRequest); })
					.Result;

				if (getDeployResponse.HttpStatusCode != HttpStatusCode.OK)
				{
					ExitDueUnexpectedResponse(getDeployResponse);
					return;
				}

				var deployment = (Deployment)null;

				if (getDeployResponse.Items.Count == 0)
				{
					System.Console.WriteLine("api gateway deployment not found, deploying api gateway...");

					var createDeployRequest = new CreateDeploymentRequest()
					{
						RestApiId = restApi.Id,
						StageName = config.ASPNETCORE_ENVIRONMENT,
						StageDescription = config.ASPNETCORE_ENVIRONMENT,
					};

					var createDeployResponse = gatewayClient
						.ExecuteWithNoExceptionsAsync(c => { return c.CreateDeploymentAsync(createDeployRequest); })
						.Result;

					if (createDeployResponse.HttpStatusCode != HttpStatusCode.Created)
					{
						ExitDueUnexpectedResponse(createDeployResponse);
						return;
					}

					System.Console.WriteLine("api gateway deployed with success.");
				}
				else
				{
					deployment = getDeployResponse.Items[0];
					System.Console.WriteLine("api gateway deployment - OK");
				}

				using (var lambdaClient = new AmazonLambdaClient(awsCredential, awsRegion))
				{
					System.Console.WriteLine("grating permission to api gateway trigger lambda...");

					var permissionResponse = lambdaClient.ExecuteWithNoExceptionsAsync
					(
						c =>
						{
							return c.AddExecuteApiPermissionAsync
							(
								config.AWS.Lambda, restApi.Id, config.AWS.DefaultRegion, config.AWS.AccountId
							);
						}
					)
					.Result;

					if (permissionResponse.HttpStatusCode != HttpStatusCode.Created && permissionResponse.HttpStatusCode != HttpStatusCode.Conflict)
					{
						ExitDueUnexpectedResponse(permissionResponse);
						return;
					}

					if (permissionResponse.HttpStatusCode == HttpStatusCode.Conflict)
					{
						System.Console.WriteLine("permission already granted.");
					}
					else
					{
						System.Console.WriteLine("permission granted with success.");
					}
				}
			}
		}
	}
}