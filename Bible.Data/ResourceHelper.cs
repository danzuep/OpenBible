using System.IO;
using System.Reflection;
using System.Text.Json;
using Bible.Core.Models;
using Unihan.Models;
using Unihan.Services;

namespace Bible.Data
{
    public static class ResourceHelper
    {
        private const string Namespace = "Bible.Data";

        public static Stream GetUsxBookStream(BibleBookMetadata metadata)
        {
            var stream = GetUsxBookStream(metadata.IsoLanguage, metadata.BibleVersion, metadata.BookCode);
            return stream;
        }

        public static Stream GetUsxBookStream(string path)
        {
            var parts = path.Split(['-', '/', '\\']);
            var language = parts.FirstOrDefault()?.ToLowerInvariant();
            var translation = parts.Skip(1)?.FirstOrDefault()?.ToUpperInvariant();
            var book = parts.Skip(2)?.FirstOrDefault()?.ToUpperInvariant();
            var fileExtension = Path.GetExtension(path)?.ToLowerInvariant();
            var stream = GetUsxBookStream(language, translation, book, fileExtension);
            return stream;
        }

        public static Stream GetUsxBookStream(string? language, string? version, string? book, string? fileExtension = null)
        {
            language = language ?? "eng";
            version = version?.ToUpperInvariant() ?? "WEBBE";
            fileExtension = string.IsNullOrEmpty(fileExtension) ? ".usx" : fileExtension.ToLowerInvariant();
            book = book?.ToUpperInvariant() ?? "JHN";
            if (book.EndsWith(fileExtension))
            {
                book = book[..^fileExtension.Length];
            }
            var resourcePath = $"{language}_{version}.{book}{fileExtension}";
            var stream = GetStreamFromExtension(resourcePath);
            return stream;
        }

        public static Stream GetStreamFromExtension(string filePath)
        {
            var fileExtension = Path.GetExtension(filePath);
            var fileName = filePath.Replace('\\', '.').Replace('/', '.').Replace('-', '_');
            var resourceName = $"{Namespace}{fileExtension}.{fileName}";
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                fileName = Path.GetFileName(filePath).Replace('-', '_');
                var resources = assembly.GetManifestResourceNames();
                var altNames = resources.Where(n => n.Contains(fileName, StringComparison.OrdinalIgnoreCase));
                var alt = altNames.FirstOrDefault();
                if (!string.IsNullOrEmpty(alt))
                {
                    stream = assembly.GetManifestResourceStream(alt);
                    if (stream != null)
                    {
                        throw new FileNotFoundException($"Resource '{resourceName}' found at: {alt}");
                    }
                    alt = string.Join("\n", altNames);
                }
                if (string.IsNullOrEmpty(alt))
                {
                    altNames = resources.Where(n => n.EndsWith(fileExtension));
                    alt = altNames.FirstOrDefault();
                }
                throw new FileNotFoundException($"Resource '{resourceName}' not found. Closest:\n{alt}");
            }

            return stream;
        }

        public static Stream? GetManifestResourceStream(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream(resourceName);
            return stream;
        }

        public static async Task<T?> GetFromJsonAsync<T>(string filePath, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
        {
            await using var inputStream = GetStreamFromExtension(filePath);
            return await JsonSerializer.DeserializeAsync<T>(inputStream, options, cancellationToken);
        }

        public static async Task<string> WriteStreamAsync(string fileName, Stream inputStream, bool normalizeFileName = true)
        {
            if (inputStream == null || inputStream.Length == 0)
            {
                throw new ArgumentNullException(nameof(inputStream), "Input stream cannot be null.");
            }
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));
            }

            // Define the path where the file will be saved on the server
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            if (normalizeFileName)
            {
                fileName = fileName.Replace('\\', '_').Replace('/', '_').ToLowerInvariant();
            }
            var filePath = Path.Combine(uploadsFolder, fileName);
            if (!normalizeFileName)
            {
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }

            // Save the file to the server's file system
            await using var fileStream = new FileStream(filePath, FileMode.Create);
            await inputStream.CopyToAsync(fileStream);
            inputStream.Position = 0; // Reset the position of the input stream

            return filePath;
        }

        public static async Task<string> ParseUnihanReadingsToFileAsync(string fileName = "Unihan_Readings", IUnihanParserService? unihanParserService = null)
        {
            unihanParserService ??= new UnihanParserService();
            await using var inputStream = ResourceHelper.GetStreamFromExtension($"{fileName}.txt");
            await using var outputStream = await unihanParserService.ProcessStreamAsync(inputStream);
            var filePath = await ResourceHelper.WriteStreamAsync($"{fileName}.json", outputStream);
            return filePath;
        }

        public static async Task<IList<string>> SplitUnihanReadingsToFilesAsync(string fileName = "Unihan_Readings")
        {
            await using var inputStream = GetStreamFromExtension($"{fileName}.txt");
            var unihan = await UnihanParserService.ParseAsync<UnihanFieldDictionary>(inputStream);

            var results = new List<string>();
            foreach (var kvp in unihan)
            {
                if (kvp.Value == null) continue;
                var result = await SplitAsync(kvp.Key, kvp.Value);
                results.Add(result);
            }
            return results;

            async Task<string> SplitAsync(UnihanField field, UnihanDictionary fieldMap, string prefix = "unihan_")
            {
                var outputStream = await SerializeAsync(fieldMap);
                var filePath = await WriteStreamAsync($"{prefix}{field}.json", outputStream, normalizeFileName: false);
                return filePath;

                //foreach (var fv in fieldMap)
                //{
                //    var codepoint = fv.Key;
                //    var values = fv.Value;
                //    if (values == null) continue;
                //}
            }
        }

        private static async Task<MemoryStream> SerializeAsync<T>(T target, CancellationToken cancellationToken = default)
        {
            var options = null as JsonSerializerOptions;
            var outputStream = new MemoryStream();
            await JsonSerializer.SerializeAsync(outputStream, target, options, cancellationToken);
            outputStream.Position = 0;
            return outputStream;
        }
    }
}