namespace ASPNetServerLessLoadTest
{
	using AwsDeveloperTraining.ServiceModel;
	using Newtonsoft.Json;
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Net;
	using System.Net.Http;
	using System.Text;
	using System.Threading.Tasks;
	using System.Linq;

	internal class PostAccountTestAgent 
	{
		private ConcurrentBag<PostAccountTestResult> _responses;

		public IEnumerable<PostAccountTestResult> Responses => _responses;

		public Uri PostAccountUri { get; private set; }

		public int RequestCount { get; private set; }

		public int RequestQueueSize { get; private set; }

		public int CurrentRequest { get; private set; }

		public string AgentName { get; private set; }

		internal PostAccountTestAgent(Uri postAcountUri, int requestCount, int requestQueueSize, string agentName = null)
		{
			_responses = new ConcurrentBag<PostAccountTestResult>();

			PostAccountUri = postAcountUri;
			RequestCount = requestCount;

			RequestQueueSize = requestQueueSize;
			AgentName = agentName ?? Guid.NewGuid().ToString().Split("-")[0].ToLower();
		}

		private async Task ExecuteRequestAsync()
		{
			const string mediaType = "application/json";

			var requestDTO = new PostAccountRequestDTO()
			{
				AccountName = $"usr.{AgentName}.{CurrentRequest}",
				Password = $"pss-{AgentName}-{CurrentRequest}"
			};

			var serializedDTO = JsonConvert.SerializeObject(requestDTO);
			var responseDTO = (PostAccountResponseDTO)null;

			var stopWatch = new Stopwatch();
			var proxyMiliseconds = (long)0;

			using (var httpClient = new HttpClient())
			{
				httpClient.Timeout = new TimeSpan(0, 0, 30);

				using (var requestContent = new StringContent(serializedDTO, Encoding.UTF8, mediaType))
				{
					using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, PostAccountUri))
					{
						requestMessage.Headers.Add(HttpRequestHeader.ContentType.ToString(), mediaType);
						requestMessage.Content = requestContent;

						stopWatch.Start();
						using (var responseMessage = await httpClient.SendAsync(requestMessage).ConfigureAwait(false))
						{
							var responseContent = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
							stopWatch.Stop();

							try
							{
								proxyMiliseconds = long.Parse(responseMessage.Headers.GetValues("X-Proxy-Time").Single());
								responseDTO = JsonConvert.DeserializeObject<PostAccountResponseDTO>(responseContent);
							}
							catch (Exception)
							{
								System.Console.WriteLine("error:");
								System.Console.WriteLine(responseContent);

								throw;
							}
						}
					}
				}
			}

			_responses.Add(new PostAccountTestResult(responseDTO, stopWatch.ElapsedMilliseconds, proxyMiliseconds));
		}

		public async Task ExecuteTestAsync()
		{
			var requestQueue = new List<Task>();

			for (int i = 0; i < RequestCount; i++)
			{
				CurrentRequest = i + 1;

				if (requestQueue.Count < RequestQueueSize)
				{
					requestQueue.Add(ExecuteRequestAsync());
				}
				else
				{
					if (requestQueue[0].Status != TaskStatus.RanToCompletion)
					{
						await requestQueue[0].ConfigureAwait(false);
					}

					requestQueue.RemoveAt(0);
					requestQueue.Add(ExecuteRequestAsync());
				}
			}

			await Task.WhenAll(requestQueue.ToArray()).ConfigureAwait(false);
		}
	}
}
