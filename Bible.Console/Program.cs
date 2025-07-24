namespace Bible.Console;

using System;
using System.Threading.Tasks;
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
        await DeserializelToHtmlAsync(logger);

        Console.WriteLine();
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    private static void MarkdownVisitorExample(ILogger logger)
    {
        XmlConverter.Visitor<UsxScriptureBook>((book, _) =>
        {
            var usxVisitor = UsxToMarkdownVisitor.Create(book);
            var text = usxVisitor.GetFullText();
            logger.LogInformation(text);
            return text;
        }, logger, isTest: true);
    }

    private static void HtmlVisitorExample(ILogger logger)
    {
        XmlConverter.Visitor<UsxScriptureBook>(static (book, _) =>
        {
            var usxVisitor = UsxToHtmlVisitor.Create(book);
            var text = usxVisitor.GetFullText();
            return text;
        }, logger);
    }

    private static async Task DeserializelToHtmlAsync(ILogger logger)
    {
        await XmlConverter.XmlMetadataToJsonAsync(logger);

        XmlConverter.Visitor<UsxScriptureBook>((book, outputPath) =>
        {
            var usxVisitor = UsxToHtmlVisitor.Create(book);
            var html = usxVisitor.GetFullText();
            var outFilePath = Path.Combine(outputPath, $"{book?.Translation.BookCode}.html");
            File.WriteAllText(outFilePath, html);
            logger.LogInformation(outFilePath);
            return html;
        }, logger);
    }
}