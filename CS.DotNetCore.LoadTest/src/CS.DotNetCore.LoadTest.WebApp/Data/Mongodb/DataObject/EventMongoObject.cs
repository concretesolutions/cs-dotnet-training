namespace CS.DotNetCore.LoadTest.WebApp.Data.Mongodb.DataObject
{
    using Logging;
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    [BsonIgnoreExtraElements]
    internal class EventMongoObject
    {
        [BsonElement]
        public EventIdMongoObject EventId { get; set; }

        [BsonElement]
        public BsonDocument[] EventInputs { get; set; }

        internal Event CreateBusinessObject()
        {
            return new Event(EventId.CreateBusinessObject(), EventInputs);
        }
    }
}
