namespace Bible.Console;

using System;
using System.Threading.Tasks;
using Bible.Backend;
using Microsoft.Extensions.Logging;

public class Program
{
    public static async Task Main()
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
            builder.AddDebug().AddConsole());
        var jsonLogger = loggerFactory.CreateLogger<JsonParser>();
        var jsonParser = new JsonParser(jsonLogger);
        var xmlLogger = loggerFactory.CreateLogger<XmlParser>();
        var xmlParser = new XmlParser(xmlLogger);
        var downloadLogger = loggerFactory.CreateLogger<DownloadExtractor>();
        var downloadExtractor = new DownloadExtractor(null, downloadLogger);
        var usxParser = new UsxParser(xmlParser);

        string xmlFilePath = "release/USX_4/JUD.usx"; // Path to your USX XML file
        string jsonOutputPath = "JUD.json"; // Path to save the JSON output
        var usxFileName = "7142879509583d59-rev146-2025-06-08-release";
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var usxFilePath = Path.Combine(userProfile, "Downloads", usxFileName);
        var files = usxParser.Deserialize<UsxScriptureBook>(usxFilePath).ToList();
        var jude = files.Single(f => f.Translation.BookCode == "JUD");
        var p = jude.ParagraphContent.Where(b => b.Style == "p");
        var ignore = new string[] { "f" };
        var content = p.Where(c => !ignore.Contains(c.Style)).ToList();
        var text = content.Select(c => string.Join("", c)).ToList();

        //await downloadExtractor.DownloadExtractAsync(xmlFilePath, jsonOutputPath);
        //var jsonData = jsonParser.Deserialize<Usx>(jsonOutputPath);
        //var xmlData = xmlParser.Deserialize<Usx>(xmlFilePath);

        Console.WriteLine();
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}