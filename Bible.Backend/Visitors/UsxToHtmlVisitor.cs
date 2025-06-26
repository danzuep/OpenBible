namespace Bible.Backend.Visitors;

using System.Net;
using System.Text;
using Bible.Backend.Models;
using Microsoft.Extensions.Options;

public sealed class UsxToHtmlVisitor : IUsxVisitor
{
    public static UsxToHtmlVisitor Create(UsxScriptureBook? usxScriptureBook, UsxVisitorOptions? options = null)
    {
        var visitor = new UsxToHtmlVisitor(options);
        visitor.Accept(usxScriptureBook);
        return visitor;
    }

    public UsxToHtmlVisitor(IOptions<UsxVisitorOptions>? options = null)
    {
        _options = options?.Value ?? new UsxVisitorOptions
        {
            EnableChapterLinks = true,
            EnableCrossReferences = true,
            EnableRedLetters = true,
            EnableStrongs = true,
            EnableFootnotes = true
        };
    }

    private readonly List<UsxFootnote> _footnotes = new();

    private readonly UsxVisitorReference _reference = new();

    private readonly UsxVisitorOptions _options;

    private readonly StringBuilder _sb = new();

    public string GetHtml() => GetFullHtml();

    public void Visit(UsxIdentification identification)
    {
        if (!string.IsNullOrEmpty(identification.Name))
        {
            _reference.BookCode = identification.BookCode;
            //int firstSpaceIndex = identification.Name.IndexOf(' ') + 1;
            //var bookName = identification.Name[firstSpaceIndex..];
            var bookName = identification.Name;
            _sb.AppendFormat("<{0} id=\"{2}-{1}\" class=\"usx-{2}\">{3}</{0}>",
                "h1", _reference, identification.Style, WebUtility.HtmlEncode(bookName));
            _sb.AppendLine();
        }
    }

    public void Visit(UsxPara para)
    {
        var hidden = new string[] { "toc", "mt" };
        if (!para.Style.StartsWith("h", StringComparison.OrdinalIgnoreCase))
        {
            var hide = hidden.Any(h => para.Style.StartsWith(h, StringComparison.OrdinalIgnoreCase));
            _sb.AppendFormat("<p id=\"{0}-{1}\" class=\"usx-{0}\" {2}>",
                para.Style, _reference, hide ? "hidden" : string.Empty);
            this.Accept(para?.Content);
            _sb.AppendLine("</p>");
        }
        else if (para.Text is string heading)
        {
            if (string.IsNullOrEmpty(_reference.BookCode))
                _reference.BookCode = heading;
            _sb.AppendFormat("<a href=\"#{1}\"><{0} id=\"{1}\" class=\"usx-{2}\">{3}</{0}></a>",
                "h2", _reference, para.Style, WebUtility.HtmlEncode(heading));
            _sb.AppendLine();
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
        if (_options.EnableStrongs)
        {
            _sb.AppendFormat("<span id=\"{0}-{1}\" class=\"usx-{0}\" link-data=\"{2}{3}\">",
                usxChar.Style, _reference, "https://www.blueletterbible.org/lexicon/",
                WebUtility.UrlEncode(usxChar.Strong));
            this.Accept(usxChar?.Content);
            _sb.Append("</span>");
        }
        else if (usxChar.Style.Equals("w", StringComparison.OrdinalIgnoreCase))
        {
            this.Accept(usxChar?.Content);
        }
        else
        {
            _sb.AppendFormat("<span id=\"{0}-{1}\" class=\"usx-{0}\">",
                usxChar.Style, _reference);
            this.Accept(usxChar?.Content);
            _sb.Append("</span>");
        }
    }

    public void Visit(string text)
    {
        _sb.Append(WebUtility.HtmlEncode(text));
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
            var index = _footnotes.Count; // _reference
            _sb.AppendFormat("<sup id=\"note-{0}\"><a class=\"usx-note\" title=\"{1}{2}\" href=\"#footnote-{0}\">†</a></sup>",
                index, note.Style, note.Caller);
            _footnotes.Add(note);
        }
    }

    private string GetFullHtml()
    {
        AddFootnotesSection();
        return _sb.GetFullHtml(_options, _reference);
    }

