using Bible.Backend.Abstractions;
using Bible.Backend.Models;

public static class UsxVisitorExtensions
{
    /// <summary>
    /// UsxScripture
    /// +--- UsxIdentification
    /// +--- 0... UsxMetadata (Title/Header/Identification/Introduction)
    /// +--- 0... UsxChapterMarker
    /// +--- 0... UsxPara
    ///      +--- UsxText
    ///      +--- UsxChar
    ///      +--- UsxVerseMarker
    ///      +--- UsxMilestone
    ///      +--- UsxLineBreak
    ///      +--- UsxReference
    ///      +--- UsxFootnote
    /// <see href="https://ubsicap.github.io/usx/structure.html"/>
    /// </summary>
    public static void Accept(this IUsxVisitor visitor, object? usxObj)
    {
        if (usxObj == null) return;
        switch (usxObj)
        {
            case string s: visitor.Visit(s); break;
            case UsxChar w: visitor.Visit(w); break;
            case UsxPara p: visitor.Visit(p); break;
            case UsxVerseMarker v: visitor.Visit(v); break;
            case UsxChapterMarker c: visitor.Visit(c); break;
            case UsxFootnote n: visitor.Visit(n); break;
            case UsxCrossReference r: visitor.Visit(r); break;
            case UsxLineBreak br: visitor.Visit(br); break;
            case UsxMilestone ms: visitor.Visit(ms); break;
            case UsxIdentification t: visitor.Visit(t); break;
            default:
                throw new NotSupportedException($"Unknown USX type: {usxObj.GetType()}");
        }
    }

    public static void Accept(this IUsxVisitor visitor, IEnumerable<object>? content)
    {
        if (content == null) return;
        foreach (var item in content)
            visitor.Accept(item);
    }

    public static void Accept(this IUsxVisitor visitor, UsxBook? book)
    {
        if (book == null) return;
        if (book.Translation != null)
            visitor.Visit(book.Translation);
        visitor.Accept(book.Content);
    }

    public static void Accept(this IUsxVisitor visitor, UsxMarker? marker)
    {
        if (marker == null) return;
        if (marker.Style.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            visitor.Visit((UsxVerseMarker)marker);
        else if (marker.Style.StartsWith("c", StringComparison.OrdinalIgnoreCase))
            visitor.Visit((UsxChapterMarker)marker);
    }
}