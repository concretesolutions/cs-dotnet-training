namespace CS.DotNetCore.LoadTest.WebApp.ServiceModel
{
    using System;

    public class GetEventClusterResponse
    {
        public GetMetricsResponse Metrics { get; set; }

        public DateTimeOffset ClusterDateTime { get; set; }
    }
}