    private void AddFootnotesSection()
    {
        if (_footnotes.Any())
        {
            _sb.AppendLine("<section id=\"footnotes\">");
            _sb.AppendLine("<details>");
            _sb.AppendLine("<summary>†</summary>");
            _sb.AppendLine("<ol>");

            for (var i = 0; i < _footnotes.Count; i++)
            {
                AppendFootnote(_footnotes[i], i);
            }

            _sb.AppendLine("</ol>");
            _sb.AppendLine("</details>");
            _sb.AppendLine("</section>");
        }
    }

    private void AppendFootnote(UsxFootnote usxFootnote, int index)
    {
        var linkAdded = false;
        _sb.AppendFormat("<li id=\"footnote-{0}\">", index);
        foreach (var item in usxFootnote.Content)
        {
            if (!linkAdded)
            {
                linkAdded = true;
                _sb.AppendFormat(
                    "<a href=\"#note-{0}\" title=\"Back to reference ↩\">{1}</a>",
                    index, UsxToHtml(item));
            }
            else
            {
                _sb.Append(UsxToHtml(item));
            }
        }
        _sb.Append("</li>");
    }

    private static string UsxToHtml(object item)
    {
        if (item is string text)
        {
            return WebUtility.HtmlEncode(text);
        }
        else if (item is UsxContent usx && usx.Content != null)
        {
            return UsxToHtml(usx.Content);
        }
        else if (item is IEnumerable<object> objects)
        {
            foreach (var obj in objects)
            {
                return UsxToHtml(obj);
            }
        }
        return string.Empty;
    }
}

internal static class HtmlCssBuilder
{
    private static readonly string _wjStyle = @"
      /* Style for words of Jesus */
      .usx-wj {
        color: #b22222; /* Firebrick red */
        font-weight: bold;
      }
";
    private static readonly string _wStyle = @"
      /* Style for Strong's number hover notes */
      .usx-w {
        position: relative;
      }
      .usx-w::after {
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
      .usx-w:hover::after {
        opacity: 1;
      }
";

    private static readonly string _aStyle = @"
      /* Remove the default blue underline from URLs */
      a {
        text-decoration: none;
        color: inherit;
      }
      /* Style for hidden text */
      .usx-id {
        display: none;
      }
      .usx-ide {
        display: none;
      }
      /* Style for a subtitle below a heading */
      .usx-toc1 {
          font-size: 0.9em;
          color: gray;
          margin-top: 0.2em;
          font-weight: normal;
      }
";

    public static void AppendStyleCss(this StringBuilder stringBuilder, UsxVisitorOptions options)
    {
        stringBuilder.AppendFormat("    <style>{0}", _aStyle);
        if (options.EnableRedLetters)
        {
            stringBuilder.Append(_wjStyle);
        }
        if (options.EnableStrongs)
        {
            stringBuilder.Append(_wStyle);
        }
        stringBuilder.AppendLine("    </style>");
    }

    public static string GetFullHtml(this StringBuilder stringBuilder, UsxVisitorOptions options, UsxVisitorReference reference)
    {
        var chapterLinks = string.Empty;
        if (options.EnableChapterLinks && int.TryParse(reference.Chapter, out var chapterNumber))
        {
            chapterLinks = GenerateChapterLinks(chapterNumber, reference.BookCode);
            stringBuilder.AppendLine();
        }

        var body = stringBuilder.ToString();
        stringBuilder.Clear();
        stringBuilder.AppendLine("<!DOCTYPE html>");
        stringBuilder.AppendLine("<html lang=\"en\">");
        stringBuilder.AppendLine("  <head>");
        stringBuilder.AppendFormat("    <title>{0}", reference.Title);
        stringBuilder.AppendLine("</title>");
        stringBuilder.AppendStyleCss(options);
        stringBuilder.AppendLine("  </head>");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("  <body>");
        stringBuilder.AppendLine(chapterLinks);
        stringBuilder.AppendLine(body);
        stringBuilder.AppendLine("  </body>");
        stringBuilder.AppendLine("</html>");
        return stringBuilder.ToString();
    }

    private static string GenerateChapterLinks(int chapterCount, string? bookCode)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendFormat("<a href=\"#{0}\">#</a> ", bookCode);
        for (int i = 1; i <= chapterCount; i++)
        {
            stringBuilder.AppendFormat("<a href=\"#{0}.{1}\">[{1}]</a> ", bookCode, i);
        }
        stringBuilder.AppendLine();
        stringBuilder.AppendLine();

        return stringBuilder.ToString().Trim();
    }
}