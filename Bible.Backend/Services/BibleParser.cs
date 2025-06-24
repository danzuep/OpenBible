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
                    paragraph.Content.FirstOrDefault() is UsxHeading bookName)
                {
                    stringBuilder.AppendLine($"## {bookName.Text}");
                    stringBuilder.AppendLine();
                    //stringBuilder.AppendLine($"### Chapter {book.Content.Number}");
                }
                else if (paragraph.Style == "p")
                {
                    foreach (var item in paragraph.Content)
                    {
                        if (item is UsxHeading text)
                        {
                            stringBuilder.Append(text.Text);
                        }
                        else if (item is UsxMarker verseMarker && !string.IsNullOrEmpty(verseMarker.Number))
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
                        else if (item is UsxCrossReference reference)
                        {
                            stringBuilder.Append($"[({reference.Location})]");
                        }
                        else if (item is UsxFootnote footNote)
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
