using System;
using System.Threading;
using System.Threading.Tasks;
using RedLockNet;

namespace WebApplication1
{
    public class UniqueResource : IUniqueResource
    {
        private readonly IDistributedLockFactory _distributedLockFactory;

        public UniqueResource(IDistributedLockFactory distributedLockFactory)
        {
            _distributedLockFactory = distributedLockFactory;
        }

        public async Task Own(TimeSpan duration, CancellationToken cancellationToken)
        {
            if (duration < TimeSpan.Zero || duration > TimeSpan.FromMinutes(1))
                throw new ArgumentOutOfRangeException(nameof(duration));
            
            using var distributedLock = await _distributedLockFactory.CreateLockAsync(
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