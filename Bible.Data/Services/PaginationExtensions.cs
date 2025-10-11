using Unihan.Models;

namespace Bible.Data.Services
{
    public sealed class Paginated<T>
    {
        public IReadOnlyList<T> Items { get; }
        public int PageSize { get; }
        public int TotalPages { get; }

        public Paginated(IEnumerable<T> items, int pageSize)
        {
            Items = items is IReadOnlyList<T> list ? list : items.ToList();
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(Items.Count / (double)pageSize);
        }
    }

    public static class PaginationExtensions
    {
        // Paginate any IDictionary<int, IList<string>> (including UnihanDictionary)
        // pageNumber is 1-based
        public static Paginated<KeyValuePair<int, IList<string>>> Paginate(
            this IDictionary<int, IList<string>> dict,
            int pageNumber,
            int pageSize,
            IComparer<int>? keyComparer = null)
        {
            if (dict == null) throw new ArgumentNullException(nameof(dict));
            if (pageNumber < 1) throw new ArgumentOutOfRangeException(nameof(pageNumber));
            if (pageSize < 1) throw new ArgumentOutOfRangeException(nameof(pageSize));

            // Use provided comparer or default comparer for ordering keys
            keyComparer ??= Comparer<int>.Default;

            long totalItems = dict.Count;

            // If dictionary already preserves an order you want, you can avoid sorting.
            // For typical Dictionary<int,...> we need a deterministic order: order by key.
            // Sorting keys once is O(n log n). If you repeatedly page the same large dictionary,
            // consider caching the ordered keys or using a SortedDictionary/SortedList instead.
            var orderedEntries = dict
                .OrderBy(kvp => kvp.Key, keyComparer)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList(); // materialize only the page

            return new Paginated<KeyValuePair<int, IList<string>>>(
                orderedEntries,
                pageSize);
        }

        // Convenience: paginate UnihanDictionary specifically
        public static Paginated<KeyValuePair<int, IList<string>>> Paginate(
            this UnihanDictionary dict,
            int pageNumber,
            int pageSize,
            IComparer<int>? keyComparer = null)
            => Paginate((IDictionary<int, IList<string>>)dict, pageNumber, pageSize, keyComparer);

        // Optionally paginate a specific UnihanField within UnihanFieldDictionary
        public static Paginated<KeyValuePair<int, IList<string>>> PaginateField(
            this UnihanFieldDictionary fieldDict,
            UnihanField field,
            int pageNumber,
            int pageSize,
            IComparer<int>? keyComparer = null)
        {
            if (fieldDict == null) throw new ArgumentNullException(nameof(fieldDict));
            if (!fieldDict.TryGetValue(field, out var dict))
                return new Paginated<KeyValuePair<int, IList<string>>>(Array.Empty<KeyValuePair<int, IList<string>>>(), pageSize);

            return Paginate(dict, pageNumber, pageSize, keyComparer);
        }
    }
}