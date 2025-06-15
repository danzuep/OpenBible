using System.Text;
using Bible.Backend.Models;

namespace Bible.Backend.Services
{
    public class BibleParser
    {
        public static string WriteToMarkdown(UsxScriptureBook? book)
        {
            if (book == null)
            {
                return string.Empty;
            }

            var stringBuilder = new StringBuilder();

            foreach (var paragraph in book.Content.OfType<UsxPara>())
            {
                if (paragraph.Style == "h" &&
                    paragraph.Content.FirstOrDefault() is string bookName)
                {
                    stringBuilder.AppendLine($"## {bookName}");
                    stringBuilder.AppendLine();
                    //stringBuilder.AppendLine($"### Chapter {book.Content.Number}");
                }
                else if (paragraph.Style == "p")
                {
                    foreach (var item in paragraph.Content)
                    {
                        if (item is string textValue)
                        {
                            stringBuilder.Append(textValue);
                        }
                        else if (item is IUsxTextBase value)
                        {
                            stringBuilder.Append(value.Text);
                        }
                        else if (item is UsxMarker verseMarker && verseMarker.Number != 0)
                        {
                            stringBuilder.Append($"[{verseMarker.Number}]");
                        }
                        else if (item is UsxLineBreak optbreak)
                        {
                            stringBuilder.AppendLine();
                        }
                        else if (item is UsxMilestone milestone)
                        {
                        }
                        else if (item is UsxReference reference)
                        {
                            stringBuilder.Append($"[({reference.Location})]");
                        }
                        else if (item is UsxNote footNote)
                        {
                            stringBuilder.Append($"[^{footNote.Caller}]");
                        }
                    }

                    // Add double space + newline for Markdown line break after paragraph
                    stringBuilder.AppendLine("  ");
                }
            }

            return stringBuilder.ToString();
        }
    }
}
