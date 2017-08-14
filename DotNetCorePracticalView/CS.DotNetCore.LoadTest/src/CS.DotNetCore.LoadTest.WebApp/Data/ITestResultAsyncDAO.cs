namespace CS.DotNetCore.LoadTest.WebApp.Data
{
    using Logging;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface ITestResultAsyncDAO
    {
        Task InsertAsync(TestResult testResult);

        Task InsertRangeAsync(IEnumerable<TestResult> testResultColl);

        Task<List<string>> SelectTestIdAsync();

        Task<List<TestResult>> SelectByTestIdAsync(string testId);

        Task<int> DeleteAllAsync();
    }
}
