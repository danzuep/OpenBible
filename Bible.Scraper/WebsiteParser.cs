using Microsoft.Extensions.Logging;

namespace Bible.Scraper
{
    public static class WebsiteParser
    {
        public static async Task TranslationScannerAsync(ILogger? logger = null)
        {
            logger?.LogInformation("Starting translation scanner...");

            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            using var scanner = new TranslationScanner(http, TimeSpan.FromMilliseconds(200));

            var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));

            IList<string> ids;
            try
            {
                ids = await scanner.ScanAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                logger?.LogInformation("Scan canceled.");
                return;
            }

            var outFile = "translation_ids.txt";
            await File.WriteAllLinesAsync(outFile, ids);
            logger?.LogInformation($"Saved {ids.Count} version ids to {outFile}.");

            foreach (var id in ids.Take(10)) // limited output sample
            {
                logger?.LogInformation($"{id} -> {TranslationScanner.BuildUsfxUrl(id)}");
            }

            logger?.LogInformation("Done.");
        }

        public static async Task<int> TableScannerAsync(ILogger? logger = null, IHtmlFetcher? htmlFetcher = null, string inputUrl = "https://ebible.org/download.php", string outputPath = "ebible_downloads.tsv")
        {
            var sourceUri = new Uri(inputUrl);
            htmlFetcher ??= new HttpHtmlFetcher();

            logger?.LogInformation("Starting table scanner...");
            var parser = new AngleSharpTableRowParser(logger);
            var versionResolver = new QueryStringOrDetailsVersionIdResolver(htmlFetcher, logger);
            var service = new TableScannerService(htmlFetcher, parser, versionResolver, logger);

            await using var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, useAsync: true);
            await using var tsvWriter = new StreamingTsvWriter(fileStream);

            var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
            try
            {
                await service.ScanToTsvAsync(sourceUri, tsvWriter, cts.Token);
                logger?.LogInformation("Scan complete. Output: {Path}", Path.GetFullPath(outputPath));
                return 0;
            }
            catch (OperationCanceledException)
            {
                logger?.LogWarning("Operation cancelled.");
                return 2;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Unhandled error scanning table");
                return 1;
            }
        }

        private static async Task FetchHtmlTableAsync(ILogger? logger, string inputUrl = "https://ebible.org/download.php", string outputPath = "ebible_downloads.html")
        {
            var htmlFetcher = new HttpHtmlFetcher();
            var html = await htmlFetcher.FetchHtmlAsync(new Uri(inputUrl));
            await File.WriteAllTextAsync(outputPath, html);
            logger?.LogInformation("File Written to: {Path}", Path.GetFullPath(outputPath));
        }

        public static async Task<int> TableScannerDemoAsync(ILogger? logger, string inputPath = "ebible_downloads.html")
        {
            if (!File.Exists(inputPath))
                await FetchHtmlTableAsync(logger, outputPath: inputPath);
            var html = await File.ReadAllTextAsync(inputPath);
            var htmlFetcher = new StaticHtmlFetcher(html);
            return await TableScannerAsync(logger, htmlFetcher);
        }
    }
}