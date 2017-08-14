namespace CS.DotNetCore.LoadTest.WebApp.Data.Mysql
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Logging;
    using Config;
    using Schema;
    using MySql.Data.MySqlClient;
    using Util.Data;
    using System.Data;
    using Newtonsoft.Json;
    using System.Linq;
    using System.Text;

    internal class TestResultMysqlDAO : ITestResultAsyncDAO
    {
        private static readonly string DeleteAllSqlCommand = string.Format
            ("delete from `{0}`;", TestResultSchema.Table);

        private static readonly string InsertSqlCommand = string.Format
            ("insert into `{0}` (`{1}`,`{2}`,`{3}`) values (@{1}, @{2}, @{3});",
            TestResultSchema.Table, TestResultSchema.TestIdColumn,
            TestResultSchema.LanguageColumn, TestResultSchema.DataColumn);

        private static readonly string SelectByTestIdSqlCommand = string
            .Format("select `{0}` from `{1}` where `{2}` = @{2};",
            TestResultSchema.DataColumn, TestResultSchema.Table, TestResultSchema.TestIdColumn);

        private static readonly string SelectTestIdSqlCommand = string
            .Format("select `{0}` from `{1}` group by (`{0}`);",
            TestResultSchema.TestIdColumn, TestResultSchema.Table);

        private IDatabaseConfig _config;

        public TestResultMysqlDAO(IDatabaseConfig config)
        {
            _config = config;
        }

        private static MySqlParameter CreateTestIdParameter(string testId)
        {
            return new MySqlParameter()
            {
                Direction = ParameterDirection.Input,
                IsNullable = false,
                MySqlDbType = MySqlDbType.VarChar,
                ParameterName = TestResultSchema.TestIdColumn,
                Value = testId
            };
        }

        private static MySqlParameter CreateLanguageParameter(string language)
        {
            return new MySqlParameter()
            {
                Direction = ParameterDirection.Input,
                IsNullable = true,
                MySqlDbType = MySqlDbType.VarChar,
                ParameterName = TestResultSchema.LanguageColumn,
                Value = language
            };
        }

        private static MySqlParameter CreateTestResultParameter(TestResult testResult)
        {
            return new MySqlParameter()
            {
                Direction = ParameterDirection.Input,
                IsNullable = false,
                MySqlDbType = MySqlDbType.JSON,
                ParameterName = TestResultSchema.DataColumn,
                Value = JsonConvert.SerializeObject(testResult)
            };
        }

        private static Tuple<string, List<MySqlParameter>> CreateInsertRangeCommand(List<TestResult> testResultList)
        {
            var paramIndex = 0;
            var insertRangeSql = new StringBuilder();
            var parameters = new List<MySqlParameter>();

            insertRangeSql.AppendFormat("insert into `{0}` (`{1}`, `{2}`, `{3}`) values",
                TestResultSchema.Table, TestResultSchema.TestIdColumn,
                TestResultSchema.LanguageColumn, TestResultSchema.DataColumn);

            foreach (var testResult in testResultList)
            {
                if (paramIndex > 0)
                {
                    insertRangeSql.Append(", ");
                }

                insertRangeSql.AppendFormat("(@p{0}, @p{1}, @p{2})",
                    paramIndex.ToString(), (paramIndex + 1).ToString(), (paramIndex + 2).ToString());

                var mysqlParam = CreateTestIdParameter(testResult.Test.TestId);
                mysqlParam.ParameterName = "p" + paramIndex.ToString();
                parameters.Add(mysqlParam);

                mysqlParam = CreateLanguageParameter(testResult.Test.Language);
                mysqlParam.ParameterName = "p" + (paramIndex + 1).ToString();
                parameters.Add(mysqlParam);

                mysqlParam = CreateTestResultParameter(testResult);
                mysqlParam.ParameterName = "p" + (paramIndex + 2).ToString();
                parameters.Add(mysqlParam);

                paramIndex += 3;
            }

            return new Tuple<string, List<MySqlParameter>>(insertRangeSql.ToString(), parameters);
        }

        public async Task<int> DeleteAllAsync()
        {
            using (var conn = new MySqlConnection(_config.String))
            {
                await conn.OpenAsync().ConfigureAwait(false);

                return await DataHelper.ExecuteNonQueryAsync(conn, DeleteAllSqlCommand)
                    .ConfigureAwait(false);
            }
        }

        public async Task InsertAsync(TestResult testResult)
        {
            if (testResult == null)
                throw new ArgumentNullException(nameof(testResult));

            var parameters = new MySqlParameter[3]
            {
                CreateTestIdParameter(testResult.Test.TestId),
                CreateLanguageParameter(testResult.Test.Language),
                CreateTestResultParameter(testResult)
            };

            using (var conn = new MySqlConnection(_config.String))
            {
                await conn.OpenAsync().ConfigureAwait(false);

                await DataHelper.ExecuteNonQueryAsync(conn, InsertSqlCommand, parameters)
                    .ConfigureAwait(false);
            }
        }

        public async Task InsertRangeAsync(IEnumerable<TestResult> testResultColl)
        {
            if (testResultColl == null || !testResultColl.Any())
            {
                return;
            }

            var testResultList = testResultColl.ToList();
            var insertRangeCommand = CreateInsertRangeCommand(testResultList);

            using (var conn = new MySqlConnection(_config.String))
            {
                await conn.OpenAsync().ConfigureAwait(false);

                await DataHelper.ExecuteNonQueryAsync(conn, insertRangeCommand.Item1, insertRangeCommand.Item2.ToArray())
                    .ConfigureAwait(false);
            }
        }

        public async Task<List<TestResult>> SelectByTestIdAsync(string testId)
        {
            if (string.IsNullOrWhiteSpace(testId))
            {
                return new List<TestResult>();
            }

            var parameters = new MySqlParameter[1]
            {
                CreateTestIdParameter(testId)
            };

            using (var conn = new MySqlConnection(_config.String))
            {
                await conn.OpenAsync().ConfigureAwait(false);

                return await DataHelper.ExecuteReaderAsync<TestResult>(conn, SelectByTestIdSqlCommand, parameters)
                    .ConfigureAwait(false);
            }
        }

        public async Task<List<string>> SelectTestIdAsync()
        {
            using (var conn = new MySqlConnection(_config.String))
            {
                await conn.OpenAsync().ConfigureAwait(false);

                return (await DataHelper.ExecuteReaderAsync(conn, SelectTestIdSqlCommand).ConfigureAwait(false))
                    .Select(id => id.ToString())
                    .ToList();
            }
        }
    }
}
