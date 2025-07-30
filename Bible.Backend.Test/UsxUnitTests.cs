using System.Diagnostics;
using System.Text;
using Bible.Backend.Models;
using Bible.Backend.Services;
using Bible.Backend.Visitors;

namespace Bible.Backend.Tests
{
    public class UsxUnitTests
    {
        [Theory]
        [InlineData("WEBBE/3JN.usx")]
        public void BibleParser_WithValidUsx_DeserializesToMarkdown(string usxFilePath)
        {
            var deserializer = new XDocDeserializer();
            var book = deserializer.Deserialize<UsxScriptureBook>(usxFilePath);
            Assert.NotNull(book);
            var markdown = UsxToMarkdownVisitor.GetFullText(book);
            Debug.WriteLine(markdown);
        }

        [Fact]
        public void BibleParser_WithUsx_DeserializesToMarkdown()
        {
            var deserializer = new XDocDeserializer();
            var book = deserializer.DeserializeXml<UsxScriptureBook>(BibleUsxSamples.UsxWebbeMat5);
            Assert.NotNull(book);
            var html = UsxToHtmlVisitor.GetFullText(book);
            Debug.WriteLine(html);
        }

        [Theory]
        [InlineData("WEBBE", "3JN")]
        //[InlineData("WEBBE", "JUD")]
        public void XmlParser_WithValidUsx_Deserializes(string translation, string bookCode)
        {
            var xmlParser = new XDocDeserializer();
            var usxFilePath = Path.Combine(translation, $"{bookCode}.usx");
            var book = xmlParser.Deserialize<UsxScriptureBook>(usxFilePath);
            Assert.NotNull(book);
            var paragraphs = book.Content.OfType<UsxPara>().Where(b => b.Style == "p");
            foreach (var paragraph in paragraphs)
            {
                var stringBuilder = new StringBuilder();
                var isInitialized = false;
                foreach (var item in paragraph.Content)
                {
                    if (item is UsxMarker verseMarker &&
                        !string.IsNullOrEmpty(verseMarker.Number) &&
                        string.IsNullOrEmpty(verseMarker.EndId))
                    {
                        stringBuilder.AppendLine();
                        stringBuilder.Append(verseMarker.Number);
                        stringBuilder.Append(" ");
                    }
                    else if (item is UsxHeading text)
                    {
                        if (!isInitialized)
                        {
                            isInitialized = true;
                        }
                        else
                        {
                            stringBuilder.Append(" ");
                        }
                        stringBuilder.Append(text.Text);
                    }
                }
                Debug.WriteLine(stringBuilder.ToString());
                break;
            }
        }

        [Theory]
        [InlineData("WEBBE", "3JN")]
        //[InlineData("WEBBE", "JUD")]
        public void XDocParser_WithValidUsx_Deserializes(string translation, string bookCode)
        {
            var xmlParser = new XDocDeserializer();
            var usxFilePath = Path.Combine(translation, $"{bookCode}.usx");
            var book = xmlParser.Deserialize<UsxScriptureBook>(usxFilePath);
            Assert.NotNull(book);
            var stringBuilder = new StringBuilder();
            var paragraphs = book.Content.OfType<UsxContent>().Where(b => b.Style == "p");
            foreach (var paragraph in paragraphs)
            {
                foreach (var item in paragraph.Content)
                {
                    if (item is UsxMarker verseMarker &&
                        !string.IsNullOrEmpty(verseMarker.Number) &&
                        string.IsNullOrEmpty(verseMarker.EndId))
                    {
                        stringBuilder.Append(verseMarker.Number);
                        stringBuilder.Append(" ");
                    }
                    else if (item is UsxHeading text)
                    {
                        stringBuilder.Append(text.Text);
                    }
                }
                Debug.WriteLine(stringBuilder.ToString());
                break;
            }
        }
    }
}