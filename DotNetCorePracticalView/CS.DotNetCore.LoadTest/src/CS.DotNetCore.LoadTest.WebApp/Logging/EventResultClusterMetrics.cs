

namespace CS.DotNetCore.LoadTest.WebApp.Logging
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class EventResultClusterMetrics
    {
        [JsonProperty]
        internal int SuccessCount { get; private set; }

        [JsonProperty]
        internal int ErrorCount { get; private set; }

        [JsonProperty]
        internal double ElapsedTimeAvg { get; private set; }

        [JsonConstructor]
        private EventResultClusterMetrics() { }

        internal EventResultClusterMetrics(int successCount, int errorCount, double elapsedTimeAvg)
        {
            SuccessCount = successCount;
            ErrorCount = errorCount;
            ElapsedTimeAvg = elapsedTimeAvg;
        }
                
        internal EventResultClusterMetrics(IEnumerable<EventResult> eventResultColl)
        {
            SuccessCount = eventResultColl
                .Where(e => e.StatusCode == Event.GetExpectedStatusCode(e.Event.EventId))
                .Count();

            ErrorCount = eventResultColl
                .Where(e => e.StatusCode != Event.GetExpectedStatusCode(e.Event.EventId))
                .Count();

            ElapsedTimeAvg = Convert.ToDouble(eventResultColl.Sum(e => e.ElapsedMiliSeconds) / (SuccessCount + ErrorCount));
        }
    }
}
