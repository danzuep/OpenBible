using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PaginatedSearch;

namespace Bible.Console
{
    internal class PaginatedSearch
    {
        public static async Task DemoAsync()
        {
            var services = new ServiceCollection();
            services.AddPagination();
            services.AddLogging(config => config.SetMinimumLevel(LogLevel.Debug).AddDebug()
                .AddSimpleConsole(options => options.TimestampFormat = "[HH:mm:ss.fff] "));

            var sp = services.BuildServiceProvider();
            var logger = sp.GetRequiredService<ILogger<Program>>();
            var cache = sp.GetRequiredService<IMemoryCache>();
            var metrics = sp.GetRequiredService<IMetricsCollector>();

            // Build sample Unicode codepoint data: Basic Latin + some extended
            var codepoints = Enumerable.Range(32, 96) // printable ASCII-ish
                .Concat([0x2603, 0x1F600, 0x1F601, 0x1F680]) // snowman, grinning face, rocket
                .Distinct()
                .OrderBy(i => i)
                .Select(i => new CodepointItem(i, $"U+{i:X4}"))
                .ToList();

            // Build data sources
            var pageNumberSource = new InMemoryPageNumberCodepointSource(codepoints);
            var pageKeySource = new InMemoryPageKeyCodepointSource(codepoints);

            // Targets: some early (small codepoint), some late, some missing (large codepoint)
            var targets = new List<int> { 32, 35, 0x2603, 0x1F600, 0x10FFFF /* missing */ , 0x1F680 };

            logger.LogInformation("Targets: {Targets}", string.Join(", ", targets.Select(t => $"U+{t:X}")));

            // Create service instance (int key)
            var service = new FindItemsByKeysService<CodepointItem, int>(cp => cp.Codepoint, cache, comparer: null, logger: sp.GetService<ILogger<FindItemsByKeysService<CodepointItem, int>>>(), metrics);

            var options = new FindOptions { PageSize = 10, MaxConcurrency = 3, CacheSlidingExpiration = TimeSpan.FromSeconds(30) };

            // Demo: page-key (cursor-like) pagination — preferred for scanning codepoints
            logger.LogInformation("--- Searching with page-key (cursor style) ---");
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            await foreach (var item in service.FindItemsByKeysByPageKeyAsync(pageKeySource, targets, options, cts.Token))
            {
                logger.LogInformation("Streamed found: {Key} -> {Char} ({Desc})", item.Codepoint, item.AsString(), item.Description);
            }

            // Show cache retrieval
            foreach (var t in targets)
            {
                if (service.TryGetFromCache(t, out var cached))
                {
                    logger.LogInformation("Cached: {Key} -> {Char}", t, cached!.AsString());
                }
                else
                {
                    logger.LogInformation("Not cached: {Key}", t);
                }
            }

            // Demo: page-number pagination (same logic but using page indices)
            logger.LogInformation("--- Searching with page-number pagination ---");
            var service2 = new FindItemsByKeysService<CodepointItem, int>(cp => cp.Codepoint, cache, logger: sp.GetService<ILogger<FindItemsByKeysService<CodepointItem, int>>>(), metrics: metrics);

            await foreach (var item in service2.FindItemsByKeysAsync(pageNumberSource, targets, options, CancellationToken.None))
            {
                logger.LogInformation("Streamed found (page-number): {Key} -> {Char} ({Desc})", item.Codepoint, item.AsString(), item.Description);
            }

            logger.LogInformation("Demo complete");
        }
    }
}
