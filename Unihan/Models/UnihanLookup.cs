using System.Data;
using System.Text;
using Unihan.Services;

namespace Unihan.Models
{
    /// <inheritdoc cref="IUnihanReadings"/>
    public class UnihanLookup : Dictionary<int, UnihanFieldLookup>, IUnihanReadings
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
            if (!TryGetValue(codepoint, out var record))
            {
                record = new UnihanFieldLookup();
                this[codepoint] = record;
            }
            record.Add(field, value);
        }

        public bool TryGetEntryText(int codepoint, IList<UnihanField>? fields, out string entryText)
        {
            if (!ContainsKey(codepoint))
            {
                entryText = string.Empty;
                return false;
            }

            var stringBuilder = new StringBuilder();

            var dictionary = this[codepoint];
            if (fields == null)
            {
                foreach (var kvp in dictionary)
                {
                    stringBuilder.Append(kvp.Key);
                    stringBuilder.Append(": { ");
                    stringBuilder.Append(string.Join("; ", kvp.Value));
                    stringBuilder.AppendLine(" }");
                }
            }
            else
            {
                foreach (var kvp in dictionary.Where(kvp => fields.Contains(kvp.Key)))
                {
                    //if (fields != null && !fields.Contains(kvp.Key)) continue;
                    if (kvp.Key == UnihanField.kDefinition)
                    {
                        stringBuilder.Append(": ");
                    }
                    stringBuilder.Append(string.Join("; ", kvp.Value));
                }
            }

            entryText = stringBuilder.ToString();
            return true;
        }
    }
}