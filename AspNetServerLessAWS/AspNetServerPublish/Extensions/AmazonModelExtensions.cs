namespace AspNetServerPublish.Extensions
{
	using Amazon.APIGateway.Model;
	using System.Collections.Generic;
	using System.Linq;

	internal static class AmazonModelExtensions
	{
		internal const string PROXY_RESOURCE_PATH = "{proxy+}";

		internal const string ANY_METHOD = "ANY";

		internal static Resource GetRootResource(this IEnumerable<Resource> resources)
		{
			return resources.FirstOrDefault(r => "/".Equals(r.Path));
		}

		internal static Resource GetProxyResource(this IEnumerable<Resource> resources)
		{
			return resources.FirstOrDefault(r => PROXY_RESOURCE_PATH.Equals(r.PathPart));
		}

		internal static Resource MapToResource(this CreateResourceResponse response)
		{
			return new Resource()
			{
				Id = response.Id,
				ParentId = response.ParentId,

				Path = response.Path,
				PathPart = response.PathPart,

				ResourceMethods = response.ResourceMethods
			};
		}
	}
}
