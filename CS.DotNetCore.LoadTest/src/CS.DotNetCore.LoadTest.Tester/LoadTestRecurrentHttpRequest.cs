namespace CS.DotNetCore.LoadTest.Tester
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class LoadTestRecurrentHttpRequest
    {
        private static int InstanceCount = 0;

        private readonly Func<int, int, string> _getBodyContent;

        private Task previousTask;

        public int InstanceNumber { get; private set; }

        public int RequestCount { get; private set; }

        public int? ExpectedResponseStatus { get; private set; }

        public DateTimeOffset TestStart { get; private set; }

        public DateTimeOffset TestEnd { get; private set; }

        public LoadTestHttpRequest HttpRequest { get; private set; }

        private LoadTestRecurrentHttpRequest
        (
            LoadTestHttpRequest httpRequest,
            DateTimeOffset testStart,
            DateTimeOffset testEnd,
            int? expectedStatus = null,
            Func<int, int, string> getBodyContent = null
        )
        {
            HttpRequest = httpRequest;
            ExpectedResponseStatus = expectedStatus;

            TestStart = testStart.ToUniversalTime();
            TestEnd = testEnd.ToUniversalTime();

            _getBodyContent = getBodyContent;
        }

        private LoadTestRecurrentHttpRequest() { }

        public void ResetRequestCount()
        {
            RequestCount = 0;
        }

        public static void ResetInstanceCount()
        {
            InstanceCount = 0;
        }

        private void WaitUntilTestStart()
        {
            while (DateTimeOffset.UtcNow.CompareTo(TestStart) < 0)
            {
                if (TestStart.Subtract(DateTimeOffset.UtcNow).TotalMilliseconds > 4)
                {
                    Thread.Sleep(3);
                }
                else
                {
                    Thread.Sleep(1);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestUri"></param>
        /// <param name="requestMethod"></param>
        /// <param name="testStart"></param>
        /// <param name="testEnd"></param>
        /// <param name="expectedStatus"></param>
        /// <param name="requestHeaders"></param>
        /// <param name="getBodyContent">Func<int(instanceNumber), int(requestCount)></param>
        /// <returns></returns>
        public static LoadTestRecurrentHttpRequest Create
        (
            LoadTestHttpRequest httpRequest,
            DateTimeOffset testStart,
            DateTimeOffset testEnd,
            int? expectedStatus = null,
            Func<int, int, string> getBodyContent = null
        )
        {

            if (httpRequest == null)
            {
                throw new ArgumentNullException(nameof(httpRequest));
            }

            var cmd = new LoadTestRecurrentHttpRequest
            (
                httpRequest,
                testStart,
                testEnd,
                expectedStatus,
                getBodyContent
            );

            InstanceCount++;
            cmd.InstanceNumber = InstanceCount;

            return cmd;
        }

        public async Task ExecuteAsync()
        {
            WaitUntilTestStart();

            while (DateTimeOffset.UtcNow.CompareTo(TestEnd) < 0)
            {
                RequestCount++;
                HttpRequest.RequestBody = _getBodyContent(InstanceNumber, RequestCount);

                var requestTask = HttpRequest.SendAsync();

                if (ExpectedResponseStatus.HasValue)
                {
                    var httpResponse = await requestTask.ConfigureAwait(false);

                    if (ExpectedResponseStatus.Value != httpResponse.StatusCode)
                    {
                        throw new Exception(string.Format("Unexpected HttpStatusCode (expected: {0} | actual: {1})",
                            ExpectedResponseStatus.ToString(), httpResponse.StatusCode.ToString()));
                    }
                }

                if (previousTask != null)
                {
                    if (previousTask.Status != TaskStatus.RanToCompletion)
                    {
                        await previousTask.ConfigureAwait(false);
                    }
                }

                previousTask = requestTask;
            }

            if (previousTask != null)
            {
                if (previousTask.Status != TaskStatus.RanToCompletion)
                {
                    await previousTask.ConfigureAwait(false);
                }
            }
        }
    }
}