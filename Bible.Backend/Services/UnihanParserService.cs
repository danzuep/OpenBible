using System.Text.Json;
using Bible.Backend.Abstractions;

namespace Bible.Backend.Services
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

            void ParseEntry(string line)
            {
                line = line.Trim();
                if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
                    return; // Skip empty or comment lines

                // Expected format: U+XXXX<TAB>field<TAB>value
                var parts = line.Split('\t');
                if (parts.Length < 3)
                    return; // malformed line, skip

                var codepoint = parts[0];
                var field = parts[1];
                var value = parts[2];

                unihanHandler.AddEntry(codepoint, field, value);
            }
        }

        /// <summary>
        /// Parses the input stream asynchronously.
        /// </summary>
        /// <param name="reader">The <see cref="StreamReader"/> to read lines from.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="reader"/> is null.</exception>
        private static async Task ParseAsync(StreamReader reader, Action<string> action)
        {
            if (reader is null)
                throw new ArgumentNullException(nameof(reader));

            string? line;
            while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
            {
                action?.Invoke(line);
            }
        }
    }
}