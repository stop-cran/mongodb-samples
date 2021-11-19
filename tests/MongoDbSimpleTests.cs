using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
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
        public async Task ShouldCreateIndex()
        {
            var db = mongoClient.GetDatabase("test");
            var coll = db.GetCollection<IntestA>("IndexTest");
            var indexKeysDefinition = Builders<IntestA>.IndexKeys.Ascending(t => t.SomeB.SomeC.SomeD.SomeE.Kr89rhdeu);

            await coll.Indexes.CreateOneAsync(new CreateIndexModel<IntestA>(indexKeysDefinition));
            
            await coll.DeleteManyAsync(_ => true);

            await coll.InsertOneAsync(new IntestA
            {
                Name = "srghs5hw4",
                SomeB = new IntestB
                {
                    Enttuu = "i8erghefge",
                    Fds = DateTime.Parse("2011-05-15"),
                    SomeC = new IntestC
                    {
                        Kd89e87 = 13245,
                        Kfr8s7sy = "srthesrh",
                        SomeD = new IntestD
                        {
                            Fer8xs = "er9fsejfs",
                            Kf8sr = 234529,
                            SomeE = new IntestE
                            {
                                FmkDhs = DateTime.Parse("2020-11-28"),
                                Kr89rhdeu = "sersr5"
                            }
                        }
                    }
                }
            });

            await coll.InsertOneAsync(new IntestA
            {
                Name = "drt67gdr6yd",
                SomeB = new IntestB
                {
                    Enttuu = "rt6gydcr6yhdy6",
                    Fds = DateTime.Parse("2012-01-15"),
                    SomeC = new IntestC
                    {
                        Kd89e87 = 658784,
                        Kfr8s7sy = "cftghcfbth",
                        SomeD = new IntestD
                        {
                            Fer8xs = "gfyutf7uflo",
                            Kf8sr = 7895343,
                            SomeE = new IntestE
                            {
                                FmkDhs = DateTime.Parse("2025-11-28"),
                                Kr89rhdeu = "t8d6du"
                            }
                        }
                    }
                }
            });

            var res = await coll.FindAsync(a => a.SomeB.SomeC.SomeD.SomeE.Kr89rhdeu == "t8d6du");
            bool b = await res.MoveNextAsync();

            b.ShouldBeTrue();
            var item = res.Current.ShouldHaveSingleItem();
            item.SomeB.SomeC.SomeD.SomeE.FmkDhs.Year.ShouldBe(2025);
        }

        private class IntestA
        {
            public ObjectId Id { get; set; }
            public string Name { get; set; }
            public IntestB SomeB { get; set; }
        }

        private class IntestB
        {
            public string Enttuu { get; set; }
            public DateTime Fds { get; set; }
            public IntestC SomeC { get; set; }
        }

        private class IntestC
        {
            public string Kfr8s7sy { get; set; }
            public int Kd89e87 { get; set; }
            public IntestD SomeD { get; set; }
        }

        private class IntestD
        {
            public string Fer8xs { get; set; }
            public long? Kf8sr { get; set; }
            public IntestE SomeE { get; set; }
        }

        private class IntestE
        {
            public string Kr89rhdeu { get; set; }
            public DateTime FmkDhs { get; set; }
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