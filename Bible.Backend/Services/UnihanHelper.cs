using System.Text.Json;
using Bible.Data;
using Unihan.Models;
using Unihan.Services;

namespace Bible.Backend.Services
{
    public static class UnihanHelper
    {
        public static async Task<UnihanLookup?> GetUnihanAsync(string isoLanguage, string fileName = "Unihan_Readings.json")
        {
            UnihanLookup? unihan = null;
            if (UnihanLookup.ISO6393UnihanLookup.TryGetValue(isoLanguage, out var unihanField))
            {
                unihan = await ResourceHelper.GetFromJsonAsync<UnihanLookup>(fileName);
                if (unihan != null)
                {
                    unihan.IsoLanguage = isoLanguage;
                }
            }
            return unihan;
        }

        public static async Task ParseUnihanAsync()
        {
            (var sitePath, var assetPath) = XmlConverter.GetPaths();
            var inputPath = Path.Combine(sitePath, "Unihan_Readings.txt");
            var outputPath = Path.Combine(sitePath, "Unihan_Readings.json");
            var unihanSerializer = new UnihanSerializer();
            await unihanSerializer.ParseAsync(inputPath, outputPath);
        }

        public static async Task<UnihanLookup> LoadUnihanAsync()
        {
            (var sitePath, var assetPath) = XmlConverter.GetPaths();
            var inputPath = Path.Combine(sitePath, "Unihan_Readings.txt");
            var outputPath = Path.Combine(sitePath, "Unihan_Readings.json");
            if (File.Exists(outputPath))
            {
                var unihanText = await File.ReadAllTextAsync(outputPath);
                var deserialized = JsonSerializer.Deserialize<UnihanLookup>(unihanText);
                if (deserialized != null)
                {
                    return deserialized;
                }
            }
            var parser = new UnihanParserService();
            var unihan = await parser.ParseAsync<UnihanLookup>(inputPath, outputPath);
            return unihan;
        }
    }
}
