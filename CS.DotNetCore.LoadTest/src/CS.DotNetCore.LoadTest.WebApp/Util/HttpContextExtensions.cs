namespace CS.DotNetCore.LoadTest.WebApp.Util
{
    using Logging;
    using Microsoft.AspNetCore.Http;

    internal static class HttpContextExtensions
    {
        private const string EventKeyName = "Event";
        private const string TestIdHeader = "X-LoadTest-Id";
        private const string TestLanguageHeader = "X-LoadTest-Lg";

        internal static void AddEvent(this HttpContext context, Event eventId)
        {
            context.Items.Add(EventKeyName, eventId);
        }

        internal static Event GetEvent(this HttpContext context)
        {
            return (Event)context.Items[EventKeyName];
        }

        internal static string GetTestId(this HttpRequest request)
        {
            if (request.Headers.ContainsKey(TestIdHeader))
            {
                return request.Headers[TestIdHeader].ToString();
            }

            return null;
        }

        internal static string GetTestLanguage(this HttpRequest request)
        {
            if (request.Headers.ContainsKey(TestLanguageHeader))
            {
                return request.Headers[TestLanguageHeader].ToString();
            }

            return null;
        }

        internal static Test GetTest(this HttpRequest request)
        {
            string testId = GetTestId(request);
            string testLanguage = GetTestLanguage(request);

            return testId == null && testLanguage == null ? null : new Test(testId, testLanguage);
        }
    }
}
