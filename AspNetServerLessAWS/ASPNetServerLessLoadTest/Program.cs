namespace ASPNetServerLessLoadTest
{
	using Microsoft.Extensions.Configuration;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Serialization;
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;

	class Program
	{
		private static void SetSerializerSettings()
		{
			JsonConvert.DefaultSettings = new Func<JsonSerializerSettings>
			(
				() =>
				{
					return new JsonSerializerSettings()
					{
						ContractResolver = new CamelCasePropertyNamesContractResolver(),
						Formatting = Formatting.None,

						CheckAdditionalContent = false,
						DateFormatHandling = DateFormatHandling.IsoDateFormat,

						DateParseHandling = DateParseHandling.DateTimeOffset,
						DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,

						NullValueHandling = NullValueHandling.Ignore
					};
				}
			);
		}

		private static IConfigurationRoot CreateConfig()
		{
			var configBuilder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

			var configRoot = configBuilder.Build();
			return configRoot;
		}

		static void Main(string[] args)
		{
			System.Console.WriteLine("loading test agents...");
			SetSerializerSettings();

			var configRoot = CreateConfig();
			var postAccountUri = new Uri(configRoot["ASPNetServerLessLoadTest:PostAccountUri"]);

			var requestCount = 1500;
			var agentCount = 4;

			var requestQueueSize = 20;
			var agentCollection = new PostAccountTestAgent[agentCount];

			for (int i = 0; i < agentCount; i++)
			{
				agentCollection[i] = new PostAccountTestAgent(postAccountUri, requestCount, requestQueueSize);
			}

			System.Console.WriteLine($"executing tests... ({postAccountUri.ToString()})");
			var stopWatch = new Stopwatch();

			stopWatch.Start();
			Task.WaitAll(agentCollection.Select(a => a.ExecuteTestAsync()).ToArray());

			stopWatch.Stop();
			var agentResults = agentCollection.SelectMany(a => a.Responses).ToList();

			System.Console.WriteLine("tests results: ");

			System.Console.WriteLine($"	total requests: {agentResults.Count()}");
			System.Console.WriteLine($"	number of test agents: {agentCount}");

			System.Console.WriteLine($"	agent's queue size: {requestQueueSize}");
			System.Console.WriteLine($"	error requests: {agentResults.Count(r => r.RequestResponse.StatusCode != 201)}");

			System.Console.WriteLine($"	request time in miliseconds (avg): {agentResults.Average(r => r.ElapsedMiliseconds)}");
			System.Console.WriteLine($"	lambda proxy time in miliseconds (avg): {agentResults.Average(r => r.ProxyMiliseconds)}");

			System.Console.WriteLine($" test time in seconds: {stopWatch.Elapsed.TotalSeconds}");
		}
	}
}
