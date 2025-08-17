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
}