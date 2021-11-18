using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cassandra;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;
using WebApplication1.Awaitable;
using WebApplication1.Repositories;

namespace WebApplication1.Tests
{
    public class CassandraWeatherRepositoryTests
    {
        private ServiceProvider serviceProvider;

        [SetUp]
        public async Task Setup()
        {
            var services = new ServiceCollection();

            new Startup(new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "Cassandra:Host", "localhost" }
                })
                .Build()).ConfigureServices(services);
            services.AddTransient<CassandraWeatherRepository>();

            serviceProvider = services.BuildServiceProvider();

            var session = await serviceProvider.GetRequiredService<ITaskAwaitable<ISession>>();

            await session.ExecuteAsync(new SimpleStatement(
                "CREATE KEYSPACE IF NOT EXISTS app WITH REPLICATION = { 'class' : 'NetworkTopologyStrategy', 'datacenter1' : 1 };"));
            await session.ExecuteAsync(new SimpleStatement(
                "CREATE TABLE IF NOT EXISTS app.weather (id int, location text, forecastDate timestamp, temperatureC double, PRIMARY KEY (id, location));"));
            await session.ExecuteAsync(new SimpleStatement(
                "TRUNCATE app.weather;"));
        }

        [TearDown]
        public void TearDown()
        {
            serviceProvider?.Dispose();
        }

        [Test]
        public async Task ShouldReturnEmptyList()
        {
            var c = serviceProvider.GetRequiredService<CassandraWeatherRepository>();

            var res = await c.GetByLocation("", default);

            res.ShouldBeEmpty();
        }
    }
}