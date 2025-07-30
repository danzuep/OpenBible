namespace Bible.Backend.Visitors;

using System.Net;
using System.Text;
using Bible.Backend.Abstractions;
using Bible.Backend.Models;
using Microsoft.Extensions.Options;

public sealed class UsxToHtmlVisitor : IUsxVisitor
{
    public static string GetFullText(UsxScriptureBook? usxScriptureBook, UnihanLookup? unihan = null, UsxVisitorOptions? options = null)
    {
        var visitor = new UsxToHtmlVisitor(options);
        visitor.Unihan = unihan;
        visitor.Accept(usxScriptureBook);
        return visitor.GetFullText();
    }

    public UsxToHtmlVisitor(IOptions<UsxVisitorOptions>? options = null)
    {
        _options = options?.Value ?? new UsxVisitorOptions
        {
            EnableChapterLinks = true,
            EnableCrossReferences = true,
            EnableRedLetters = true,
            EnableStrongs = true,
            EnableRubyText = true,
            EnableFootnotes = true
        };
    }

    public UnihanLookup? Unihan { get; set; }

    private readonly List<UsxFootnote> _footnotes = new();

    private readonly UsxVisitorReference _reference = new();

    private readonly UsxVisitorOptions _options;

    private readonly StringBuilder _sb = new();

    private static readonly IReadOnlyList<string> _paraStylesToHide =
        UsxToMarkdownVisitor.ParaStylesToHide;

    public void Visit(UsxIdentification identification)
    {
        if (!string.IsNullOrEmpty(identification.BookName))
        {
            _reference.BookCode = identification.BookCode;
            //int firstSpaceIndex = identification.Name.IndexOf(' ') + 1;
            //var bookName = identification.Name[firstSpaceIndex..];
            var bookName = identification.BookName;
            _sb.AppendFormat("<{0} id=\"{2}-{1}\" class=\"usx-{2}\">{3}</{0}>",
                "h1", _reference, identification.Style, WebUtility.HtmlEncode(bookName));
            _sb.AppendLine();
        }
    }

    public void Visit(UsxPara para)
    {
        if (para.Style.StartsWith("h", StringComparison.OrdinalIgnoreCase) &&
            para.Text is string heading)
        {
            if (string.IsNullOrEmpty(_reference.BookCode))
                _reference.BookCode = heading;
            _sb.AppendFormat("<a href=\"#{1}\"><{0} id=\"{1}\" class=\"usx-{2}\">{3}</{0}></a>",
                "h2", _reference, para.Style, WebUtility.HtmlEncode(heading));
            _sb.AppendLine();
        }
        else
        {
            var hide = _paraStylesToHide.Any(h => para.Style.StartsWith(h, StringComparison.OrdinalIgnoreCase));
            _sb.AppendFormat("<p id=\"{0}-{1}\" class=\"usx-{0}\" {2}>",
                para.Style, _reference, hide ? "hidden" : string.Empty);
            this.Accept(para?.Content);
            _sb.AppendLine("</p>");
        }
    }

    public void Visit(UsxChapterMarker marker)
    {
        if (!string.IsNullOrEmpty(marker.Number))
        {
            _reference.Chapter = marker.Number;
            _reference.Verse = null;
            _sb.AppendFormat("<a href=\"#{1}\"><{0} id=\"{1}\" class=\"usx-{2}\">{3}</{0}></a>",
                "h3", _reference, marker.Style, WebUtility.HtmlEncode(marker.Number));
            _sb.AppendLine();
        }
    }

    public void Visit(UsxVerseMarker marker)
    {
        if (!string.IsNullOrEmpty(marker.Number))
        {
            _reference.Verse = marker.Number;
            _sb.AppendFormat("<a href=\"#{1}\"><{0} id=\"{1}\" class=\"usx-{2}\">{3}</{0}></a>",
                "sup", _reference, marker.Style, WebUtility.HtmlEncode(marker.Number));
            _sb.AppendLine();
        }
    }

