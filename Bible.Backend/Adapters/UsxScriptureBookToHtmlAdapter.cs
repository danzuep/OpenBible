using System.Globalization;
using System.Net;
using System.Text;
using Bible.Backend.Models;

namespace Bible.Backend.Adapters
{
    public static class UsxScriptureBookToHtmlAdapter
    {
        private static readonly string _wjStyle = @"
  /* Style for words of Jesus */
  .words-of-jesus {
    color: #b22222; /* Firebrick red */
    font-weight: bold;
  }
";
        private static readonly string _wStyle = @"
  /* Style for Strong's number hover notes */
  .word-link {
    position: relative;
  }
  .word-link::after {
    content: attr(link-data);
    position: absolute;
    left: 50%;
    bottom: 120%;
    transform: translateX(-50%);
    background: #333;
    color: #fff;
    padding: 3px 6px;
    border-radius: 4px;
    white-space: nowrap;
    font-size: 0.8em;
    opacity: 0;
    pointer-events: none;
    transition: opacity 0.3s ease;
    z-index: 10;
  }
  .word-link:hover::after {
    opacity: 1;
  }
";

        private static readonly string _aStyle = @"
  /* Remove the default blue underline from URLs */
  a {
    text-decoration: none;
    color: inherit;
  }
";

        public static string ToHtml(this UsxScriptureBook? book, bool enableFootnotes = true, bool enableStrongs = false, bool enableRedLetters = true)
        {
            if (book == null)
            {
                return string.Empty;
            }

            var stringBuilder = new StringBuilder();
            var footnotes = new List<string>();
            var bookName = string.Empty;
            var chapterNumber = 0;

            stringBuilder.AppendFormat("<style>{0}", _aStyle);
            if (enableRedLetters)
            {
                stringBuilder.Append(_wjStyle);
            }
            if (enableStrongs)
            {
                stringBuilder.Append(_wStyle);
            }
            stringBuilder.AppendLine("</style>");
            stringBuilder.AppendLine();

            foreach (var content in book.Content)
            {
                if (content is UsxPara heading && heading.Style == "h" &&
                    heading.Content.FirstOrDefault() is string headingText)
                {
                    bookName = headingText;
                    stringBuilder.AppendFormat("<a href=\"#{0}-h\">", bookName);
                    stringBuilder.Append($"<h2 id=\"{heading.Style}-{headingText}\">");
                    stringBuilder.Append(headingText);
                    stringBuilder.AppendLine("</h2></a>");
                }
                else if (content is UsxMarker chapterMarker && chapterMarker.Number != 0)
                {
                    chapterNumber = chapterMarker.Number;
                    stringBuilder.AppendFormat("<a href=\"#{0}-c-{1}\">", bookName, chapterNumber);
                    stringBuilder.Append($"<h3 id=\"{bookName}-{chapterMarker.Style}-{chapterNumber}\">");
                    stringBuilder.Append(chapterMarker.Number.ToString());
                    stringBuilder.AppendLine("</h3></a>");
                }
                else if (content is UsxPara paragraph && paragraph.Content != null && chapterNumber > 0)
                {
                    stringBuilder.AppendFormat("<p>{0}</p>",
                        paragraph.ToHtml(footnotes, enableFootnotes, enableStrongs));
                }
            }

            var htmlLinks = GenerateChapterLinks(chapterNumber, bookName);
            stringBuilder.AppendLine(htmlLinks);

            // Append footnotes etc. at the bottom of the document
            if (footnotes.Any())
            {
                stringBuilder.AppendLine("<section id=\"footnotes\">");
                stringBuilder.AppendLine("<details>");
                stringBuilder.AppendLine("<summary>†</summary>");
                stringBuilder.AppendLine("<ol>");

                foreach (var footnote in footnotes)
                {
                    stringBuilder.AppendLine(footnote);
                }

                stringBuilder.AppendLine("</ol>");
                stringBuilder.AppendLine("</details>");
                stringBuilder.AppendLine("</section>");
            }

            return stringBuilder.ToString();
        }

