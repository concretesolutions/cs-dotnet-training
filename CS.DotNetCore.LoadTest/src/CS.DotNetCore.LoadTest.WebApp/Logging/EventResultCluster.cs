namespace CS.DotNetCore.LoadTest.WebApp.Logging
{
    using Newtonsoft.Json;
    using System;

    internal class EventResultCluster
    {
        [JsonProperty]
        public EventResultClusterMetrics Metrics { get; private set; }

        [JsonProperty]
        internal DateTimeOffset ClusterDateTime { get; private set; }

        [JsonConstructor]
        private EventResultCluster() { }

        internal EventResultCluster(EventResultClusterMetrics clusterMetrics, DateTimeOffset clusterDateTime)
        {
            Metrics = clusterMetrics;
            ClusterDateTime = clusterDateTime;
        }
    }
}
