namespace CS.DotNetCore.LoadTest.WebApp.Data.Pgsql
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Logging;
    using Config;
    using Schema;
    using Util.Data;
    using Npgsql;
    using System.Data;
    using NpgsqlTypes;
    using Newtonsoft.Json;
    using System.Linq;

    internal class TestResultPgsqlDAO : ITestResultAsyncDAO
    {
        private static readonly string DeleAllSqlCommand = string.Format("delete from \"{0}\";",
            TestResultSchema.Table);

        private static readonly string InsertSqlCommmand = string.Format
            ("insert into \"{0}\" (\"{1}\", \"{2}\", \"{3}\") values (:{1},:{2},:{3});",
            TestResultSchema.Table, TestResultSchema.TestIdColumn,
            TestResultSchema.LanguageColumn, TestResultSchema.DataColumn);

        private static readonly string InsertRangeSqlCommand = string.Format
            ("COPY \"{0}\" (\"{1}\", \"{2}\" ,\"{3}\") FROM STDIN(FORMAT BINARY)",
                TestResultSchema.Table, TestResultSchema.TestIdColumn,
                TestResultSchema.LanguageColumn, TestResultSchema.DataColumn);

        private static readonly string SelectByTestIdSqlCommand = string.Format
            ("select \"{0}\" from \"{1}\" where \"{2}\"=:{2};",
            TestResultSchema.DataColumn, TestResultSchema.Table, TestResultSchema.TestIdColumn);

        private static readonly string SelectTestIdSqlCommand = string.Format
            ("select \"{0}\" from \"{1}\" group by (\"{0}\");", TestResultSchema.TestIdColumn,
            TestResultSchema.Table);

        private IDatabaseConfig _config;

        public TestResultPgsqlDAO(IDatabaseConfig config)
        {
            _config = config;
        }

        private static NpgsqlParameter CreateTestIdParameter(string testId)
        {
            return new NpgsqlParameter()
            {
                Direction = ParameterDirection.Input,
                IsNullable = false,
                NpgsqlDbType = NpgsqlDbType.Varchar,
                ParameterName = TestResultSchema.TestIdColumn,
                NpgsqlValue = testId
            };
        }

        private static NpgsqlParameter CreateLanguageParameter(string language)
        {
            return new NpgsqlParameter()
            {
                Direction = ParameterDirection.Input,
                IsNullable = true,
                NpgsqlDbType = NpgsqlDbType.Varchar,
                NpgsqlValue = language,
                ParameterName = TestResultSchema.LanguageColumn,
            };
        }

        private static NpgsqlParameter[] CreateInsertParameter(TestResult testResult)
        {
            return new NpgsqlParameter[3]
            {
                CreateTestIdParameter(testResult.Test.TestId),
                CreateLanguageParameter(testResult.Test.Language),
                new NpgsqlParameter()
                {
                    Direction = ParameterDirection.Input,
                    IsNullable = false,
                    NpgsqlDbType = NpgsqlDbType.Json,
                    NpgsqlValue = JsonConvert.SerializeObject(testResult),
                    ParameterName = TestResultSchema.DataColumn
                }
            };
        }

        public async Task<int> DeleteAllAsync()
        {
            using (var conn = new NpgsqlConnection(_config.String))
            {
                await conn.OpenAsync().ConfigureAwait(false);
                var affected = await DataHelper.ExecuteNonQueryAsync(conn, DeleAllSqlCommand).ConfigureAwait(false);

                return affected;
            };
        }

        public async Task InsertAsync(TestResult testResult)
        {
            using (var conn = new NpgsqlConnection(_config.String))
            {
                await conn.OpenAsync().ConfigureAwait(false);

                await DataHelper.ExecuteNonQueryAsync
                (
                    conn,
                    InsertSqlCommmand,
                    CreateInsertParameter(testResult)
                )
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

            using (var conn = new NpgsqlConnection(_config.String))
            {
                await conn.OpenAsync().ConfigureAwait(false);

                using (var importer = conn.BeginBinaryImport(InsertRangeSqlCommand))
                {
                    foreach (var testResult in testResultList)
                    {
                        importer.WriteRow
                        (
                            new object[3]
                            {
                                testResult.Test.TestId,
                                testResult.Test.Language,
                                JsonConvert.SerializeObject(testResult)
                            }
                        );
                    }

                    testResultList = null;
                }
            }
        }

        public async Task<List<TestResult>> SelectByTestIdAsync(string testId)
        {
            if (string.IsNullOrWhiteSpace(testId))
            {
                return new List<TestResult>();
            }

            using (var conn = new NpgsqlConnection(_config.String))
            {
                await conn.OpenAsync().ConfigureAwait(false);

                var result = await DataHelper.ExecuteReaderAsync<TestResult>
                (
                    conn,
                    SelectByTestIdSqlCommand,
                    new NpgsqlParameter[1] { CreateTestIdParameter(testId) }
                )
                .ConfigureAwait(false);

                return result;
            }
        }

        public async Task<List<string>> SelectTestIdAsync()
        {
            using (var conn = new NpgsqlConnection(_config.String))
            {
                await conn.OpenAsync().ConfigureAwait(false);

                var result = await DataHelper.ExecuteReaderAsync
                (
                    conn,
                    SelectTestIdSqlCommand
                )
                .ConfigureAwait(false);

                return result.Select(id => id.ToString()).ToList();
            }
        }
    }
}
