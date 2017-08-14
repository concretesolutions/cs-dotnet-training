namespace CS.DotNetCore.LoadTest.WebApp.Data.Mongodb
{
    using MongoDB.Bson;
    using MongoDB.Driver;
    using System.Linq;
    using System;
    using Config;
    using Schema;
    using DataObject;

    internal class DataMigrationMongodbDAO : BaseDataMigrationDAO, IDataMigrationDAO
    {
        private IMongoClient _mongoClient;
        private IDatabaseConfig _config;

        public override string LatestVersion { get { return "1.0.0"; } }

        internal DataMigrationMongodbDAO(IDatabaseConfig config)
        {
            var clientSettings = MongoClientSettings.FromUrl(new MongoUrl(config.AdminString));
            _mongoClient = new MongoClient(clientSettings);

            _config = config;
            OpenConnection(new Action(() => { GetCurrentVersion(); }));
        }

        public string GetCurrentVersion()
        {
            var cursor = _mongoClient.GetDatabase(_config.DataBase)
              .GetCollection<BsonDocument>(SchemaVersionMongodbSchema.Collection)
              .Find(Builders<BsonDocument>.Filter.Empty);

            var doc = cursor.FirstOrDefault();

            if (doc == null)
            {
                return null;
            }

            BsonValue version = null;
            var versionWasResolved = doc.TryGetValue(SchemaVersionMongodbSchema.VersionProp, out version);

            if (!versionWasResolved || version == null)
            {
                return null;
            }

            return version.AsString;
        }

        public void MigrateSchemaVersion(string fromVersion, string toVersion)
        {
            if (!ValidateMigrationOperation("Migrate Schema Version", fromVersion, toVersion))
            {
                return;
            }

            //save version
            var updateVersion = Builders<BsonDocument>.Update.Set(SchemaVersionMongodbSchema.VersionProp, LatestVersion);
            var updateVersionOpt = new FindOneAndUpdateOptions<BsonDocument>() { IsUpsert = true };

            _mongoClient.GetDatabase(_config.DataBase)
                .GetCollection<BsonDocument>(SchemaVersionMongodbSchema.Collection)
                .FindOneAndUpdate(Builders<BsonDocument>.Filter.Empty, updateVersion, updateVersionOpt);
        }

        public void MigrateIdentity(string fromVersion, string toVersion)
        {
            if (!ValidateMigrationOperation("Migrate Identity", fromVersion, toVersion))
            {
                return;
            }

            //creating collection
            var db = _mongoClient.GetDatabase(_config.DataBase);
            db.CreateCollection(IdentityMongodbSchema.Collection);

            //creating index
            var fieldDef = new StringFieldDefinition<BsonDocument>(IdentityMongodbSchema.IdentityNameProp);
            var indexDef = Builders<BsonDocument>.IndexKeys.Ascending(fieldDef);

            var indexOpt = new CreateIndexOptions<BsonDocument>()
            {
                Unique = true,
                Name = IdentityMongodbSchema.IdentityNameIndex
            };

            db.GetCollection<BsonDocument>(IdentityMongodbSchema.Collection)
                .Indexes
                .CreateOne(indexDef, indexOpt);
        }

        public void MigrateDbmsUsers(string fromVersion, string toVersion)
        {
            Console.WriteLine("Migrate DMS Users does no apply to mongo");
        }

        public void MigrateTestResult(string fromVersion, string toVersion)
        {
            if (!ValidateMigrationOperation("Migrate Test Result", fromVersion, toVersion))
            {
                return;
            }

            //creating collection
            var db = _mongoClient.GetDatabase(_config.DataBase);
            db.CreateCollection(TestResultMongodbSchema.Collection);

            //creating index
            var indexOpt = new CreateIndexOptions<TestResultMongoObject>()
            {
                Unique = true,
                Name = TestResultMongodbSchema.TestResultTestIndex
            };

            var indexDef = Builders<TestResultMongoObject>.IndexKeys.Ascending(d => d.Test.TestId);
            indexDef = indexDef.Ascending(d => d.Test.Language);

            db.GetCollection<TestResultMongoObject>(TestResultMongodbSchema.Collection)
                .Indexes
                .CreateOne(indexDef, indexOpt);
        }

        public void Dispose()
        {
            _mongoClient = null;
            _config = null;
        }
    }
}
