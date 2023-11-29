global using Xunit;
global using Bible.Reader.Models;
using System.Xml.Linq;
using Bible.Reader.Services;

namespace Bible.Reader.Tests
{
    public class BibleReaderTests
    {
        private static readonly string _baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private const string _relativeDirectory = "..\\..\\..\\..\\Bible.Data\\";
        private static string ExpandFilePath(string fileName, FileType fileType) =>
            Path.Combine(_baseDirectory, _relativeDirectory, fileType.ToString(), fileName);
        private static string ExpandXsltPath(string fileName) =>
            Path.Combine(_baseDirectory, _relativeDirectory, "xslt", fileName);

        [Theory]
        [InlineData("eng/WEB")]
        public void GetXmlFile_WithXmlFilePath_ReturnsZefaniaBibleFormat(string fileName, FileType fileType = FileType.Xml)
        {
            var filePath = ExpandFilePath(fileName, fileType);
            var bible = BibleReader.GetFromFile<XmlZefania05>(filePath, fileType);
            Assert.NotNull(bible);
            Assert.Equal(66, bible.BibleBooks.Length);
            Assert.Equal(50, bible.BibleBooks[0].Chapters.Length);
            Assert.Equal(38, bible.BibleBooks[1].Chapters[39].Verses.Length);
        }

        [Theory]
        [InlineData("zho/OCCB/GEN")]
        public void GetFromFile_WithUsxFilePath_ReturnsUsxBibleFormat(string fileName, FileType fileType = FileType.Usx)
        {
            var filePath = ExpandFilePath(fileName, fileType);
            var bible = BibleReader.GetFromFile<Usx3>(filePath, fileType);
            Assert.NotNull(bible);
            var book = bible.Items.OfType<XmlUsx3Book>().First();
            Assert.Equal("GEN", book.Id);
            var chapter = bible.Items.OfType<XmlUsx3Chapter>().First();
            Assert.Equal(1, chapter.Number);
        }

        [Fact]
        public void GetTextBetweenUsxElements()
        {
            string xml = @"<usx>
                <chapter number='1' style='c' sid='TST 1'/>
                <verse number='1' style='v' sid='TST 1:1'/>Text to find.<verse eid='TST 1:1'/>
                <chapter eid='TST 1'/>
              </usx>";
            var textToFind = XDocument.Parse(xml).Descendants().DescendantNodes().OfType<XText>().First();
            Assert.Equal("Text to find.", textToFind.Value);
        }

        [Theory]
        [InlineData("zho/OCCB/GEN")]
        public void TransformUsx2Xml(string file)
        {
            BibleReader.TransformUsx2Xml($"{file}.usx");
        }
    }
}