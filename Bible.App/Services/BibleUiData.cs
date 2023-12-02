using Bible.App.Abstractions;
using Bible.Core.Abstractions;
using Bible.Core.Models;
using Bible.Reader.Models;
using Bible.Reader.Adapters;
using Bible.App.Models;
using System.Xml.Serialization;

namespace Bible.Reader.Services
{
    public sealed class BibleUiData : IUiDataService
    {
        private readonly IDataService<BibleModel> _dataService;

        public BibleUiData(IDataService<BibleModel> dataService)
        {
            _dataService = dataService;
        }

        /// <summary>
        /// Map XML file data to an object.
        /// </summary>
        /// <typeparam name="T">Object to map to.</typeparam>
        /// <param name="xmlFileName">XML file name.</param>
        /// <inheritdoc cref="XmlSerializer.Deserialize(Stream)"/>
        internal static async Task<T?> GetFromXmlFileAsync<T>(string xmlFileName, FileType fileType = FileType.Xml) where T : class
        {
            var typeName = fileType == FileType.Xml ? "xml" : fileType.ToString().ToLowerInvariant();
            (string prefix, string suffix) = (typeName, $".{typeName}");
            //string suffix = Path.GetExtension(xmlFileName);
            //string prefix = suffix.TrimStart('.');
            if (!Path.HasExtension(xmlFileName) || !xmlFileName.EndsWith(suffix))
                xmlFileName += suffix;
            var xmlFilePath = Path.Combine(prefix, xmlFileName);
            T? result = default;
            if (await FileSystem.AppPackageFileExistsAsync(xmlFilePath))
            {
                using var fileStream = await FileSystem.OpenAppPackageFileAsync(xmlFilePath);
                var serializer = new XmlSerializer(typeof(T));
                result = serializer.Deserialize(fileStream) as T;
            }
            else
            {
                //var xmlFilePath2 = Path.Combine("..", xmlFilePath);
                //bool found2 = await FileSystem.Current.AppPackageFileExistsAsync(xmlFilePath2);
                //var xmlFilePath3 = Path.Combine("..", xmlFilePath2);
                //bool found3 = await FileSystem.Current.AppPackageFileExistsAsync(xmlFilePath3);
                //var xmlFilePath4 = Path.Combine("Bible.Data", xmlFilePath3);
                //bool found4 = await FileSystem.Current.AppPackageFileExistsAsync(xmlFilePath4);
                //string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                //var path = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()?.Location);
                //var fullPath = Path.GetFullPath(xmlFilePath);
                //var appDataDirectory = FileSystem.Current.AppDataDirectory;
                //var folders1 = Directory.EnumerateDirectories(appDataDirectory).ToList();
                //var files1 = Directory.EnumerateFiles(appDataDirectory).ToList();
                //var cacheDirectory = FileSystem.Current.CacheDirectory;
                //var folders2 = Directory.EnumerateDirectories(cacheDirectory).ToList();
                //var files2 = Directory.EnumerateFiles(cacheDirectory).ToList();
                //var personalFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                //var folders3 = Directory.EnumerateDirectories(personalFolder).ToList();
                //var files3 = Directory.EnumerateFiles(personalFolder).ToList();
                System.Diagnostics.Debugger.Break();
            }
            return result;
        }

        private BibleModel? _bible = null;

        public BibleUiModel Load(string fileName, string suffix = ".xml")
        {
            _bible = _dataService.Load(fileName, suffix);
            return GetBible(_bible);
        }

        public async Task<BibleUiModel> LoadAsync(string fileName)
        {
            var info = fileName.Split('/');
            var translation = Path.GetFileName(info[1]);
            var bibleFile = await GetFromXmlFileAsync<XmlZefania05>(fileName);
            _bible = bibleFile?.ToBibleFormat(info[0], translation);
            return GetBible(_bible);
        }

        public static BibleUiModel GetBible(BibleModel? bible, bool addChapters = true)
        {
            var bibleModel = new BibleUiModel(bible?.Information?.Translation);
            if (bible != null)
            {
                foreach (var book in bible.Books)
                {
                    var bibleBook = new BookUiModel(book.Id, book.Reference.BookName, book.ChapterCount) { Copyright = bibleModel.Translation };
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
                    bibleModel.Add(bibleBook);
                }
            }
            return bibleModel;
        }
    }
}
