namespace CS.DotNetCore.LoadTest.WebApp.ServiceModel
{
    using System.Collections.Generic;

    public class TestLanguageResponse
    {
        public string TestLanguage { get; set; }

        public GetMetricsResponse Metrics { get; set; }

        public List<GetEventClusterResponse> EventSuperCluster { get; set; }

        public TestLanguageResponse()
        {
            EventSuperCluster = new List<GetEventClusterResponse>();
        }
    }
}
