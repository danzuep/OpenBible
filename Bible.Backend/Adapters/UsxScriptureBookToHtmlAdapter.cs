using System.Globalization;
using System.Net;
using System.Text;
using Bible.Backend.Models;

namespace Bible.Backend.Adapters
{
    public static class UsxScriptureBookToHtmlAdapter
    {
        public static string ToHtml(this UsxScriptureBook? book)
        {
            if (book == null)
            {
                return string.Empty;
            }

            var stringBuilder = new StringBuilder();

            var crossReferences = new List<string>();

            foreach (var content in book.Content)
            {
                if (content is UsxPara heading && heading.Style == "h" &&
                    heading.Content.FirstOrDefault() is string bookName)
                {
                    stringBuilder.Append("<h2>");
                    stringBuilder.Append(bookName);
                    stringBuilder.Append("</h2>");
                }
                else if (content is UsxMarker chapterMarker && chapterMarker.Style == "c" && chapterMarker.Number != 0)
                {
                    stringBuilder.Append("<h3>");
                    stringBuilder.Append(chapterMarker.Number.ToString(CultureInfo.InvariantCulture));
                    stringBuilder.Append("</h3>");
                }
                else if (content is UsxPara paragraph && paragraph.Style == "p")
                {
                    stringBuilder.Append("<p>");
                    stringBuilder.Append(paragraph.ToHtml(crossReferences));
                    stringBuilder.Append("</p>");
                }
            }

            // Append footnotes etc. at the bottom of the document
            if (crossReferences.Count > 0)
            {
                stringBuilder.AppendLine("<section aria-label=\"Footnotes\">");
                stringBuilder.AppendLine("<h4 id=\"footnote-label\">Footnotes</h4>");
                stringBuilder.AppendLine("<ol>");

                for (int i = 0; i < crossReferences.Count; i++)
                {
                    var key = 'a' + i;
                    var crossReference = crossReferences[i];
                    stringBuilder.AppendLine(
                        $"<li id=\"footnote-{key}\">{crossReference} " +
                        $"<a href=\"#ref-{key}\" aria-label=\"Back to content\">↩</a></li>");
                }

                stringBuilder.AppendLine("</ol>");
                stringBuilder.AppendLine("</section>");
            }

            return stringBuilder.ToString();
        }

        internal static string ToHtml(this UsxPara paragraph, IList<string> crossReferences)
        {
            var stringBuilder = new StringBuilder();

            foreach (var item in paragraph.Content)
            {
                if (item is string textValue)
                {
                    stringBuilder.Append(WebUtility.HtmlEncode(textValue).Replace("\n", "<br />"));
                }
                else if (item is IUsxTextBase value)
                {
                    stringBuilder.Append(WebUtility.HtmlEncode(value.Text).Replace("\n", "<br />"));
                }
                else if (item is UsxMarker verseMarker && verseMarker.Style == "v" && verseMarker.Number != 0)
                {
                    stringBuilder.Append("<sup>");
                    stringBuilder.Append(verseMarker.Number.ToString(CultureInfo.InvariantCulture));
                    stringBuilder.Append("</sup>");
                }
                else if (item is UsxReference crossReference)
                {
                    var footnoteId = 'a' + crossReferences.Count;
                    crossReferences.Add(WebUtility.HtmlEncode(crossReference.Location));

                    stringBuilder.Append(
                        $"<sup><a href=\"#footnote-{footnoteId}\" id=\"ref-{footnoteId}\" " +
                        $"aria-describedby=\"footnote-label\">[{footnoteId}]</a></sup>");
                }
            }

            return stringBuilder.ToString();
        }
    }
}
