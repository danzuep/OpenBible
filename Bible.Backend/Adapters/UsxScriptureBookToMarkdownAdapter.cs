using System.Net;
using System.Text;
using Bible.Backend.Models;
using Bible.Backend.Services;

namespace Bible.Backend.Adapters
{
    public static class UsxScriptureBookToMarkdownAdapter
    {
        public static string ToMarkdown(this UsxScriptureBook book, bool addFootnotes = false, bool addInlineStrongs = false)
        {
            var visitor = UsxToMarkdownVisitor.Create(book);
            var markdown = visitor.GetMarkdown();
            return markdown;
        }

        private static string UsxToMarkdown(this UsxScriptureBook? book, bool addFootnotes = false, bool addInlineStrongs = false)
        {
            if (book == null)
            {
                return string.Empty;
            }

            var stringBuilder = new StringBuilder();
            var footnotes = new Dictionary<string, UsxFootnote>();

            foreach (var content in book.Content)
            {
                if (content is UsxPara heading && heading.Style == "h" &&
                    heading.Content.FirstOrDefault() is UsxHeading bookName)
                {
                    stringBuilder.AppendLine($"## {bookName.Text}");
                }
                else if (content is UsxMarker chapterMarker && !string.IsNullOrEmpty(chapterMarker.Number))
                {
                    stringBuilder.AppendLine($"### {chapterMarker.Number}");
                }
                else if (content is UsxPara paragraph && paragraph.Content != null)
                {
                    stringBuilder.AppendMarkdownPara(paragraph, footnotes, addFootnotes, addInlineStrongs);
                }
            }

            stringBuilder.AppendMarkdownNotes(footnotes);

            var fullText = stringBuilder.ToString();
                //.Replace(" ", "&nbsp;")
                //.Replace("\n", "\n> ");

            return fullText;
        }

        private static void AppendMarkdownPara(this StringBuilder stringBuilder, UsxPara paragraph, IDictionary<string, UsxFootnote> footnotes, bool addNotes, bool addStrongs)
        {
            foreach (var item in paragraph.Content)
            {
                if (item is UsxHeading text)
                {
                    stringBuilder.Append(text.Text);
                }
                else if (item is UsxMarker verseMarker && !string.IsNullOrEmpty(verseMarker.Number))
                {
                    stringBuilder.AppendFormat("*{0}* ", verseMarker.Number);
                }
                else if (addNotes && item is UsxFootnote usxNote)
                {
                    var footnoteId = footnotes.Count + 1;
                    var key = $"{usxNote.Caller}{footnoteId:000}";
                    stringBuilder.AppendFormat("[^{0}]", key);
                    footnotes.Add(key, usxNote);
                }
                else
                {
                    stringBuilder.Append(UsxToMarkdown(item, addStrongs));
                }
            }
        }

        private static void AppendMarkdownNotes(this StringBuilder stringBuilder, IDictionary<string, UsxFootnote> footnotes)
        {
            stringBuilder.AppendLine();
            stringBuilder.AppendLine();

            if (footnotes.Any())
            {
                stringBuilder.AppendLine("---");
                stringBuilder.AppendLine();

                foreach (var footnote in footnotes)
                {
                    stringBuilder.AppendFormat("[^{0}]: {1}",
                        footnote.Key,
                        footnote.Value.ToMarkdownRefs());
                    stringBuilder.AppendLine();
                }

                stringBuilder.AppendLine();
            }
        }

        private static string ToMarkdownRefs(this UsxFootnote usxChar)
        {
            var stringBuilder = new StringBuilder();

            foreach (var item in usxChar.Content)
            {
                if (usxChar.Content == null)
                {
                    continue;
                }
                stringBuilder.Append(UsxToMarkdown(item));
            }

            return stringBuilder.ToString();
        }

        private static string UsxToMarkdown(object item, bool addStrongs = false)
        {
            if (item == null)
            {
                return string.Empty;
            }

            if (item is UsxHeading text)
            {
                return text.Text;
            }
            else if (item is UsxChar usxCharWj && usxCharWj.Content != null)
            {
                return string.Concat(usxCharWj.Content.Select(c => UsxToMarkdown(c, addStrongs)));
            }
            else if (addStrongs && item is UsxChar usxCharW && !string.IsNullOrEmpty(usxCharW.Strong))
            {
                var usxCharWText = UsxToMarkdown(usxCharW.Content, addStrongs);
                return string.Format("[{0}]({1}{2})", usxCharWText,
                    "https://www.blueletterbible.org/lexicon/",
                    WebUtility.UrlEncode(usxCharW.Strong));
            }
            else if (item is UsxCrossReference crossReference)
            {
                return string.Format("[{0}]({1}{2})", crossReference.Location,
                    "https://www.biblegateway.com/passage/?search=",
                    WebUtility.UrlEncode(crossReference.Location));
            }
            else if (item is UsxContent usx)
            {
                UsxToMarkdown(usx.Content, addStrongs);
            }

            return string.Empty;
        }
    }
}
