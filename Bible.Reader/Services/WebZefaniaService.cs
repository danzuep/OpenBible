using Bible.Core.Models;
using Bible.Reader.Adapters;
using Bible.Reader.Models;
using CsvHelper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bible.Reader.Services
{
    public sealed class WebZefaniaService : IDisposable
    {
        public static readonly string RawGitHubUserContentWebEndpoint = "https://raw.githubusercontent.com/";

        // https://github.com/kohelet-net-admin/zefania-xml-bibles/blob/master/Bibles/index.csv
        private static readonly string _serviceIndexPath = "kohelet-net-admin/zefania-xml-bibles/master/Bibles/";
        private static readonly string _serviceIndexName = "index.csv";

        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public WebZefaniaService(HttpClient httpClient = null, ILogger logger = null)
        {
            _httpClient = httpClient ?? new HttpClient() { BaseAddress = new Uri(RawGitHubUserContentWebEndpoint) };
            _logger = logger ?? NullLogger<WebZefaniaService>.Instance;
        }

        public async Task<WebBibleInfoModel> GetBibleInfoAsync(string appDataDirectory, string identifier, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(identifier)) return null;
            await using var csvStream = await GetCsvStreamAsync(appDataDirectory, cancellationToken);
            using var textReader = new StreamReader(csvStream);
            using var csvReader = new CsvReader(textReader, CultureInfo.InvariantCulture);
            var records = csvReader.GetRecordsAsync<WebZefaniaIndexModel>(cancellationToken);
            await foreach (var record in records)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (record?.ZefaniaBibleInfoIdentifier == identifier)
                {
                    var bibleInfo = new WebBibleInfoModel
                    {
                        Name = record.ZefaniaBibleName,
                        Identifier = record.ZefaniaBibleInfoIdentifier,
                        DownloadUrl = record.DownloadUrl
                    };
                    if (Uri.TryCreate(record.DownloadUrl, default, out Uri uri) &&
                        uri.Segments.Length >= 6 && uri.Segments[5].Length == 4)
                        bibleInfo.Language = uri.Segments[5].TrimEnd('/').ToLowerInvariant();
                    return bibleInfo;
                }
            }
            return null;
        }

        public async IAsyncEnumerable<WebBibleInfoModel> AsyncGetBibleInfo(string appDataDirectory, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await using var csvStream = await GetCsvStreamAsync(appDataDirectory, cancellationToken);
            using var textReader = new StreamReader(csvStream);
            using var csvReader = new CsvReader(textReader, CultureInfo.InvariantCulture);
            var records = csvReader.GetRecordsAsync<WebZefaniaIndexModel>(cancellationToken);
            await foreach (var record in records)
            {
                if (string.IsNullOrWhiteSpace(record?.ZefaniaBibleInfoIdentifier))
                    continue;
                var bibleInfo = new WebBibleInfoModel
                {
                    Name = record.ZefaniaBibleName,
                    Identifier = record.ZefaniaBibleInfoIdentifier,
                    DownloadUrl = record.DownloadUrl
                };
                if (Uri.TryCreate(record.DownloadUrl, default, out Uri uri) &&
                    uri.Segments.Length >= 6 && uri.Segments[5].Length == 4)
                    bibleInfo.Language = uri.Segments[5].TrimEnd('/').ToLowerInvariant();
                if (!string.IsNullOrEmpty(bibleInfo.Language) &&
                    !string.IsNullOrEmpty(bibleInfo.Identifier) &&
                    bibleInfo.Identifier != "BOM")
                    yield return bibleInfo;
            }
        }

        internal async Task<string> DownloadBibleAsync(string downloadUrl, string directory, CancellationToken cancellationToken = default)
        {
            var uri = new Uri(downloadUrl);
            var fileName = uri.Segments.Length < 7 ? uri.Segments.Last() :
                Uri.UnescapeDataString(uri.Segments[7].TrimEnd('/'));
            if (!Path.HasExtension(fileName))
                fileName = $"{fileName}.xml";
            var fileType = Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant();
            var filePath = Path.Combine(directory, fileType, fileName);
            await using var _ = await DownloadAsync(downloadUrl, filePath, cancellationToken).ConfigureAwait(false);
            return filePath;
        }

        public async Task<BibleModel> GetBibleAsync(WebBibleInfoModel bibleInfo, string appDataDirectory, CancellationToken cancellationToken = default)
        {
            if (bibleInfo == null || string.IsNullOrWhiteSpace(appDataDirectory))
                return null;
            var uri = new Uri(bibleInfo.DownloadUrl);
            if (uri.Segments.Length >= 6 && uri.Segments[5].Length == 4)
                bibleInfo.Language = uri.Segments[5].TrimEnd('/').ToLowerInvariant();
            var suffix = Path.GetExtension(bibleInfo.DownloadUrl).TrimStart('.').ToLowerInvariant();
            var fileDirectory = Path.Combine(appDataDirectory, suffix);
            Directory.CreateDirectory(fileDirectory);
            var filePath = Path.Combine(fileDirectory, $"{bibleInfo.Name}.{suffix}");
            var bibleFile = await DeserializeAsync<XmlZefania05>(bibleInfo.DownloadUrl, filePath, cancellationToken).ConfigureAwait(false);
            var bibleModel = bibleFile?.ToBibleFormat(bibleInfo.Language, bibleInfo.Identifier);
            return bibleModel;
        }

        static T Deserialize<T>(Stream stream) where T : class
        {
            var serializer = new XmlSerializer(typeof(T));
            T result = serializer.Deserialize(stream) as T;
            return result;
        }

        static async Task<Stream> SaveToFileAsync(Stream stream, string filePath, FileAccess fileAccess = FileAccess.ReadWrite)
        {
            var fileStream = new FileStream(filePath, FileMode.Create, fileAccess);
            await stream.CopyToAsync(fileStream).ConfigureAwait(false);
            fileStream.Position = 0;
            return fileStream;
        }

        async Task<Stream> DownloadAsync(string downloadUrl, string filePath, CancellationToken cancellationToken = default)
        {
            await using var downloadStream = await _httpClient.GetStreamAsync(downloadUrl).ConfigureAwait(false);
            var fileStream = await SaveToFileAsync(downloadStream, filePath).ConfigureAwait(false);
            _logger.LogDebug($"File saved to {filePath} (downloaded from {downloadUrl})");
            return fileStream;
        }

        async Task<Stream> GetCsvStreamAsync(string appDataDirectory, CancellationToken cancellationToken = default)
        {
            var filePath = Path.Combine(appDataDirectory, _serviceIndexName);
            var downloadUrl = $"{_serviceIndexPath}{_serviceIndexName}";
            var fileStream = File.Exists(filePath) ? File.OpenRead(filePath) :
                await DownloadAsync(downloadUrl, filePath, cancellationToken).ConfigureAwait(false);
            _logger.LogDebug($"File saved to {filePath} (downloaded from {downloadUrl})");
            return fileStream;
        }

        async Task<T> DeserializeAsync<T>(string downloadUrl, string filePath, CancellationToken cancellationToken = default) where T : class
        {
            await using var fileStream = File.Exists(filePath) ? File.OpenRead(filePath) :
                await DownloadAsync(downloadUrl, filePath, cancellationToken).ConfigureAwait(false);
            var result = Deserialize<T>(fileStream);
            return result;
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
