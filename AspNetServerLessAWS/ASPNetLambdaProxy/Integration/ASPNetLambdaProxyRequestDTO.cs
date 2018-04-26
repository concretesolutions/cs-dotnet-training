using System.Collections.Generic;

namespace ASPNetLambdaProxy.Integration
{
	public class ASPNetLambdaProxyRequestDTO
	{
		public string Resource { get; set; }

		public string Path { get; set; }

		public string HttpMethod { get; set; }

		public Dictionary<string, string> Headers { get; set; }

		public Dictionary<string, string> QueryStringParameters { get; set; }

		public string Body { get; set; }
	}
}
