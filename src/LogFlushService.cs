using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Appender;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebApplication1
{
    public sealed class LogFlushService : IHostedService
    {
        private readonly CancellationTokenSource loopCancellation = new CancellationTokenSource();
        private readonly ILogger<LogFlushService> logger;
        private Task? loop;

        public LogFlushService(ILogger<LogFlushService> logger)
        {
            this.logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            loop = Loop(loopCancellation.Token);

            return Task.CompletedTask;
        }

        private async Task Loop(CancellationToken cancellationToken)
        {
            for (; ; )
            {
                FlushLogs();

                await Task.Delay(30_000, cancellationToken);
            }
        }

        private void FlushLogs()
        {
            try
            {
                foreach (var appender in LogManager.GetAllRepositories()
                    .SelectMany(repo => repo.GetAppenders())
                    .OfType<BufferingAppenderSkeleton>())
                    appender.Flush();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error flushing logs.");
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            loopCancellation.Cancel();

            if (loop != null)
                try
                {
                    await loop;
                }
                catch (TaskCanceledException)
                { }

            FlushLogs();
        }
    }
}