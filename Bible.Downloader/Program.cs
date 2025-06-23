namespace Bible.Downloader;

using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

class Program
{
    static async Task Main()
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
            builder.AddDebug().AddConsole());
        var logger = loggerFactory.CreateLogger<Program>();

        //await DownloadExtractAllAsync(logger);
        await XmlMetadataToJsonAsync(logger);
    }

    private static async Task DownloadExtractAllAsync(ILogger logger)
    {
        var biblePath = GetBiblePath();
        var json = File.ReadAllText(Path.Combine(biblePath, "data.json"));

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var rootObject = JsonSerializer.Deserialize<MetadataRoot>(json, options);

        if (rootObject != null)
        {
            var downloadExtractor = new DownloadExtractor(DigitalBibleLibraryConstants.HttpClient);

            foreach (var kvp in rootObject.Metadata)
            {
                var extractPath = Path.Combine(biblePath, "texts", $"{kvp.Value.Path}");
                if (Directory.Exists(extractPath))
                {
                    continue;
                }
                var downloadUrl = DigitalBibleLibraryConstants.GetDowloadUrl(kvp.Key, kvp.Value.LicenseId);

                Debug.WriteLine($"GET {downloadUrl}");
                Debug.WriteLine($"Name: {kvp.Value.Name}");
                Console.WriteLine($"Name: {kvp.Value.Name}");
                Console.WriteLine($"GET {downloadUrl}");
                Console.WriteLine($"Dowloading to: {extractPath}");
                Console.WriteLine("...");

                await downloadExtractor.DownloadExtractZipAsync(downloadUrl, extractPath);
            }
        }
    }

    private static async Task XmlMetadataToJsonAsync(ILogger logger, string suffix = "-usx")
    {
        var biblePath = GetBiblePath();
        var sitePath = Path.Combine(biblePath, "_site");
        var textsPath = Path.Combine(biblePath, "texts");
        logger.LogInformation(textsPath);

        foreach (var versionPath in Directory.EnumerateDirectories(sitePath))
        {
            var suffixLength = versionPath.EndsWith(suffix) ? suffix.Length : 0;
            var versionName = Path.GetFileName(versionPath)[..^suffixLength];
            logger.LogInformation(versionName);

            var xmlFilePath = Directory.EnumerateFiles(versionPath, "metadata.xml").FirstOrDefault();
            if (xmlFilePath == null)
            {
                return;
            }
            logger.LogInformation(xmlFilePath);

            var textVersionPath = Path.Combine(textsPath, versionName);
            logger.LogInformation(textVersionPath);

            var outputPath = Directory.Exists(textVersionPath) ?
                Path.Combine(textVersionPath, "_metadata.json") :
                $"{textVersionPath}_metadata.json";

            await XmlJsonConverter.ConvertXmlToJsonAsync(xmlFilePath, outputPath, omitRootObject: false);
        }
    }

    private static string GetBiblePath(string thisProject = "OpenBible", string bibleProjectName = "Bible")
    {
        do
        {
            thisProject = Path.Combine("..", thisProject);
        }
        while (!Directory.Exists(thisProject));

        var biblePath = Path.Combine(thisProject, "..", bibleProjectName);

        return biblePath;
    }
}