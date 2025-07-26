namespace Bible.Backend.Visitors;

using System;
using System.Net;
using System.Text;
using Bible.Backend.Models;
using Microsoft.Extensions.Options;

public sealed class UsxToMarkdownVisitor : IUsxVisitor
{
    public static string GetFullText(UsxScriptureBook? usxScriptureBook, UsxVisitorOptions? options = null)
    {
        var visitor = new UsxToMarkdownVisitor(options);
        visitor.Accept(usxScriptureBook);
        return visitor.GetFullText();
    }

    public UsxToMarkdownVisitor(IOptions<UsxVisitorOptions>? options = null)
    {
        _options = options?.Value ?? new UsxVisitorOptions
        {
            EnableCrossReferences = false,
            EnableFootnotes = true
        };
    }

    private readonly List<UsxFootnote> _footnotes = new();

    private readonly UsxVisitorOptions _options;

    private readonly StringBuilder _sb = new();

    private static string[] _paraStylesToHide = ["ide", "toc", "mt"];

    public void Visit(UsxIdentification identification)
    {
        if (!string.IsNullOrEmpty(identification.Name))
        {
            _sb.AppendLine($"# {identification.Name}");
            _sb.AppendLine();
        }
    }

    public void Visit(UsxPara para)
    {
        if (para.Style.StartsWith("h", StringComparison.OrdinalIgnoreCase) &&
            para.Text is string heading)
        {
            _sb.AppendLine($"## {heading}");
            _sb.AppendLine();
        }
        else if (!_paraStylesToHide.Any(h => para.Style.StartsWith(h, StringComparison.OrdinalIgnoreCase)))
        {
            this.Accept(para?.Content);
        }
    }

    public void Visit(UsxChapterMarker marker)
    {
        if (!string.IsNullOrEmpty(marker.Number))
        {
            _sb.AppendLine($"### Chapter {marker.Number}");
            _sb.AppendLine();
        }
        else
        {
            _sb.AppendLine();
            _sb.AppendLine();
        }
    }

    public void Visit(UsxVerseMarker marker)
    {
        if (!string.IsNullOrEmpty(marker.Number))
        {
            _sb.Append($"**{marker.Number}** ");
        }
    }

    public void Visit(UsxChar ch)
    {
        this.Accept(ch?.Content);
    }

    public void Visit(string text)
    {
        _sb.Append(text);
    }

    public void Visit(UsxMilestone milestone)
    {
    }

    public void Visit(UsxLineBreak lineBreak)
    {
        _sb.Append("  \n");
    }

    public void Visit(UsxCrossReference reference)
    {
        if (_options.EnableCrossReferences)
            _sb.Append($"[{reference.Location}](https://www.biblegateway.com/passage/?search={WebUtility.UrlEncode(reference.Location)})");
    }

    public void Visit(UsxFootnote note)
    {
        if (_options.EnableFootnotes)
        {
            var index = _footnotes.Count + 1;
            _sb.Append($"[^{index}]");
            _footnotes.Add(note);
        }
    }

    public string GetFullText()
    {
        if (_footnotes.Any())
        {
            _sb.AppendLine();
            for (var i = 0; i < _footnotes.Count; i++)
            {
                AppendFootnote(i);
            }
        }

        return _sb.ToString();
    }

    private void AppendFootnote(int index)
    {
        if (_footnotes[index].Content is object[] items)
        {
            _sb.Append($"[^{index + 1}]: ");
            foreach (var item in items)
            {
                this.Accept(item);
            }
            _sb.AppendLine();
        }
    }
}