using Bible.App.Abstractions;
using Bible.Core.Models;
using Bible.Reader.Models;
using Bible.Reader.Adapters;
using Bible.App.Models;
using System.Xml.Serialization;

namespace Bible.Reader.Services
{
    public sealed class BibleUiData : IUiDataService
    {
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

        private BibleModel? _bible = null;

        public async Task<BibleUiModel> LoadFileAsync(string fileName)
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
