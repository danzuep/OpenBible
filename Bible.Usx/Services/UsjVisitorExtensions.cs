using Bible.Usx.Models;

namespace Bible.Usx.Services
{
    public static class UsjVisitorExtensions
    {
        public static void Accept(this IUsjVisitor visitor, UsjBook? book)
        {
            if (book == null) return;
            visitor.Accept(book.Metadata);
            visitor.Accept(book.Content);
        }

        public static void Accept(this IUsjVisitor visitor, IEnumerable<IUsjNode>? content)
        {
            if (content == null) return;
            foreach (var item in content)
                visitor.Accept(item);
        }

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
                default:
                    throw new NotSupportedException($"Unknown USX type: {usjNode.GetType()}");
            }
        }
    }
}