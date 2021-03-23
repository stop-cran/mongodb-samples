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
        private readonly ILogger<LogFlushService> _logger;
        private readonly CancellationTokenSource _loopCancellation = new();
        private Task _loop = Task.CompletedTask;

        public LogFlushService(ILogger<LogFlushService> logger)
        {
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _loop = Loop(_loopCancellation.Token);

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _loopCancellation.Cancel();

            if (_loop != null)
                try
                {
                    await _loop;
                }
                catch (TaskCanceledException)
                {
                }

            FlushLogs();
        }

        private async Task Loop(CancellationToken cancellationToken)
        {
            for (;;)
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
                _logger.LogError(ex, "Error flushing logs");
            }
        }
    }
}