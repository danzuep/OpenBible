using System.Runtime.CompilerServices;
using AngleSharp;
using AngleSharp.Dom;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Polly;
using Polly.Retry;

namespace Bible.Scraper
{
    // Model representing a row from the HTML table
    public record TableRow(
        string Territory,
        string Language,
        string LanguageEnglish,
        string VernacularTitle,
        string EnglishTitle,
        string DetailsHref // relative or absolute URL to details.php?id=...
    );

    // Interfaces
    public interface IHtmlFetcher
    {
        Task<string> FetchHtmlAsync(Uri uri, CancellationToken ct = default);
    }

    public interface ITableRowParser
    {
        IAsyncEnumerable<TableRow> ParseTableRowsAsync(string html, Uri baseUri, CancellationToken ct);
    }

    public interface IVersionIdResolver
    {
        /// <summary>
        /// Resolve a version id from a details page URI. Might fetch the details page or parse the query string.
        /// </summary>
        Task<string?> ResolveVersionIdAsync(Uri detailsUri, CancellationToken ct);
    }

    public interface ITsvWriter : IAsyncDisposable
    {
        Task WriteHeaderAsync(CancellationToken ct);
        Task WriteRowAsync(TableRow row, string? versionId, CancellationToken ct);
    }

    public interface ITableScannerService
    {
        Task ScanToTsvAsync(Uri pageUri, ITsvWriter tsvWriter, CancellationToken ct);
    }

    public class StaticHtmlFetcher : IHtmlFetcher
    {
        private readonly string _html;
        public StaticHtmlFetcher(string html) => _html = html;
        public Task<string> FetchHtmlAsync(Uri uri, CancellationToken ct = default) => Task.FromResult(_html);
    }

