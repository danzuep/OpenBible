using System.Reflection;
using System.Text.Json;
using Bible.Core.Models;

namespace Bible.Data
{
    public static class ResourceHelper
    {
        private const string Namespace = "Bible.Data";

        public static Stream GetUsjBookStream(BibleBookMetadata metadata)
        {
            var stream = GetBookStream(metadata.IsoLanguage, metadata.BibleVersion, metadata.BookCode, ".usj");
            return stream;
        }

        public static Stream GetUsxBookStream(BibleBookMetadata metadata)
        {
            var stream = GetBookStream(metadata.IsoLanguage, metadata.BibleVersion, metadata.BookCode, ".usx");
            return stream;
        }

        public static Stream GetUsxBookStream(string path)
        {
            var parts = path.Split(['-', '/', '\\']);
            var language = parts.FirstOrDefault()?.ToLowerInvariant();
            var translation = parts.Skip(1)?.FirstOrDefault()?.ToUpperInvariant();
            var book = parts.Skip(2)?.FirstOrDefault()?.ToUpperInvariant();
            var fileExtension = Path.GetExtension(path)?.ToLowerInvariant();
            var stream = GetBookStream(language, translation, book, fileExtension);
            return stream;
        }

        public static Stream GetBookStream(string? language, string? version, string? book, string? fileExtension = null)
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

        public static async Task<string> WriteJsonAsync<T>(T input, string fileName, bool normalizeFileName = true, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input), "Input cannot be null.");
            }

            var filePath = GetFilePath(fileName, normalizeFileName);

            await using var outputStream = new FileStream(filePath, FileMode.Create);
            await JsonSerializer.SerializeAsync(outputStream, input, options, cancellationToken);

            return filePath;
        }

        public static async Task<string> WriteStreamAsync(Stream inputStream, string fileName, bool normalizeFileName = true)
        {
            if (inputStream == null || inputStream.Length == 0)
            {
                throw new ArgumentNullException(nameof(inputStream), "Input stream cannot be null.");
            }

            var filePath = GetFilePath(fileName, normalizeFileName);

            // Save the file to the server's file system
            await using var fileStream = new FileStream(filePath, FileMode.Create);
            await inputStream.CopyToAsync(fileStream);
            inputStream.Position = 0; // Reset the position of the input stream

            return filePath;
        }

        private static string GetFilePath(string fileName, bool normalizeFileName = true)
        {
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
                var charsToReplace = Path.GetInvalidFileNameChars()
                    .Concat(['\\', ':']).ToArray();
                foreach (var c in charsToReplace)
                {
                    fileName = fileName.Replace(c, '_');
                }
                fileName = fileName.ToLowerInvariant();
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

            return filePath;
        }
    }
}