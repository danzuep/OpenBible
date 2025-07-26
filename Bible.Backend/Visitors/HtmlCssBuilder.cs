namespace Bible.Backend.Visitors;

using System.Text;
using Bible.Backend.Models;

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

    private static readonly string _rtHideStyle = @"
      /* Hide the rt elements by default */
      rt.unihan {
        display: none;
      }
";

    private static readonly string _rubyStyle = @"
      /* When checkbox is checked, show the ruby text */
      #toggle-ruby:checked ~ p ruby rt.unihan {
        display: ruby-text; /* shows the ruby annotation */
      }

      ruby {
        font-size: 1em;
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
        if (options.EnableRubyText)
        {
            stringBuilder.Append(_rubyStyle);
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
        stringBuilder.AppendLine("    <meta charset=\"UTF-8\" />");
        stringBuilder.AppendFormat("    <title>{0}", reference.Title);
        stringBuilder.AppendLine("</title>");
        stringBuilder.AppendStyleCss(options);
        stringBuilder.AppendLine("  </head>");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("  <body>");
        //stringBuilder.AppendLine("  <label><input type=\"checkbox\" id=\"toggle-ruby\" />Show Ruby Text</label>");
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