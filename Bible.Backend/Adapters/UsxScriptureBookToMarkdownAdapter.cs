using System.Net;
using System.Text;
using Bible.Backend.Models;

namespace Bible.Backend.Adapters
{
    public static class UsxScriptureBookToMarkdownAdapter
    {
        public static string ToMarkdown(this UsxScriptureBook? book, bool addFootnotes = false, bool addInlineStrongs = false)
        {
            if (book == null)
            {
                return string.Empty;
            }

            var stringBuilder = new StringBuilder();
            var footnotes = new Dictionary<string, UsxCharRef[]>();

            foreach (var content in book.Content)
            {
                if (content is UsxPara heading && heading.Style == "h" &&
                    heading.Content.FirstOrDefault() is string bookName)
                {
                    stringBuilder.AppendLine($"## {bookName}");
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

        private static void AppendMarkdownPara(this StringBuilder stringBuilder, UsxPara paragraph, IDictionary<string, UsxCharRef[]> footnotes, bool addNotes, bool addStrongs)
        {
            foreach (var item in paragraph.Content)
            {
                if (item is string textValue)
                {
                    stringBuilder.Append(textValue);
                }
                else if (item is IUsxTextBase value)
                {
                    stringBuilder.Append(UsxToMarkdown(value, addStrongs));
                }
                else if (item is UsxMarker verseMarker && !string.IsNullOrEmpty(verseMarker.Number))
                {
                    stringBuilder.AppendFormat("*{0}* ", verseMarker.Number);
                }
                else if (addNotes && item is UsxNote usxNote)
                {
                    var footnoteId = footnotes.Count + 1;
                    var key = $"{usxNote.Caller}{footnoteId:000}";
                    stringBuilder.AppendFormat("[^{0}]", key);
                    footnotes.Add(key, usxNote.Entries);
                }
            }
        }

        private static void AppendMarkdownNotes(this StringBuilder stringBuilder, IDictionary<string, UsxCharRef[]> footnotes)
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

        private static string ToMarkdownRefs(this IEnumerable<UsxCharRef> usxChars)
        {
            var stringBuilder = new StringBuilder();

            foreach (var usxChar in usxChars)
            {
                foreach (var item in usxChar.Content)
                {
                    stringBuilder.Append(UsxToMarkdown(item));
                }
            }

            return stringBuilder.ToString();
        }

        private static string UsxToMarkdown(object item, bool addStrongs = false)
        {
            if (item == null)
            {
                return string.Empty;
            }

            if (item is string text)
            {
                return text;
            }
            else if (item is UsxCharContent usxCharWj && usxCharWj.Content != null)
            {
                return string.Concat(usxCharWj.Content.Select(c => UsxToMarkdown(c, addStrongs)));
            }
            else if (addStrongs && item is UsxCharStrong usxCharW && !string.IsNullOrEmpty(usxCharW.Strong))
            {
                return string.Format("[{0}]({1}{2})", usxCharW.Text,
                    "https://www.blueletterbible.org/lexicon/",
                    WebUtility.UrlEncode(usxCharW.Strong));
            }
            else if (item is UsxReference crossReference)
            {
                return string.Format("[{0}]({1}{2})", crossReference.Location,
                    "https://www.biblegateway.com/passage/?search=",
                    WebUtility.UrlEncode(crossReference.Location));
            }
            else if (item is IUsxTextBase value)
            {
                return value.Text;
            }

            return string.Empty;
        }
    }
}
