using Bible.Backend.Services;
using Unihan.Models;

namespace Unihan.Services
{
    /// <inheritdoc cref="IUnihanReadings"/>
    public class UnihanSerializer : IUnihanReadings, IDisposable
    {
        private JsonBufferWriter<UnihanJsonEntry>? _jsonBufferWriter;

        public string? OutputPath { get; set; }

        public static async Task ParseUnihanAsync(string sitePath, string fileName = "Unihan_Readings")
        {
            var inputPath = Path.Combine(sitePath, $"{fileName}.txt");
            var outputPath = Path.Combine(sitePath, $"{fileName}.json");
            var unihanSerializer = new UnihanSerializer();
            await unihanSerializer.ParseAsync(inputPath, outputPath);
        }

        /// <inheritdoc cref="UnihanParserService.ParseAsync{T}(StreamReader)"/>
        public async Task ParseAsync(string inputPath, string outputPath)
        {
            OutputPath = outputPath;
            await UnihanParserService.ParseAsync(inputPath, this);
            _jsonBufferWriter?.Dispose();
            _jsonBufferWriter = null;
        }

        public void AddEntry(string codepoint, string field, string value)
        {
            var unicodeCodepoint = UnihanParserService.ConvertToCodepoint(codepoint);
            // Parse field string to UnihanField enum (Unknown by default)
            _ = Enum.TryParse<UnihanField>(field, out var unihanField);
            // Invoke action with these values
            AddEntry(unicodeCodepoint, unihanField, value);
        }

        // Adds entry to an ansynchronous write buffer
        public void AddEntry(int codepoint, UnihanField field, string value)
        {
            if (_jsonBufferWriter == null)
            {
                var options = new JsonBufferWriterOptions { OutputPath = OutputPath };
                _jsonBufferWriter = new JsonBufferWriter<UnihanJsonEntry>(options);
            }
            var jsonEntry = new UnihanJsonEntry(codepoint, field, value);
            _jsonBufferWriter.AddEntry(jsonEntry);
        }

        public void Dispose()
        {
            _jsonBufferWriter?.Dispose();
        }

        private class UnihanJsonEntry
        {
            public UnihanJsonEntry(int codepoint, UnihanField field, string value)
            {
                Key = codepoint;
                Field = field;
                Value = value;
            }

            public int Key { get; }
            public UnihanField Field { get; }
            public string Value { get; }
        }
    }
}