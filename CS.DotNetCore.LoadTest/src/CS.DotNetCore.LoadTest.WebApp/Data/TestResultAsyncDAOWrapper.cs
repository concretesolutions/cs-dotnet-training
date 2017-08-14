namespace CS.DotNetCore.LoadTest.WebApp.Data
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Logging;
    using Config;
    using Mongodb;
    using Pgsql;
    using Mysql;

    internal class TestResultAsyncDAOWrapper : ITestResultAsyncDAO
    {
        private ITestResultAsyncDAO _dao;

        public TestResultAsyncDAOWrapper(ILoadTestConfig config)
        {
            switch (config.DBMS)
            {
                case LoadTestDBMS.Mongodb:
                    _dao = new TestResultMongodbDAO(config.MongoConnection);
                    break;

                case LoadTestDBMS.Pgsql:
                    _dao = new TestResultPgsqlDAO(config.PgsqlConnection);
                    break;

                case LoadTestDBMS.Mysql:
                    _dao = new TestResultMysqlDAO(config.MysqlConnection);
                    break;

                default:
                    throw new NotImplementedException(config.DBMS.ToString() + "not implemented");
            }
        }

        public Task<int> DeleteAllAsync()
        {
            return _dao.DeleteAllAsync();
        }

        public Task InsertAsync(TestResult testResult)
        {
            return _dao.InsertAsync(testResult);
        }

        public Task InsertRangeAsync(IEnumerable<TestResult> testResultColl)
        {
            return _dao.InsertRangeAsync(testResultColl);
        }

        public Task<List<TestResult>> SelectByTestIdAsync(string testId)
        {
            return _dao.SelectByTestIdAsync(testId);
        }

        public Task<List<string>> SelectTestIdAsync()
        {
            return _dao.SelectTestIdAsync();
        }
    }
}
