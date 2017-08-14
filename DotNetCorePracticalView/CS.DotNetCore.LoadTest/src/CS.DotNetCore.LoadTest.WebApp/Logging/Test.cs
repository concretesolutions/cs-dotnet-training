namespace CS.DotNetCore.LoadTest.WebApp.Logging
{
    using Newtonsoft.Json;

    public class Test
    {
        [JsonProperty]
        internal string TestId { get; private set; }

        [JsonProperty]
        internal string Language { get; private set; }

        [JsonConstructor]
        private Test() { }

        internal Test(string testId, string language)
        {
            TestId = testId;
            Language = language;
        }
    }
}
