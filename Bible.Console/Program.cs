namespace Bible.Console;

using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Net.Security;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Bible.Backend.Adapters;
using Bible.Backend.Models;
using Bible.Backend.Services;
using Bible.Backend.Visitors;
using Bible.Core.Models;
using Bible.Core.Models.Meta;
using Bible.Core.Models.Scripture;
using Bible.Data;
using Bible.Data.Services;
using Bible.Usx.Models;
using Bible.Usx.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Unihan.Models;
using Unihan.Services;

public class Program
{
    public static async Task Main()
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
            builder.SetMinimumLevel(LogLevel.Trace).AddDebug().AddConsole());
        var logger = loggerFactory.CreateLogger<Program>();

        //await PaginatedSearch.DemoAsync();
        //await ParseUnihanReadingsToDictionaryAsync(logger);
        //await SplitUnihanReadingsToFilesAsync(logger);
        //await ParseToFileAsync(logger);
        //await ParseFromFileAsync(char.ConvertFromUtf32(23383));
        //Sample.UnifiedScripture(logger);
        //LoadBible(loggerFactory);
        //await LoadBibleBookAsync(loggerFactory);
        //await ParseScriptureBookAsync(logger);
        //await ParseBibleBookAsync(logger);
        //await ParseToUnihanAsync();
        //await ParseAsync(logger);
        await DeserializeToUsjAsync(logger);
        //await DeserializeOneToUsjAsync(logger);

        //var converter = new XmlConverter(logger);
        //await converter.ParseUnihanAsync();
        //var unihan = await converter.LoadUnihanAsync();
        //ScriptureBookVisitorExample(converter);
        //MarkdownVisitorExample(converter);
        //HtmlVisitorExample(converter, unihan);
        //await DeserializeToHtmlAsync(converter, unihan);

        Console.WriteLine();
        //Console.WriteLine("Press any key to exit...");
        //Console.ReadKey();
    }

    static async Task ParseToUnihanAsync(UnihanField unihanField = UnihanField.kCantonese, string text = "中文和合本 繁體中文版連史特朗經文滙篇")
    {
        var unihanMetadata = await UnihanService.ParseAsync(text, unihanField);
        Debug.WriteLine(unihanMetadata.ToString());
    }

    //static async Task ParseUnihanRunesAsync(string isoLanguage = "cmn", string text = "中文和合本 繁體中文版連史特朗經文滙篇")
    //{
    //    var unihanService = new UnihanService(new InMemoryStorageService(), null);
    //    var unihanMetadata = await unihanService.ParseUnihanRunesAsync(text, isoLanguage);
    //    Debug.WriteLine(unihanMetadata.ToString());
    //}

    static async Task ParseAsync(ILogger logger, string language = "zho-Hant", string version = "OCCB", string book = "3JN")
    {
        var unihan = await UnihanService.GetUnihanAsync(language, dictionary: true);
        var unihanDictionary = unihan.Dictionary ?? new();
        var bibleBookService = new BibleBookService(logger);
        await using var stream = ResourceHelper.GetBookStream(language, version, book, ".usx");
        var usxParserFactory = new UsxParserFactory();
        usxParserFactory.SetTextParser(unihanDictionary.GetValue);
        var converter = new UsxToUsjConverter(usxParserFactory);
        var usjBook = await converter.ConvertUsxStreamToUsjBookAsync(stream);
        //logger.LogInformation(UsjToMarkdownVisitor.GetFullText(usjBook));
        var path = string.Join("/", [language, version, book]);
        var filePath = await ResourceHelper.WriteJsonAsync(usjBook, $"{path}.json");
        logger.LogInformation("{Path} created successfully.", filePath);
    }

    static async Task<BibleBook?> ParseBibleBookAsync(ILogger logger, string language = "zho-Hant", string version = "OCCB", string book = "3JN")
    {
        var bibleBookService = new BibleBookService(logger);
        var bibleBook = await bibleBookService.GetBibleBookAsync(language, version, book);
        if (bibleBook != null)
        {
            //var unihan = await UnihanService.GetUnihanAsync(language);
            logger.LogInformation(bibleBook.GetMarkdown());
            //logger.LogInformation(bibleBook.GetHtml());
            //var path = string.Join("/", [language, version, book]);
            //var filePath = await ResourceHelper.WriteJsonAsync(bibleBook, $"{path}.json");
            //logger.LogInformation("{Path} created successfully.", filePath);
        }
        return bibleBook;
    }

    private static async Task<ScriptureBook?> ParseScriptureBookAsync(ILogger logger, string path = "zho-Hant-OCCB/3JN.usx") // "eng/webbe/3jn"
    {
        (var unihan, var options) = await TryGetUnihanOptionsAsync(path, dictionary: true);
        await using var stream = ResourceHelper.GetStreamFromExtension(path);
        var scriptureBook = await UsxToScriptureBookVisitor.DeserializeAsync(stream, unihan, options);
        if (scriptureBook != null)
        {
            //logger.LogInformation(scriptureBook.ToMarkdownChapter(1));
            //var result = scriptureBook.ToChapterDto(1);
            var serializedStream = await scriptureBook.SerializeAsync().ConfigureAwait(false);
            var filePath = await ResourceHelper.WriteStreamAsync(serializedStream, $"{path}.json");
            logger.LogInformation("{Path} created successfully.", filePath);
        }
        return scriptureBook;
    }

    private static async Task<(UnihanLanguage?, UsxVisitorOptions?)> TryGetUnihanOptionsAsync(string path, bool dictionary = false)
    {
        var isoLanguage = string.Join("-", path.Split('-').SkipLast(1));
        var unihan = await UnihanService.GetUnihanAsync(isoLanguage, dictionary);
        var options = new UsxVisitorOptions { EnableRunes = unihan?.Field };
        return (unihan, options);
    }

    private static async Task ScriptureBookVisitorAsync(XmlConverter converter, string version = "eng-WEBBE")
    {
        (var unihan, var options) = await TryGetUnihanOptionsAsync(version);
        converter.Visitor<UsxBook>((bookModel, outputPath) =>
        {
            if (!bookModel.Metadata.BookCode.Equals("3JN"))
            {
                return null!;
            }
            var book = UsxToScriptureBookVisitor.GetBook(bookModel, unihan, options);
            var textV = book.GetChapter(1).ToMarkdown();
            converter.Logger.LogInformation(textV);
            var text = book.Metadata.Name;
            return text;
        }, sample: version);
    }

    private static void ScriptureBookVisitorExample(XmlConverter converter)
    {
        converter.Visitor<UsxBook>((bookModel, outputPath) =>
        {
            if (!bookModel.Metadata.BookCode.Equals("3JN"))
            {
                return null!;
            }
            var book = UsxToScriptureBookVisitor.GetBook(bookModel);
            var textV = book.GetChapter(1).ToMarkdown();
            converter.Logger.LogInformation(textV);
            var text = book.Metadata.Name;
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

    private static void HtmlVisitorExample(XmlConverter converter, UnihanLanguage? unihan)
    {
        converter.Visitor<UsxBook>((book, outputPath) =>
        {
            if (!book.Metadata.BookCode.Equals("3JN"))
            {
                return null!;
            }
            if (unihan != null)
            {
                var fileName = Path.GetFileNameWithoutExtension(outputPath);
                var langScript = string.Join("-", fileName.Split('-').SkipLast(1));
                //unihan.IsoLanguage = langScript;
            }
            var text = UsxToHtmlVisitor.GetFullText(book, unihan);
            var outFilePath = Path.Combine(outputPath, $"{book?.Metadata.BookCode}.html");
            File.WriteAllText(outFilePath, text, Encoding.UTF8);
            converter.Logger.LogInformation(outFilePath);
            return text;
        }, sample: "zho-Hant-OCCB");
    }

    private static async Task DeserializeToHtmlAsync(XmlConverter converter, UnihanLanguage? unihan)
    {
        await converter.XmlMetadataToJsonAsync();

        converter.Visitor<UsxBook>((book, outputPath) =>
        {
            if (unihan != null)
            {
                var fileName = Path.GetFileNameWithoutExtension(outputPath);
                var langScript = string.Join("-", fileName.Split('-').SkipLast(1));
                //unihan.IsoLanguage = langScript;
            }
            var html = UsxToHtmlVisitor.GetFullText(book, unihan);
            var outFilePath = Path.Combine(outputPath, $"{book?.Metadata.BookCode}.html");
            File.WriteAllText(outFilePath, html, Encoding.UTF8);
            converter.Logger.LogInformation(outFilePath);
            return html;
        });
    }

    private static async Task DeserializeOneToUsjAsync(ILogger logger, string language = "zho-Hant", string version = "OCCB", string book = "3JN")
    {
        var metadata = new BibleBookMetadata
        {
            IsoLanguage = language,
            BibleVersion = version,
            BookCode = book
        };
        var usjBook = await UsjBookService.SerializeBookAsync(metadata, logger);
    }

    private static async Task DeserializeToUsjAsync(ILogger logger)
    {
        var deserializer = new XmlReaderDeserializer(logger);
        await deserializer.ParseVisitor("eng-WEBBE");
    }

    private static async Task<BibleBook?> LoadBibleBookAsync(ILoggerFactory loggerFactory, string version = "eng-WEBBE", string bookName = "JHN")
    {
        var logger = loggerFactory.CreateLogger<XDocDeserializer>();
        var deserializer = new XDocDeserializer(logger);
        var usxParser = new UsxToBibleBookParser(deserializer);
        await using var stream = ResourceHelper.GetStreamFromExtension($"{version}.{bookName}.usx");
        var bibleBook = await usxParser.ParseAsync(stream);
        var verse = bibleBook?.Chapters[0].Verses[0];
        logger.LogInformation(verse?.ToString());
        return bibleBook;
    }

    private static async Task<string> ParseUnihanReadingsFromFileAsync(string text)
    {
        var unihanLookup = await ResourceHelper.GetFromJsonAsync<UnihanLookup>("Unihan_Readings.json");
        return ParseToString(text, unihanLookup);
    }

    private static async Task ParseUnihanReadingsToFileAsync(ILogger logger)
    {
        var filePath = await UnihanFileService.ParseUnihanReadingsToFileAsync();
        logger.LogInformation("{Path} created successfully.", filePath);
    }

    private static async Task ParseUnihanReadingsToDictionaryAsync(ILogger logger)
    {
        var filePath = await UnihanFileService.ParseUnihanReadingsToDictionaryAsync();
        logger.LogInformation("{Path} created successfully.", filePath);
    }

    private static async Task SplitUnihanReadingsToFilesAsync(ILogger logger)
    {
        var unihanFileService = new UnihanFileService(logger);
        await unihanFileService.SplitUnihanReadingsToFilesAsync();
    }

    private static async Task ParseScriptureBookToFileAsync(ILogger logger, string path = "eng/webbe/3jn") // "zho-Hant-OCCB/3JN.usx"
    {
        await using var inputStream = ResourceHelper.GetUsxBookStream(path);
        var scriptureBook = await UsxToScriptureBookVisitor.DeserializeAsync(inputStream);
        await using var outputStream = new MemoryStream();
        await JsonSerializer.SerializeAsync(outputStream, scriptureBook);
        outputStream.Position = 0;
        var jsonPath = path.ToLowerInvariant() + ".json";
        var filePath = await ResourceHelper.WriteStreamAsync(outputStream, jsonPath);
        logger.LogInformation("{Path} created successfully.", filePath);
    }

    private static string ParseToString(string text, UnihanLookup? unihanLookup)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        var stringBuilder = new StringBuilder();
        foreach (Rune rune in text.EnumerateRunes())
        {
            stringBuilder.AppendLine($"Rune: {rune.ToString()} ({rune.Value})");
            if (unihanLookup != null && unihanLookup.TryGetValue(rune.Value, out var metadata))
            {
                foreach (var kvp in metadata)
                {
                    stringBuilder.AppendLine($"{kvp.Key}: {string.Join("; ", kvp.Value)}");
                }
            }
        }
        return stringBuilder.ToString();
    }
}