using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using NUnit.Framework;
using Shouldly;

namespace WebApplication1.Tests
{
    // docker run --rm -p 27017:27017 -e MONGO_INITDB_ROOT_USERNAME=mongoadmin -e MONGO_INITDB_ROOT_PASSWORD=_Test123 -d mongo
    public class MongoDbSimpleTests
    {
        private CancellationTokenSource cancel;
        private IMongoCollection<TestLogEvent> logEvents;
        private IMongoClient mongoClient;


        [SetUp]
        public async Task Setup()
        {
            mongoClient = new MongoClient("mongodb://mongoadmin:_Test123@localhost");
            cancel = new CancellationTokenSource(10000);
            logEvents = mongoClient.GetDatabase("test").GetCollection<TestLogEvent>("testLogEvent1");

            await logEvents.DeleteManyAsync(l => true, cancel.Token);
        }

        [TearDown]
        public async Task TearDown()
        {
            await logEvents.DeleteManyAsync(l => true, cancel.Token);
            cancel.Dispose();
        }

        private async Task InsertTestItems()
        {
            await logEvents.InsertManyAsync(
                new[]
                {
                    new TestLogEvent
                    {
                        Level = "ERROR",
                        Message = "some message",
                        TimeStamp = DateTime.Now
                    },
                    new TestLogEvent
                    {
                        Level = "INFO",
                        Message = "another message",
                        TimeStamp = DateTime.Now
                    },
                    new TestLogEvent
                    {
                        Level = "ERROR",
                        Message = "an old message",
                        TimeStamp = DateTime.Now.AddYears(-1)
                    }
                }, cancellationToken: cancel.Token);
        }

        [Test]
        public async Task ShouldAggregate()
        {
            await InsertTestItems();
            var res11 = await logEvents
                .Aggregate()
                .Group(l => new
                    {
                        l.TimeStamp.Year
                    },
                    group => new
                    {
                        group.Key, Count = group.Count()
                    })
                .ToListAsync(cancel.Token);

            res11.Count.ShouldBe(2);
        }

        [Test]
        public async Task ShouldMapReduce()
        {
            await InsertTestItems();

            var cursor = await logEvents.MapReduceAsync(
                @"function mapYear() {
                    if (this.Level === 'ERROR')
                        emit(this.TimeStamp.getFullYear(), { total: 1});
                };",
                @"function reduceYear(year, values) {
                    let sum = 0;
                    values.forEach(v => {
                        sum += v.total;
                    });
                    return {total: NumberInt(sum)};
                };",
                new MapReduceOptions<TestLogEvent, ReducedResult>
                {
                    MaxTime = TimeSpan.FromMinutes(1)
                }, cancel.Token);

            var result = await cursor.ToListAsync(cancel.Token);

            result.Count.ShouldBe(2);
        }

        private class ReducedResult
        {
            public int Id { get; set; }
            [BsonElement("value")] public ReducedValue Value { get; set; }
        }

        private class ReducedValue
        {
            [BsonElement("total")] public int Total { get; set; }
        }
    }
}