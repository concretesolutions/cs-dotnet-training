namespace CS.DotNetCore.LoadTest.WebApp.Data.Mongodb
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Logging;
    using System.Linq;
    using DataObject;
    using MongoDB.Driver;
    using Config;
    using Schema;

    internal class TestResultMongodbDAO : ITestResultAsyncDAO
    {
        private IDatabaseConfig _config;
        private IMongoClient _mongoClient;

        private static readonly InsertManyOptions InsertManyOptions = new InsertManyOptions()
        {
            BypassDocumentValidation = false,
            IsOrdered = false
        };

        private static readonly InsertOneOptions InsertOneOptions = new InsertOneOptions()
        {
            BypassDocumentValidation = false
        };

        private static readonly FilterDefinition<TestResultMongoObject> EmptyFilterDef =
            Builders<TestResultMongoObject>.Filter.Empty;

        public TestResultMongodbDAO(IDatabaseConfig config)
        {
            _config = config;

            var clientSettings = MongoClientSettings.FromUrl(new MongoUrl(config.String));
            _mongoClient = new MongoClient(clientSettings);
        }

        public Task InsertAsync(TestResult testResult)
        {
            if (testResult == null)
            {
                throw new ArgumentNullException(nameof(testResult));
            }

            return _mongoClient.GetDatabase(_config.DataBase)
                .GetCollection<TestResultMongoObject>(TestResultMongodbSchema.Collection)
                .InsertOneAsync(new TestResultMongoObject(testResult), InsertOneOptions);
        }

        public Task InsertRangeAsync(IEnumerable<TestResult> testResultColl)
        {
            if (testResultColl == null || !testResultColl.Any())
            {
                return Task.CompletedTask;
            }

            var documentColl = testResultColl.Select(t => new TestResultMongoObject(t));

            return _mongoClient.GetDatabase(_config.DataBase)
                .GetCollection<TestResultMongoObject>(TestResultMongodbSchema.Collection)
                .InsertManyAsync(documentColl, InsertManyOptions);
        }

        public async Task<List<TestResult>> SelectByTestIdAsync(string testId)
        {
            FilterDefinition<TestResultMongoObject> filterDef = null;

            if (testId == null)
            {
                var testNullDef = Builders<TestResultMongoObject>.Filter.Eq((tr => tr.Test), null);
                var testIdNullDef = Builders<TestResultMongoObject>.Filter.Eq((tr => tr.Test.TestId), null);

                filterDef = Builders<TestResultMongoObject>.Filter.Or(new FilterDefinition<TestResultMongoObject>[2]
                {
                    testNullDef,
                    testIdNullDef
                });
            }
            else
            {
                filterDef = Builders<TestResultMongoObject>.Filter.Eq((tr => tr.Test.TestId), testId);
            }

            var cursor = await _mongoClient.GetDatabase(_config.DataBase)
                .GetCollection<TestResultMongoObject>(TestResultMongodbSchema.Collection)
                .FindAsync(filterDef)
                .ConfigureAwait(false);

            List<TestResult> result = new List<TestResult>();

            using (cursor)
            {
                while (await cursor.MoveNextAsync().ConfigureAwait(false))
                {
                    result.AddRange(cursor.Current.Select(d => d.CreateBusinessObject()));
                }
            }

            return result;
        }

        public async Task<List<string>> SelectTestIdAsync()
        {
            var cursor = await _mongoClient.GetDatabase(_config.DataBase)
                .GetCollection<TestResultMongoObject>(TestResultMongodbSchema.Collection)
                .FindAsync(EmptyFilterDef)
                .ConfigureAwait(false);

            var result = new List<string>();

            using (cursor)
            {
                while (await cursor.MoveNextAsync().ConfigureAwait(false))
                {
                    result.AddRange(cursor.Current.Select(d => d.Test.TestId));
                }
            }

            result = result.Distinct().ToList();
            return result;
        }

        public async Task<int> DeleteAllAsync()
        {
            var result = await _mongoClient.GetDatabase(_config.DataBase)
                .GetCollection<TestResultMongoObject>(TestResultMongodbSchema.Collection)
                .DeleteManyAsync(EmptyFilterDef)
                .ConfigureAwait(false);

            var affected = Convert.ToInt32(result.DeletedCount);
            result = null;

            return affected;
        }
    }
}
