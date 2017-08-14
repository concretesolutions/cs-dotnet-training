namespace CS.DotNetCore.LoadTest.WebApp.Data.Mongodb.DataObject
{
    using Logging;
    using MongoDB.Bson.Serialization.Attributes;
    using System;

    [BsonIgnoreExtraElements]
    internal class EventClusterMongoObject
    {
        [BsonElement]
        internal EventClusterMetricsMongoObject Metrics { get; set; }

        [BsonElement]
        internal DateTime ClusterDateTime { get; set; }

        internal EventClusterMongoObject() { }

        internal EventClusterMongoObject(EventResultCluster eventCluster)
        {
            Metrics = new EventClusterMetricsMongoObject(eventCluster.Metrics);
            ClusterDateTime = eventCluster.ClusterDateTime.UtcDateTime;
        }

        internal EventResultCluster CreateBusinessObject()
        {
            return new EventResultCluster(Metrics.CreateBusinessObject(), new DateTimeOffset(ClusterDateTime));
        }
    }
}