    // Implementation: HtmlFetcher using HttpClient
    public class HttpHtmlFetcher : IHtmlFetcher, IDisposable
    {
        private readonly HttpClient _http;
        public HttpHtmlFetcher(HttpClient? http = null) => _http = http ?? new HttpClient();
        public async Task<string> FetchHtmlAsync(Uri uri, CancellationToken ct = default)
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, uri);
            using var res = await _http.SendAsync(req, HttpCompletionOption.ResponseContentRead, ct).ConfigureAwait(false);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        }

        public void Dispose() => _http?.Dispose();
    }

    // Implementation: AngleSharp-based parser tailored to the page structure (but generic)
    public class AngleSharpTableRowParser : ITableRowParser
    {
        private readonly ILogger _logger;
        private readonly IBrowsingContext _browsingContext;

        public AngleSharpTableRowParser(ILogger? logger = null)
        {
            _logger = logger ?? NullLogger<AngleSharpTableRowParser>.Instance;
            var config = Configuration.Default;
            _browsingContext = BrowsingContext.New(config);
        }

        private static string Normalize(string? s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return string.Empty;
            return s.Trim().ToLowerInvariant();
        }

        private IElement? GetTargetTable(IDocument doc, IEnumerable<string> patterns)
        {
            if (doc == null) throw new ArgumentNullException(nameof(doc));
            if (patterns == null) throw new ArgumentNullException(nameof(patterns));

            var headersToCheck = patterns
                .Where(h => !string.IsNullOrWhiteSpace(h))
                .Select(Normalize)
                .ToArray();

            if (headersToCheck.Length == 0)
                return null;

            foreach (var table in doc.QuerySelectorAll("table"))
            {
                // find the first meaningful row (skip empty rows and rows made of only whitespace)
                var firstRow = table.QuerySelectorAll("tr")
                    .FirstOrDefault(r => r.QuerySelectorAll("th,td")
                        .Any(c => !string.IsNullOrWhiteSpace(c.TextContent)));
                if (firstRow == null)
                    continue;

                // gather normalized cell texts from that row
                var cellTexts = firstRow.QuerySelectorAll("th,td");

                foreach (var item in cellTexts)
                {
                    var header = Normalize(item?.TextContent);
                    if (string.IsNullOrEmpty(header)) continue;
                    if (headersToCheck.Any(header.StartsWith))
                        return table;
                }
            }

            return null;
        }

        public async IAsyncEnumerable<TableRow> ParseTableRowsAsync(string html, Uri baseUri, [EnumeratorCancellation] CancellationToken ct = default)
        {
            var doc = await _browsingContext
                .OpenAsync(req => req.Content(html).Address(baseUri), ct)
                .ConfigureAwait(false);

            var headersToCheck = new[] { "territory", "language", "vernacular" };
            var targetTable = GetTargetTable(doc, headersToCheck);

            if (targetTable == null)
            {
                _logger.LogWarning("No target table found.");
                yield break;
            }

            // iterate over rows skipping header row(s)
            var rows = targetTable.QuerySelectorAll("tr");
            foreach (var r in rows)
            {
                ct.ThrowIfCancellationRequested();

                // skip header rows that contain <th>
                if (r.QuerySelector("th") != null)
                    continue;

                var cells = r.QuerySelectorAll("td").ToArray();
                if (cells.Length < 5)
                {
                    // unexpectedly small row - skip
                    _logger.LogDebug("Skipping row with {Count} cells", cells.Length);
                    continue;
                }

                string cellText(int i) => cells[i].TextContent?.Trim() ?? string.Empty;

                // Territory cell often includes <a href='country.php?c=...'><img.../> CountryName</a>
                var territory = cellText(0);

                // The Language / LanguageEnglish / VernacularTitle / EnglishTitle are usually links in the subsequent cells,
                // prefer link text if available.
                static string GetAnchorTextOrText(IElement cell)
                {
                    var a = cell.QuerySelector("a");
                    if (a != null)
                        return a.TextContent?.Trim() ?? string.Empty;
                    return cell.TextContent?.Trim() ?? string.Empty;
                }

                var language = GetAnchorTextOrText(cells[1]);
                var languageEnglish = GetAnchorTextOrText(cells[2]);
                var vernacular = GetAnchorTextOrText(cells[3]);
                string englishTitle = string.Empty;
                string detailsHref = string.Empty;
                // The last cell usually contains an <a href='details.php?id=...'>EnglishTitle</a>
                var lastA = cells[4].QuerySelector("a");
                if (lastA != null)
                {
                    englishTitle = lastA.TextContent?.Trim() ?? string.Empty;
                    var href = lastA.GetAttribute("href") ?? string.Empty;
                    try
                    {
                        var resolved = new Uri(baseUri, href).ToString();
                        detailsHref = resolved;
                    }
                    catch
                    {
                        detailsHref = href;
                    }
                }
                else
                {
                    englishTitle = cells[4].TextContent?.Trim() ?? string.Empty;
                }

                yield return new TableRow(territory, language, languageEnglish, vernacular, englishTitle, detailsHref);
            }
        }
    }

    // Implementation: try extract version id quickly; fallback to fetching details page and parsing title for id
    public class QueryStringOrDetailsVersionIdResolver : IVersionIdResolver
    {
        private readonly IHtmlFetcher _fetcher;
        private readonly ILogger _logger;
        private readonly AsyncRetryPolicy _retryPolicy;

        public QueryStringOrDetailsVersionIdResolver(IHtmlFetcher fetcher, ILogger? logger = null)
        {
            _fetcher = fetcher ?? throw new ArgumentNullException(nameof(fetcher));
            _logger = logger ?? NullLogger.Instance;

            // simple retry policy for network calls
            _retryPolicy = Policy.Handle<Exception>()
                                 .WaitAndRetryAsync(3, i => TimeSpan.FromSeconds(Math.Pow(2, i)),
                                     (ex, ts) => _logger.LogWarning(ex, "Retrying due to error"));
        }

        public async Task<string?> ResolveVersionIdAsync(Uri detailsUri, CancellationToken ct)
        {
            if (detailsUri == null) return null;

            // Quick parse from query string if present: id=thaKJV
            var q = System.Web.HttpUtility.ParseQueryString(detailsUri.Query);
            var id = q["id"];
            if (!string.IsNullOrWhiteSpace(id))
                return id.Trim();

            // fallback: fetch details page and attempt to parse an ID from canonical link or page content (robust)
            try
            {
                var content = await _retryPolicy.ExecuteAsync((ct) => _fetcher.FetchHtmlAsync(detailsUri, ct), ct).ConfigureAwait(false);

                // parse with AngleSharp minimal parse
                var ctx = BrowsingContext.New(Configuration.Default);
                var doc = await ctx.OpenAsync(req => req.Content(content).Address(detailsUri), ct).ConfigureAwait(false);

                // heuristic 1: meta property or link rel=canonical with id querystring
                var canonical = doc.QuerySelector("link[rel=canonical]")?.GetAttribute("href") ?? string.Empty;
                if (!string.IsNullOrEmpty(canonical))
                {
                    var cUri = new Uri(detailsUri, canonical);
                    var qc = System.Web.HttpUtility.ParseQueryString(cUri.Query);
                    var cid = qc["id"];
                    if (!string.IsNullOrEmpty(cid)) return cid;
                }

                // heuristic 2: look for "ID:" or "ID =" or "id=" text inside page
                var text = doc.Body?.TextContent ?? string.Empty;
                var idToken = ExtractIdFromText(text);
                if (!string.IsNullOrEmpty(idToken)) return idToken;

                // heuristic 3: last path segment if it looks like an id
                var lastSeg = detailsUri.Segments.LastOrDefault()?.Trim('/');
                if (!string.IsNullOrEmpty(lastSeg) && lastSeg.Length <= 10) return lastSeg;

                return null;
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to resolve version id for {Uri}", detailsUri);
                return null;
            }
        }

        private static string? ExtractIdFromText(string text)
        {
            if (string.IsNullOrEmpty(text)) return null;
            // look for patterns id=XXXX or id: XXXX or ID: XXXX or THAKJV-like tokens
            var tokens = text.Split(new[] { ' ', '\r', '\n', '\t', ',', ';', ':', '=', '"' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var t in tokens)
            {
                if (t.Length >= 3 && t.Length <= 20 && t.All(ch => char.IsLetterOrDigit(ch) || ch == '-' || ch == '_'))
                {
                    // crude heuristic: many ids are all-lower alpha (thaKJV is mixed), accept safe-looking tokens
                    if (t.Any(char.IsLetter) && t.Any(char.IsDigit) == false)
                        return t;
                }
            }
            return null;
        }
    }

    // TSV writer that streams to file
    public class StreamingTsvWriter : ITsvWriter
    {
        private readonly StreamWriter _writer;
        private readonly ILogger<StreamingTsvWriter> _logger;
        private bool _headerWritten;

        public StreamingTsvWriter(Stream stream, ILogger<StreamingTsvWriter>? logger = null)
        {
            _writer = new StreamWriter(stream) { NewLine = "\n" };
            _logger = logger ?? NullLogger<StreamingTsvWriter>.Instance;
        }

        public async Task WriteHeaderAsync(CancellationToken ct)
        {
            if (_headerWritten) return;
            // columns: Territory, Language, LanguageEnglish, VernacularTitle, EnglishTitle, DetailsHref, VersionId
            await _writer.WriteLineAsync("Territory\tLanguage\tLanguageEnglish\tVernacularTitle\tEnglishTitle\tDetailsHref\tVersionId").ConfigureAwait(false);
            await _writer.FlushAsync().ConfigureAwait(false);
            _headerWritten = true;
        }

        public async Task WriteRowAsync(TableRow row, string? versionId, CancellationToken ct)
        {
            // safe TSV escaping: replace tabs/newlines with spaces, wrap fields if needed - TSV typically avoids quoting; we sanitize.
            static string Sanitize(string? s) =>
                (s ?? string.Empty).Replace("\t", " ").Replace("\r", " ").Replace("\n", " ").Trim();

            var parts = new[]
            {
                Sanitize(row.Territory),
                Sanitize(row.Language),
                Sanitize(row.LanguageEnglish),
                Sanitize(row.VernacularTitle),
                Sanitize(row.EnglishTitle),
                Sanitize(versionId ?? row.DetailsHref)
            };

            await _writer.WriteLineAsync(string.Join('\t', parts)).ConfigureAwait(false);

            // flush periodically to keep memory usage low and ensure streaming
            await _writer.FlushAsync().ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            await _writer.FlushAsync().ConfigureAwait(false);
            _writer.Dispose();
        }
    }

    // The orchestrator service
    public class TableScannerService : ITableScannerService
    {
        private readonly IHtmlFetcher _fetcher;
        private readonly ITableRowParser _parser;
        private readonly IVersionIdResolver _versionResolver;
        private readonly ILogger _logger;

        public TableScannerService(
            IHtmlFetcher fetcher,
            ITableRowParser parser,
            IVersionIdResolver versionResolver,
            ILogger? logger = null)
        {
            _fetcher = fetcher ?? throw new ArgumentNullException(nameof(fetcher));
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
            _versionResolver = versionResolver ?? throw new ArgumentNullException(nameof(versionResolver));
            _logger = logger ?? NullLogger.Instance;
        }

        public async Task ScanToTsvAsync(Uri pageUri, ITsvWriter tsvWriter, CancellationToken ct)
        {
            _logger.LogInformation("Fetching page {Uri}", pageUri);
            var html = await _fetcher.FetchHtmlAsync(pageUri, ct).ConfigureAwait(false);

            _logger.LogInformation("Parsing rows");

            await foreach (var row in _parser.ParseTableRowsAsync(html, pageUri, ct).WithCancellation(ct))
            {
                ct.ThrowIfCancellationRequested();

                string? versionId = null;
                if (!string.IsNullOrWhiteSpace(row.DetailsHref))
                {
                    if (Uri.TryCreate(row.DetailsHref, UriKind.Absolute, out var detailsUri) ||
                        Uri.TryCreate(pageUri, row.DetailsHref, out detailsUri))
                    {
                        try
                        {
                            versionId = await _versionResolver.ResolveVersionIdAsync(detailsUri, ct).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException) { throw; }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to get version id for {Href}", row.DetailsHref);
                        }
                    }
                }

                await tsvWriter.WriteRowAsync(row, versionId, ct).ConfigureAwait(false);
                _logger.LogDebug("Wrote row for {Title} (versionId={VersionId})", row.EnglishTitle, versionId ?? "<null>");
            }
        }
    }
}