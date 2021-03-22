using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using NUnit.Framework;
using Shouldly;
using WebApplication1;
using WebApplication1.Controllers;

namespace MongoDbSamples
{
    public class ControllerTests
    {
        private IHost host;
        private CancellationTokenSource cancel;


        [SetUp]
        public async Task Setup()
        {
            cancel = new CancellationTokenSource(10_000);
            host = Program.CreateHostBuilder()
                .ConfigureServices(services =>
                    services.AddTransient<WeatherController>()).Build();

            var weather = host.Services.GetRequiredService<IMongoCollection<WeatherDto>>();

            await weather.DeleteManyAsync(l => true, cancel.Token);
            await weather.InsertManyAsync(new[]
            {
                new WeatherDto
                {
                    Location = "Moscow",
                    ForecastDate = DateTime.Today.AddDays(1),
                    TemperatureC = -3
                },
                new WeatherDto
                {
                    Location = "Moscow",
                    ForecastDate = DateTime.Today.AddDays(2),
                    TemperatureC = -6
                }
            });
        }


        [TearDown]
        public async Task TearDown()
        {
            var weather = host.Services.GetRequiredService<IMongoCollection<WeatherDto>>();

            await weather.DeleteManyAsync(l => true, cancel.Token);
            host.Dispose();
            cancel.Dispose();
        }

        [Test]
        public async Task ShouldGetWeather()
        {
            var controller = host.Services.GetRequiredService<WeatherController>();

            var weather = await controller.Get("Moscow", cancel.Token);
            
            weather.ShouldNotBeEmpty();
        }
    }
}