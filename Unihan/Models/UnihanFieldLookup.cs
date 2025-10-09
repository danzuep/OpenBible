namespace Unihan.Models
{
    public sealed class UnihanFieldLookup : Dictionary<UnihanField, IList<string>>
    {
        public void Add(UnihanField field, string value)
        {
            if (!TryGetValue(field, out var record))
            {
                record = new List<string>();
                this[field] = record;
            }
            record.Add(value);
        }
    }
}