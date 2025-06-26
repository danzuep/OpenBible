using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bible.Backend.Services
{
    public class Downloader
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<Downloader> _logger;

        public Downloader(HttpClient? httpClient = null, ILogger<Downloader>? logger = null)
        {
            _httpClient = httpClient ?? DigitalBibleLibraryConstants.HttpClient;
            _logger = logger ?? NullLogger<Downloader>.Instance;
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
    }
}