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

app.MapGet("/convert", GetConvert)
    .WithName(nameof(GetConvert));

static string GetConvert([FromQuery] string text)
{
    return $"Yay! {text}";
}

app.MapPost("/convert", PostConvert)
    .WithName(nameof(PostConvert));

static string PostConvert([FromBody] string text)
{
    return $"Yay! {text}";
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
