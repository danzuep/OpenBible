using System.Diagnostics;
using System.Text;
using Bible.Backend.Models;
using Bible.Backend.Services;

namespace Bible.Backend.Test
{
    public class UsxUnitTests
    {
        [Theory]
        [InlineData("WEBBE/3JN.usx")]
        public void BibleParser_WithValidUsx_DeserializesToMarkdown(string usxFilePath)
        {
            var deserializer = new XDocParser();
            var book = deserializer.Deserialize<UsxScriptureBook>(usxFilePath);
            Assert.NotNull(book);
            var text = BibleParser.WriteToMarkdown(book);
            Debug.WriteLine(text);
        }

        [Theory]
        [InlineData("WEBBE", "3JN")]
        //[InlineData("WEBBE", "JUD")]
        public void XmlParser_WithValidUsx_Deserializes(string translation, string bookCode)
        {
            var xmlParser = new XmlParser();
            var usxFilePath = Path.Combine(translation, $"{bookCode}.usx");
            var book = xmlParser.Deserialize<UsxScriptureBook>(usxFilePath);
            Assert.NotNull(book);
            var paragraphs = book.Content.Where(b => b.Style == "p");
            foreach (var paragraph in paragraphs)
            {
                var stringBuilder = new StringBuilder();
                var isInitialized = false;
                foreach (var item in paragraph.Content)
                {
                    if (item is UsxMarker verseMarker &&
                        verseMarker.Number != 0 &&
                        string.IsNullOrEmpty(verseMarker.EndId))
                    {
                        stringBuilder.AppendLine();
                        stringBuilder.Append(verseMarker.Number);
                        stringBuilder.Append(" ");
                    }
                    else if (item is string textValue)
                    {
                        stringBuilder.Append(textValue);
                    }
                    else if (item is IUsxTextBase value)
                    {
                        if (!isInitialized)
                        {
                            isInitialized = true;
                        }
                        else
                        {
                            stringBuilder.Append(" ");
                        }
                        stringBuilder.Append(value.Text);
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
            var xmlParser = new XDocParser();
            var usxFilePath = Path.Combine(translation, $"{bookCode}.usx");
            var book = xmlParser.Deserialize<UsxScriptureBook>(usxFilePath);
            Assert.NotNull(book);
            var stringBuilder = new StringBuilder();
            var paragraphs = book.Content.Where(b => b.Style == "p");
            foreach (var paragraph in paragraphs)
            {
                foreach (var item in paragraph.Content)
                {
                    if (item is UsxMarker verseMarker &&
                        verseMarker.Number != 0 &&
                        string.IsNullOrEmpty(verseMarker.EndId))
                    {
                        stringBuilder.Append(verseMarker.Number);
                        stringBuilder.Append(" ");
                    }
                    else if (item is string textValue)
                    {
                        stringBuilder.Append(textValue);
                    }
                    else if (item is IUsxTextBase value)
                    {
                        stringBuilder.Append(value.Text);
                    }
                }
                Debug.WriteLine(stringBuilder.ToString());
                break;
            }
        }
    }
}