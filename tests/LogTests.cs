using System;
using System.Threading;
using System.Threading.Tasks;
using Nest;
using NUnit.Framework;
using Shouldly;

namespace WebApplication1.Tests
{
    public class LogTests
    {
        private CancellationTokenSource cancel;
        private ElasticClient elasticClient;

        [SetUp]
        public void Setup()
        {
            var connectionSettings = new ConnectionSettings(new Uri("http://localhost:9200"));

            elasticClient = new ElasticClient(connectionSettings);
            cancel = new CancellationTokenSource(1000);

            Program.RunCancellation = cancel.Token;
        }

        [TearDown]
        public void TearDown()
        {
            cancel.Dispose();
        }

        [Test]
        public async Task ShouldWriteLogs()
        {
            await Program.Main();

            await Task.Delay(5_000);

            var search = await elasticClient.SearchAsync<LogRecord>(s => s
                .Index($"my-service.local-dev-{DateTime.Today:yyyy.MM.dd}")
                .From(0)
                .Size(10)
                .Query(q => q
                    .DateRange(d => d.GreaterThan(DateTime.Today)
                        .LessThan(DateTime.Today.AddDays(1))))
                .QueryOnQueryString("application"));

            search.Documents.ShouldNotBeEmpty();

            foreach (var document in search.Documents)
                await TestContext.Out.WriteLineAsync("Found log record:" + document.Message);
        }
    }

    public class LogRecord
    {
        public DateTime TimeStamp { get; set; }
        public string Message { get; set; }
    }
}