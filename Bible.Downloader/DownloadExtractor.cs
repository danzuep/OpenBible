using System.Diagnostics;
using System.IO.Compression;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bible.Downloader
{
    public class DownloadExtractor
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public DownloadExtractor(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = NullLogger<DownloadExtractor>.Instance;
        }

        public DownloadExtractor(HttpClient httpClient, ILogger? logger) : this(httpClient)
        {
            if (logger != null)
            {
                _logger = logger;
            }
        }

        public async Task<string?> DownloadAsync(string url, string? outputPath = null, string? extension = null)
        {
            outputPath ??= Path.GetRandomFileName();
            if (!string.IsNullOrEmpty(extension))
            {
                outputPath += extension;
            }
            if (Path.Exists(outputPath))
            {
                return outputPath;
            }

            _logger.LogInformation($"GET {_httpClient.BaseAddress}/{url}");
            using var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            if (response.Content.Headers.ContentDisposition?.FileName is string filename)
            {
                _logger.LogInformation($"FileName: {filename}");
                await using var fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
                await response.Content.CopyToAsync(fs);
                return outputPath;
            }
            return null;
        }

        public void ExtractToDirectory(string downloadsPath, string extractPath)
        {
            _logger.LogInformation($"Extracting to: {extractPath}");
            ZipFile.ExtractToDirectory(downloadsPath, extractPath);
        }

        public async Task<bool> DownloadExtractZipAsync(string url, string extractPath)
        {
            try
            {
                var downloadsPath = await DownloadAsync(url, extractPath, ".zip");
                if (string.IsNullOrEmpty(downloadsPath))
                {
                    return false;
                }
                ExtractToDirectory(downloadsPath, extractPath);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }
    }
}