using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Bible.ServiceDefaults.Models;

namespace Bible.Web;

public class BackendApiClient(HttpClient httpClient)
{
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
}