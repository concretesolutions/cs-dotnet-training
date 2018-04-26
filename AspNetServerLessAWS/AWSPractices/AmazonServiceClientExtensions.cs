namespace AWSPractices
{
	using Amazon.Runtime;
	using System;
	using System.Threading.Tasks;

	public static class AmazonServiceClientExtensions
	{
		public static async Task<TReturn> ExecuteWithNoExceptionsAsync<TClient, TReturn>(this TClient client, Func<TClient, Task<TReturn>> asyncRequest) where TReturn : AmazonWebServiceResponse, new()
		{
			try
			{
				return await asyncRequest(client).ConfigureAwait(false);
			}
			catch (AmazonServiceException e)
			{
				var response = new TReturn { HttpStatusCode = e.StatusCode };
				response.ResponseMetadata = new ResponseMetadata() { RequestId = e.RequestId };

				response.ResponseMetadata.Metadata.Add("ExceptionMessage", e.Message);
				return response;
			}
		}
	}
}
