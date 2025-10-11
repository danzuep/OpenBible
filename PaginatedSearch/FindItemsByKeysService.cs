using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace PaginatedSearch
{
    // Streaming result: the async enumerable yields found items as T
    public class FindItemsByKeysService<T, TKey>
    {
        private readonly Func<T, TKey> _keySelector;
        private readonly IEqualityComparer<TKey> _comparer;
        private readonly IMemoryCache _cache;
        private readonly ILogger<FindItemsByKeysService<T, TKey>>? _logger;
        private readonly IMetricsCollector _metrics;

        public FindItemsByKeysService(
            Func<T, TKey> keySelector,
            IMemoryCache cache,
            IEqualityComparer<TKey>? comparer = null,
            ILogger<FindItemsByKeysService<T, TKey>>? logger = null,
            IMetricsCollector? metrics = null)
        {
            _keySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _comparer = comparer ?? EqualityComparer<TKey>.Default;
            _logger = logger;
            _metrics = metrics ?? new LoggingMetricsCollector();
        }

        // For page-number pagination
        public async IAsyncEnumerable<T> FindItemsByKeysAsync(
            IPageNumberDataSource<T> dataSource,
            IEnumerable<TKey> keys,
            FindOptions options,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken externalCancellationToken = default)
        {
            if (dataSource == null) throw new ArgumentNullException(nameof(dataSource));
            if (options == null) throw new ArgumentNullException(nameof(options));

            var cts = CancellationTokenSource.CreateLinkedTokenSource(externalCancellationToken);
            var ct = cts.Token;
            var sw = Stopwatch.StartNew();

            var remaining = new ConcurrentDictionary<TKey, byte>(_comparer);
            foreach (var k in keys) remaining.TryAdd(k, 0);

            var pagesFetched = 0;
            var requestsMade = 0;

            var pageChannel = Channel.CreateBounded<int>(new BoundedChannelOptions(options.PageChannelCapacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = false,
                SingleWriter = false
            });

            // Output channel used to buffer items to yield
            var outputChannel = Channel.CreateUnbounded<T>(new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });

            // Enqueue first page
            await pageChannel.Writer.WriteAsync(0, ct).ConfigureAwait(false);
            var pageIndexEnqueued = 0;

            var maxConcurrency = Math.Max(1, options.MaxConcurrency);
            var pageSize = Math.Max(1, options.PageSize);
            var maxPagesToSearch = options.MaxPagesToSearch;

            // Worker consumers
            var workers = new List<Task>();
            for (int i = 0; i < maxConcurrency; i++)
            {
                workers.Add(Task.Run(async () =>
                {
                    try
                    {
                        while (await pageChannel.Reader.WaitToReadAsync(ct).ConfigureAwait(false))
                        {
                            if (!pageChannel.Reader.TryRead(out var pageIndex)) continue;

                            Interlocked.Increment(ref requestsMade);
                            _metrics.Started();
                            PageNumberResult<T> page;
                            try
                            {
                                page = await dataSource.GetPageAsync(pageIndex, pageSize, ct).ConfigureAwait(false)
                                    ?? new PageNumberResult<T> { Items = Array.Empty<T>(), HasMore = false };
                            }
                            catch (OperationCanceledException) when (ct.IsCancellationRequested)
                            {
                                break;
                            }
                            catch (Exception ex)
                            {
                                _logger?.LogError(ex, "Error fetching page {PageIndex}", pageIndex);
                                break;
                            }
                            _metrics.Finished();
                            Interlocked.Increment(ref pagesFetched);
                            _logger?.LogDebug("Fetched page {PageIndex} with {Count} items", pageIndex, page.Items.Count);

                            // process items
                            foreach (var item in page.Items)
                            {
                                if (ct.IsCancellationRequested) break;
                                var key = _keySelector(item);
                                if (remaining.TryRemove(key, out _))
                                {
                                    // store in cache with sliding expiration
                                    var cacheKey = CacheKeyFor(key);
                                    _cache.Set(cacheKey, item, new MemoryCacheEntryOptions { SlidingExpiration = options.CacheSlidingExpiration });

                                    // emit to output channel
                                    await outputChannel.Writer.WriteAsync(item, ct).ConfigureAwait(false);
                                    _logger?.LogDebug("Matched key {Key} on page {PageIndex}", key, pageIndex);

                                    if (remaining.IsEmpty)
                                    {
                                        _logger?.LogInformation("All keys found; cancelling remaining work");
                                        cts.Cancel();
                                        break;
                                    }
                                }
                            }

                            if (ct.IsCancellationRequested) break;

                            // enqueue next page if indicated
                            bool hasMore = page.HasMore || (page.TotalPages.HasValue && pageIndex + 1 < page.TotalPages.Value);
                            if (page.TotalPages.HasValue)
                            {
                                // stop when exceeding total pages
                                if (pageIndex + 1 < page.TotalPages.Value)
                                {
                                    // enqueue next index
                                    var nextIdx = pageIndex + 1;
                                    // apply MaxPagesToSearch cap
                                    if (!maxPagesToSearch.HasValue || nextIdx < maxPagesToSearch.Value)
                                    {
                                        await pageChannel.Writer.WriteAsync(nextIdx, ct).ConfigureAwait(false);
                                    }
                                }
                            }
                            else if (hasMore)
                            {
                                var nextIdx = pageIndex + 1;
                                if (!maxPagesToSearch.HasValue || nextIdx < maxPagesToSearch.Value)
                                {
                                    await pageChannel.Writer.WriteAsync(nextIdx, ct).ConfigureAwait(false);
                                }
                            }
                            else
                            {
                                // If no more info and no total pages, we still can enqueue next if under maxPagesToSearch
                                if (maxPagesToSearch.HasValue && pageIndex + 1 < maxPagesToSearch.Value)
                                {
                                    await pageChannel.Writer.WriteAsync(pageIndex + 1, ct).ConfigureAwait(false);
                                }
                                // else no more pages implied
                            }
                        }
                    }
                    catch (OperationCanceledException) { /* expected */ }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Worker encountered an exception");
                    }
                }, ct));
            }

            // Task to complete output channel once workers done
            var completion = Task.Run(async () =>
            {
                try
                {
                    await Task.WhenAll(workers).ConfigureAwait(false);
                }
                catch { /* ignore */ }
                finally
                {
                    outputChannel.Writer.TryComplete();
                }
            }, ct);

            // Stream results to caller
            await foreach (var found in outputChannel.Reader.ReadAllAsync(ct).ConfigureAwait(false))
            {
                yield return found;
            }

            // wait for completion tasks
            try { await completion.ConfigureAwait(false); } catch { }

            sw.Stop();
            _logger?.LogInformation("Search complete: pages={Pages}, requests={Req}, elapsed={ElapsedMs}ms", pagesFetched, requestsMade, sw.Elapsed.TotalMilliseconds);
        }

        // For page-key (cursor-like) data source
        public async IAsyncEnumerable<T> FindItemsByKeysByPageKeyAsync(
            IPageKeyDataSource<T> dataSource,
            IEnumerable<TKey> keys,
            FindOptions options,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken externalCancellationToken = default)
        {
            if (dataSource == null) throw new ArgumentNullException(nameof(dataSource));
            if (options == null) throw new ArgumentNullException(nameof(options));

            var cts = CancellationTokenSource.CreateLinkedTokenSource(externalCancellationToken);
            var ct = cts.Token;
            var sw = Stopwatch.StartNew();

            var remaining = new ConcurrentDictionary<TKey, byte>(_comparer);
            foreach (var k in keys) remaining.TryAdd(k, 0);

            var pagesFetched = 0;
            var requestsMade = 0;

            var pageChannel = Channel.CreateBounded<string?>(new BoundedChannelOptions(options.PageChannelCapacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = false,
                SingleWriter = false
            });

            var outputChannel = Channel.CreateUnbounded<T>(new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });

            // enqueue initial
            //var key = _keySelector(item);
            //remaining.TryRemove(key, out _);
            await pageChannel.Writer.WriteAsync("", ct).ConfigureAwait(false);

            var maxConcurrency = Math.Max(1, options.MaxConcurrency);
            var pageSize = Math.Max(1, options.PageSize);
            var maxPagesToSearch = options.MaxPagesToSearch;

            var workers = new List<Task>();
            var pagesConsumed = new ConcurrentDictionary<string?, byte>(); // optional dedupe if page keys might repeat

            for (int i = 0; i < maxConcurrency; i++)
            {
                workers.Add(Task.Run(async () =>
                {
                    try
                    {
                        while (await pageChannel.Reader.WaitToReadAsync(ct).ConfigureAwait(false))
                        {
                            if (!pageChannel.Reader.TryRead(out var pageKey) || pageKey == null) continue;
                            // optional dedupe
                            if (!pagesConsumed.TryAdd(pageKey, 0)) continue;

                            Interlocked.Increment(ref requestsMade);
                            _metrics.Started();
                            PageKeyResult<T> page;
                            try
                            {
                                page = await dataSource.GetPageAsync(pageKey, pageSize, ct).ConfigureAwait(false)
                                    ?? new PageKeyResult<T> { Items = Array.Empty<T>(), NextKey = null };
                            }
                            catch (OperationCanceledException) when (ct.IsCancellationRequested)
                            {
                                break;
                            }
                            catch (Exception ex)
                            {
                                _logger?.LogError(ex, "Error fetching page with key {PageKey}", pageKey);
                                break;
                            }
                            _metrics.Finished();
                            Interlocked.Increment(ref pagesFetched);
                            _logger?.LogDebug("Fetched page key={PageKey} items={Count}", pageKey, page.Items.Count);

                            // process items
                            foreach (var item in page.Items)
                            {
                                if (ct.IsCancellationRequested) break;
                                var key = _keySelector(item);
                                if (remaining.TryRemove(key, out _))
                                {
                                    var cacheKey = CacheKeyFor(key);
                                    _cache.Set(cacheKey, item, new MemoryCacheEntryOptions { SlidingExpiration = options.CacheSlidingExpiration });
                                    await outputChannel.Writer.WriteAsync(item, ct).ConfigureAwait(false);
                                    _logger?.LogDebug("Matched key {Key} on pageKey {PageKey}", key, pageKey);

                                    if (remaining.IsEmpty)
                                    {
                                        _logger?.LogInformation("All keys found; cancelling remaining work");
                                        cts.Cancel();
                                        break;
                                    }
                                }
                            }

                            if (ct.IsCancellationRequested) break;

                            if (!string.IsNullOrEmpty(page.NextKey))
                            {
                                // optional MaxPagesToSearch logic (we don't have page count, so use a counter)
                                if (!maxPagesToSearch.HasValue || pagesConsumed.Count < maxPagesToSearch.Value)
                                {
                                    await pageChannel.Writer.WriteAsync(page.NextKey, ct).ConfigureAwait(false);
                                }
                            }
                        }
                    }
                    catch (OperationCanceledException) { /* expected */ }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Worker encountered exception (page-key)");
                    }
                }, ct));
            }

            var completion = Task.Run(async () =>
            {
                try { await Task.WhenAll(workers).ConfigureAwait(false); }
                catch { }
                finally { outputChannel.Writer.TryComplete(); }
            }, ct);

            await foreach (var found in outputChannel.Reader.ReadAllAsync(ct).ConfigureAwait(false))
            {
                yield return found;
            }

            try { await completion.ConfigureAwait(false); } catch { }

            sw.Stop();
            _logger?.LogInformation("Search complete: pages={Pages}, requests={Req}, elapsed={ElapsedMs}ms", pagesFetched, requestsMade, sw.Elapsed.TotalMilliseconds);
        }

        // Helper to read item from cache by key
        public bool TryGetFromCache(TKey key, out T? item)
        {
            if (_cache.TryGetValue(CacheKeyFor(key), out var boxed) && boxed is T t)
            {
                item = t; return true;
            }
            item = default;
            return false;
        }

        private static string CacheKeyFor(TKey key) => $"FindByKeys:{key}";
    }
}