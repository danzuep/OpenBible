using System.Text.Json.Serialization;

namespace Bible.Usx.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UsjNodeType
{
    Book,
    Identification,
    ChapterMarker,
    VerseMarker,
    Para,
    Char,
    Footnote,
    Milestone,
    LineBreak,
    CrossReference,
    Text
}

public sealed record UsjBook(
    string SchemaVersion,
    UsjIdentification Metadata,
    List<IUsjNode> Content
) : IUsjNode
{
    public UsjNodeType Type => UsjNodeType.Book;
    public UsjBook() : this(string.Empty, new UsjIdentification(), new List<IUsjNode>()) { }
}

public sealed record UsjIdentification(
    string BookCode,
    string VersionDescription,
    string Style
) : IUsjNode
{
    public UsjNodeType Type => UsjNodeType.Identification;
    public UsjIdentification() : this(string.Empty, string.Empty, string.Empty) { }
}

public sealed record UsjChapterMarker(
    string Number,
    string Style,
    string? StartId,
    string? EndId
) : IUsjNode
{
    public UsjNodeType Type => UsjNodeType.ChapterMarker;
    public UsjChapterMarker() : this(string.Empty, string.Empty, null, null) { }
}

public sealed record UsjVerseMarker(
    string Number,
    string Style,
    string? StartId,
    string? EndId
) : IUsjNode
{
    public UsjNodeType Type => UsjNodeType.VerseMarker;
    public UsjVerseMarker() : this(string.Empty, string.Empty, null, null) { }
}

public sealed record UsjPara(
    string Style,
    List<IUsjNode>? Content
) : IUsjNode
{
    public UsjNodeType Type => UsjNodeType.Para;
    public UsjPara() : this(string.Empty, new List<IUsjNode>()) { }

    // Helper to get concatenated text content of child UsjText nodes
    public string? Text => Content == null ? null : string.Concat(Content.OfType<UsjText>().Select(t => t.Text));
}

public sealed record UsjChar(
    string Style,
    bool Closed,
    string? Strong,
    string Text
) : IUsjNode
{
    public UsjNodeType Type => UsjNodeType.Char;
    public UsjChar() : this(string.Empty, false, null, string.Empty) { }
}

public sealed record UsjFootnote(
    string Caller,
    string Style,
    List<IUsjNode> Content
) : IUsjNode
{
    public UsjNodeType Type => UsjNodeType.Footnote;
    public UsjFootnote() : this(string.Empty, string.Empty, new List<IUsjNode>()) { }
}

public sealed record UsjMilestone(
    string Style,
    string? StartId,
    string? EndId
) : IUsjNode
{
    public UsjNodeType Type => UsjNodeType.Milestone;
    public UsjMilestone() : this(string.Empty, null, null) { }
}

public sealed record UsjLineBreak(string Style) : IUsjNode
{
    public UsjNodeType Type => UsjNodeType.LineBreak;
    public UsjLineBreak() : this(string.Empty) { }
}

public sealed record UsjCrossReference(
    string Location,
    string Style,
    List<IUsjNode> Content
) : IUsjNode
{
    public UsjNodeType Type => UsjNodeType.CrossReference;
    public UsjCrossReference() : this(string.Empty, string.Empty, new List<IUsjNode>()) { }
}

public sealed record UsjText(string Text) : IUsjNode
{
    public UsjNodeType Type => UsjNodeType.Text;
    public UsjText() : this(string.Empty) { }
}