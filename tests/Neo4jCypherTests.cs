using System;
using System.Linq;
using System.Threading.Tasks;
using Neo4j.Driver;
using Neo4jClient;
using Neo4jClient.Cypher;
using NUnit.Framework;
using Shouldly;

namespace WebApplication1.Tests
{
    // docker run --rm -d -p 7687:7687 -e NEO4J_AUTH=none neo4j
    public class Neo4jCypherTests
    {
        private IGraphClient client;

        [SetUp]
        public async Task Setup()
        {
            client = new GraphClient(new Uri("http://localhost:7474"));
            await client.ConnectAsync();

            await client.Cypher.Match("(m:Movie)").DetachDelete("m").ExecuteWithoutResultsAsync();
            await client.Cypher.Match("(m:Person)").DetachDelete("m").ExecuteWithoutResultsAsync();
            await client.Cypher.Match("(m:Greeting)").DetachDelete("m").ExecuteWithoutResultsAsync();
        }

        [TearDown]
        public void TearDown()
        {
            client.Dispose();
        }

        [Test]
        [TestCase("some message")]
        public async Task ShouldShowMessage(string message)
        {
            await client.Cypher
                .WithParams(new {message})
                .Create("(a:Greeting {message: $message})")
                .ExecuteWithoutResultsAsync();

            var result = await client.Cypher
                .Match("(a:Greeting)")
                .Return(a => new
                {
                    Greeting = a.As<Greeting>().message,
                    Node = Return.As<int>("id(a)")
                })
                .Limit(100)
                .ResultsAsync;

            var g = result.ShouldHaveSingleItem();
            g.Greeting.ShouldBe(message);
            g.Node.ShouldBeGreaterThanOrEqualTo(0);
        }
    }
    public class Greeting
    {
        public string message { get; set; }
    }
}