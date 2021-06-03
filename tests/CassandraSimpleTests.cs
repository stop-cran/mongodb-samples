using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cassandra;
using Cassandra.Mapping;
using MongoDB.Driver;
using NUnit.Framework;
using Shouldly;

namespace WebApplication1.Tests
{
    public class CassandraSimpleTests
    {
        private CancellationTokenSource cancel;
        private Cluster cluster;
        private ISession session;


        [SetUp]
        public async Task Setup()
        {
            cluster = Cluster.Builder().AddContactPoint("localhost").Build();
            cancel = new CancellationTokenSource(10000);

            session = await cluster.ConnectAsync();
            session.Execute("CREATE KEYSPACE IF NOT EXISTS uprofile WITH REPLICATION = { 'class' : 'NetworkTopologyStrategy', 'datacenter1' : 1 };");
            session.Execute("USE uprofile");
            session.Execute("CREATE TABLE IF NOT EXISTS uprofile.user (id int PRIMARY KEY, name text, city text)");
        }

        [TearDown]
        public void TearDown()
        {
            session.Dispose();
            cluster.Dispose();
        }

        private async Task InsertTestItems()
        {
            var mapper = new Mapper(session);

            await mapper.InsertAsync(new User {Id = 1, Name = "LyubovK", City = "Dubai"});
            await mapper.InsertAsync(new User {Id = 2, Name = "JiriK", City = "Toronto"});
            await mapper.InsertAsync(new User {Id = 3, Name = "IvanH", City = "Mumbai"});
        }

        [Test]
        public async Task ShouldAggregate()
        {
            await InsertTestItems();
            
            var mapper = new Mapper(session);
            
            var users = await mapper.FetchAsync<User>("Select * from user");
            var user = await mapper.FirstOrDefaultAsync<User>("Select * from user where id = ?", 3);

            users.Count().ShouldBe(3);
            user.ShouldNotBeNull();
        }
   }

    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
    }
}