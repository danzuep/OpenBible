using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using Bible.Backend.Models;
using Bible.Backend.Services;

namespace Bible.Backend.Tests;

public class UnihanTests
{
    [Theory]
    [InlineData("U+20000", "𠀀", "\uD840\uDC00")]
    public void ConvertToUnicode_WithoutSerialization_Succeeds(string input, string expectedResult, string actualResult)
    {
        var codepoint = UnihanParserService.ConvertToCodepoint(input);
        var character = char.ConvertFromUtf32(codepoint);
        Assert.Equal(expectedResult, character);
        Assert.Equal(expectedResult, actualResult);
        Trace.WriteLine($"{input} == {expectedResult}");
    }

    [Theory]
    [InlineData("U+20000", "\uD840\uDC00")]
    public void ConvertToUnicode_WithSerialization_Fails(string input, string actualResult)
    {
        var codepoint = UnihanParserService.ConvertToCodepoint(input);
        var character = char.ConvertFromUtf32(codepoint);
        var jsonSerializerOptions = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        var json = JsonSerializer.Serialize(character, jsonSerializerOptions);
        Assert.Equal(actualResult, character);
        Assert.NotEqual(character, json);
        Trace.WriteLine($"{actualResult} != {character}");
    }

    [Theory]
    [InlineData("U+20000", "𠀀", "\uD840\uDC00")]
    public void ConvertToUnicode_WithSerialization_Succeeds(string input, string expectedResult, string actualResult)
    {
        var codepoint = UnihanParserService.ConvertToCodepoint(input);
        var character = char.ConvertFromUtf32(codepoint);
        var jsonSerializerOptions = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        var json = JsonSerializer.Serialize(character, jsonSerializerOptions);
        var deserialized = JsonSerializer.Deserialize<string>(json, jsonSerializerOptions);

        Assert.Equal(expectedResult, character);
        Assert.Equal(expectedResult, actualResult);
        Assert.Equal(character, deserialized);

        Trace.WriteLine($"{json} != {expectedResult}");
    }
}