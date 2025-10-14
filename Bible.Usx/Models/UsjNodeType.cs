using System.Text.Json.Serialization;

namespace Bible.Usx.Models;

[JsonConverter(typeof(JsonStringEnumConverter<UsjNodeType>))]
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
    IList<IUsjNode> Content
) : IUsjNode
{
    [JsonIgnore]
    public UsjNodeType Type => UsjNodeType.Book;
    public UsjBook() : this(string.Empty, new UsjIdentification(), new List<IUsjNode>()) { }
}

public sealed record UsjIdentification(
    string Style,
    string BookCode,
    string VersionDescription
) : IUsjNode
{
    [JsonIgnore]
    public UsjNodeType Type => UsjNodeType.Identification;
    public UsjIdentification() : this(string.Empty, string.Empty, string.Empty) { }
}

public sealed record UsjChapterMarker(
    string Style,
    string Number,
    string? StartId,
    string? EndId
) : IUsjNode
{
    [JsonIgnore]
    public UsjNodeType Type => UsjNodeType.ChapterMarker;
    public UsjChapterMarker() : this(string.Empty, string.Empty, null, null) { }
}

public sealed record UsjVerseMarker(
    string Style,
    string Number,
    string? StartId,
    string? EndId
) : IUsjNode
{
    [JsonIgnore]
    public UsjNodeType Type => UsjNodeType.VerseMarker;
    public UsjVerseMarker() : this(string.Empty, string.Empty, null, null) { }
}

public sealed record UsjPara(
    string Style,
    IList<IUsjNode>? Content
) : IUsjNode
{
    [JsonIgnore]
    public UsjNodeType Type => UsjNodeType.Para;
    public UsjPara() : this(string.Empty, new List<IUsjNode>()) { }

    [JsonIgnore] // Helper to get concatenated text content of child UsjText nodes
    public string? Text => Content == null ? null : string.Concat(Content.OfType<UsjText>().Select(t => t.Text));
}

public sealed record UsjChar(
    string Style,
    string Text,
    string? Metadata,
    string? Closed,
    IList<IUsjNode>? Content
) : IUsjNode
{
    [JsonIgnore]
    public UsjNodeType Type => UsjNodeType.Char;
    public UsjChar() : this(string.Empty, string.Empty, null, null, null) { }
}

public sealed record UsjFootnote(
    string Style,
    string Caller,
    IList<IUsjNode> Content
) : IUsjNode
{
    [JsonIgnore]
    public UsjNodeType Type => UsjNodeType.Footnote;
    public UsjFootnote() : this(string.Empty, string.Empty, new List<IUsjNode>()) { }
}

public sealed record UsjMilestone(
    string Style,
    string? StartId,
    string? EndId
) : IUsjNode
{
    [JsonIgnore]
    public UsjNodeType Type => UsjNodeType.Milestone;
    public UsjMilestone() : this(string.Empty, null, null) { }
}

public sealed record UsjLineBreak(string Style) : IUsjNode
{
    [JsonIgnore]
    public UsjNodeType Type => UsjNodeType.LineBreak;
    public UsjLineBreak() : this(string.Empty) { }
}

public sealed record UsjCrossReference(
    string Style,
    string Location,
    IList<IUsjNode> Content
) : IUsjNode
{
    [JsonIgnore]
    public UsjNodeType Type => UsjNodeType.CrossReference;
    public UsjCrossReference() : this(string.Empty, string.Empty, new List<IUsjNode>()) { }
}

public sealed record UsjText(string Text) : IUsjNode
{
    [JsonIgnore]
    public UsjNodeType Type => UsjNodeType.Text;
    public UsjText() : this(string.Empty) { }
}

public static class UsjConstants
{
    public static readonly IReadOnlyList<string> ParaStylesToHide = ["ide", "toc", "mt"];
}