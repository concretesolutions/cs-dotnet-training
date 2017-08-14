namespace CS.DotNetCore.LoadTest.WebApp.Logging
{
    using Newtonsoft.Json;
    using System;
    using System.Linq;
    using System.Collections.Generic;

    public class TestResult
    {
        [JsonProperty]
        internal EventResultClusterMetrics Metrics { get; private set; }

        [JsonProperty]
        internal Test Test { get; private set; }

        private List<EventResultCluster> _superCluster;

        [JsonProperty]
        internal IEnumerable<EventResultCluster> SuperCluster
        {
            get
            {
                return _superCluster;
            }

            private set
            {
                _superCluster = value.ToList();
            }
        }

        [JsonConstructor]
        private TestResult() { }

        internal TestResult(Test test, EventResultClusterMetrics metrics, IEnumerable<EventResultCluster> eventCluster)
        {
            Test = test;
            Metrics = metrics;
            SuperCluster = eventCluster == null ? new List<EventResultCluster>() : eventCluster;
        }

        private static Dictionary<DateTimeOffset, List<EventResult>> ClusterizeEvents(List<EventResult> eventResultColl)
        {
            var eventSuperCluster = new Dictionary<DateTimeOffset, List<EventResult>>();

            foreach (var eventResult in eventResultColl)
            {
                var clusterKey = new DateTimeOffset
                (
                    eventResult.EventStart.Year,
                    eventResult.EventStart.Month,
                    eventResult.EventStart.Day,
                    eventResult.EventStart.Hour,
                    eventResult.EventStart.Minute,
                    eventResult.EventStart.Second,
                    eventResult.EventStart.Offset
                );

                if (!eventSuperCluster.ContainsKey(clusterKey))
                {
                    eventSuperCluster.Add(clusterKey, new List<EventResult>());
                }

                eventSuperCluster[clusterKey].Add(eventResult);
            }

            return eventSuperCluster;
        }

        internal static List<TestResult> CompileTestResults(IEnumerable<EventResult> eventResultColl)
        {
            //resolving testid and languages from event results
            var testEvents = eventResultColl.Where(e => e.Test != null && e.Test.TestId != null);
            var testIdColl = testEvents.Select(e => e.Test.TestId).Distinct();

            var testLanguageColl = testEvents.Select(e => e.Test.Language).Distinct();
            var testResultColl = new List<TestResult>();

            foreach (var testId in testIdColl)
            {
                foreach (var language in testLanguageColl)
                {
                    //resolving events from a specific id and language
                    var testLanguageEvents = testEvents
                        .Where(e => e.Test.TestId == testId && e.Test.Language == language)
                        .ToList();

                    //calculating metrics from all events in the test
                    var testResult = new TestResult()
                    {
                        Test = new Test(testId, language),
                        Metrics = new EventResultClusterMetrics(testLanguageEvents),
                    };

                    //clusterizing events in the test
                    var eventSuperCluster = ClusterizeEvents(testLanguageEvents);

                    testResult._superCluster = eventSuperCluster.Select
                    (
                        sc => new EventResultCluster(new EventResultClusterMetrics(sc.Value), sc.Key)
                    )
                    .ToList();

                    testResult._superCluster.Sort((c1, c2) => { return c1.ClusterDateTime.CompareTo(c2.ClusterDateTime); });
                    testResultColl.Add(testResult);
                }
            }

            return testResultColl;
        }
    }
}
