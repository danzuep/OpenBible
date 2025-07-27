namespace Bible.Console;

using System;
using System.Text;
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
        var converter = new XmlConverter(logger);

        //await converter.ParseUnihanAsync();
        var unihan = await converter.LoadUnihanAsync();

        //MarkdownVisitorExample(converter);
        //HtmlVisitorExample(converter, unihan);
        await DeserializeToHtmlAsync(converter, unihan);

        Console.WriteLine();
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    private static void MarkdownVisitorExample(XmlConverter converter)
    {
        converter.Visitor<UsxScriptureBook>((book, outputPath) =>
        {
            var text = UsxToMarkdownVisitor.GetFullText(book);
            converter.Logger.LogInformation(text);
            return text;
        }, sample: "eng-WEBBE");
    }

    private static void HtmlVisitorExample(XmlConverter converter, UnihanLookup? unihan)
    {
        converter.Visitor<UsxScriptureBook>((book, outputPath) =>
        {
            if (!book.Translation.BookCode.Equals("3JN"))
            {
                return null!;
            }
            if (unihan != null)
            {
                var fileName = Path.GetFileNameWithoutExtension(outputPath);
                var langScript = string.Join("-", fileName.Split('-').SkipLast(1));
                if (UnihanLookup.NameUnihanLookup.TryGetValue(langScript, out var unihanFields))
                {
                    unihan.Field = unihanFields.FirstOrDefault();
                }
            }
            var text = UsxToHtmlVisitor.GetFullText(book, unihan);
            var outFilePath = Path.Combine(outputPath, $"{book?.Translation.BookCode}.html");
            File.WriteAllText(outFilePath, text, Encoding.UTF8);
            converter.Logger.LogInformation(outFilePath);
            return text;
        }, sample: "zho-Hant-OCCB");
    }

    private static async Task DeserializeToHtmlAsync(XmlConverter converter, UnihanLookup? unihan)
    {
        await converter.XmlMetadataToJsonAsync();

        converter.Visitor<UsxScriptureBook>((book, outputPath) =>
        {
            if (unihan != null)
            {
                var fileName = Path.GetFileNameWithoutExtension(outputPath);
                var langScript = string.Join("-", fileName.Split('-').SkipLast(1));
                if (UnihanLookup.NameUnihanLookup.TryGetValue(langScript, out var unihanFields))
                {
                    unihan.Field = unihanFields.FirstOrDefault();
                }
            }
            var html = UsxToHtmlVisitor.GetFullText(book, unihan);
            var outFilePath = Path.Combine(outputPath, $"{book?.Translation.BookCode}.html");
            File.WriteAllText(outFilePath, html, Encoding.UTF8);
            converter.Logger.LogInformation(outFilePath);
            return html;
        });
    }
}