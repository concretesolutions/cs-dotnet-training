namespace CS.DotNetCore.LoadTest.WebApp.Data.Mongodb.DataObject
{
    using Logging;
    using MongoDB.Bson.Serialization.Attributes;
    using System;

    [BsonIgnoreExtraElements]
    internal class EventClusterMetricsMongoObject
    {
        [BsonElement]
        internal int SuccessCount { get; set; }

        [BsonElement]
        internal int ErrorCount { get; set; }

        [BsonElement]
        internal double ElapsedTimeAvg { get; set; }

        internal EventClusterMetricsMongoObject() { }

        internal EventClusterMetricsMongoObject(EventResultClusterMetrics metrics)
        {
            SuccessCount = metrics.SuccessCount;
            ErrorCount = metrics.ErrorCount;
            ElapsedTimeAvg = metrics.ElapsedTimeAvg;
        }

        internal EventResultClusterMetrics CreateBusinessObject()
        {
            return new EventResultClusterMetrics(SuccessCount, ErrorCount, ElapsedTimeAvg);
        }
    }
}
