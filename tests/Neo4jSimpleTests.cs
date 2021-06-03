using System.Threading.Tasks;
using Neo4j.Driver;
using NUnit.Framework;
using Shouldly;

namespace WebApplication1.Tests
{
    public class Neo4jimpleTests
    {
        private IDriver driver;
        private IAsyncSession session;


        [SetUp]
        public async Task Setup()
        {
            driver = GraphDatabase.Driver("bolt://localhost:7687");
            session = driver.AsyncSession();

            await session.RunAsync("MATCH (m:Movie) DETACH DELETE m ");
            await session.RunAsync("MATCH (p:Person) DETACH DELETE p ");
            await session.RunAsync("MATCH (a:Greeting) DELETE a ");
        }

        [TearDown]
        public async Task TearDown()
        {
            await session.CloseAsync();
            driver.Dispose();
        }

        [Test]
        [TestCase("some message")]
        public async Task ShouldShowMessage(string message)
        {
            var query = await session.RunAsync(
                "CREATE (a:Greeting {message: $message}) " +
                "RETURN a.message + ', from node ' + id(a)",
                new {message});
            var result = await query.SingleAsync();
            result[0].As<string>().ShouldStartWith(message);
        }

        [Test]
        public async Task ShouldJoin()
        {
            await session.RunAsync(
                "CREATE (m1:Movie {title: 'Terminator 1'}) " +
                "CREATE (m2:Movie {title: 'Terminator 2'}) " +
                "CREATE (p:Person {first_name: 'Arnold'}) " +
                "CREATE (p)-[:acted_in]->(m1)" +
                "CREATE (p)-[:acted_in]->(m2)");

            var query = await session.RunAsync(
                "MATCH ((m:Movie)<-[:acted_in]-(a:Person)) " +
                "WHERE a.first_name='Arnold' " +
                "RETURN m.title " +
                "ORDER BY m.title");
            var result = await query.ToListAsync();

            result.Count.ShouldBe(2);
            result[0][0].As<string>().ShouldBe("Terminator 1");
            result[1][0].As<string>().ShouldBe("Terminator 2");
        }
    }


    public class Movie
    {
        public string Title { get; set; }
    }
}