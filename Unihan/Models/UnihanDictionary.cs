namespace Unihan.Models
{
    public sealed class UnihanDictionary : SortedDictionary<int, IList<string>>
    {
        public UnihanField Field { get; set; }

        public void Add(int index, string value)
        {
            if (!TryGetValue(index, out var record))
            {
                record = new List<string>();
                this[index] = record;
            }
            record.Add(value);
        }
    }
}