namespace CS.DotNetCore.LoadTest.WebApp.Data.Mongodb
{
    using System;
    using System.Threading.Tasks;
    using Business;
    using MongoDB.Driver;
    using MongoDB.Bson;
    using Data.Schema;
    using Config;
    using Schema;

    internal class IdentityMongodbDAO : IIdentityAsyncDAO
    {
        private const string Schema = "db_loadtest";

        private static readonly InsertOneOptions InsertOneOptions = new InsertOneOptions()
        {
            BypassDocumentValidation = false
        };

        private IMongoClient _mongoClient;

        public IdentityMongodbDAO(IDatabaseConfig config)
        {
            var clientSettings = MongoClientSettings.FromUrl(new MongoUrl(config.String));
            _mongoClient = new MongoClient(clientSettings);
        }

        private BsonDocument MapToBsonDocument(Identity identity)
        {
            var doc = new BsonDocument();
            doc.Set(IdentityMongodbSchema.IdentityNameProp, BsonValue.Create(identity.IdentityName));

            doc.Set(IdentityMongodbSchema.SaltProp, BsonValue.Create(identity.Salt));
            doc.Set(IdentityMongodbSchema.PasswordProp, BsonValue.Create(identity.Password));

            return doc;
        }

        public async Task<int> DeleteAllAsync()
        {
            var result = await _mongoClient.GetDatabase(Schema)
                .GetCollection<BsonDocument>(IdentitySchema.Table)
                .DeleteManyAsync((b) => true)
                .ConfigureAwait(false);

            var affected = Convert.ToInt32(result.DeletedCount);

            result = null;
            return affected;
        }

        public Task InsertAsync(Identity identity)
        {
            return _mongoClient.GetDatabase(Schema)
                .GetCollection<BsonDocument>(IdentitySchema.Table)
                .InsertOneAsync(MapToBsonDocument(identity), InsertOneOptions);
        }
    }
}
