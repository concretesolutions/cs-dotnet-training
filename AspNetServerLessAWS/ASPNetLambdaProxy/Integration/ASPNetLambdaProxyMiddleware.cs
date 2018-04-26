namespace ASPNetLambdaProxy.Integration
{
	using Amazon.Lambda;
	using Amazon.Lambda.Model;
	using ASPNetLambdaProxy.Config;
	using Microsoft.AspNetCore.Http;
	using System.Collections.Generic;
	using System.IO;
	using System.Threading.Tasks;
	using System.Linq;
	using Newtonsoft.Json;
	using System;
	using AWSPractices;
	using System.Diagnostics;

	internal class ASPNetLambdaProxyMiddleware :IDisposable
	{
		private RequestDelegate _next;
		private IAmazonLambda _lambdaClient;
		private IASPNetLambdaProxyConfigSection _config;

		public ASPNetLambdaProxyMiddleware(RequestDelegate next, IAmazonLambda lambdaClient, IASPNetLambdaProxyConfigSection config)
		{
			_next = next;
			_lambdaClient = lambdaClient;
			_config = config;
		}
				
		public async Task Invoke(HttpContext context)
		{
			var excMessage = "";

			try
			{
				using (var requestReader = new StreamReader(context.Request.Body))
				{
					//mapping headers and query string from request
					var requestHeaders = context.Request.Headers.Select(p => new KeyValuePair<string, string>(p.Key, p.Value));
					var requestParameters = context.Request.Query.Select(p => new KeyValuePair<string, string>(p.Key, p.Value));

					//mapping request payload
					var requestPayload = new ASPNetLambdaProxyRequestDTO()
					{
						Headers = new Dictionary<string, string>(requestHeaders),
						Resource = "/{proxy+}",

						HttpMethod = context.Request.Method,
						Path = context.Request.Path.Value,

						Body = await requestReader.ReadToEndAsync().ConfigureAwait(false),
						QueryStringParameters = new Dictionary<string, string>(requestParameters)
					};

					requestHeaders = null;
					requestParameters = null;

					//invoking lambda
					var invokeRequest = new InvokeRequest()
					{
						FunctionName = _config.AWS.Lambda.FunctionName,
						InvocationType = InvocationType.RequestResponse,
						Payload = JsonConvert.SerializeObject(requestPayload)
					};

					var integrationWatch = new Stopwatch();
					integrationWatch.Start();

					var invokeResponse = await _lambdaClient
						.ExecuteWithNoExceptionsAsync(c => c.InvokeAsync(invokeRequest))
						.ConfigureAwait(false);

					integrationWatch.Stop();

					//executing pipeline
					invokeRequest = null;
					await _next.Invoke(context).ConfigureAwait(false);

					//mapping lambda response
					var responsePayload = (ASPNetLambdaProxyResponseDTO)null;

					using (var responseReader = new StreamReader(invokeResponse.Payload))
					{
						responsePayload = JsonConvert.DeserializeObject<ASPNetLambdaProxyResponseDTO>(responseReader.ReadToEnd());
					}

					responsePayload.Headers.Add("X-Proxy-Time", integrationWatch.ElapsedMilliseconds.ToString());
					integrationWatch = null;

					//loading response content
					foreach (var pair in responsePayload.Headers)
					{
						if (context.Response.Headers.ContainsKey(pair.Key))
						{
							context.Response.Headers[pair.Key] = pair.Value;
						}
						else
						{
							context.Response.Headers.Add(pair.Key, pair.Value);
						}
					}

					using (var responseWriter = new StreamWriter(context.Response.Body))
					{
						context.Response.StatusCode = (int)invokeResponse.HttpStatusCode;
						await responseWriter.WriteAsync(responsePayload.Body).ConfigureAwait(false);
					}

					invokeResponse = null;
					responsePayload = null;
				}
			}
			catch (Exception e)
			{
				context.Response.StatusCode = 500;
				excMessage = e.Message;
			}
			finally
			{
				if (string.IsNullOrWhiteSpace(excMessage) == false)
				{
					await context.Response.WriteAsync(excMessage).ConfigureAwait(false);
				}
			}
		}

		public void Dispose()
		{
			_lambdaClient.Dispose();
			_lambdaClient = null;
			_config = null;
		}
	}
}
