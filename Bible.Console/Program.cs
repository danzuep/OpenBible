namespace Bible.Console;

using System;
using System.Threading.Tasks;
using System.Xml.Linq;
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
        //MarkdownVisitorExample(logger);
        //HtmlVisitorExample(logger);
        //HtmlExample(logger);
        //await ConvertToHtmlAsync();
        //await DownloadToHtmlAsync();
        //await DeserializeToHtmlAsync();
        await DeserializeAllToHtmlAsync(logger);

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

    private static async Task MiniMarkdownExample(ILogger logger)
    {
        try
        {
            var deserializer = new XDocParser();
            var book = deserializer.DeserializeXml<UsxParaBaseTest>(BibleApiConstants.UsxWebbe3JN_1_1);
            var visitor = new UsxToMarkdownVisitor();
            visitor.Accept(book?.Para);
            var markdown = visitor.GetMarkdown();
            logger.LogInformation(markdown);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
        }
    }

    private static async Task MarkdownVisitorExample(ILogger logger)
    {
        try
        {
            var deserializer = new XDocParser();
            var book = deserializer.DeserializeXml<UsxScriptureBook>(BibleApiConstants.UsxWebbeMat5);
            var visitor = UsxToMarkdownVisitor.Create(book);
            var markdown = visitor.GetMarkdown();
            logger.LogInformation(markdown);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
        }
    }

    private static async Task HtmlVisitorExample(ILogger logger)
    {
        try
        {
            var deserializer = new XDocParser();
            var book = deserializer.DeserializeXml<UsxScriptureBook>(BibleApiConstants.UsxWebbe3JN);
            var visitor = UsxToHtmlVisitor.Create(book);
            logger.LogInformation(visitor.GetHtml());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
        }
    }

    private static async Task MarkdownExample(ILogger logger)
    {
        try
        {
            var deserializer = new XDocParser();
            var book = deserializer.DeserializeXml<UsxScriptureBook>(BibleApiConstants.UsxWebbeMat5);
            logger.LogInformation(book?.ToMarkdown());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
        }
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

    private static async Task DownloadToHtmlAsync(string translation = "OCCB")
    {
        // https://app.thedigitalbiblelibrary.org/entry/download_archive?id=a6e06d2c5b90ad89&license=42446&revision=5&type=release
        var usxFolderName = "a6e06d2c5b90ad89-rev5-2024-04-29-release";
        var extract = $"{translation}-a6e06d2c5b90ad89";

        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var downloadPath = Path.Combine(userProfile, "Downloads");
        var usxFilePath = Path.Combine(downloadPath, usxFolderName);
        var outputPath = Path.Combine(downloadPath, translation);
        var extractPath = Path.Combine(downloadPath, extract);

        //var downloadExtractor = new DownloadExtractor();
        //await downloadExtractor.DownloadExtractAsync(translation, usxFolderName);
        //ZipFile.ExtractToDirectory($"{usxFilePath}.zip", extractPath);

        Directory.CreateDirectory(outputPath);

        var deserializer = new XDocParser();
        var usxParser = new UsxParser(deserializer);
        var files = usxParser.Deserialize<UsxScriptureBook>(extractPath);

        foreach (var book in files)
        {
            var outFilePath = Path.Combine(outputPath, $"{book?.Translation.BookCode}.html");
            await File.WriteAllTextAsync(outFilePath, book?.ToHtml());
        }
    }

    private static async Task DeserializeToHtmlAsync(string name = "eng-WEBU", string usxFolderName = "9879dbb7cfe39e4d-rev139-2025-06-20-release")
    {
        var biblePath = GetBiblePath();
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var usxFilePath = Path.Combine(userProfile, "Downloads", usxFolderName);
        var outputPath = Path.Combine(biblePath, "texts", name);

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

    private static async Task DeserializeAllToOldHtmlAsync(ILogger logger, string suffix = "-usx")
    {
        var biblePath = GetBiblePath();
        var deserializer = new XDocParser();
        var usxParser = new UsxParser(deserializer);
        foreach (var usxPath in Directory.EnumerateDirectories(biblePath))
        {
            var folderName = Path.GetFileName(usxPath);
            if (string.IsNullOrEmpty(folderName) || !folderName.EndsWith(suffix))
            {
                continue;
            }
            var bibleVersion = folderName[..^suffix.Length];
            logger.LogInformation(bibleVersion);
            Directory.CreateDirectory(Path.Combine(biblePath, bibleVersion));

            var files = usxParser.Deserialize<UsxScriptureBook>(usxPath);
            var outputPath = Path.Combine(biblePath, bibleVersion);

            foreach (var book in files)
            {
                var outFilePath = Path.Combine(outputPath, $"{book?.Translation.BookCode}.html");
                await File.WriteAllTextAsync(outFilePath, book?.ToHtml());
                logger.LogInformation(outFilePath);
            }
        }
    }

    private static async Task DeserializeAllToHtmlAsync(ILogger logger, string suffix = "-usx")
    {
        var biblePath = GetBiblePath();
        var sitePath = Path.Combine(biblePath, "_site");
        var textsPath = Path.Combine(biblePath, "texts");
        logger.LogInformation(textsPath);

        var deserializer = new XDocParser();
        var usxParser = new UsxParser(deserializer);

        foreach (var versionPath in Directory.EnumerateDirectories(sitePath))
        {
            var suffixLength = versionPath.EndsWith(suffix) ? suffix.Length : 0;
            var versionName = Path.GetFileName(versionPath)[..^suffixLength];
            logger.LogInformation(versionName);

            var outputPath = Path.Combine(textsPath, versionName);
            Directory.CreateDirectory(outputPath);

            var files = usxParser.Deserialize<UsxScriptureBook>(versionPath);

            foreach (var book in files)
            {
                //var usxToMarkdownVisitor = UsxToMarkdownVisitor.Create(book);
                //logger.LogInformation(usxToMarkdownVisitor.GetMarkdown());
                var outFilePath = Path.Combine(outputPath, $"{book?.Translation.BookCode}.html");
                var usxToHtmlVisitor = UsxToHtmlVisitor.Create(book);
                await File.WriteAllTextAsync(outFilePath, usxToHtmlVisitor.GetHtml());
                logger.LogInformation(outFilePath);
            }
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