namespace CS.DotNetCore.LoadTest.WebApp.ServiceModel
{
    public class GetMetricsResponse
    {
        public int SuccessCount { get; set; }

        public int ErrorCount { get; set; }

        public double ElapsedTimeAvg { get; set; }
    }
}
