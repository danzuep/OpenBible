using Bible.Backend.Abstractions;
using Bible.Core.Models;
using Bible.Data;
using Bible.Usx.Models;
using Bible.Usx.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Unihan.Models;
using Unihan.Services;

namespace Bible.Backend.Services
{
    public class UsjBookService
    {
        private readonly ILogger _logger;
        private readonly IStorageService _storageService;
        private readonly UsxToUsjConverter _usxToUsjConverter;
        private readonly UnihanService _unihanService;

        public UsjBookService(IStorageService storageService, UnihanService? unihanService = null, UsxToUsjConverter? usxToUsjConverter = null, ILogger? logger = null)
        {
            _storageService = storageService;
            _unihanService = unihanService ?? new UnihanService(storageService, logger);
            _usxToUsjConverter = usxToUsjConverter ?? new UsxToUsjConverter();
            _logger = logger ?? NullLogger.Instance;
        }

        public async Task<UsjBook?> GetBookAsync(string? language, string? version, string? book, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(language) || string.IsNullOrEmpty(version) || string.IsNullOrEmpty(book))
            {
                _logger.LogWarning("Invalid parameters for GetBibleBookAsync: language={Language}, version={Version}, book={Book}", language, version, book);
                return null;
            }
            var metadata = new BibleBookMetadata
            {
                IsoLanguage = language,
                BibleVersion = version,
                BookCode = book
            };
            return await GetBookAsync(metadata, cancellationToken);
        }

        public async Task<UsjBook> GetBookAsync(BibleBookMetadata bibleBookMetadata, CancellationToken cancellationToken = default)
        {
            try
            {
                var usjBook = await _storageService.GetSerializedItemAsync<UsjBook>(bibleBookMetadata.ToString());
                if (usjBook == null)
                {
                    await using var usxStream = ResourceHelper.GetUsxBookStream(bibleBookMetadata);
                    usjBook = await _usxToUsjConverter.ConvertUsxStreamToUsjBookAsync(usxStream, cancellationToken);
                    _ = SetSerializedItemAsync(bibleBookMetadata, usjBook, cancellationToken);
                    //var unihanLanguage = new UnihanLanguage(bibleBookMetadata.IsoLanguage);
                    //if (unihanLanguage.Field != UnihanField.Unknown)
                    //    _ = AddUnihanAsync(usjBook, unihanLanguage.Field);
                }
                return usjBook;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deserializing USX to USJ book");
                throw;
            }
        }

        public async Task AddUnihanAsync(IUsjNode usjNode, UnihanField type, CancellationToken cancellationToken = default)
        {
            if (usjNode is UsjBook usjBook)
            {
                foreach (var para in usjBook.Content.OfType<UsjPara>())
                {
                    if (para.Content == null) continue;
                    foreach (var node in para.Content)
                    {
                        await AddUnihanAsync(node, type);
                    }
                }
            }
            else if (usjNode is UsjText usjText && usjText.Text is string text)
            {
                var metadata = await _unihanService.ParseUnihanRunesAsync(text, type);
                var words = new List<UsjChar>();
                foreach (var word in metadata.Words)
                {
                    if (word.Metadata == null)
                    {
                        words.Add(new UsjChar { Text = word.Text });
                        continue;
                    }
                    words.Add(new UsjChar
                    {
                        Text = word.Text,
                        Metadata = string.Join("; ", word.Metadata)
                    });
                }
                //usjNode = new UsjPara { Content = words.ToArray() };
            }
        }

        private async Task SetSerializedItemAsync(BibleBookMetadata bibleBookMetadata, UsjBook usjBook, CancellationToken cancellationToken = default)
        {
            //var chapters = usjBook.Content.OfType<UsjChapterMarker>().ToList();
            await _storageService.SetSerializedItemAsync(bibleBookMetadata.ToString(), usjBook);
        }
    }
}
