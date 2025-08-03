namespace Bible.Console;

using System;
using System.Text;
using System.Threading.Tasks;
using Bible.Backend.Models;
using Bible.Backend.Services;
using Bible.Backend.Visitors;
using Bible.Core.Models;
using Bible.Data;
using Microsoft.Extensions.Logging;

public class Program
{
    public static async Task Main()
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
            builder.SetMinimumLevel(LogLevel.Trace).AddDebug().AddConsole());
        var logger = loggerFactory.CreateLogger<Program>();

        //Sample.UnifiedScripture(logger);
        //LoadBible(loggerFactory);
        await LoadBibleBookAsync(loggerFactory);

        //var converter = new XmlConverter(logger);
        //await converter.ParseUnihanAsync();
        //var unihan = await converter.LoadUnihanAsync();
        //ScriptureBookVisitorExample(converter);
        //MarkdownVisitorExample(converter);
        //HtmlVisitorExample(converter, unihan);
        //await DeserializeToHtmlAsync(converter, unihan);

        Console.WriteLine();
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    private static async Task<BibleBook?> LoadBibleBookAsync(ILoggerFactory loggerFactory, string version = "eng-WEBBE", string bookName = "JHN")
    {
        var logger = loggerFactory.CreateLogger<XDocDeserializer>();
        var deserializer = new XDocDeserializer(logger);
        var usxParser = new UsxToBibleBookParser(deserializer);
        var stream = ResourceHelper.GetStream($"usx.{version}.{bookName}.usx");
        var bibleBook = await usxParser.ParseAsync(stream);
        var verse = bibleBook?.Chapters[0].Verses[0];
        logger.LogInformation(verse?.ToString());
        return bibleBook;
    }

    //private static BibleModel LoadBible(ILoggerFactory loggerFactory, string version = "eng-WEBBE")
    //{
    //    var deserializer = new XDocDeserializer(loggerFactory.CreateLogger<XDocDeserializer>());
    //    var usxParser = new UsxVersionParser(deserializer);
    //    var dataService = new UsxToBibleModelParser(usxParser);
    //    var bible = dataService.Load(version);
    //    return bible;
    //}

    private static void ScriptureBookVisitorExample(XmlConverter converter)
    {
        converter.Visitor<UsxBook>((bookModel, outputPath) =>
        {
            if (!bookModel.Translation.BookCode.Equals("3JN"))
            {
                return null!;
            }
            var book = UsxToScriptureBookVisitor.GetBook(bookModel);
            var textV = book.GetChapter(1).ToText();
            converter.Logger.LogInformation(textV);
            var text = book.Name;
            return text;
        }, sample: "eng-WEBBE");
    }

    private static void MarkdownVisitorExample(XmlConverter converter)
    {
        converter.Visitor<UsxBook>((book, outputPath) =>
        {
            var text = UsxToMarkdownVisitor.GetFullText(book);
            converter.Logger.LogInformation(text);
            return text;
        }, sample: "eng-WEBBE");
    }

    private static void HtmlVisitorExample(XmlConverter converter, UnihanLookup? unihan)
    {
        converter.Visitor<UsxBook>((book, outputPath) =>
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

        converter.Visitor<UsxBook>((book, outputPath) =>
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