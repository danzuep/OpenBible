using System.IO.Compression;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bible.Backend
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
            using var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
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

        public void Extract(string downloadsPath, string extractPath)
        {
            ZipFile.ExtractToDirectory(downloadsPath, extractPath);
        }

        public async Task DownloadExtractAsync(string url, string extractPath)
        {
            var downloadsPath = await DownloadAsync(url);
            ZipFile.ExtractToDirectory(downloadsPath, extractPath);
        }
    }
}