global using Xunit;
using Bible.Data.Models;
using Bible.Data.Services;
using System.Net.Http.Json;
using System.Xml.Linq;

namespace Bible.Data.Tests
{
    public class BibleBookServiceTests : IDisposable
    {
        private readonly HttpClient _httpClient = new() { BaseAddress = new Uri(Constants.RawGitHubUserContentWebEndpoint) };

        [Theory]
        [InlineData(BibleBookIndex.Genesis, "gn")]
        public async Task GetBibleBooksFromFileAsync_WithJsonFilePath_ReturnsBibleBooks(BibleBookIndex book, string abbreviation)
        {
            var bible = await SerializerService.GetFromJsonFileAsync<IReadOnlyList<SimpleBibleBookJson>>("KJV") ?? [];
            var result = bible[(int)book];
            Assert.Equal(book.ToString(), result.Name);
            Assert.Equal(abbreviation, result.Abbreviation);
        }

        [Theory]
        [InlineData("Genesis", 50)]
        [InlineData("Exodus", 40)]
        public void GetBibleBookFromFile_WithJsonFilePath_ReturnsConvertedBibleBooks(string bookName, int chapterCount)
        {
            var books = SerializerService.GetBibleBooks();
            var book = books.GetBibleBook(bookName);
            Assert.NotNull(book);
            Assert.Equal(chapterCount, book.ChapterCount);
        }

        [Theory]
        [InlineData("Jesus", 943)]
        public async Task SearchBibleAsync_WithJsonFilePath_ReturnsVerses(string searchQuery, int expectedCount)
        {
            var books = SerializerService.GetBibleBooks();
            var verses = await books.SearchBibleAsync(searchQuery);
            Assert.NotNull(verses);
            Assert.Equal(expectedCount, verses.Count);
        }

        [Theory]
        [InlineData(BibleBookIndex.SecondCorinthians, 1, 3)]
        public async Task GetBibleBooksFromFileAsync_WithJsonFilePath_ReturnsBibleBook(BibleBookIndex book, int chapter, int verse)
        {
            var verseText = await SerializerService.GetChapterVerseFromFileAsync("KJV", (int)book, chapter, verse);
            Assert.NotNull(verseText);
        }

        [Theory]
        [InlineData(BibleBookIndex.SecondCorinthians, "2 Corinthians", "2co", "God of all comfort")] // 1:3
        public async Task GetBibleBooksFromFileStreamAsync_WithJsonFilePath_ReturnsBibleBook(BibleBookIndex bookIndex, string bookName, string abbreviation, string expected)
        {
            var result = await SerializerService.GetBibleBookFromStreamAsync("KJV", (int)bookIndex);
            Assert.NotNull(result);
            Assert.Equal(bookName, result.Name);
            Assert.Equal(abbreviation, result.Abbreviation);
            var verseText = result.Content[0][2];
            Assert.Contains(expected, verseText);
        }

        [Theory]
        [InlineData(BibleBookIndex.Genesis)]
        public async Task GetBooksFromFileAsync_WithJsonFilePath_ReturnsBibleBooks(BibleBookIndex book)
        {
            var bible = await SerializerService.GetFromJsonFileAsync<YouVersionBooksJsonResponse>("books");
            Assert.NotNull(bible);
            Assert.Equal(book.ToString(), bible.Books[(int)book].Book);
        }

        [Theory]
        [InlineData("GEN", "Genesis", 50)]
        public async Task GetBibleBookMetadataFromFileAsync_WithJsonFilePath_ReturnsBibleBooks(string bookId, string name, int chapters)
        {
            var bible = await SerializerService.GetFromJsonFileAsync<Dictionary<string, BibleBookMetadataJson>>("BibleCanonBooks.en");
            Assert.NotNull(bible);
            Assert.Equal(name, bible[bookId].Name);
            Assert.Equal(chapters, bible[bookId].Chapters);
        }

        [Fact]
        public async Task GetTranslationsFromFileAsync_WithJsonFilePath_ReturnsBibleTranslations()
        {
            var bible = await SerializerService.GetFromJsonFileAsync<Dictionary<string, string>>("translations");
            Assert.NotNull(bible);
            Assert.Equal("9879dbb7cfe39e4d-01", bible["WEBUS"]);
        }

        [Fact]
        public async Task GetVersionsFromFileAsync_WithJsonFilePath_ReturnsBibleVersions()
        {
            var bible = await SerializerService.GetFromJsonFileAsync<Dictionary<string, int>>("versions");
            Assert.NotNull(bible);
            Assert.Equal(206, bible.GetValueOrDefault("WEBUS"));
        }

