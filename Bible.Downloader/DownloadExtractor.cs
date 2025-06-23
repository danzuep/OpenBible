using System.Diagnostics;
using System.IO.Compression;

namespace Bible.Downloader
{
    public class DownloadExtractor
    {
        private readonly HttpClient _httpClient;

        public DownloadExtractor(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        private async Task DownloadAsync(string url, string outputPath)
        {
            using var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            if (response.Content.Headers.ContentDisposition?.FileName is string filename)
            {
                Debug.WriteLine(filename);
                Console.WriteLine(filename);
            }
            await using var fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await response.Content.CopyToAsync(fs);
        }

        public async Task DownloadExtractZipAsync(string url, string extractPath)
        {
            extractPath ??= Path.GetRandomFileName();
            var downloadsPath = $"{extractPath}.zip";
            await DownloadAsync(url, downloadsPath);
            ZipFile.ExtractToDirectory(downloadsPath, extractPath);
        }
    }
}