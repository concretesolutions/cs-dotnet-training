namespace CS.DotNetCore.LoadTest.WebApp.Controllers
{
    using Data;
    using Logging;
    using Microsoft.AspNetCore.Mvc;
    using ServiceModel;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class TestController : Controller
    {
        private readonly IEventResultDAO _eventResultDAO;
        private readonly ITestResultAsyncDAO _testResultAsyncDAO;

        public TestController(
            IEventResultDAO eventResultDAO,
            ITestResultAsyncDAO testResultAsyncDAO)
        {
            _eventResultDAO = eventResultDAO;
            _testResultAsyncDAO = testResultAsyncDAO;
        }

        private static GetMetricsResponse MapToResponse(EventResultClusterMetrics metrics)
        {
            return new GetMetricsResponse()
            {
                ElapsedTimeAvg = metrics.ElapsedTimeAvg,
                ErrorCount = metrics.ErrorCount,
                SuccessCount = metrics.SuccessCount
            };
        }

        private static GetEventClusterResponse MapToResponse(EventResultCluster cluster)
        {
            return new GetEventClusterResponse()
            {
                ClusterDateTime = cluster.ClusterDateTime,
                Metrics = MapToResponse(cluster.Metrics)
            };
        }

        private static TestLanguageResponse MapToResponse(TestResult testResult)
        {
            return new TestLanguageResponse()
            {
                Metrics = MapToResponse(testResult.Metrics),
                TestLanguage = testResult.Test.Language,
                EventSuperCluster = testResult.SuperCluster.Select(c => MapToResponse(c)).ToList()
            };
        }

        private static GetTestResponse MapToResponse(List<TestResult> testResultList)
        {
            //counting test result metrics and clustering events
            var testResponse = new GetTestResponse()
            {
                TestId = testResultList.Select(e => e.Test.TestId).Distinct().SingleOrDefault(),
            };

            if (testResultList.Count < 1)
            {
                return testResponse;
            }

            testResponse.Languages = testResultList.Select(tr => MapToResponse(tr)).ToList();
            return testResponse;
        }

        [HttpGet]
        [Route("api/v1/test/{testId}")]
        public async Task<IActionResult> GetTestResultAsync(string testId)
        {
            //resolving test result
            var result = await _testResultAsyncDAO.SelectByTestIdAsync(testId).ConfigureAwait(false);
            return Json(MapToResponse(result));
        }

        [HttpPost]
        [Route("api/v1/test/all/event_result")]
        public async Task<IActionResult> PostTestResultsFromEventsAsync()
        {
            //select from memory
            var eventResultList = _eventResultDAO.SelectAll();
            var testResultList = TestResult.CompileTestResults(eventResultList);

            //save test result
            await _testResultAsyncDAO.InsertRangeAsync(testResultList).ConfigureAwait(false);
            return Ok();
        }

        [HttpDelete]
        [Route("api/v1/test/all")]
        public async Task<IActionResult> DeleteAllAsync()
        {
            await _testResultAsyncDAO.DeleteAllAsync().ConfigureAwait(false);
            return Ok();
        }
                
        public async Task<IActionResult> Index()
        {
            var result = await _testResultAsyncDAO.SelectTestIdAsync().ConfigureAwait(false);
            result.Sort();

            ViewData["TestIdCollection"] = result.ToArray();
            return View();
        }
    }
}
