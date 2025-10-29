using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;
using Bible.Backend.Services;
using Bible.Backend.Visitors;
using Bible.Core.Models;
using Bible.Core.Models.Scripture;
using Bible.Data;
using Bible.Razor.Models;
using Microsoft.Extensions.Logging;
using Unihan.Models;

public static class Examples
{
    static async IAsyncEnumerable<ScriptureSegmentDto> GetScriptureRecordsStreamAsync(string language, string version, string book, byte chapter, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var scriptureBook = await ParseScriptureBookAsync(language, version, book);
        if (scriptureBook == null)
        {
            yield break;
        }
        yield return scriptureBook.Metadata.ToDto();
        var segments = scriptureBook.IndexManager.GetChapter(chapter);
        foreach (var segment in segments.ToImmutableArray())
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return segment.ToDto();
        }
    }

    static async Task<ScriptureRange?> ParseScriptureBookChapterAsync(string language, string version, string book, byte chapter)
    {
        //language = "zho-Hant"; version = "OCCB"; book = "3JN"; chapter = 1;
        var scriptureBook = await ParseScriptureBookAsync(language, version, book);
        var scriptureRange = scriptureBook?.ToChapterDto(chapter);
        return scriptureRange;
    }

    static async Task<ScriptureRange?> ParseScriptureBookDtoAsync(string language, string version, string book)
    {
        var scriptureBook = await ParseScriptureBookAsync(language, version, book);
        var scriptureRange = scriptureBook?.ToDto();
        return scriptureRange;
    }

    static async Task<ScriptureBookMetadata?> ParseScriptureBookMetadataAsync(string language, string version, string book, byte chapter)
    {
        var scriptureBook = await ParseScriptureBookAsync(language, version, book);
        return scriptureBook?.Metadata;
    }

    static async Task<ScriptureBook?> ParseScriptureBookAsync(string isoLanguage, string version, string book)
    {
        var unihan = await UnihanService.GetUnihanAsync(isoLanguage);
        await using var stream = GetUsxBookStream(isoLanguage, version, book);
        var scriptureBook = await UsxToScriptureBookVisitor.DeserializeAsync(stream, unihan);
        if (scriptureBook != null)
        {
            scriptureBook.Metadata.IsoLanguage = isoLanguage;
        }
        return scriptureBook;
    }

    static async Task<ScriptureBook?> ParseScriptureBookAsync(string path)
    {
        var isoLanguage = string.Join("-", path.Split('-').SkipLast(1));
        var unihan = await UnihanService.GetUnihanAsync(isoLanguage);
        await using var stream = ResourceHelper.GetStreamFromExtension(path);
        var scriptureBook = await UsxToScriptureBookVisitor.DeserializeAsync(stream, unihan);
        return scriptureBook;
    }

    static async Task<ScriptureBook?> ParseScriptureBookAsync(ILogger logger, string path = "zho-Hant-OCCB/3JN.usx") // "eng-webbe/3jn"
    {
        var isoLanguage = string.Join("-", path.Split('-').SkipLast(1));
        var unihan = await UnihanService.GetUnihanAsync(isoLanguage);
        await using var stream = ResourceHelper.GetStreamFromExtension(path);
        var scriptureBook = await UsxToScriptureBookVisitor.DeserializeAsync(stream, unihan);
        if (scriptureBook != null)
        {
            logger.LogInformation(scriptureBook.ToMarkdown());
            //var result = scriptureBook.ToChapterDto(1);
        }
        return scriptureBook;
    }

    static async Task<BibleBook?> ParseBibleBookAsync(string language = "eng", string version = "WEBBE", string book = "JHN")
    {
        await using var stream = GetUsxBookStream(language, version, book);
        var deserializer = new XDocDeserializer();
        var usxParser = new UsxToBibleBookParser(deserializer);
        var bibleBook = await usxParser.ParseAsync(stream);
        return bibleBook;
    }

    static Stream GetUsxBookStream(string? language, string? version, string? book)
    {
        if (string.IsNullOrEmpty(language) || string.IsNullOrEmpty(version) || string.IsNullOrEmpty(book))
        {
            throw new ArgumentException("Language, version, and book parameters must be provided.");
        }
        var metadata = new BibleBookMetadata
        {
            IsoLanguage = language,
            BibleVersion = version,
            BookCode = book
        };
        return ResourceHelper.GetUsxBookStream(metadata);
    }

    static async Task<string> ParseAsync(string text)
    {
        var unihanLookup = await ResourceHelper.GetFromJsonAsync<UnihanLookup>("Unihan_Readings.json");
        return ParseToString(text, unihanLookup);
    }

    static string ParseToString(string text, UnihanLookup? unihanLookup)
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

    static IReadOnlyList<UnihanCharacter> Unihan(string? text)
    {
        // Unicode range for common Han characters
        int start = 0x4E00;
        int end = 0x9FFF;

        var hanChars = Enumerable.Range(1, 5).Select(index =>
            new UnihanCharacter
            (
                char.ConvertFromUtf32(Random.Shared.Next(start, end + 1)),
                new Dictionary<string, IList<string>>()
                {
                    ["kDefinition"] = ["Chinese character"],
                    ["kMandarin"] = ["hàn", "kan"]
                }
            ))
            .ToArray();
        return hanChars;
    }
}
