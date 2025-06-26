namespace Bible.Backend.Visitors;

using System.Net;
using System.Text;
using Bible.Backend.Models;
using Microsoft.Extensions.Options;

public sealed class UsxToMarkdownVisitor : IUsxVisitor
{
    public static UsxToMarkdownVisitor Create(UsxScriptureBook? usxScriptureBook, UsxVisitorOptions? options = null)
    {
        var visitor = new UsxToMarkdownVisitor(options);
        visitor.Accept(usxScriptureBook);
        return visitor;
    }

    public UsxToMarkdownVisitor(IOptions<UsxVisitorOptions>? options = null)
    {
        _options = options?.Value ?? new UsxVisitorOptions();
    }

    private readonly UsxVisitorOptions _options;

    private readonly StringBuilder _sb = new();

    public string GetMarkdown() => _sb.ToString();

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
        if (!para.Style.StartsWith("h", StringComparison.OrdinalIgnoreCase))
        {
            this.Accept(para?.Content);
        }
        else if (para.Text is string heading)
        {
            _sb.AppendLine($"## {heading}");
            _sb.AppendLine();
        }
    }

    public void Visit(UsxChapterMarker marker)
    {
        if (!string.IsNullOrEmpty(marker.Number))
        {
            _sb.AppendLine($"### Chapter {marker.Number}");
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
        _sb.Append(ch.Text);
        this.Accept(ch?.Content);
    }

    public void Visit(string text)
    {
        _sb.Append(text);
    }

    public void Visit(UsxMilestone milestone)
    {
        // Can be ignored or treated as special markup if needed
    }

    public void Visit(UsxLineBreak lineBreak)
    {
        _sb.Append("  \n"); // Markdown line break
    }

    public void Visit(UsxCrossReference reference)
    {
        _sb.Append($"[{reference.Location}](https://www.biblegateway.com/passage/?search={WebUtility.UrlEncode(reference.Location)})");
    }

    public void Visit(UsxFootnote footnote)
    {
        if (_options.EnableFootnotes)
            _sb.Append($"[^footnote{footnote.Caller}]");
        // Actual footnote text can be collected separately for appending at end
    }
}