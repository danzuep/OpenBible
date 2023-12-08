using Bible.App.Abstractions;
using Bible.Core.Models;
using Bible.Reader.Models;
using Bible.Reader.Adapters;
using Bible.App.Models;
using System.Xml.Serialization;
using System.Net;

namespace Bible.Reader.Services
{
    public sealed class BibleUiDataService : IUiDataService
    {
        private readonly WebZefaniaService _webZefaniaService;

        public BibleUiDataService(WebZefaniaService webZefaniaService)
        {
            _webZefaniaService = webZefaniaService;
        }

        public IAsyncEnumerable<WebBibleInfoModel> AsyncGetBibleInfo(CancellationToken cancellationToken = default) =>
            _webZefaniaService.AsyncGetBibleInfo(FileSystem.AppDataDirectory, cancellationToken);

        public async Task<BibleUiModel?> GetBibleAsync(string identifier, CancellationToken cancellationToken = default)
        {
            var bibleInfo = await _webZefaniaService.GetBibleInfoAsync(FileSystem.AppDataDirectory, identifier, cancellationToken);
            var bibleModel = await _webZefaniaService.GetBibleAsync(bibleInfo, FileSystem.AppDataDirectory, cancellationToken);
            if (bibleModel == null && Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
                throw new WebException("No Internet access, check the network connectivity.", WebExceptionStatus.ConnectFailure);
            var bibleUiModel = GetBible(bibleModel);
            return bibleUiModel;
        }

        public async Task<BibleUiModel> LoadFileAsync(string fileName)
        {
            var info = fileName.Split('/');
            var translation = Path.GetFileName(info[1]);
            var bibleFile = await GetFromXmlFileAsync<XmlZefania05>(fileName);
            var bibleModel = bibleFile?.ToBibleFormat(info[0], translation);
            return GetBible(bibleModel);
        }

        /// <summary>
        /// Map XML file data to an object.
        /// </summary>
        /// <typeparam name="T">Object to map to.</typeparam>
        /// <param name="xmlFileName">XML file name.</param>
        /// <inheritdoc cref="XmlSerializer.Deserialize(Stream)"/>
        internal static async Task<T?> GetFromXmlFileAsync<T>(string xmlFileName, string suffix = ".xml") where T : class
        {
            if (!Path.HasExtension(xmlFileName) || !xmlFileName.EndsWith(suffix))
                xmlFileName += suffix;
            var xmlFilePath = Path.Combine(suffix.TrimStart('.'), xmlFileName);
            T? result = default;
            if (await FileSystem.AppPackageFileExistsAsync(xmlFilePath))
            {
                using var fileStream = await FileSystem.OpenAppPackageFileAsync(xmlFilePath);
                var serializer = new XmlSerializer(typeof(T));
                result = serializer.Deserialize(fileStream) as T;
            }
            return result;
        }

        static BibleUiModel GetBible(BibleModel? bible, bool addChapters = true)
        {
            var bibleUiModel = new BibleUiModel(bible?.Information?.Translation);
            if (bible != null)
            {
                foreach (var book in bible.Books)
                {
                    var bibleBook = new BookUiModel(book.Id, book.Reference.BookName, book.ChapterCount)
                    {
                        Copyright = bibleUiModel.Translation
                    };
                    if (addChapters)
                    {
                        foreach (var chapter in book.Chapters)
                        {
                            var bibleChapter = new ChapterUiModel(chapter.Id);
                            foreach (var verse in chapter.Verses)
                            {
                                var bibleVerse = new VerseUiModel(verse.Id, verse.Text);
                                bibleChapter.Add(bibleVerse);
                            }
                            bibleBook.Add(bibleChapter);
                        }
                    }
                    bibleUiModel.Add(bibleBook);
                }
            }
            return bibleUiModel;
        }
    }
}
