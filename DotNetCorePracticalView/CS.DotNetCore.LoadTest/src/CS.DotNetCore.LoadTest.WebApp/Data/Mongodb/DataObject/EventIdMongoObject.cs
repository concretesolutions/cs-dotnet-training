namespace CS.DotNetCore.LoadTest.WebApp.Data.Mongodb.DataObject
{
    using Microsoft.Extensions.Logging;
    using MongoDB.Bson.Serialization.Attributes;

    [BsonIgnoreExtraElements]
    internal class EventIdMongoObject
    {
        [BsonElement("id")]
        public int _Id { get; set; }

        [BsonElement]
        public string Name { get; set; }

        internal EventId CreateBusinessObject()
        {
            return new EventId(_Id, Name);
        }
    }
}
