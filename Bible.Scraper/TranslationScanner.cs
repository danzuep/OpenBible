using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace Bible.Scraper
{
    /// <summary>
    /// A simple service that scrapes ebible.org/download.php for translation details links,
    /// extracts the translation version ID (query param id) and stores them in a list.
    /// </summary>
    public class TranslationScanner : IDisposable
    {
        private readonly HttpClient _http;
        private readonly Uri _indexUri = new Uri("https://ebible.org/download.php");
        private readonly TimeSpan _requestDelay;
        private bool _disposed;

        public TranslationScanner(HttpClient? httpClient = null, TimeSpan? requestDelay = null)
        {
            if (httpClient?.BaseAddress != null)
            {
                _indexUri = httpClient.BaseAddress;
            }
            _http = httpClient ?? new HttpClient();
            _requestDelay = requestDelay ?? TimeSpan.FromMilliseconds(300); // polite delay between requests
        }

        /// <summary>
        /// Scans the download index page and returns a list of translation IDs (e.g. "thaKJV").
        /// </summary>
        /// <param name="cancellationToken"></param>
        public async Task<IList<string>> ScanAsync(CancellationToken cancellationToken = default)
        {
            var ids = new List<string>();

            // 1) download index page
            var indexHtml = await GetStringWithRetriesAsync(_indexUri, cancellationToken);
            if (string.IsNullOrWhiteSpace(indexHtml))
            {
                Console.WriteLine("Index page empty or failed to download.");
                return ids;
            }

            // 2) parse HTML and find links to translation details
            var doc = new HtmlDocument();
            doc.LoadHtml(indexHtml);

            // The site links look like: details.php?id=thaKJV
            // We'll search for <a> nodes with href containing "details.php"
            var anchorNodes = doc.DocumentNode.SelectNodes("//a[@href]") ?? Enumerable.Empty<HtmlNode>();

            var hrefPattern = new Regex(@"details\.php\?id=([^&""']+)", RegexOptions.IgnoreCase);

            foreach (var a in anchorNodes)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var href = a.Attributes["href"]?.Value?.Trim();
                if (string.IsNullOrEmpty(href))
                    continue;

                // Some links may be relative paths; we only need the query param value
                var m = hrefPattern.Match(href);
                if (m.Success)
                {
                    var id = m.Groups[1].Value;
                    // normalize id (optional)
                    id = Uri.UnescapeDataString(id);

                    if (!ids.Contains(id))
                    {
                        ids.Add(id);
                        Console.WriteLine($"Found translation id: {id}");
                    }
                }
            }

            // Optionally: follow pagination or other pages — for this generic example, we only scan main index.
            return ids;
        }

        /// <summary>
        /// Builds an example USFX download URL for a translation id.
        /// </summary>
        public static string BuildUsfxUrl(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            return $"https://ebible.org/Scriptures/{id}_usfx.zip";
        }

        /// <summary>
        /// Basic HTTP GET with simple retry logic.
        /// </summary>
        private async Task<string?> GetStringWithRetriesAsync(Uri uri, CancellationToken cancellationToken)
        {
            const int maxAttempts = 3;
            int attempt = 0;
            while (true)
            {
                attempt++;
                try
                {
                    using (var req = new HttpRequestMessage(HttpMethod.Get, uri))
                    {
                        req.Headers.UserAgent.ParseAdd("TranslationScanner/1.0 (+https://danzuep.github.io/OpenBible/)");
                        var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                        resp.EnsureSuccessStatusCode();
                        var content = await resp.Content.ReadAsStringAsync(cancellationToken);

                        // polite pause
                        await Task.Delay(_requestDelay, cancellationToken);

                        return content;
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex) when (attempt < maxAttempts)
                {
                    Console.WriteLine($"Request failed (attempt {attempt}) for {uri}: {ex.Message}. Retrying...");
                    await Task.Delay(TimeSpan.FromSeconds(1 * attempt), cancellationToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Request failed after {attempt} attempts: {ex.Message}");
                    return null;
                }
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _http?.Dispose();
                _disposed = true;
            }
        }
    }
}