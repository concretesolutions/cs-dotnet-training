namespace CS.DotNetCore.LoadTest.WebApp.Data.Mongodb.DataObject
{
    using Logging;
    using MongoDB.Bson.Serialization.Attributes;

    [BsonIgnoreExtraElements]
    internal class TestMongoObject
    {
        [BsonElement]
        internal string TestId { get; set; }

        [BsonElement]
        internal string Language { get; set; }

        internal TestMongoObject() { }

        internal TestMongoObject(Test test)
        {
            TestId = test.TestId;
            Language = test.Language;
        }

        internal Test CreateBusinessObject()
        {
            return new Test(TestId, Language);
        }
    }
}