        static string GenerateChapterLinks(int chapterCount, string bookName)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendFormat("<a href=\"#{0}-h\">^</a> ", bookName);
            for (int i = 1; i <= chapterCount; i++)
            {
                stringBuilder.AppendFormat("<a href=\"#{0}-c-{1}\">[{1}]</a> ", bookName, i);
            }

            return stringBuilder.ToString().Trim();
        }

        internal static string ToHtml(this UsxPara paragraph, IList<string> footnotes, bool addNotes, bool addStrongs)
        {
            var stringBuilder = new StringBuilder();

            foreach (var item in paragraph.Content)
            {
                if (item is string textValue)
                {
                    stringBuilder.Append(WebUtility.HtmlEncode(textValue));
                }
                else if (item is IUsxTextBase value)
                {
                    stringBuilder.Append(UsxToHtml(value, addStrongs));
                }
                else if (item is UsxMarker verseMarker && verseMarker.Number != 0)
                {
                    stringBuilder.Append("<sup>");
                    stringBuilder.Append(verseMarker.Number.ToString(CultureInfo.InvariantCulture));
                    stringBuilder.Append("</sup>");
                }
                else if (addNotes && item is UsxNote usxNote)
                {
                    var index = footnotes.Count;
                    stringBuilder.Append(
                        $"<sup><a href=\"#footnote-{index}\" id=\"ref-{index}\">†</a></sup>");
                    footnotes.Add(usxNote.Entries.ToFootnoteRefs(index));
                }
            }

            return stringBuilder.ToString();
        }

        private static string ToFootnoteRefs(this IEnumerable<UsxCharRef> usxChars, int index)
        {
            var stringBuilder = new StringBuilder();
            var linkAdded = false;

            stringBuilder.AppendFormat("<li id=\"footnote-{0}\">", index);
            foreach (var usxChar in usxChars)
            {
                foreach (var item in usxChar.Content)
                {
                    if (!linkAdded)
                    {
                        linkAdded = true;
                        stringBuilder.AppendFormat(
                            "<a href=\"#ref-{0}\" title=\"Back to reference ↩\">{1}</a>",
                            index, UsxToHtml(item));
                    }
                    else
                    {
                        stringBuilder.Append(UsxToHtml(item));
                    }
                }
            }
            stringBuilder.Append("</li>");

            return stringBuilder.ToString();
        }

        private static string UsxToHtml(object item, bool addStrongs = false)
        {
            if (item == null)
            {
                return string.Empty;
            }

            if (item is string text)
            {
                return WebUtility.HtmlEncode(text);
            }
            else if (item is UsxCharContent usxCharWj && usxCharWj.Content != null)
            {
                var contents = usxCharWj.ToHtml(addStrongs);
                if (usxCharWj.Style == "wj")
                {
                    return string.Format("<span class=\"words-of-jesus\">{0}</span>", contents);
                }
                else
                {
                    return contents;
                }
            }
            else if (addStrongs && item is UsxCharStrong usxCharW && !string.IsNullOrEmpty(usxCharW.Strong))
            {
                return string.Format("<span class=\"word-link\" link-data=\"{0}{1}\">{2}</span>",
                    "https://www.blueletterbible.org/lexicon/",
                    WebUtility.UrlEncode(usxCharW.Strong),
                    WebUtility.HtmlEncode(usxCharW.Text));
            }
            else if (item is UsxReference crossReference)
            {
                return string.Format("<a href=\"{0}{1}\" title=\"Bible link\" target=\"_blank\">{2}</a>",
                    "https://www.biblegateway.com/passage/?search=",
                    WebUtility.UrlEncode(crossReference.Location),
                    WebUtility.HtmlEncode(crossReference.Location));
            }
            else if (item is IUsxTextBase value)
            {
                return WebUtility.HtmlEncode(value.Text);
            }

            return string.Empty;
        }

        private static string ToHtml(this UsxCharContent usxChar, bool addStrongs)
        {
            var stringBuilder = new StringBuilder();

            foreach (var item in usxChar.Content)
            {
                stringBuilder.Append(UsxToHtml(item, addStrongs));
            }

            return stringBuilder.ToString();
        }
    }
}
