namespace CS.DotNetCore.LoadTest.WebApp.Data.Mongodb.DataObject
{
    using Logging;
    using MongoDB.Bson.Serialization.Attributes;
    using System.Collections.Generic;
    using System.Linq;

    [BsonIgnoreExtraElements]
    internal class TestResultMongoObject
    {
        [BsonElement]
        internal TestMongoObject Test { get; set; }

        [BsonElement]
        internal EventClusterMetricsMongoObject Metrics { get; set; }

        [BsonElement]
        internal List<EventClusterMongoObject> SuperCluster { get; set; }

        internal TestResultMongoObject() { }

        internal TestResultMongoObject(TestResult testResult)
        {
            Test = new TestMongoObject(testResult.Test);
            Metrics = new EventClusterMetricsMongoObject(testResult.Metrics);
            SuperCluster = testResult.SuperCluster.Select(c => new EventClusterMongoObject(c)).ToList();
        }

        internal TestResult CreateBusinessObject()
        {
            return new TestResult
            (
                Test.CreateBusinessObject(),
                Metrics.CreateBusinessObject(),
                SuperCluster == null ? null : SuperCluster.Select(c => c.CreateBusinessObject())
            );
        }
    }
}
