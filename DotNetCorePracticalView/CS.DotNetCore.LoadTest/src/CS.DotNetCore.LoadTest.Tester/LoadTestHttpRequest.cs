namespace CS.DotNetCore.LoadTest.Tester
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    public class LoadTestHttpRequest : IDisposable
    {
        private HttpClient _httpClient;

        public Uri RequestUri { get; set; }

        public HttpMethod RequestMethod { get; set; }

        public Dictionary<string, string> RequestHeaders { get; private set; }

        public string RequestBody { get; set; }

        public LoadTestHttpRequest()
        {
            RequestHeaders = new Dictionary<string, string>();
            var messageHandler = new HttpClientHandler() { MaxConnectionsPerServer = 2 };
            _httpClient = new HttpClient(messageHandler, true);
        }

        private HttpRequestMessage CreateRequestMessage()
        {
            var message = new HttpRequestMessage(RequestMethod, RequestUri);

            foreach (var header in RequestHeaders)
            {
                message.Headers.Add(header.Key, header.Value);
            }

            return message;
        }

        public void SetRequestHeaders(IEnumerable<KeyValuePair<string, string>> requestHeaders)
        {
            if (requestHeaders != null)
            {
                foreach (var header in requestHeaders)
                {
                    if (RequestHeaders.ContainsKey(header.Key))
                    {
                        RequestHeaders[header.Key] = header.Value;
                    }
                    else
                    {
                        RequestHeaders.Add(header.Key, header.Value);
                    }
                }
            }
        }

        public async Task<LoadTestHttpResponse> SendAsync()
        {
            using (var httpContent = new StringContent(RequestBody == null ? string.Empty : RequestBody, Encoding.UTF8, "application/json"))
            {
                using (var requestMessage = CreateRequestMessage())
                {
                    requestMessage.Content = httpContent;

                    using (var httpResponse = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false))
                    {
                        return new LoadTestHttpResponse((int)httpResponse.StatusCode,
                            await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false));
                    }
                }
            }
        }

        public void Dispose()
        {
            _httpClient.Dispose();
            _httpClient = null;
        }
    }
}
