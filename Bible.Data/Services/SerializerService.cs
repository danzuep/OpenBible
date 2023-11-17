using Bible.Data.Models;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Bible.Data.Services
{
    public class SerializerService
    {
        /// <summary>
        /// Map the streamed data from a JSON file to an object.
        /// </summary>
        /// <typeparam name="T">Object to map to.</typeparam>
        /// <param name="jsonFilePath">JSON file path.</param>
        /// <returns>Mapped object.</returns>
        public static async Task<T?> GetFromFileAsync<T>(string jsonFilePath)
        {
            using var fileStream = File.OpenRead(jsonFilePath);
            var result = await JsonSerializer.DeserializeAsync<T>(fileStream);
            return result;
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
        /// <param name="jsonFilePath">JSON file path.</param>
        /// <returns>Mapped object.</returns>
        public static async Task<string?> GetChapterVerseFromFileAsync(string jsonFilePath, int bookIndex, int chapter, int verse, string contentProperty = "Chapters")
        {
            var bookNode = await GetIndexedNodeFromFileAsync(jsonFilePath, bookIndex);
            var contentNode = bookNode == null ? null : bookNode[contentProperty];
            var verseNode = contentNode == null ? null : contentNode[chapter - 1]![verse - 1];
            var result = verseNode == null ? default : verseNode.GetValue<string>();
            return result;
        }

        /// <summary>
        /// Map the streamed data from a JSON file to an object.
        /// </summary>
        /// <param name="jsonFilePath">JSON file path.</param>
        /// <returns>Mapped object.</returns>
        public static async Task<SimpleBibleBookJson> GetBibleBookFromStreamAsync(string jsonFilePath, int bookIndex, string contentProperty = "Chapters", string nameProperty = "Name", string abbreviationProperty = "Abbreviation")
        {
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
