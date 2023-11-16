using Bible.Data.Models;
using System.Text.Json;

namespace Bible.Data.Services
{
    internal class BibleBookService
    {
        /// <Summary>Get bible books, chapters and verses in JSON format</Summary>:
        public static async Task<IReadOnlyList<JsonSimpleBibleBook>> GetBibleFromJson(string jsonFilePath)
        {
            using FileStream fs = File.OpenRead(jsonFilePath);
            var books = await JsonSerializer.DeserializeAsync<JsonSimpleBibleBook[]>(fs);
            return books ?? Array.Empty<JsonSimpleBibleBook>();
        }
    }
}
