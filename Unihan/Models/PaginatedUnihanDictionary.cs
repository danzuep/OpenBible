using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Unihan.Models
{
    public sealed class PaginatedUnihanDictionary : SortedDictionary<int, UnihanDictionary>
    {
        private readonly int _pageSize;

        public PaginatedUnihanDictionary(int pageSize)
        {
            if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize));
            _pageSize = pageSize;
        }

        // Construct from a pre-populated UnihanDictionary, paging by ordinal (ordered by key)
        public PaginatedUnihanDictionary(int pageSize, UnihanDictionary values) : this(pageSize)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));

            // values is already sorted (SortedDictionary), so no OrderBy needed.
            var groupedEntries = values
                .Select((kvp, idx) => (page: idx / _pageSize, kvp))
                .GroupBy(t => t.page, t => t.kvp);

            foreach (var group in groupedEntries)
            {
                var pageDict = new UnihanDictionary();
                foreach (var kv in group)
                {
                    pageDict[kv.Key] = kv.Value;
                }

                this[group.Key] = pageDict;
            }
        }

        // Add an entry where page index is computed from the key (key / pageSize).
        // Use this when the key itself determines the page (e.g., numeric codepoint).
        public void AddByKeyIndex(KeyValuePair<int, IList<string>> item)
        {
            if (item.Value == null) throw new ArgumentNullException(nameof(item.Value));

            int pageIndex = item.Key / _pageSize;

            if (!TryGetValue(pageIndex, out var page))
            {
                page = new UnihanDictionary();
                this[pageIndex] = page;
            }

            page[item.Key] = item.Value;
        }

        // Add an entry to an explicit page index (ordinal paging).
        // Use this when you're paging by ordinal position (idx / pageSize) computed externally.
        public void AddToPage(int pageIndex, KeyValuePair<int, IList<string>> item)
        {
            if (pageIndex < 0) throw new ArgumentOutOfRangeException(nameof(pageIndex));
            if (item.Value == null) throw new ArgumentNullException(nameof(item.Value));

            if (!TryGetValue(pageIndex, out var page))
            {
                page = new UnihanDictionary();
                this[pageIndex] = page;
            }

            page[item.Key] = item.Value;
        }

        // Bulk-add entries where the page is computed from the key (key / pageSize).
        public void AddRangeByKeyIndex(IEnumerable<KeyValuePair<int, IList<string>>> entries)
        {
            if (entries == null) throw new ArgumentNullException(nameof(entries));
            foreach (var kv in entries) AddByKeyIndex(kv);
        }

        // Bulk-add entries given as an ordered sequence (pages computed by ordinal index).
        public void AddRangeByOrdinalIndex(IEnumerable<KeyValuePair<int, IList<string>>> orderedEntries)
        {
            if (orderedEntries == null) throw new ArgumentNullException(nameof(orderedEntries));

            int idx = 0;
            foreach (var kv in orderedEntries)
            {
                int pageIndex = idx++ / _pageSize;
                AddToPage(pageIndex, kv);
            }
        }

        // Try get a page (0-based pageIndex)
        public bool TryGetPage(int pageIndex, out UnihanDictionary? page) =>
            TryGetValue(pageIndex, out page);

        // Get page or empty dictionary (never returns null)
        public UnihanDictionary GetPageOrEmpty(int pageIndex) =>
            TryGetValue(pageIndex, out var page) ? page : new UnihanDictionary();

        // Highest page index present, or -1 when empty
        public int MaxPageIndex => this.Count == 0 ? -1 : this.Keys.Max();

        // Number of pages currently stored
        public int PageCount => this.Count;
    }
}