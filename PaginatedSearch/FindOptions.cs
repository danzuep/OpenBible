using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PaginatedSearch
{
    public class FindOptions
    {
        public int PageSize { get; init; } = 100;
        public int MaxConcurrency { get; init; } = 4;
        public int? MaxPagesToSearch { get; init; } = null;
        public TimeSpan CacheSlidingExpiration { get; init; } = TimeSpan.FromMinutes(5);
        public int PageChannelCapacity { get; init; } = 1024;
    }

    public class PageNumberResult<T>
    {
        public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
        public int? TotalPages { get; init; }
        public bool HasMore { get; init; } = false;
    }

    public class PageKeyResult<T>
    {
        public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
        public string? NextKey { get; init; }
    }

    public interface IPageNumberDataSource<T>
    {
        Task<PageNumberResult<T>> GetPageAsync(int pageIndex, int pageSize, CancellationToken ct);
    }

    public interface IPageKeyDataSource<T>
    {
        // pageKey is a string token representing where to start; null/empty => start
        Task<PageKeyResult<T>> GetPageAsync(string? pageKey, int pageSize, CancellationToken ct);
    }

    public record CodepointItem(int Codepoint, string Description)
    {
        public string AsString() => char.ConvertFromUtf32(Codepoint);
    }

    public class InMemoryPageNumberCodepointSource : IPageNumberDataSource<CodepointItem>
    {
        private readonly List<CodepointItem> _items;
        public InMemoryPageNumberCodepointSource(IEnumerable<CodepointItem> items) => _items = items.ToList();

        public Task<PageNumberResult<CodepointItem>> GetPageAsync(int pageIndex, int pageSize, CancellationToken ct)
        {
            if (pageIndex < 0) throw new ArgumentOutOfRangeException(nameof(pageIndex));
            var skip = pageIndex * pageSize;
            var page = _items.Skip(skip).Take(pageSize).ToList();
            var total = _items.Count;
            var totalPages = (int)Math.Ceiling(total / (double)pageSize);
            return Task.FromResult(new PageNumberResult<CodepointItem>
            {
                Items = page,
                TotalPages = totalPages,
                HasMore = pageIndex + 1 < totalPages
            });
        }
    }

    public class InMemoryPageKeyCodepointSource : IPageKeyDataSource<CodepointItem>
    {
        private readonly List<CodepointItem> _items;
        public InMemoryPageKeyCodepointSource(IEnumerable<CodepointItem> items) => _items = items.ToList();

        public Task<PageKeyResult<CodepointItem>> GetPageAsync(string? pageKey, int pageSize, CancellationToken ct)
        {
            int start = 0;
            if (!string.IsNullOrEmpty(pageKey)) int.TryParse(pageKey, out start);
            var page = _items.Skip(start).Take(pageSize).ToList();
            string? next = null;
            if (start + pageSize < _items.Count) next = (start + pageSize).ToString();
            return Task.FromResult(new PageKeyResult<CodepointItem>
            {
                Items = page,
                NextKey = next
            });
        }
    }

    public record FindSummary<TKey>(IReadOnlyList<TKey> MissingKeys, int PagesSearched, int RequestsMade, TimeSpan Elapsed);
}