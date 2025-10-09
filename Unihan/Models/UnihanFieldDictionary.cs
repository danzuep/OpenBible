using Unihan.Services;

namespace Unihan.Models
{
    public sealed class UnihanFieldDictionary : Dictionary<UnihanField, UnihanDictionary>, IUnihanReadings
    {
        public void AddEntry(string codepoint, string field, string value)
        {
            var unicodeCodepoint = UnihanParserService.ConvertToCodepoint(codepoint);
            // Parse field string to UnihanField enum (Unknown by default)
            _ = Enum.TryParse<UnihanField>(field, out var unihanField);
            // Invoke action with these values
            AddEntry(unicodeCodepoint, unihanField, value);
        }

        public void AddEntry(int codepoint, UnihanField field, string value)
        {
            if (!TryGetValue(field, out var record))
            {
                record = new UnihanDictionary();
                record.Field = field;
                this[field] = record;
            }
            record.Add(codepoint, value);
        }
    }
}