        [Theory]
        [InlineData("1 John 4:7", "webbe", "love one another")]
        public async Task GetVerseAsync_WithUrlPath_ReturnsJson(string reference, string translation, string expected)
        {
            using var httpClient = new HttpClient();
            var query = $"{Constants.BibleWebEndpointBibleApi}{reference}?translation={translation}";
            var stream = await httpClient.GetStreamAsync(query);
            var verse = await SerializerService.GetPropertyFromStreamAsync(stream); // ~1.1s
            Assert.NotNull(verse);
            Assert.Contains(expected, verse);
        }

        [Theory]
        [InlineData("1John", "4", "7", "love one another")]
        public async Task GetBibleBookAsync_WithUrlPath_ReturnsJsonBibleBook(string book, string chapter, string verse, string expected)
        {
            var query = $"{Constants.RawGitHubUserContentBibleKjvPath}{book}.json";
            var response = await _httpClient.GetFromJsonAsync<SimpleBookJson>(query); // ~0.5s
            Assert.NotNull(response?.Book);
            var chapterIndex = int.Parse(chapter) - 1;
            var chapterResponse = response.Chapters[chapterIndex];
            Assert.Equal(chapter, chapterResponse.Chapter);
            var verseIndex = int.Parse(verse) - 1;
            var verseResponse = chapterResponse.Verses[verseIndex];
            Assert.Equal(verse, verseResponse.Verse);
            Assert.Contains(expected, verseResponse.Text);
        }

        [Theory]
        [InlineData("Romans", "12", "1", "web", "living sacrifice")]
        public async Task GetBibleFromJsonAsync_WithUrlPath_ReturnsBible(string book, string chapter, string verse, string version, string expected)
        {
            using var httpClient = new HttpClient { BaseAddress = new Uri(Constants.BibleWebEndpointSuperSearch) };
            var query = $"api?bible={version}&reference={book}+{chapter}:{verse}";
            var response = await httpClient.GetFromJsonAsync<BibleReferenceResponseJson>(query); // ~1.3s
            Assert.NotNull(response);
            var result0 = response.Results[0];
            Assert.Equal(book, result0.BookName);
            var text = result0.Verses[version][chapter][verse].Text;
            Assert.Contains(expected, text);
        }

        [Theory(Skip = "API key required.")]
        [InlineData(Constants.BibleYouVersionWebbeId, "ROM.12.1")]
        public async Task GetBibleBooksAsync_WithUrlPath_ReturnsBible(string bibleId, string verseId)
        {
            using var httpClient = new HttpClient { BaseAddress = new Uri(Constants.BibleWebEndpointYouVersion1) };
            httpClient.DefaultRequestHeaders.Add("api-key", Constants.BibleYouVersionApiKey);
            var query = $"v1/bibles/{bibleId}/verses/{verseId}?content-type=json&include-notes=false&include-titles=false&include-chapter-numbers=false&include-verse-numbers=false&include-verse-spans=false&use-org-id=false";
            var response = await httpClient.GetFromJsonAsync<YouVersionVersesJsonResponse>(query); // ~2.0s
            Assert.NotNull(response?.Data?.Id);
            Assert.Equal(verseId, response.Data.Id);
        }

        [Theory(Skip = "Takes too long (~2.8s).")]
        [InlineData("eng-gb-webbe.usfx.xml", "GEN", "1", "1")]
        public async Task GetBibleAsync_WithUrlPath_ReturnsXmlBible(string fileName, string bookId, string chapter, string verse)
        {
            var query = Path.Combine(Constants.RawGitHubUserContentOpenBiblesPath, fileName).Replace("\\", "/");
            using var xmlStream = await _httpClient.GetStreamAsync(query);
            var doc = await XDocument.LoadAsync(xmlStream, LoadOptions.None, CancellationToken.None);
            var bookElement = doc.Descendants("book")
                .FirstOrDefault(e => e.Attribute("id")?.Value == bookId);
            var chapterElement = bookElement?.Descendants("c")
                .FirstOrDefault(e => e.Attribute("id")?.Value == chapter);
            var verseElement = chapterElement?.ElementsAfterSelf()
                .FirstOrDefault(e => e.Descendants("v").Any(p => p.Attribute("id")?.Value == verse));
            var verseContent = verseElement?.Descendants("v").FirstOrDefault();
            var verseId = verseContent?.Attribute("bcv")?.Value;
            Assert.NotNull(verseId);
            Assert.Equal($"{bookId}.{chapter}.{verse}", verseId);
        }

        public void Dispose() =>
            _httpClient.Dispose();
    }
}