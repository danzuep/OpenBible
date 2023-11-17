global using Xunit;
using Bible.Data.Models;
using Bible.Data.Services;
using System.Net.Http.Json;
using System.Text.Json;
using System.Xml.Linq;

namespace Bible.Data.Tests
{
    public class BibleBookServiceTests : IDisposable
    {
        private readonly HttpClient _httpClient = new() { BaseAddress = new Uri(Constants.RawGitHubUserContentWebEndpoint) };
        private static readonly string _baseFilePath = Directory.GetCurrentDirectory(); // AppDomain.CurrentDomain.BaseDirectory
        private string GetJsonFilePath(string name) => Path.Combine(_baseFilePath, "Json", $"{name}.json");

        [Theory]
        [InlineData(BibleBookIndex.Genesis, "gn")]
        public async Task GetBibleBooksFromFileAsync_WithJsonFilePath_ReturnsBibleBooks(BibleBookIndex book, string abbreviation)
        {
            var jsonFilePath = GetJsonFilePath("kjv");
            var bible = await SerializerService.GetFromFileAsync<IReadOnlyList<SimpleBibleBookJson>>(jsonFilePath) ?? [];
            var result = bible[(int)book];
            Assert.Equal(book.ToString(), result.Name);
            Assert.Equal(abbreviation, result.Abbreviation);
        }

        [Theory]
        [InlineData(BibleBookIndex.SecondCorinthians, 1, 3)]
        public async Task GetBibleBooksFromFileAsync_WithJsonFilePath_ReturnsBibleBook(BibleBookIndex book, int chapter, int verse)
        {
            var jsonFilePath = GetJsonFilePath("kjv");
            var verseText = await SerializerService.GetChapterVerseFromFileAsync(jsonFilePath, (int)book, chapter, verse);
            Assert.NotNull(verseText);
        }

        [Theory]
        [InlineData(BibleBookIndex.SecondCorinthians, "2 Corinthians", "2co", "God of all comfort")] // 1:3
        public async Task GetBibleBooksFromFileStreamAsync_WithJsonFilePath_ReturnsBibleBook(BibleBookIndex bookIndex, string bookName, string abbreviation, string expected)
        {
            var jsonFilePath = GetJsonFilePath("kjv");
            var result = await SerializerService.GetBibleBookFromStreamAsync(jsonFilePath, (int)bookIndex);
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
            var jsonFilePath = GetJsonFilePath("books");
            var bible = await SerializerService.GetFromFileAsync<YouVersionBooksJsonResponse>(jsonFilePath);
            Assert.NotNull(bible);
            Assert.Equal(book.ToString(), bible.Books[(int)book].Book);
        }

        [Fact]
        public async Task GetTranslationsFromFileAsync_WithJsonFilePath_ReturnsBibleTranslations()
        {
            var jsonFilePath = GetJsonFilePath("translations");
            var bible = await SerializerService.GetFromFileAsync<Dictionary<string, string>>(jsonFilePath);
            Assert.NotNull(bible);
            Assert.True(bible["WEBUS"] == "9879dbb7cfe39e4d-01");
        }

        [Fact]
        public async Task GetVersionsFromFileAsync_WithJsonFilePath_ReturnsBibleVersions()
        {
            var jsonFilePath = GetJsonFilePath("versions");
            var bible = await SerializerService.GetFromFileAsync<Dictionary<string, int>>(jsonFilePath);
            Assert.NotNull(bible);
            Assert.True(bible.GetValueOrDefault("WEBUS") == 206);
        }

        [Theory]
        [InlineData("Romans", "12", "1", "web", "living sacrifice")]
        public async Task GetBibleFromJsonAsync_WithUrlPath_ReturnsBible(string book, string chapter, string verse, string version, string expected)
        {
            using var httpClient = new HttpClient { BaseAddress = new Uri(Constants.BibleWebEndpointSuperSearch) };
            var query = $"api?bible={version}&reference={book}+{chapter}:{verse}";
            var response = await httpClient.GetFromJsonAsync<BibleReferenceJsonResponse>(query); // ~1.3s
            Assert.NotNull(response);
            var result0 = response.Results[0];
            Assert.Equal(book, result0.BookName);
            var text = result0.Verses[version][chapter][verse].Text;
            Assert.Contains(expected, text);
        }

        [Theory]
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

        [Theory]
        [InlineData("1John", "4", "7")]
        public async Task GetBibleBookAsync_WithUrlPath_ReturnsJsonBibleBook(string book, string chapter, string verse)
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
            Assert.NotNull(verseResponse.Text);
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