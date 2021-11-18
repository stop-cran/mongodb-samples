using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace WebApplication1.Awaitable
{
    public static class TaskAwaitableServiceCollectionExtensions
    {
        public static IServiceCollection AddSingletonTaskAwaitable<T>(this IServiceCollection services,
            Func<IServiceProvider, Task<T>> implementationFactory) =>
            services.AddSingleton<ITaskAwaitable<T>>(serviceProvider =>
                new DisposeResultAwaitable<T>(implementationFactory(serviceProvider)));
        
        public static IServiceCollection AddScopedTaskAwaitable<T>(this IServiceCollection services,
            Func<IServiceProvider, Task<T>> implementationFactory) =>
            services.AddScoped<ITaskAwaitable<T>>(serviceProvider =>
                new DisposeResultAwaitable<T>(implementationFactory(serviceProvider)));
    }
}