    public void Visit(UsxChar usxChar)
    {
        if (Unihan != null && Unihan.Field.HasValue && usxChar.Text is string text)
        {
            _sb.AppendFormat("<span class=\"usx-{0}\">", usxChar.Style);
            foreach (var rune in text.EnumerateRunes())
            {
                AddRubyText(rune.Value, Unihan.Field.Value);
            }
            _sb.Append("</span>");
        }
        else if (_options.EnableStrongs && !string.IsNullOrEmpty(usxChar.Strong))
        {
            _sb.AppendFormat("<span id=\"{0}-{1}\" class=\"usx-{0}\" link-data=\"{2}{3}\">",
                usxChar.Style, _reference, "https://www.blueletterbible.org/lexicon/",
                WebUtility.UrlEncode(usxChar.Strong));
            this.Accept(usxChar.Content);
            _sb.Append("</span>");
        }
        else if (!_options.EnableStrongs && usxChar.Style.Equals("w", StringComparison.OrdinalIgnoreCase))
        {
            this.Accept(usxChar.Content);
        }
        else
        {
            _sb.AppendFormat("<span id=\"{0}-{1}\" class=\"usx-{0}\">",
                usxChar.Style, _reference);
            this.Accept(usxChar.Content);
            _sb.Append("</span>");
        }
    }

    private void AddRubyText(int codepoint, UnihanField unihanField)
    {
        var fields = new UnihanField[] { unihanField }; // UnihanField.kDefinition
        var unihanCharacter = char.ConvertFromUtf32(codepoint);
        if (Unihan != null &&
            Unihan.TryGetEntryText(codepoint, fields, out var entryText))
        {
            _sb.Append("<ruby>");
            _sb.Append(unihanCharacter);
            _sb.AppendFormat("<rt class=\"unihan\">{0}</rt>", entryText);
            _sb.Append("</ruby>");
        }
        else
        {
            _sb.Append(unihanCharacter);
        }
    }

    public void Visit(string text)
    {
        if (Unihan != null && Unihan.Field.HasValue)
        {
            foreach (var rune in text.EnumerateRunes())
            {
                AddRubyText(rune.Value, Unihan.Field.Value);
            }
        }
        else
        {
            _sb.Append(WebUtility.HtmlEncode(text));
        }
    }

    public void Visit(UsxMilestone milestone)
    {
        // Can be treated as special markup if needed
    }

    public void Visit(UsxLineBreak lineBreak)
    {
        _sb.Append("<br />");
    }

    public void Visit(UsxCrossReference reference)
    {
        if (_options.EnableCrossReferences)
        {
            _sb.AppendFormat("<a id=\"ref-{0}\" class=\"usx-ref\" title=\"{1}\" href=\"{2}{3}\" target=\"_blank\">{4} ",
                _reference, "Bible link", "https://www.biblegateway.com/passage/?search=",
                WebUtility.UrlEncode(reference.Location), WebUtility.HtmlEncode(reference.Location));
            this.Accept(reference?.Content);
            _sb.AppendLine("</a>");
        }
    }
    
    public void Visit(UsxFootnote note)
    {
        if (_options.EnableFootnotes)
        {
            var index = _footnotes.Count + 1;
            _sb.AppendFormat("<sup id=\"note-{0}\"><a class=\"usx-note\" title=\"{1}{2}\" href=\"#footnote-{0}\">†</a></sup>",
                index, note.Style, note.Caller);
            _footnotes.Add(note);
        }
    }

    public string GetFullText()
    {
        if (_footnotes.Any())
        {
            _sb.AppendLine("<section id=\"footnotes\">");
            _sb.AppendLine("<details>");
            _sb.AppendLine("<summary>†</summary>");
            _sb.AppendLine("<ol>");

            for (var i = 0; i < _footnotes.Count; i++)
            {
                AppendFootnote(i);
            }

            _sb.AppendLine("</ol>");
            _sb.AppendLine("</details>");
            _sb.AppendLine("</section>");
        }

        return _sb.GetFullHtml(_options, _reference);
    }

    private void AppendFootnote(int index)
    {
        var linkAdded = false;
        if (_footnotes[index].Content is object[] items)
        {
            var id = index + 1;
            _sb.AppendFormat("<li id=\"footnote-{0}\">", id);
            foreach (var item in items)
            {
                if (!linkAdded)
                {
                    linkAdded = true;
                    _sb.AppendFormat("<a href=\"#note-{0}\" title=\"Back to reference ↩\">", id);
                    this.Accept(item);
                    _sb.Append("</a>");
                }
                else
                {
                    this.Accept(item);
                }
            }
            _sb.AppendLine("</li>");
        }
    }
}