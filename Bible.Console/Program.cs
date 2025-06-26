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
        await DeserializeAllToHtmlAsync(logger);

        Console.WriteLine();
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
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