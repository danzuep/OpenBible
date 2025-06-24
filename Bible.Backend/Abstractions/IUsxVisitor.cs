namespace Bible.Backend.Models
{
    public interface IUsxVisitor
    {
        void Visit(UsxIdentification translation);
        void Visit(UsxChapterMarker marker);
        void Visit(UsxVerseMarker marker);
        void Visit(UsxChar metatext);
        void Visit(UsxPara heading);
        void Visit(string text);
        void Visit(UsxMilestone milestone);
        void Visit(UsxLineBreak lineBreak);
        void Visit(UsxCrossReference reference);
        void Visit(UsxFootnote footnote);
    }
}