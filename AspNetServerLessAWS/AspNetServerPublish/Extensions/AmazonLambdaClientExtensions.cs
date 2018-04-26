namespace AspNetServerPublish.Extensions
{
	using Amazon.Lambda;
	using Amazon.Lambda.Model;
	using AspNetServerPublish.Config;
	using System.Collections.Generic;
	using System.IO;
	using System.Threading;
	using System.Threading.Tasks;

	internal static class AmazonLambdaClientExtensions
	{
		internal static async Task<CreateFunctionResponse> CreateFunctionAsync
		(
			this AmazonLambdaClient client,
			AppPackage package,

			ASPNetServerLessPublishAWSLambdaConfigSection lambdaConfig,
			Runtime lambdaRuntime,

			CancellationToken cancellationToken = default(CancellationToken)
		)
		{
			var environmentVariables = new Dictionary<string, string>(lambdaConfig.EnvironmentVAriables);
			var functionTags = new Dictionary<string, string> { { "Name", lambdaConfig.FunctionName } };
			var response = (CreateFunctionResponse)null;

			using (var packageStream = new MemoryStream(package.PackageBytes))
			{
				var createFunctionRequest = new CreateFunctionRequest()
				{
					Code = new FunctionCode() { ZipFile = packageStream },
					Environment = new Environment() { Variables = environmentVariables },

					FunctionName = lambdaConfig.FunctionName,
					Handler = lambdaConfig.FunctionHandler,

					MemorySize = lambdaConfig.FunctionMemory.Value,
					Role = lambdaConfig.FunctionRole,

					Tags = functionTags,
					Timeout = lambdaConfig.FunctionTimeoutSeconds.Value,

					Runtime = lambdaRuntime
				};

				response = await client.CreateFunctionAsync(createFunctionRequest, cancellationToken).ConfigureAwait(false);
				createFunctionRequest = null;
			}

			functionTags = null;
			environmentVariables = null;

			return response;
		}

		internal static async Task<UpdateFunctionCodeResponse> UpdateFunctionCodeAsync
		(
			this AmazonLambdaClient client,
			AppPackage package,

			ASPNetServerLessPublishAWSLambdaConfigSection lambdaConfig,
			CancellationToken cancellationToken = default(CancellationToken)
		)
		{
			var response = (UpdateFunctionCodeResponse)null;

			using (var packageStream = new MemoryStream(package.PackageBytes))
			{
				var updateCodeRequest = new UpdateFunctionCodeRequest()
				{
					FunctionName = lambdaConfig.FunctionName,
					ZipFile = packageStream
				};

				response = await client.UpdateFunctionCodeAsync(updateCodeRequest, cancellationToken).ConfigureAwait(false);
				updateCodeRequest = null;
			}

			return response;
		}

		internal static Task<AddPermissionResponse> AddExecuteApiPermissionAsync
		(
			this AmazonLambdaClient client,
			ASPNetServerLessPublishAWSLambdaConfigSection lambdaConfig,

			string restApiId,
			string awsRegion,

			string accountId
		)
		{
			var permissionRequest = new AddPermissionRequest()
			{
				Action = lambdaConfig.PermissionAction,
				FunctionName = lambdaConfig.FunctionName,

				Principal = lambdaConfig.PermissionPrincipal,
				StatementId = lambdaConfig.PermissionStatementId,

				SourceArn = $"arn:aws:execute-api:{awsRegion}:{accountId}:{restApiId}/*/*/*"
			};

			return client.AddPermissionAsync(permissionRequest);
		}
	}
}
