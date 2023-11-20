global using Xunit;
global using Bible.Reader.Models;

namespace Bible.Reader.Tests
{
    public class BibleReaderTests
    {
        private static readonly string _baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private const string _relativeDirectory = "..\\..\\..\\..\\Bible.Data\\";
        private static string ExpandFilePath(string fileName) =>
            Path.Combine(_baseDirectory, _relativeDirectory, fileName);

        [Theory]
        [InlineData("zho-CUV.zefania")]
        public void GetFromXmlFile_WithXmlFilePath_ReturnsZefaniaBibleFormat(string fileName)
        {
            var filePath = ExpandFilePath(fileName);
            var bible = BibleReader.GetFromFile<XmlZefania>(filePath, FileType.Xml);
            Assert.NotNull(bible);
            Assert.Equal(66, bible.BibleBooks.Length);
            Assert.Equal(50, bible.BibleBooks[0].Chapters.Length);
            Assert.Equal(38, bible.BibleBooks[1].Chapters[39].Verses.Length);
        }
    }
}