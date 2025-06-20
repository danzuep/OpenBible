namespace Bible.Console;

using System;
using System.Threading.Tasks;
using Bible.Backend;
using Bible.Backend.Abstractions;
using Bible.Backend.Adapters;
using Bible.Backend.Models;
using Bible.Backend.Services;
using Microsoft.Extensions.Logging;

public class Program
{
    public static async Task Main()
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
            builder.AddDebug().AddConsole());
        var logger = loggerFactory.CreateLogger<Program>();

        //Example1();
        //Example2();
        //MarkdownExample(logger);
        //HtmlExample(logger);
        await ConvertToHtmlAsync();

        Console.WriteLine();
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    private static void Example1()
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
        var p = jude.Content.Where(b => b.Style == "p");
        var ignore = new string[] { "f" };
        var content = p.Where(c => !ignore.Contains(c.Style)).ToList();
        var text = content.Select(c => string.Join("", c)).ToList();

        //await downloadExtractor.DownloadExtractAsync(xmlFilePath, jsonOutputPath);
        //var jsonData = jsonParser.Deserialize<Usx>(jsonOutputPath);
        //var xmlData = xmlParser.Deserialize<Usx>(xmlFilePath);
    }

    private static void Example2()
    {
        var deserializer = new XDocParser();
        var book = deserializer.DeserializeXml<UsxScriptureBook>(BibleApiConstants.UsxWebbe3JN);
        var text = BibleParser.WriteToMarkdown(book);

        using var loggerFactory = LoggerFactory.Create(builder =>
            builder.AddDebug().AddConsole());
        var logger = loggerFactory.CreateLogger<Program>();
        //logger.LogInformation(text);

        var bibleBook = book?.ToBibleFormat();
        foreach (var chapter in bibleBook!.Chapters)
        {
            foreach (var verse in chapter.Verses)
            {
                logger.LogInformation(verse.Text);
            }
        }

        var html = book?.ToHtml();
        logger.LogInformation(html);
    }

    private static async Task Example2Async()
    {
        var deserializer = new XDocParser();
        var book = deserializer.DeserializeXml<UsxScriptureBook>(BibleApiConstants.UsxWebbe3JN);
        var html = book?.ToHtml();

        var docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var outputPath = Path.Combine(docPath, "3JN.html");
        using (var outputFile = new StreamWriter(outputPath))
        {
            await outputFile.WriteAsync(html);
        }
    }

    private static async Task WriteAllTextAsync(IDeserialize deserializer, string usxFilePath, string outputPath)
    {
        var book = deserializer.Deserialize<UsxScriptureBook>(usxFilePath);
        var html = book?.ToHtml();
        await File.WriteAllTextAsync(outputPath, html);
    }

    private static async Task MarkdownExample(ILogger logger)
    {
        var deserializer = new XDocParser();
        var book = deserializer.DeserializeXml<UsxScriptureBook>(BibleApiConstants.UsxWebbeMat5);
        logger.LogInformation(book?.ToMarkdown());
        //logger.LogInformation(book?.ToHtml());
    }

    private static async Task HtmlExample(ILogger logger)
    {
        var deserializer = new XDocParser();
        var book = deserializer.DeserializeXml<UsxScriptureBook>(BibleApiConstants.UsxWebbeMat5);
        logger.LogInformation(book?.ToHtml());
    }

    private static async Task ConvertToHtmlAsync(string translation = "WEBBE")
    {
        //var xmlFilePath = $"release/USX_4/{bookCode}.usx"; // Path to your USX XML file
        var usxFolderName = "7142879509583d59-rev146-2025-06-08-release";

        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var usxFilePath = Path.Combine(userProfile, "Downloads", usxFolderName);
        var outputPath = Path.Combine(userProfile, "Downloads", translation);
        Directory.CreateDirectory(outputPath);

        var deserializer = new XDocParser();
        var usxParser = new UsxParser(deserializer);
        var files = usxParser.Deserialize<UsxScriptureBook>(usxFilePath);

        foreach (var book in files)
        {
            var outFilePath = Path.Combine(outputPath, $"{book?.Translation.BookCode}.html");
            await File.WriteAllTextAsync(outFilePath, book?.ToHtml());
        }
    }
}