using System.Diagnostics;
using System.IO.Compression;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bible.Backend.Services
{
    public class DownloadExtractor
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DownloadExtractor> _logger;

        public DownloadExtractor(HttpClient? httpClient = null, ILogger<DownloadExtractor>? logger = null)
        {
            _httpClient = httpClient ?? DigitalBibleLibraryConstants.HttpClient;
            _logger = logger ?? NullLogger<DownloadExtractor>.Instance;
        }

        private async Task DownloadAsync(string url, string outputPath)
        {
            if (File.Exists(outputPath))
            {
                return;
            }
            _logger.LogInformation($"GET {_httpClient.BaseAddress}/{url}");
            using var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            if (response.Content.Headers.ContentDisposition?.FileName is string filename)
            {
                _logger.LogInformation($"FileName: {filename}");
            }
            await using var fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await response.Content.CopyToAsync(fs);
        }

        public async Task<string> DownloadAsync(string url)
        {
            var extension = Path.GetExtension(url);
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var downloadsPath = Path.Combine(userProfile, "Downloads", Path.GetRandomFileName() + extension);
            await DownloadAsync(url, downloadsPath);
            return downloadsPath;
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
                extractPath ??= Path.GetRandomFileName();
                await DownloadAsync(url, extractPath);
                if (string.IsNullOrEmpty(extractPath))
                {
                    return false;
                }
                ExtractToDirectory(extractPath, extractPath);
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