using System.Runtime.CompilerServices;

namespace WebApplication1.Awaitable
{
    public interface ITaskAwaitable<T>
    {
        TaskAwaiter<T> GetAwaiter();
    }
}