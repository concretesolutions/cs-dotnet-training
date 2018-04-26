using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASPNetLambdaProxy.Integration
{
    public class ASPNetLambdaProxyResponseDTO
    {
		public int StatusCode { get; set; }

		public Dictionary<string, string> Headers { get; set; }

		public string Body { get; set; }

		public bool IsBase64Encoded { get; set; }
	}
}
