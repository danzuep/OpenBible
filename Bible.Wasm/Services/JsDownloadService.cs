using System.Collections.Concurrent;
using System.Net.Http.Headers;
using Microsoft.JSInterop;

namespace Bible.Wasm.Services
{
    public interface IDownloadService
    {
        /// <summary>
        /// Stream file content to a raw binary data buffer on the client.
        /// Typically, this approach is used for relatively small files (< 250 MB).
        /// </summary>
        ValueTask TryDownloadFileFromStream(Stream fileStream, string fileName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Download a file via a URL without streaming.
        /// Usually, this approach is used for relatively large files (> 250 MB).
        /// </summary>
        ValueTask TryDownloadFileFromUrl(Uri downloadUrl, CancellationToken cancellationToken = default);
    }

    /// <see href="https://learn.microsoft.com/en-us/aspnet/core/blazor/file-downloads?view=aspnetcore-7.0"/>
    public class JsDownloadService : IDownloadService
    {
        private readonly IJSRuntime _jsInterop;
        private readonly ILogger<JsDownloadService> logger;

        public JsDownloadService(IJSRuntime jsInterop, ILogger<JsDownloadService> logger)
        {
            _jsInterop = jsInterop;
            this.logger = logger;
        }

        private readonly ConcurrentDictionary<string, HttpClient> _httpClients = new();

        public async ValueTask DownloadFileFromStream(Stream fileStream, string fileName)
        {
            if (fileStream == null)
                throw new ArgumentNullException(nameof(fileStream));
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException(nameof(fileName));
            fileStream.Position = 0;
            using var streamRef = new DotNetStreamReference(fileStream);
            await _jsInterop.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef).ConfigureAwait(false);
            return;
        }

        public async ValueTask TryDownloadFileFromStream(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(fileName))
                    await Task.Run(() => DownloadFileFromStream(fileStream, fileName), cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to download {fileName}.");
            }
            return;
        }

        public async ValueTask DownloadFileFromUrl(string fileUrl, string? fileName = null)
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
                throw new ArgumentNullException(nameof(fileUrl));
            if (string.IsNullOrWhiteSpace(fileName))
                fileName = Path.GetFileName(fileUrl);
            if (!string.IsNullOrWhiteSpace(fileName))
                await _jsInterop.InvokeVoidAsync("triggerFileDownload", fileName, fileUrl).ConfigureAwait(false);
            return;
        }

        public async ValueTask TryDownloadFileFromUrl(Uri downloadUrl, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogDebug($"Download address is {downloadUrl}.");
                if (!_httpClients.TryGetValue(downloadUrl.Host, out HttpClient? client))
                {
                    client = new HttpClient
                    {
                        BaseAddress = downloadUrl
                    };
                    _httpClients.TryAdd(downloadUrl.Host, client);
                    logger.LogDebug($"HttpClient added for {client.BaseAddress}.");
                }
                using var requestMessage = new HttpRequestMessage(HttpMethod.Head, downloadUrl.PathAndQuery);
                logger.LogTrace($"HttpClient request Uri is {requestMessage.RequestUri}.");
                using var responseMessage = await client.SendAsync(requestMessage, cancellationToken).ConfigureAwait(false);
                responseMessage.EnsureSuccessStatusCode();
                await Task.Run(() => DownloadFileFromUrl(downloadUrl.OriginalString), cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Automatic download of {Url} failed.", downloadUrl);
                var url = downloadUrl.ToString();
                logger.LogInformation($"Opening a new tab to {url}.");
                await _jsInterop.InvokeVoidAsync("open", url, "_blank").ConfigureAwait(false);
            }
            return;
        }

        public static async ValueTask<Exception?> TryDownloadFileFromUrl(Uri downloadUrl, HttpClient? client = null, string? jwt = null, CancellationToken cancellationToken = default)
        {
            Exception? exception = null;
            try
            {
                client ??= new HttpClient
                {
                    BaseAddress = downloadUrl
                };
                if (!string.IsNullOrWhiteSpace(jwt))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
                }
                using var requestMessage = new HttpRequestMessage(HttpMethod.Get, downloadUrl.PathAndQuery);
                using var responseMessage = await client.SendAsync(requestMessage, cancellationToken).ConfigureAwait(false);
                responseMessage.EnsureSuccessStatusCode();
                await DownloadFileAsync(responseMessage.Content).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            return exception;


            async Task DownloadFileAsync(HttpContent content, string? fileName = null, bool overwrite = true)
            {
                if (string.IsNullOrWhiteSpace(fileName))
                    fileName = content.Headers.ContentDisposition?.FileName != null ?
                        content.Headers.ContentDisposition.FileName : $"{Guid.NewGuid()}:N";
                if (!overwrite && File.Exists(fileName))
                    throw new InvalidOperationException($"File {Path.GetFullPath(fileName)} already exists.");
                using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await content.CopyToAsync(fileStream, cancellationToken);
                }
            }
        }
    }
}
