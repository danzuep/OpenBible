using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using Bible.Backend.Models;
using Bible.Backend.Services;
using Bible.Data;
using Bible.ServiceDefaults.Models;
using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;

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

app.MapGet("/convert", GetConvertAsync)
    .WithName(nameof(GetConvertAsync));

static Task<string> GetConvertAsync([FromQuery] string text)
{
    return ConvertAsync(text);
}

app.MapPost("/convert", PostConvertAsync)
    .WithName(nameof(PostConvertAsync));

static Task<string> PostConvertAsync([FromBody] string text)
{
    return ConvertAsync(text);
}

app.MapGet("/unihan", Unihan)
    .WithName($"Get{nameof(Unihan)}");

static IList<UnihanCharacter> Unihan(string? text)
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
                [ "kDefinition" ] = [ "Chinese character" ],
                [ "kMandarin" ] = [ "hàn", "kan" ]
            }
        ))
        .ToArray();
    return hanChars;
}

app.MapDefaultEndpoints();

await app.RunAsync();


static async Task<string> ConvertAsync(string text)
{
    //await ParseToFileAsync();
    return await ParseFromFileAsync(text);
    return await ParseDemoAsync(text);
}

static async Task<string> ParseDemoAsync(string? text, IEnumerable<UnihanField>? fields = null)
{
    if (string.IsNullOrWhiteSpace(text))
    {
        text = char.ConvertFromUtf32(23383); // test char
    }
    if (fields is null || !fields.Any())
    {
        fields = [
            UnihanField.kDefinition,
            UnihanField.kMandarin,
            UnihanField.kCantonese,
            UnihanField.kJapanese
        ];
    }
    await using var stream = ResourceHelper.GetStreamFromExtension("Unihan_Readings.txt");
    var unihanLookup = await UnihanParserService.ParseAsync(stream, fields);
    return ParseToString(text, unihanLookup);
}

static async Task<string> ParseFromFileAsync(string? text, IEnumerable<UnihanField>? fields = null)
{
    if (string.IsNullOrWhiteSpace(text))
    {
        text = char.ConvertFromUtf32(23383); // test char
    }
    await using var inputStream = ResourceHelper.GetStreamFromExtension("Unihan_Readings.json");
    var ser = new JsonDeserializer();
    var unihanLookup = await ser.DeserializeAsync<UnihanLookup>(inputStream);
    return ParseToString(text, unihanLookup);
}

static async Task ParseToFileAsync()
{
    await using var inputStream = ResourceHelper.GetStreamFromExtension("Unihan_Readings.txt");
    await using var outputStream = await UnihanParserService.ParseToStreamAsync(inputStream);
    await ResourceHelper.WriteStreamAsync("Unihan_Readings.json", outputStream);
    Debug.WriteLine("Unihan_Readings.json created successfully.");
}

static string ParseToString(string text, UnihanLookup unihanLookup)
{
    var stringBuilder = new StringBuilder();
    foreach (Rune rune in text.EnumerateRunes())
    {
        stringBuilder.AppendLine($"Rune: {rune.ToString()} ({rune.Value})");
        if (unihanLookup.TryGetValue(rune.Value, out var metadata))
        {
            foreach (var kvp in metadata)
            {
                stringBuilder.AppendLine($"{kvp.Key}: {string.Join("; ", kvp.Value)}");
            }
        }
    }
    return stringBuilder.ToString();
}
