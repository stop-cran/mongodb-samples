using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace WebApplication1
{
    public class Program
    {
        // For testing
        public static CancellationToken RunCancellation { get; set; } = CancellationToken.None;

        public static async Task Main(params string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync(RunCancellation);
        }

        public static IHostBuilder CreateHostBuilder(params string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, configBuilder) =>
                    configBuilder.AddJsonFile("Environment/appsettings.json", false, false))
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
        }
    }
}