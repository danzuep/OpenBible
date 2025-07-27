using System.Globalization;
using Bible.Backend.Abstractions;

namespace Bible.Backend.Models
{
    /// <inheritdoc cref="IUnihanReadings"/>
    public class UnihanRelay : IUnihanReadings
    {
        public Action<int, UnihanField, string>? Action { get; set; }

        public void AddEntry(string codepoint, string field, string value)
        {
            //var unicodeCodepoint = ConvertToUnicode(codepoint);
            var unicodeCodepoint = ConvertToCodepoint(codepoint);
            // Parse field string to UnihanField enum (Unknown by default)
            _ = Enum.TryParse<UnihanField>(field, out var unihanField);
            // Invoke optional action with these values
            Action?.Invoke(unicodeCodepoint, unihanField, value);
        }

        /// <summary>
        /// Extract codepoint from key string hex code like "U+XXXX" or "U+XXXXX"
        /// <seealso cref="System.Text.Rune"/>
        /// </summary>
        internal static int ConvertToCodepoint(string codepointStr)
        {
            if (!codepointStr.AsSpan(0, 2).Equals("U+", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Invalid format", nameof(codepointStr));

            if (!int.TryParse(codepointStr.AsSpan(2), NumberStyles.HexNumber, null, out int codepoint))
                throw new FormatException("Invalid hex number.");

            if (codepoint < 0 || codepoint > 0x10FFFF)
                throw new ArgumentOutOfRangeException(nameof(codepoint), "Invalid Unicode codepoint.");

            return codepoint;
        }

        internal static string ConvertToUnicode(string codepointStr)
        {
            var codepoint = ConvertToCodepoint(codepointStr);
            var unicode = char.ConvertFromUtf32(codepoint);
            return unicode;
        }
    }
}