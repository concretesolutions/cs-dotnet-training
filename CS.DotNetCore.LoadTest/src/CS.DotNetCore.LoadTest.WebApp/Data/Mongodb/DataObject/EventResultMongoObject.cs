namespace CS.DotNetCore.LoadTest.WebApp.Data.Mongodb
{
    using DataObject;
    using Logging;
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;
    using System;

    [BsonIgnoreExtraElements]
    internal class EventResultMongoObject
    {
        [BsonElement]
        public EventMongoObject Event { get; set; }

        [BsonElement]
        public TestMongoObject Test { get; set; }

        [BsonElement]
        public double ElapsedMiliSeconds { get; set; }

        [BsonElement]
        public int StatusCode { get; set; }

        [BsonElement]
        public DateTime EventStart { get; set; }

        internal EventResult CreateBusinessObject()
        {
            return new EventResult
            (
                Event.CreateBusinessObject(),
                new DateTimeOffset(EventStart),
                ElapsedMiliSeconds,
                StatusCode,
                Test.CreateBusinessObject()
            );
        }
    }
}
