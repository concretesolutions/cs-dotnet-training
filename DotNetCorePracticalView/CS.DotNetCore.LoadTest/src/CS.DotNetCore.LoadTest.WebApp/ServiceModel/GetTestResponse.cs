namespace CS.DotNetCore.LoadTest.WebApp.ServiceModel
{
    using System.Collections.Generic;

    public class GetTestResponse
    {      
        public string TestId { get; set; }

        public List<TestLanguageResponse> Languages { get; set; }

        public GetTestResponse()
        {
            Languages = new List<TestLanguageResponse>();
        }
    }
}
