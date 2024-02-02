using Bible.Core.Models;
using Bible.Data.Models;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Bible.Data.Services
{
    public class SerializerService
    {
        private static readonly string _baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private const string _relativeDirectory = "..\\..\\..\\..\\Bible.Data\\";
        public static string GetJsonFilePath(string name, string prefix = $"{_relativeDirectory}Json", string suffix = ".json") =>
            Path.Combine(_baseDirectory, prefix, $"{name}{suffix}");

        /// <summary>
        /// Map the streamed data from a JSON file to an object.
        /// </summary>
        /// <typeparam name="T">Object to map to.</typeparam>
        /// <param name="jsonFileName">JSON file name.</param>
        /// <returns>Mapped object.</returns>
        public static async Task<T?> GetFromJsonFileAsync<T>(string jsonFileName)
        {
            var jsonFilePath = GetJsonFilePath(jsonFileName);
            using var fileStream = File.OpenRead(jsonFilePath);
            var result = await JsonSerializer.DeserializeAsync<T>(fileStream);
            return result;
        }

        /// <summary>
        /// Map the streamed data from a JSON resource to an object.
        /// </summary>
        /// <typeparam name="T">Object to map to.</typeparam>
        /// <param name="resourceName">JSON resource name.</param>
        /// <returns>Mapped object.</returns>
        public static async Task<T?> GetFromJsonResourceAsync<T>(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var fileStream = assembly.GetManifestResourceStream(resourceName);
            T? result = default;
            if (fileStream != null)
            {
                result = await JsonSerializer.DeserializeAsync<T>(fileStream);
            }
            return result;
        }

        public static async Task<BibleBook> GetBibleBookFromResourceAsync(string bibleTranslation = "KJV", string bookName = "John")
        {
            var resourceName = $"Bible.Data.Json.{bibleTranslation.ToLowerInvariant()}.json";
            var bible = await GetFromJsonResourceAsync<SimpleBibleBookJson[]>(resourceName) ?? [];
            var books = bible.Where(b => b.Name == bookName).Select(b => b.GetBibleBook(bibleTranslation));
            var book = books.Single();
            return book;
        }

        public static async Task<BibleModel> GetBibleFromResourceAsync(string bibleTranslation = "KJV")
        {
            var resourceName = $"Bible.Data.Json.{bibleTranslation.ToLowerInvariant()}.json";
            var bibleJson = await GetFromJsonResourceAsync<SimpleBibleBookJson[]>(resourceName) ?? [];
            var books = bibleJson.Select(b => b.GetBibleBook(bibleTranslation));
            var bible = new BibleModel() { Books = books.ToArray(), Information = new() { Translation = bibleTranslation } };
            return bible;
        }

        public static IEnumerable<BibleBook> GetBibleBooks(string version = "KJV")
        {
            var jsonFilePath = GetJsonFilePath(version);
            using var fileStream = File.OpenRead(jsonFilePath);
            var bible = JsonSerializer.Deserialize<SimpleBibleBookJson[]>(fileStream) ?? [];
            var books = bible.Select(b => b.GetBibleBook(version));
            return books;
        }

        private static async Task<JsonNode?> GetIndexedNodeFromFileAsync(string jsonFilePath, int index)
        {
            using var fileStream = File.OpenRead(jsonFilePath);
            var jsonNode = await JsonNode.ParseAsync(fileStream);
            var indexedNode = jsonNode == null ? null : jsonNode[index];
            return indexedNode;
        }

        private static string[][]? GetJaggedArrayFromJsonArray(JsonArray? chapters)
        {
            var result = chapters?.Select(c => (c as JsonArray)?.Select(v =>
                v?.ToString() ?? string.Empty).ToArray() ?? []).ToArray() ?? [];
            return result;
        }

        /// <summary>
        /// Map the streamed data from a JSON file to an object.
        /// </summary>
        /// <typeparam name="T">Object to map to.</typeparam>
        /// <param name="jsonFileName">JSON file name.</param>
        /// <returns>Mapped object.</returns>
        public static async Task<string?> GetChapterVerseFromFileAsync(string jsonFileName, int bookIndex, int chapter, int verse, string contentProperty = "Chapters")
        {
            var jsonFilePath = GetJsonFilePath(jsonFileName);
            var bookNode = await GetIndexedNodeFromFileAsync(jsonFilePath, bookIndex);
            var contentNode = bookNode == null ? null : bookNode[contentProperty];
            var verseNode = contentNode == null ? null : contentNode[chapter - 1]![verse - 1];
            var result = verseNode == null ? default : verseNode.GetValue<string>();
            return result;
        }

        /// <summary>
        /// Map the streamed data from a JSON file to an object.
        /// </summary>
        /// <param name="jsonFileName">JSON file name.</param>
        /// <returns>Mapped object.</returns>
        public static async Task<SimpleBibleBookJson> GetBibleBookFromStreamAsync(string jsonFileName, int bookIndex, string contentProperty = "Chapters", string nameProperty = "Name", string abbreviationProperty = "Abbreviation")
        {
            var jsonFilePath = GetJsonFilePath(jsonFileName);
            var bookNode = await GetIndexedNodeFromFileAsync(jsonFilePath, bookIndex);
            var nameValue = bookNode == null ? null : bookNode[nameProperty]?.GetValue<string>();
            var abbreviationValue = bookNode == null ? null : bookNode[abbreviationProperty]?.GetValue<string>();
            var contentNode = bookNode == null ? null : bookNode[contentProperty];
            var content = GetJaggedArrayFromJsonArray(contentNode as JsonArray);
            var result = new SimpleBibleBookJson
            {
                Name = nameValue ?? string.Empty,
                Abbreviation = abbreviationValue ?? string.Empty,
                Content = content ?? []
            };
            return result;
        }

        public static async Task<string?> GetPropertyFromStreamAsync(Stream stream, string property = "text")
        {
            var jsonNode = await JsonNode.ParseAsync(stream);
            var contentNode = jsonNode == null ? null : jsonNode[property];
            var result = contentNode == null ? default : contentNode.GetValue<string>();
            return result;
        }

        public static async Task<T?> GetFromDocumentStreamAsync<T>(Stream stream, string value, string property = "Name")
        {
            JsonDocumentOptions options = new() { AllowTrailingCommas = true };
            using var jsonDocument = await JsonDocument.ParseAsync(stream, options);
            var jsonElement = jsonDocument.RootElement.EnumerateArray()
                .FirstOrDefault(j => j.TryGetProperty(property, out JsonElement nameProperty) && nameProperty.ValueEquals(value));
            var result = JsonSerializer.Deserialize<T>(jsonElement.GetRawText());
            return default;
        }
    }
}
