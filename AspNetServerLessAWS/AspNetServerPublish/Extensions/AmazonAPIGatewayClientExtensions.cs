namespace AspNetServerPublish.Extensions
{
	using Amazon.APIGateway;
	using Amazon.APIGateway.Model;
	using System.Threading.Tasks;

	internal static class AmazonAPIGatewayClientExtensions
	{
		internal static async Task<GetRestApisResponse> GetRestApisAsync(this AmazonAPIGatewayClient client, string apiName)
		{
			var listApiRequest = new GetRestApisRequest();
			var listApiResponse = await client.GetRestApisAsync(listApiRequest).ConfigureAwait(false);

			listApiResponse.Items.RemoveAll(a => string.Equals(apiName, a.Name) == false);
			return listApiResponse;
		}

		internal static Task<CreateRestApiResponse> CreateRestApiAsync(this AmazonAPIGatewayClient client, string apiName)
		{
			var createApiRequest = new CreateRestApiRequest()
			{
				ApiKeySource = ApiKeySourceType.HEADER,
				EndpointConfiguration = new EndpointConfiguration() { Types = { "EDGE" } },
				Name = apiName,
			};

			return client.CreateRestApiAsync(createApiRequest);
		}

		internal static Task<CreateResourceResponse> CreateProxyResourceAsync(this AmazonAPIGatewayClient client, string restApiId, string parentId)
		{
			var createResourceRequest = new CreateResourceRequest()
			{
				RestApiId = restApiId,
				ParentId = parentId,
				PathPart = AmazonModelExtensions.PROXY_RESOURCE_PATH
			};

			return client.CreateResourceAsync(createResourceRequest);
		}

		internal static Task<PutMethodResponse> PutProxyMethodAsync
		(
			this AmazonAPIGatewayClient client,
			string proxyResourceId,

			string restApiId,
			string httpMethod,

			string operationName
		)
		{
			var putMethodRequest = new PutMethodRequest()
			{
				ApiKeyRequired = false,
				AuthorizationType = "NONE",

				HttpMethod = httpMethod,
				OperationName = operationName,

				ResourceId = proxyResourceId,
				RestApiId = restApiId
			};

			return client.PutMethodAsync(putMethodRequest);
		}

		internal static Task<PutIntegrationResponse> PutLambdaProxyIntegrationAsync
		(
			this AmazonAPIGatewayClient client,

			string proxyResourceId,
			string restApiId,

			string httpMethod,
			int integrationTimeoutMilis,

			string lambdaARN,
			string awsRegion
		)
		{
			var putIntegrationRequest = new PutIntegrationRequest()
			{
				HttpMethod = httpMethod,
				IntegrationHttpMethod = "POST",

				Type = IntegrationType.AWS_PROXY,
				Uri = $"arn:aws:apigateway:{awsRegion}:lambda:path/2015-03-31/functions/{lambdaARN}/invocations",

				ResourceId = proxyResourceId,
				RestApiId = restApiId,

				TimeoutInMillis = integrationTimeoutMilis,
				ContentHandling = ContentHandlingStrategy.CONVERT_TO_TEXT,

				PassthroughBehavior = "WHEN_NO_MATCH"
			};

			return client.PutIntegrationAsync(putIntegrationRequest);
		}
	}
}
