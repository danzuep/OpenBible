namespace Bible.Backend.Abstractions
{
    /// <summary>
    /// Unicode Unihan readings, see
    /// <see href="https://www.unicode.org/charts/unihan.html"/> and
    /// <seealso href="https://www.unicode.org/Public/UCD/latest/ucd/"/>.
    /// Outer key: Unicode codepoint string (e.g., "U+3400")
    /// Inner dictionary key: field name (e.g., "kCantonese")
    /// Inner dictionary value: field value string
    /// </summary>
    public interface IUnihanReadings
    {
        void AddEntry(string codepoint, string field, string value);
    }
}