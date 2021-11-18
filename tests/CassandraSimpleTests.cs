using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cassandra;
using Cassandra.Mapping;
using Cassandra.Mapping.Attributes;
using NUnit.Framework;
using Shouldly;

namespace WebApplication1.Tests
{
    // docker run --rm -p 9042:9042 -p 9160:9160 -d cassandra
    public class CassandraSimpleTests
    {
        private Cluster cluster;
        private ISession session;

        [SetUp]
        public async Task Setup()
        {
            cluster = Cluster.Builder().AddContactPoint("localhost")
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
                "CREATE KEYSPACE IF NOT EXISTS test WITH REPLICATION = { 'class' : 'NetworkTopologyStrategy', 'datacenter1' : 1 };"));
            await session.ExecuteAsync(new SimpleStatement(
                "USE test;"));
            await session.ExecuteAsync(new SimpleStatement(
                "CREATE TABLE IF NOT EXISTS user (id int PRIMARY KEY, name text, city text, roles list<text>);"));
            await session.ExecuteAsync(new SimpleStatement(
                "TRUNCATE user;"));
        }

        [TearDown]
        public void TearDown()
        {
            session?.Dispose();
            cluster?.Dispose();
        }

        private async Task InsertTestItems()
        {
            var mapper = new Mapper(session);
            var batch = mapper.CreateBatch();

            batch.Insert(new User { Id = 1, Name = "Vasya", City = "Krasnoyarsk" });
            batch.Insert(new User { Id = 2, Name = "Petya", City = "Apatity" });
            batch.Insert(new User { Id = 3, Name = "Katya", City = "Perm" });

            await mapper.ExecuteAsync(batch);
        }

        [Test]
        public async Task ShouldSelectUsers()
        {
            await InsertTestItems();

            var mapper = new Mapper(session);

            var users = await mapper.FetchAsync<User>("Select * from user");

            users.Count().ShouldBe(3);
        }

        [Test]
        public async Task ShouldUpdateUsingTtl()
        {
            await InsertTestItems();

            var mapper = new Mapper(session);
            var user = await mapper.FirstOrDefaultAsync<User>("Select * from user where id = ?", 2);
            user.Name.ShouldBe("Petya");
            await mapper.UpdateAsync<User>("USING TTL 1 SET City=:city WHERE Id = :id;", "Murmansk", 2);
            user = await mapper.FirstOrDefaultAsync<User>("Select * from user where id = :id", 2);
            user.City.ShouldBe("Murmansk");

            await Task.Delay(1100);

            user = await mapper.FirstOrDefaultAsync<User>("Select * from user where id = :id", 2);
            user.Name.ShouldBe("Petya");
            user.City.ShouldBeNull();
        }

        [Test]
        public async Task ShouldUpdateUserByMapper()
        {
            await InsertTestItems();

            var mapper = new Mapper(session);
            var user = await mapper.FirstOrDefaultAsync<User>("Select * from user where id = :id", 2);
            user.Name.ShouldBe("Petya");
            user.City = "Murmansk";
            user.Roles.Add("admin");
            await mapper.UpdateAsync(user);
            user = await mapper.FirstOrDefaultAsync<User>("Select * from user where id = :id", 2);
            user.City.ShouldBe("Murmansk");
            user.Roles.ShouldHaveSingleItem();
        }

        [Test]
        public async Task ShouldDeleteColumn()
        {
            await InsertTestItems();

            await session.ExecuteAsync(new SimpleStatement("DELETE City FROM user WHERE id = :id", 2));

            var mapper = new Mapper(session);
            var user = await mapper.FirstOrDefaultAsync<User>("Select * from user where id = :id", 2);
            user.Name.ShouldBe("Petya");
            user.City.ShouldBeNull();
        }
    }

    public class User
    {
        [PartitionKey] public int Id { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public List<string> Roles { get; set; }
    }
}