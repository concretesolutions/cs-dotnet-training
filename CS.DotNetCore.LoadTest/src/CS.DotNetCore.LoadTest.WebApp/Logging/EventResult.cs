namespace CS.DotNetCore.LoadTest.WebApp.Logging
{
    using Newtonsoft.Json;
    using System;

    public class EventResult
    {
        [JsonProperty]
        internal Event Event { get; private set; }

        [JsonProperty]
        internal Test Test { get; private set; }

        [JsonProperty]
        internal double ElapsedMiliSeconds { get; private set; }

        [JsonProperty]
        internal int StatusCode { get; private set; }

        [JsonProperty]
        internal DateTimeOffset EventStart { get; private set; }

        [JsonConstructor]
        private EventResult() { }

        internal EventResult(Event eventObj, DateTimeOffset eventStart, double elapsedMiliSeconds,
            int statusCode, Test test = null)
        {
            Event = eventObj;
            ElapsedMiliSeconds = elapsedMiliSeconds;
            StatusCode = statusCode;
            Test = test;
            EventStart = eventStart;
        }
    }
}
