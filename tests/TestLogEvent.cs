using System;

namespace MongoDbSamples
{
    public class TestLogEvent
    {
        public MongoDB.Bson.ObjectId Id { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }

        public DateTime TimeStamp { get; set; }
    }
}