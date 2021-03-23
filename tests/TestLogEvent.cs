using System;
using MongoDB.Bson;

namespace WebApplication1.Tests
{
    public class TestLogEvent
    {
        public ObjectId Id { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }

        public DateTime TimeStamp { get; set; }
    }
}