using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Bible.Core.Models;
using Bible.Core.Models.Scripture;
using Bible.ServiceDefaults.Models;

namespace Bible.Web;

public class BackendApiClient(HttpClient httpClient)
{
    [Obsolete("Use PostConvertedAsync instead.")]
    public async Task<string> GetConvertedAsync(string text, CancellationToken cancellationToken = default)
    {
        var encodedText = Uri.EscapeDataString(text);
        var url = $"/convert?text={encodedText}";
        var response = await httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsStringAsync(cancellationToken);
        return result;
    }

    public async Task<string> PostConvertedAsync(string text, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(text);
        var content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
        var response = await httpClient.PostAsync("/convert", content, cancellationToken);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsStringAsync(cancellationToken);
        return result;
    }

    public async Task<UnihanCharacter[]> GetUnihanAsync(string text, CancellationToken cancellationToken = default)
    {
        List<UnihanCharacter>? unicodeHanCharacters = null;

        await foreach (var unicodeHanCharacter in httpClient.GetFromJsonAsAsyncEnumerable<UnihanCharacter>("/unihan", cancellationToken))
        {
            if (unicodeHanCharacter is not null)
            {
                unicodeHanCharacters ??= [];
                unicodeHanCharacters.Add(unicodeHanCharacter);
            }
        }

        return unicodeHanCharacters?.ToArray() ?? [];
    }

    public async Task<BibleBook?> GetBibleBookAsync(string? language, string? version, string? book, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(language) || string.IsNullOrEmpty(version) || string.IsNullOrEmpty(book))
        {
            return null;
        }
        var url = $"/BibleBook/{language}/{version}/{book}";
        var result = await httpClient.GetFromJsonAsync<BibleBook>(url, cancellationToken);
        return result;
    }

    public async Task<ScriptureBook?> GetScriptureBookAsync(string? language, string? version, string? book, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(language) || string.IsNullOrEmpty(version) || string.IsNullOrEmpty(book))
        {
            return null;
        }
        var url = $"/ScriptureBook/{language}/{version}/{book}";
        var result = await httpClient.GetFromJsonAsync<ScriptureBook>(url, cancellationToken);
        return result;
    }

    public async Task<ScriptureRange?> GetScriptureBookChapterAsync(string? language, string? version, string? book, int chapter, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(language) || string.IsNullOrEmpty(version) || string.IsNullOrEmpty(book))
        {
            return null;
        }
        var url = $"/{language}/{version}/{book}/{chapter}";
        var result = await httpClient.GetFromJsonAsync<ScriptureRange>(url, cancellationToken);
        return result;
    }

    public async Task<ScriptureBookMetadata?> GetScriptureBookMetadataAsync(string? language, string? version, string? book, int chapter, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(language) || string.IsNullOrEmpty(version) || string.IsNullOrEmpty(book))
        {
            return null;
        }
        var url = $"/{language}/{version}/{book}/{chapter}/metadata";
        var result = await httpClient.GetFromJsonAsync<ScriptureBookMetadata>(url, cancellationToken);
        return result;
    }

    public async IAsyncEnumerable<ScriptureSegmentDto> GetScriptureRecordsStreamAsync(string? language, string? version, string? book, int chapter, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(language) || string.IsNullOrEmpty(version) || string.IsNullOrEmpty(book))
        {
            yield break;
        }
        var url = $"/{language}/{version}/{book}/{chapter}/stream";
        await foreach (var result in httpClient.GetFromJsonAsAsyncEnumerable<ScriptureSegmentDto>(url, cancellationToken))
        {
            if (result is not null)
            {
                yield return result;
            }
        }
    }

    public async IAsyncEnumerable<ScriptureSegmentDto> GetRuneMetadataStreamAsync(string? language, string? version, string? book, int chapter, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(language) || string.IsNullOrEmpty(version) || string.IsNullOrEmpty(book))
        {
            yield break;
        }
        var url = $"/{language}/{version}/{book}/{chapter}/unihan";
        await foreach (var result in httpClient.GetFromJsonAsAsyncEnumerable<ScriptureSegmentDto>(url, cancellationToken))
        {
            if (result is not null)
            {
                yield return result;
            }
        }
    }
}