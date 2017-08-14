namespace CS.DotNetCore.LoadTest.Tester
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Linq;
    using System.Threading;

    public class Program
    {
        private const string TestIdHeaderName = "X-LoadTest-Id";
        private const string TestLanguageHeaderName = "X-LoadTest-Lg";

        private static string GetBodyContent(int instanceNumber, int requestCount)
        {
            return string.Concat("{ \"identityName\":\"user_", instanceNumber.ToString(), "_", requestCount.ToString(), "\", \"password\":\"pass\" }");
        }

        private static string GetArgValue(string[] args, string argPrefix, bool isRequired = false, string requiredMessage = null)
        {
            var argValue = args.Where(v => v.StartsWith(argPrefix)).FirstOrDefault();

            if (argValue != null)
            {
                argValue = argValue.Trim();

                if (argValue.Length > argPrefix.Length)
                {
                    argValue = argValue.Substring(argPrefix.Length).Trim();
                }
                else
                {
                    argValue = string.Empty;
                }
            }

            if (isRequired && string.IsNullOrWhiteSpace(argValue))
            {
                Console.Error.WriteLine(requiredMessage);
                Environment.Exit(1);
            }

            return argValue;
        }

        private static void ExecuteRecurrentRequests
        (
            Uri requestUri,
            HttpMethod requestMethod,
            DateTimeOffset testStart,
            DateTimeOffset testEnd,
            int threadCount,
            IEnumerable<KeyValuePair<string, string>> requestHeaders = null,
            int? expectedStatus = null
        )
        {
            var commandColl = new LoadTestRecurrentHttpRequest[threadCount];
            var taskColl = new Task[threadCount];

            try
            {
                for (var i = 0; i < threadCount; i++)
                {
                    var httpRequest = new LoadTestHttpRequest()
                    {
                        RequestBody = null,
                        RequestMethod = requestMethod,
                        RequestUri = requestUri,
                    };

                    httpRequest.SetRequestHeaders(requestHeaders);

                    commandColl[i] = LoadTestRecurrentHttpRequest.Create
                    (
                        httpRequest,
                        testStart,
                        testEnd,
                        expectedStatus,
                        GetBodyContent
                    );

                    taskColl[i] = commandColl[i].ExecuteAsync();
                }

                Task.WaitAll(taskColl);
                LoadTestRecurrentHttpRequest.ResetInstanceCount();
            }
            finally
            {
                foreach (var command in commandColl)
                {
                    command.HttpRequest.Dispose();
                }
            }
        }

        private static void ExecuteTestSection
        (
            string sectionId,
            string testId,
            int threadCount,
            int sectionSeconds,
            int socketTimeout,
            Uri postIdentityUri,
            Dictionary<string, string> requestHeaders,
            LoadTestHttpRequest compileTest,
            LoadTestHttpRequest deleteEventResult,
            LoadTestHttpRequest deleteIdentity
        )
        {
            for (int i = 1; i < 4; i++)
            {
                //executing recurrent requests
                var testStart = DateTimeOffset.UtcNow.AddMilliseconds(1000);
                var testEnd = testStart.AddSeconds(sectionSeconds);

                Console.WriteLine(string.Concat("Executing ", sectionId, " - ", i.ToString(), "..."));

                requestHeaders[TestIdHeaderName] = string.Concat(testId, "-", sectionId, "-", i.ToString());
                ExecuteRecurrentRequests(postIdentityUri, HttpMethod.Post, testStart, testEnd, threadCount, requestHeaders);

                //compiling test result
                Console.WriteLine(string.Concat("Compiling Test Result ", sectionId, " - ", i.ToString(), "..."));
                var httpResponse = compileTest.SendAsync().Result;

                if (httpResponse.StatusCode != 200)
                {
                    throw new Exception("Test Result Compilation Fails");
                }

                //removing event result
                Console.WriteLine(string.Concat("Deleting Event Result - ", sectionId, " - ", i.ToString(), "..."));
                httpResponse = deleteEventResult.SendAsync().Result;

                if (httpResponse.StatusCode != 200)
                {
                    throw new Exception("Deleting Event Result Fails.");
                }

                //removing identity
                Console.WriteLine(string.Concat("Deleting Identity ", sectionId, " - ", i.ToString(), "..."));
                httpResponse = deleteIdentity.SendAsync().Result;

                if (httpResponse.StatusCode != 200)
                {
                    throw new Exception("Deleting Identity Fails.");
                }

                Console.WriteLine("Waiting O.S Close TCP Ports...");
                Thread.Sleep(socketTimeout * 1000);
            }
        }

        //TODO: get test actions from appsettings .json
        public static void Main(string[] args)
        {
            //resolving app arguments
            var host = GetArgValue(args, "-h=", true, "-h arg must be provided | usage: -h={api host}");
            var port = GetArgValue(args, "-p=", true, "-p arg must be defined | usage: -p={api port}");

            var threadCount = Convert.ToInt32(GetArgValue(args, "-tc=", true, "-tc arg must be defined | usage: -tc={thread count}"));
            var sectionSeconds = Convert.ToInt32(GetArgValue(args, "-ss=", true, "-ss arg must be defined | usage: -ss={section time in seconds}"));

            var language = GetArgValue(args, "-lg=", true, "-lg arg must be defined | usage: -lg={api language}");
            var testId = GetArgValue(args, "-id=", true, "-id arg must be defined | usage: -id={testId}");

            var socketTimeout = Convert.ToInt32(GetArgValue(args, "-ct=", true, "-ct arg mus be defined | usage: -ct={connection timeout: time to wait O.S to close connections}"));

            //creating common request objects;
            var postIdentityDbUri = new Uri(string.Format("http://{0}:{1}/api/v1/identity/db", host, port));
            var postIdentityMeUri = new Uri(string.Format("http://{0}:{1}/api/v1/identity/memory", host, port));

            var databaseSectionId = "DB";
            var memorySectionId = "ME";

            var headers = new Dictionary<string, string>();
            headers.Add(TestIdHeaderName, string.Empty);
            headers.Add(TestLanguageHeaderName, language);

            var compileTestRequest = new LoadTestHttpRequest()
            {
                RequestMethod = HttpMethod.Post,
                RequestUri = new Uri(string.Format("http://{0}:{1}/api/v1/test/all/event_result", host, port))
            };

            var deleteEventResultRequest = new LoadTestHttpRequest()
            {
                RequestMethod = HttpMethod.Delete,
                RequestUri = new Uri(string.Format("http://{0}:{1}/api/v1/event_result/all", host, port))
            };

            var deleteIdentityDbRequest = new LoadTestHttpRequest()
            {
                RequestMethod = HttpMethod.Delete,
                RequestUri = new Uri(string.Format("http://{0}:{1}/api/v1/identity/all/db", host, port))
            };

            var deleteIdentityMeRequest = new LoadTestHttpRequest()
            {
                RequestMethod = HttpMethod.Delete,
                RequestUri = new Uri(string.Format("http://{0}:{1}/api/v1/identity/all/memory", host, port))
            };

            //executing database test section
            ExecuteTestSection
            (
                databaseSectionId,
                testId,
                threadCount,
                sectionSeconds,
                socketTimeout,
                postIdentityDbUri,
                headers,
                compileTestRequest,
                deleteEventResultRequest,
                deleteIdentityDbRequest
            );

            ExecuteTestSection
            (
                memorySectionId,
                testId,
                threadCount,
                sectionSeconds,
                socketTimeout,
                postIdentityMeUri,
                headers,
                compileTestRequest,
                deleteEventResultRequest,
                deleteIdentityMeRequest
            );
        }
    }
}