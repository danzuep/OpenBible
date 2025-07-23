namespace Bible.Console;

using System;
using System.Threading.Tasks;
using Bible.Backend;
using Bible.Backend.Models;
using Bible.Backend.Services;
using Bible.Backend.Visitors;
using Microsoft.Extensions.Logging;

public class Program
{
    public static async Task Main()
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
            builder.AddDebug().AddConsole());
        var logger = loggerFactory.CreateLogger<Program>();

        //MarkdownVisitorExample(logger);
        //HtmlVisitorExample(logger);
        DeserializelToHtml(logger);

        Console.WriteLine();
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    private static void Visitor<T>(Func<T,string,string> function, ILogger logger, string? suffix = null, bool isTest = false)
    {
        var biblePath = GetBiblePath();
        var sitePath = Path.Combine(biblePath, "_site");
        var textsPath = Path.Combine(biblePath, "texts");
        logger.LogInformation(textsPath);

        var deserializer = new XDocParser();
        var usxParser = new UsxParser(deserializer);

        foreach (var versionPath in Directory.EnumerateDirectories(sitePath))
        {
            var suffixLength = !string.IsNullOrEmpty(suffix) && versionPath.EndsWith(suffix) ? suffix.Length : 0;
            var versionName = Path.GetFileName(versionPath)[..^suffixLength];
            logger.LogInformation(versionName);
            if (!versionName.StartsWith("eng-WEBBE"))
            {
                continue;
            }

            var outputPath = Path.Combine(textsPath, versionName);

            var versions = usxParser.DeserializeAll<T>(versionPath);

            foreach (var version in versions)
            {
                var key = Path.GetFileName(version.Key);
                var pathWithKey = Path.Combine(outputPath, key);
                if (!isTest)
                {
                    Directory.CreateDirectory(pathWithKey);
                }
                foreach (var book in version.Value)
                {
                    var text = function(book, pathWithKey);
                    if (isTest)
                    {
                        logger.LogInformation($"{versionName}-{key}-{book}");
                        return;
                    }
                }
            }
        }
    }

    private static void MarkdownVisitorExample(ILogger logger)
    {
        Visitor<UsxScriptureBook>((book, _) =>
        {
            var usxVisitor = UsxToMarkdownVisitor.Create(book);
            var text = usxVisitor.GetFullText();
            logger.LogInformation(text);
            return text;
        }, logger, isTest: true);
    }

    private static void HtmlVisitorExample(ILogger logger)
    {
        Visitor<UsxScriptureBook>(static (book, _) =>
        {
            var usxVisitor = UsxToHtmlVisitor.Create(book);
            var text = usxVisitor.GetFullText();
            return text;
        }, logger);
    }

    private static void DeserializelToHtml(ILogger logger)
    {
        Visitor<UsxScriptureBook>((book, outputPath) =>
        {
            var usxVisitor = UsxToHtmlVisitor.Create(book);
            var html = usxVisitor.GetFullText();
            var outFilePath = Path.Combine(outputPath, $"{book?.Translation.BookCode}.html");
            File.WriteAllText(outFilePath, html);
            logger.LogInformation(outFilePath);
            return html;
        }, logger);
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