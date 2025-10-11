using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace PaginatedSearch
{
    public static class DependencyInjectionExtensions
    {
        // Registers the FindItemsByKeysService factory for a concrete T,TKey with sensible defaults.
        // Example: services.AddFindByKeys<Item, int>((sp) => new Func<...>)  <-- but we can't inject TFunc easily.
        // Provide registration helpers for common needs: memory cache + metrics + logging.
        public static IServiceCollection AddPagination(this IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddSingleton<IMetricsCollector, LoggingMetricsCollector>();
            // Consumers will construct FindItemsByKeysService<T,TKey> via DI, providing IMemoryCache, ILogger<...>, IMetricsCollector.
            services.AddTransient(typeof(FindItemsByKeysService<,>), typeof(FindItemsByKeysService<,>));
            return services;
        }

        // Optional: register a specific service instance factory with key selector
        public static IServiceCollection AddFindByKeysFor<T, TKey>(this IServiceCollection services, Func<T, TKey> keySelector, IEqualityComparer<TKey>? comparer = null)
        {
            services.AddPagination();
            services.AddTransient(sp =>
            {
                var cache = sp.GetRequiredService<IMemoryCache>();
                var logger = sp.GetService<ILogger<FindItemsByKeysService<T, TKey>>>();
                var metrics = sp.GetService<IMetricsCollector>();
                return new FindItemsByKeysService<T, TKey>(keySelector, cache, comparer, logger, metrics);
            });
            return services;
        }
    }
}