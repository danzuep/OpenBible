using System.Collections.Immutable;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Bible.Backend.Models;
using Bible.Backend.Services;
using Bible.Backend.Visitors;
using Bible.Core.Models;
using Bible.Core.Models.Scripture;
using Bible.Data;
using Bible.ServiceDefaults.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;

/// <summary>
/// The main entry point for the Bible API Service application.
/// </summary>
public static partial class Program
{
    /// <summary>
    /// Application entry point.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add service defaults & Aspire client integrations.
        builder.AddServiceDefaults();

        // Add services to the container.
        builder.Services.AddProblemDetails();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.MapOpenApi();
            app.MapScalarApiReference();
        }
        else
        {
            app.UseExceptionHandler();
        }

        app.UseHttpsRedirection();

        app.MapGet("/unihan", Unihan)
            .WithName($"Get{nameof(Unihan)}");

        app.MapGet("/convert", static ([FromQuery] string text) => ParseAsync(text));

        app.MapPost("/convert", static ([FromBody] string text) => ParseAsync(text));

        app.MapGet("/BibleBook/{language}/{version}/{book}",
            static (string language, string version, string book) =>
            ParseBibleBookAsync(language, version, book));

        app.MapGet("/{language}/{version}/{book}",
            static (string language, string version, string book) =>
            ParseScriptureBookAsync(language, version, book));

        app.MapGet("/{language}/{version}/{book}/{chapter}",
            static (string language, string version, string book, byte chapter) =>
            ParseScriptureBookChapterAsync(language, version, book, chapter));

        app.MapGet("/{language}/{version}/{book}/{chapter}/stream",
            static (string language, string version, string book, byte chapter, CancellationToken cancellationToken) =>
            GetScriptureRecordsStreamAsync(language, version, book, chapter, cancellationToken));

        app.MapDefaultEndpoints();

        await app.RunAsync();

        app.MapDefaultEndpoints();

        await app.RunAsync();
    }

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

    static async Task<ScriptureBookMetadata?> ParseScriptureBookMetadataAsync(string language, string version, string book, byte chapter)
    {
        var scriptureBook = await ParseScriptureBookAsync(language, version, book);
        return scriptureBook?.Metadata;
    }

    static async Task<ScriptureBook?> ParseScriptureBookAsync(string language, string version, string book)
    {
        (var unihan, var options) = await TryGetUnihanOptionsAsync(language);
        await using var stream = ResourceHelper.GetUsxBookStream(language, version, book);
        var scriptureBook = await UsxToScriptureBookVisitor.DeserializeAsync(stream, unihan, options);
        return scriptureBook;
    }

    static async Task<ScriptureBook?> ParseScriptureBookAsync(string path)
    {
        var isoLanguage = string.Join("-", path.Split('-').SkipLast(1));
        (var unihan, var options) = await TryGetUnihanOptionsAsync(path);
        await using var stream = ResourceHelper.GetStreamFromExtension(path);
        var scriptureBook = await UsxToScriptureBookVisitor.DeserializeAsync(stream, unihan, options);
        return scriptureBook;
    }

    static async Task<ScriptureBook?> ParseScriptureBookAsync(ILogger logger, string path = "zho-Hant-OCCB/3JN.usx") // "eng-webbe/3jn"
    {
        var isoLanguage = string.Join("-", path.Split('-').SkipLast(1));
        (var unihan, var options) = await TryGetUnihanOptionsAsync(isoLanguage);
        await using var stream = ResourceHelper.GetStreamFromExtension(path);
        var scriptureBook = await UsxToScriptureBookVisitor.DeserializeAsync(stream, unihan, options);
        if (scriptureBook != null)
        {
            logger.LogInformation(scriptureBook.ToMarkdown());
            //var result = scriptureBook.ToChapterDto(1);
        }
        return scriptureBook;
    }

    static async Task<(UnihanLookup?, UsxVisitorOptions?)> TryGetUnihanOptionsAsync(string isoLanguage, string fileName = "Unihan_Readings.json")
    {
        UnihanLookup? unihan = null;
        UsxVisitorOptions? options = null;
        if (UnihanLookup.NameUnihanLookup.TryGetValue(isoLanguage, out var unihanFields))
        {
            unihan = await ResourceHelper.GetFromJsonAsync<UnihanLookup>(fileName);
            options = new UsxVisitorOptions { EnableRunes = unihanFields?.FirstOrDefault() };
        }
        return (unihan, options);
    }

    static async Task<BibleBook?> ParseBibleBookAsync(string language = "eng", string version = "WEBBE", string book = "JHN")
    {
        await using var stream = ResourceHelper.GetUsxBookStream(language, version, book);
        var deserializer = new XDocDeserializer();
        var usxParser = new UsxToBibleBookParser(deserializer);
        var bibleBook = await usxParser.ParseAsync(stream);
        return bibleBook;
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
