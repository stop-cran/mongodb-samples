using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebApplication1
{
    public interface IUniqueResource
    {
        Task Own(TimeSpan duration, CancellationToken cancellationToken);
    }
}