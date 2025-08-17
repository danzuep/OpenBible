using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using Unihan.Models;

namespace Unihan.Services
{
    public sealed class UnihanParserService
    {
        /// <summary>
        /// Parses the input stream asynchronously and populates an instance of <typeparamref name="T"/> with Unihan readings.
        /// </summary>
        /// <inheritdoc cref="ParseAsync(StreamReader, Action{string})"/>
        /// <typeparam name="T">Type implementing <see cref="IUnihanReadings"/> with a parameterless constructor.</typeparam>
        /// <returns>A non-null instance of <typeparamref name="T"/> populated with parsed entries.</returns>
        public async Task<T> ParseAsync<T>(string inputPath, string outputPath, T? unihan = null)
            where T : class, IUnihanReadings, new()
        {
            unihan ??= new T();
            await ParseAsync(inputPath, unihan);
            var json = JsonSerializer.Serialize(unihan);
            await File.WriteAllTextAsync(outputPath, json);
            return unihan;
        }

        /// <summary>
        /// Parses the input stream asynchronously and populates Unihan readings.
        /// </summary>
        /// <param name="inputPath">The file to open and read lines from.</param>
        /// <param name="unihanHandler">The <see cref="IUnihanReadings"/> to populate with.</param>
        /// <inheritdoc cref="ParseAsync(StreamReader, Action{string})"/>
        /// <inheritdoc cref="StreamReader(string)"/>
        public static async Task ParseAsync(string inputPath, IUnihanReadings unihanHandler)
        {
            using var reader = new StreamReader(inputPath);
            await ParseAsync(reader, ParseEntry);

            bool ParseEntry(string line)
            {
                bool continueParsing = true;
                if (!TryParseLine(line, out var parts))
                    return continueParsing; // malformed line, skip

                var codepoint = parts[0];
                var field = parts[1];
                var value = parts[2];

                unihanHandler.AddEntry(codepoint, field, value);
                return continueParsing;
            }
        }

        public static async Task<Stream> ParseToStreamAsync(Stream inputStream, IEnumerable<UnihanField>? fields = null)
        {
            if (fields is null || !fields.Any())
            {
                fields = [
                    UnihanField.kDefinition,
                    UnihanField.kMandarin,
                    UnihanField.kCantonese,
                    UnihanField.kJapanese
                ];
            }
            var results = await ParseAsync(inputStream, fields);
            var outputStream = new MemoryStream();
            await JsonSerializer.SerializeAsync(outputStream, results);
            outputStream.Position = 0;
            return outputStream;
        }

        public static async Task<UnihanLookup> ParseAsync(Stream stream, IEnumerable<UnihanField>? fields = null)
        {
            var hashtable = fields == null ? null : new HashSet<UnihanField>(fields);
            using var reader = new StreamReader(stream);
            var results = new UnihanLookup();
            await ParseAsync(reader, ParseEntry);
            stream.Seek(0, SeekOrigin.Begin); // Reset stream position
            return results;

            bool ParseEntry(string line)
            {
                bool continueParsing = true;
                if (!TryParseLine(line, out var parts))
                    return continueParsing; // malformed line, skip

                var codepoint = ConvertToCodepoint(parts[0]);

                _ = Enum.TryParse<UnihanField>(parts[1], out var unihanField);

                if (hashtable != null && !hashtable.Contains(unihanField))
                    return continueParsing; // skip fields not in the provided list

                results.AddEntry(codepoint, unihanField, value: parts[2]);

                return continueParsing;
            }
        }

        public static async Task<UnihanFieldLookup> FindAsync(int codepointToFind, Stream streamToSearch)
        {
            bool isFound = false;
            var results = new UnihanFieldLookup();
            using var reader = new StreamReader(streamToSearch);
            await ParseAsync(reader, ParseEntry);
            streamToSearch.Seek(0, SeekOrigin.Begin); // Reset stream position for next rune
            return results;

            bool ParseEntry(string line)
            {
                bool continueParsing = true;
                if (!TryParseLine(line, out var parts))
                    return continueParsing; // malformed line, skip

                var codepoint = ConvertToCodepoint(parts[0]);

                if (isFound && codepoint != codepointToFind)
                    return false; // no more codepoint matches

                if (codepoint != codepointToFind)
                    return continueParsing; // not the codepoint we are looking for

                if (!isFound)
                    isFound = true; // if we reach here, we found the codepoint

                _ = Enum.TryParse<UnihanField>(parts[1], out var unihanField);

                results.Add(unihanField, value: parts[2]);

                return continueParsing;
            }
        }

        private const int UnihanColumns = 3;
        private static bool TryParseLine(string text, [NotNullWhen(true)] out string[]? parts)
        {
            parts = null;
            text = text.Trim();
            if (text.Length == 0 || text.StartsWith("#", StringComparison.Ordinal))
                return false; // skip empty or comment lines

            // Expected format: U+XXXX<TAB>field<TAB>value
            parts = text.Split('\t');
            if (parts == null || parts.Length < UnihanColumns)
                return false; // malformed line, skip

            return true;
        }

        /// <summary>
        /// Extract codepoint from key string hex code like "U+XXXX" or "U+XXXXX"
        /// <seealso cref="System.Text.Rune"/> `var unicode = char.ConvertFromUtf32(codepoint);`
        /// </summary>
        public static int ConvertToCodepoint(string codepointStr)
        {
            if (!codepointStr.AsSpan(0, 2).Equals("U+", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Invalid format", nameof(codepointStr));

            if (!int.TryParse(codepointStr.AsSpan(2), NumberStyles.HexNumber, null, out int codepoint))
                throw new FormatException("Invalid hex number.");

            if (codepoint < 0 || codepoint > 0x10FFFF)
                throw new ArgumentOutOfRangeException(nameof(codepoint), "Invalid Unicode codepoint.");

            return codepoint;
        }

        /// <summary>
        /// Parses the input stream asynchronously.
        /// </summary>
        /// <param name="reader">The <see cref="StreamReader"/> to read lines from.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="reader"/> is null.</exception>
        private static async Task ParseAsync(StreamReader reader, Func<string, bool> function)
        {
            if (reader is null)
                throw new ArgumentNullException(nameof(reader));

            string? line;
            while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
            {
                var continueParsing = function(line);
                if (!continueParsing)
                {
                    break;
                }
            }
        }
    }
}