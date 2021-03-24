using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using NUnit.Framework;
using Shouldly;
using WebApplication1.Controllers;

namespace WebApplication1.Tests
{
    public class UniqueResourceControllerTests
    {
        private CancellationTokenSource cancel;
        private IHost host;


        [SetUp]
        public void Setup()
        {
            cancel = new CancellationTokenSource(10_000);
            host = Program.CreateHostBuilder()
                .ConfigureServices(services =>
                    services.AddTransient<UniqueResourceController>()).Build();
        }


        [TearDown]
        public void TearDown()
        {
            host.Dispose();
            cancel.Dispose();
        }

        [Test]
        public async Task ShouldOwn()
        {
            using var cancel2 = new CancellationTokenSource(1000);
            var controller = host.Services.GetRequiredService<UniqueResourceController>();

            var result = await controller.Own(TimeSpan.FromMilliseconds(100), cancel2.Token);
            
            result.ShouldBeOfType<OkObjectResult>();
        }
        
        [Test]
        public async Task ShouldCancelLock()
        {
            using var cancel2 = new CancellationTokenSource(1000);
            var controller = host.Services.GetRequiredService<UniqueResourceController>();

            var task1 = controller.Own(TimeSpan.FromSeconds(10), cancel2.Token);
            var task2 = controller.Own(TimeSpan.FromMilliseconds(100), cancel2.Token);

            await task1.ShouldThrowAsync<TaskCanceledException>();
            await task2.ShouldThrowAsync<TaskCanceledException>();
        }
        
        [Test]
        public async Task ShouldLock()
        {
            using var cancel2 = new CancellationTokenSource(1000);
            var controller = host.Services.GetRequiredService<UniqueResourceController>();

            var task1 = controller.Own(TimeSpan.FromMilliseconds(100), cancel2.Token);
            var task2 = controller.Own(TimeSpan.FromSeconds(10), cancel2.Token);
            var result1 = await task1;
            
            result1.ShouldBeOfType<OkObjectResult>();
            await task2.ShouldThrowAsync<TaskCanceledException>();
        }
    }
}