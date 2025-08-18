using Bible.Usx.Models;

namespace Bible.Usx.Services
{
    public interface IUsjVisitor
    {
        void Visit(UsjIdentification translation);
        void Visit(UsjChapterMarker marker);
        void Visit(UsjVerseMarker marker);
        void Visit(UsjChar metatext);
        void Visit(UsjPara heading);
        void Visit(UsjText text);
        void Visit(UsjMilestone milestone);
        void Visit(UsjLineBreak lineBreak);
        void Visit(UsjCrossReference reference);
        void Visit(UsjFootnote footnote);
    }

    public static class UsjVisitorExtensions
    {
        public static void Accept(this IUsjVisitor visitor, IUsjNode? usjNode)
        {
            if (usjNode == null) return;
            switch (usjNode)
            {
                case UsjText s: visitor.Visit(s); break;
                case UsjChar w: visitor.Visit(w); break;
                case UsjPara p: visitor.Visit(p); break;
                case UsjVerseMarker v: visitor.Visit(v); break;
                case UsjChapterMarker c: visitor.Visit(c); break;
                case UsjFootnote n: visitor.Visit(n); break;
                case UsjCrossReference r: visitor.Visit(r); break;
                case UsjLineBreak br: visitor.Visit(br); break;
                case UsjMilestone ms: visitor.Visit(ms); break;
                case UsjIdentification t: visitor.Visit(t); break;
                case UsjBook b:
                    visitor.Accept(b.Metadata);
                    visitor.Accept(b.Content);
                    break;
                default:
                    throw new NotSupportedException($"Unknown USX type: {usjNode.GetType()}");
            }
        }

        public static void Accept(this IUsjVisitor visitor, IEnumerable<IUsjNode>? content)
        {
            if (content == null) return;
            foreach (var item in content)
                visitor.Accept(item);
        }
    }
}