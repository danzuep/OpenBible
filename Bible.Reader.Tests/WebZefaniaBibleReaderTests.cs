using Bible.Reader.Services;

namespace Bible.Reader.Tests
{
    public class WebZefaniaBibleReaderTests
    {
        private readonly WebZefaniaService _webZefaniaService = new();

        [Theory]
        [InlineData("https://raw.githubusercontent.com/kohelet-net-admin/zefania-xml-bibles/master/Bibles/ENG/World%20English%20Bible/SF_2009-01-20_ENG_WEB_(WORLD%20ENGLISH%20BIBLE).xml")]
        public async Task DownloadBibleAsync_WithUrlPath_ReturnsDownloadedXmlBible(string downloadUrl)
        {
            string personalFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            //var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var downloadFolderPath = Path.Combine(personalFolder, "Downloads");
            await _webZefaniaService.DownloadBibleAsync(downloadUrl, downloadFolderPath);
        }

        [Theory]
        [InlineData("eng", "WEB", "World English Bible", "https://raw.githubusercontent.com/kohelet-net-admin/zefania-xml-bibles/master/Bibles/ENG/World%20English%20Bible/SF_2009-01-20_ENG_WEB_(WORLD%20ENGLISH%20BIBLE).xml")]
        public void GetFileNameFromUrl(string iso3Language, string identifier, string bibleName, string downloadUrl)
        {
            var uri = new Uri(downloadUrl);

            string? languageCode = null;
            string? translationName = null;
            string? translationAcronym = null;
            if (uri.Segments.Length >= 8)
            {
                languageCode = uri.Segments[5].Trim('/').ToLowerInvariant();
                translationName = Uri.UnescapeDataString(uri.Segments[6].Trim('/'));
                var fileName = Uri.UnescapeDataString(uri.Segments[7].Trim('/'));
                var fileNameParts = fileName.Split('_');
                // special cases for sf_czecep.xml and sf_czech_bkr.xml
                if (fileNameParts.Length == 2 || fileNameParts.Length == 3)
                    translationAcronym = fileNameParts[^1].Split('.')[0].ToUpperInvariant();
                else if (fileNameParts.Length >= 4)
                    translationAcronym = fileNameParts[^2];
            }

            Assert.Equal(iso3Language, languageCode);
            Assert.Equal(identifier, translationAcronym);
            Assert.Equal(bibleName, translationName);
        }
    }
}