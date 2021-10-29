using System;
using System.Threading;
using System.Threading.Tasks;
using RedLockNet;
using WebApplication1.Awaitable;

namespace WebApplication1
{
    public class UniqueResource : IUniqueResource
    {
        private readonly ITaskAwaitable<IDistributedLockFactory> _distributedLockFactoryTask;

        public UniqueResource(ITaskAwaitable<IDistributedLockFactory> distributedLockFactoryTask)
        {
            _distributedLockFactoryTask = distributedLockFactoryTask;
        }

        public async Task Own(TimeSpan duration, CancellationToken cancellationToken)
        {
            if (duration < TimeSpan.Zero || duration > TimeSpan.FromMinutes(1))
                throw new ArgumentOutOfRangeException(nameof(duration));

            var distributedLockFactory = await _distributedLockFactoryTask;
            using var distributedLock = await distributedLockFactory.CreateLockAsync(
                "UniqueResource",
                duration,
                TimeSpan.MaxValue,
                TimeSpan.FromSeconds(1),
                cancellationToken);

            if (!distributedLock.IsAcquired)
                throw new ApplicationException("Failed to acquire the unique resource.");

            await Task.Delay(duration, cancellationToken);
        }
    }
}