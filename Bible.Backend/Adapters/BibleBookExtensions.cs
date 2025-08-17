using System.Net;
using System.Text;
using Bible.Core.Models;
using Unihan.Models;

namespace Bible.Backend.Adapters
{
    public static class BibleBookExtensions
    {
        public static string GetMarkdown(this BibleBook bibleBook)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"# {bibleBook.Reference.Version}");
            sb.AppendLine($"## {bibleBook.Reference.BookName}");
            foreach (var chapter in bibleBook.Chapters)
            {
                sb.AppendLine($"### {chapter.Reference.Chapter}");
                foreach (var verse in chapter.Verses)
                {
                    sb.AppendLine($"**{verse.Reference.Verse}** {verse.Text}");
                    //if (!string.IsNullOrEmpty(verse.Pronunciation))
                    //{
                    //    sb.AppendLine($"*【{verse.Pronunciation}】*");
                    //}
                }
            }
            return sb.ToString();
        }

        public static string GetHtml(this BibleBook bibleBook, UnihanLookup? unihan = null)
        {
            var sb = new StringBuilder();

            // Add document title and book name
            sb.AppendLine($"<h1>{WebUtility.HtmlEncode(bibleBook.Reference.Version)}</h1>");
            sb.AppendLine($"<h2>{WebUtility.HtmlEncode(bibleBook.Reference.BookName)}</h2>");

            // Iterate chapters
            foreach (var chapter in bibleBook.Chapters)
            {
                sb.Append(chapter.GetHtml(unihan));
            }
            sb.AppendLine();

            return sb.ToString();
        }

        public static string GetHtml(this BibleChapter? chapter, UnihanLookup? unihan = null)
        {
            if (chapter == null) return string.Empty;

            var sb = new StringBuilder();

            sb.AppendLine($"<h3>{chapter.Reference.Chapter}</h3>");
            // Iterate verses
            foreach (var verse in chapter.Verses)
            {
                sb.Append(verse.GetHtml(unihan));
            }

            return sb.ToString();
        }

        public static string GetHtml(this BibleVerse? verse, UnihanLookup? unihan = null)
        {
            if (verse == null) return string.Empty;
            var sb = new StringBuilder();

            sb.AppendFormat("<sup>{0}</sup>", verse.Reference.Verse);
            var html = ToRubyRunes(WebUtility.HtmlEncode(verse.Text), unihan);
            sb.AppendLine(html.Replace("\n", "<br />"));

            return sb.ToString();
        }

        public static string ToRubyRunes(string text, UnihanLookup? unihan)
        {
            var sb = new StringBuilder();

            if (unihan?.Field != null)
            {
                sb.Append("<ruby>");
                foreach (var rune in text.EnumerateRunes())
                {
                    sb.Append(rune.ToString());
                    AddUnihan(rune.Value, unihan.Field.Value);
                }
                sb.Append("</ruby>");
            }
            else
            {
                sb.Append(text);
            }

            return sb.ToString();

            void AddUnihan(int codepoint, UnihanField unihanField)
            {
                if (unihan != null && unihan.TryGetValue(codepoint, out var metadata))
                {
                    sb.Append("<rt>");
                    foreach (var kvp in metadata)
                    {
                        var pronunciation = string.Join("; ", Extract(kvp, unihanField));
                        sb.Append(WebUtility.HtmlEncode(pronunciation));
                    }
                    sb.Append("</rt>");
                }

                IEnumerable<string> Extract(KeyValuePair<UnihanField, IList<string>> kvp, UnihanField field)
                {
                    if (kvp.Key == field)
                    {
                        foreach (var value in kvp.Value)
                        {
                            yield return value;
                        }
                    }
                }
            }
        }
    }
}
