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
        var unihan = await converter.ConvertUnihanAsync();
        //var unihan = await converter.GetUnihanAsync();

        //MarkdownVisitorExample(converter);
        HtmlVisitorExample(converter, unihan);
        //await DeserializelToHtmlAsync(converter);

        Console.WriteLine();
        //Console.WriteLine("Press any key to exit...");
        //Console.ReadKey();
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
            if (unihan != null)
            {
                var fileName = Path.GetFileNameWithoutExtension(outputPath);
                var langScript = string.Join("-", fileName.Split('-').SkipLast(1));
                if (UnihanLookup.NameUnihanLookup.TryGetValue(langScript, out var fields))
                {
                    var unihanField = fields.First();
                    unihan.Field = unihanField;
                }
            }
            var text = UsxToHtmlVisitor.GetFullText(book, unihan);
            var outFilePath = Path.Combine(outputPath, $"{book?.Translation.BookCode}.html");
            File.WriteAllText(outFilePath, text, Encoding.UTF8);
            converter.Logger.LogInformation(outFilePath);
            return text;
        }, sample: "zho-Hant-OCCB");
    }

    private static async Task DeserializelToHtmlAsync(XmlConverter converter, UnihanLookup? unihan)
    {
        await converter.XmlMetadataToJsonAsync();

        converter.Visitor<UsxScriptureBook>((book, outputPath) =>
        {
            if (unihan != null)
            {
                var fileName = Path.GetFileNameWithoutExtension(outputPath);
                var langScript = string.Join("-", fileName.Split('-').SkipLast(1));
                if (UnihanLookup.NameUnihanLookup.TryGetValue(langScript, out var fields))
                {
                    var unihanField = fields.First();
                    unihan.Field = unihanField;
                }
                else
                {
                    unihan = null;
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