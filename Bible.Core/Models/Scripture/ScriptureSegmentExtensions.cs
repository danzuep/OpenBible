using System.Text;

namespace Bible.Core.Models.Scripture
{
    public static class ScriptureSegmentExtensions
    {
        private static readonly string[] _markup = ["w", "p"];

        public static string ToMarkdown(this ReadOnlySpan<ScriptureSegment> segments, ScriptureBookMetadata metadata = null)
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            if (!string.IsNullOrEmpty(metadata?.Version))
            {
                sb.Append("# ");
                sb.AppendLine(metadata?.Version);
                sb.AppendLine();
            }
            if (!string.IsNullOrEmpty(metadata?.Name))
            {
                sb.Append("## ");
                sb.AppendLine(metadata?.Name);
                sb.AppendLine();
            }
            foreach (var segment in segments)
            {
                if (segment.Category == MetadataCategory.Text)
                    sb.Append(segment.Text);
                else if (segment.Category == MetadataCategory.Meta ||
                    segment.Category == MetadataCategory.Style)
                    continue;
                else if (segment.Category == MetadataCategory.Verse)
                    sb.AppendFormat("**{0}** ", segment.Text);
                else if (segment.Category == MetadataCategory.Chapter)
                {
                    sb.Append("### ");
                    sb.AppendLine(segment.Text);
                }
                else if (segment.Category == MetadataCategory.Markup)
                    sb.AppendFormat("{0}", segment.Text);
                else if (segment.Category == MetadataCategory.Pronunciation)
                    sb.AppendFormat("({0})", segment.Text);
                else if (!_markup.Contains(segment.Text))
                    sb.AppendFormat("<{0} />", segment.Text);
                else
                    sb.AppendFormat("({0})", segment.Text);
            }
            return sb.ToString();
        }

        public static string ToVerseHtml(this ReadOnlySpan<ScriptureSegment> segments)
        {
            var sb = new StringBuilder();
            foreach (var segment in segments)
            {
                if (segment.Category == MetadataCategory.Text)
                    sb.Append(segment.Text);
                else if (segment.Category == MetadataCategory.Markup)
                    sb.AppendFormat("{0}", segment.Text.Replace("\n", "<br />"));
            }
            return sb.ToString();
        }
    }
}
