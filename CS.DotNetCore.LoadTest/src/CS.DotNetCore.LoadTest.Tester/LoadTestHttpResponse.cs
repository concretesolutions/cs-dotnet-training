namespace CS.DotNetCore.LoadTest.Tester
{
    public class LoadTestHttpResponse
    {
        public int StatusCode { get; private set; }

        public string ResponseBody { get; private set; }

        public LoadTestHttpResponse(int statusCode, string responseBody)
        {
            StatusCode = statusCode;
            ResponseBody = responseBody;
        }
    }
}
