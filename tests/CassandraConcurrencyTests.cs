using System;
using System.Linq;
using System.Threading.Tasks;
using Cassandra;
using NUnit.Framework;
using Shouldly;

namespace WebApplication1.Tests
{
    // docker network create some-net
    // docker run --network some-net --name=cassandra1 -d -p 9042:9042 -p 9160:9160 cassandra
    // docker run --network some-net --name=cassandra2 -d -e CASSANDRA_SEEDS="cassandra1" cassandra
    // docker exec -it cassandra1 nodetool status
    // See https://issues.apache.org/jira/browse/CASSANDRA-9328
    public class CassandraConcurrencyTests
    {
        private Cluster cluster;
        private ISession session;

        [SetUp]
        public async Task Setup()
        {
            cluster = Cluster.Builder()
                .AddContactPoint("localhost")
                .WithExecutionProfiles(opts => opts
                    .WithProfile("default", profile => profile
                        .WithConsistencyLevel(ConsistencyLevel.Quorum)
                        .WithLoadBalancingPolicy(new DefaultLoadBalancingPolicy("datacenter1"))))
                .Build();

            for (int i = 0;; i++)
                try
                {
                    session = await cluster.ConnectAsync();
                    break;
                }
                catch (NoHostAvailableException)when (i < 100)
                {
                    await Task.Delay(1000);
                }

            await session.ExecuteAsync(new SimpleStatement(
                "CREATE KEYSPACE IF NOT EXISTS LWT_TEST with replication= { 'class' : 'SimpleStrategy', 'replication_factor' : 3 }"));
            await session.ExecuteAsync(new SimpleStatement(
                "USE LWT_TEST;"));
            await session.ExecuteAsync(new SimpleStatement(
                "CREATE TABLE IF NOT EXISTS test(key text PRIMARY KEY, version int);"));
            await session.ExecuteAsync(new SimpleStatement(
                "TRUNCATE test;"));
        }

        [TearDown]
        public void TearDown()
        {
            session?.Dispose();
            cluster?.Dispose();
        }

        [Test]
        public async Task ShouldThrowWriteTimeoutException()
        {
            for (int i = 0;; i++)
                try
                {
                    await session.ExecuteAsync(new SimpleStatement(
                        "INSERT INTO test(key, version) VALUES (:key, :version)",
                        "test", 0));
                    break;
                }
                catch (UnavailableException)when (i < 100)
                {
                    await Task.Delay(1000);
                }

            var selectStatement = await session.PrepareAsync("SELECT * FROM test WHERE key=:key");
            var updateStatement = await session.PrepareAsync(
                "UPDATE test SET version=:new_version WHERE key=:key IF version=:current_version");

            async Task Test()
            {
                for (;;)
                {
                    var rows = await session.ExecuteAsync(selectStatement.Bind("test"));
                    var version = rows.First().GetValue<int>("version");

                    var changeResult = await session.ExecuteAsync(updateStatement
                        .Bind(version + 1, "test", version));

                    if (changeResult.First().GetValue<bool>(0))
                        break;
                }
            }


            async Task Repeat(Func<Task> t)
            {
                for (int i = 0; i < 1000; i++)
                    await t();
            }

            await Repeat(() => Task.WhenAll(Enumerable.Range(0, 10).Select(_ => Test())))
                .ShouldThrowAsync<WriteTimeoutException>();
        }
    }
}