using System.Xml;
using System.Xml.Serialization;

namespace Bible.Usx.Models;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

/// <see href="https://ubsicap.github.io/"/>
[XmlRoot("usx")]
public sealed class UsxBook : IUsxNode
{
    [XmlAttribute("version")]
    public string UsxVersion { get; set; }

    [XmlElement("book")]
    public UsxIdentification Metadata { get; set; }

    [XmlElement("chapter", typeof(UsxChapterMarker))]
    [XmlElement("para", typeof(UsxPara))]
    public UsxStyleBase[] Content { get; set; }

    public override string ToString() =>
        $"{Metadata?.BookCode}";
}

public sealed class UsxIdentification : UsxStyleBase
{
    [XmlAttribute("code")]
    public string BookCode { get; set; }

    [XmlText]
    public string VersionName { get; set; }

    public static implicit operator string?(UsxIdentification text) =>
        text?.VersionName;

    public static implicit operator UsxIdentification(string value) =>
        new UsxIdentification { VersionName = value };

    public override string ToString() =>
        $"{VersionName} ({Style}, {BookCode})";
}

public abstract class UsxMarker : UsxSidEidBase
{
    [XmlAttribute("number")]
    public string Number { get; set; }

    public override string ToString() =>
        $"{Number} ({Style}, {StartId}{EndId})";
}

public sealed class UsxChapterMarker : UsxMarker
{
}

public sealed class UsxVerseMarker : UsxMarker
{
}

public sealed class UsxHeading : UsxContent
{
}

public sealed class UsxPara : UsxContent
{
}

public sealed class UsxChar : UsxContent
{
    [XmlAttribute("closed")]
    public bool Closed { get; set; }

    [XmlAttribute("strong")]
    public string? Strong { get; set; }

    public override string ToString() =>
        $"{Text} ({Style}, {Strong})";
}

public sealed class UsxFootnote : UsxStyleBase
{
    [XmlAttribute("caller")]
    public string Caller { get; set; }

    [XmlElement("char", typeof(UsxChar))]
    [XmlText(typeof(string))]
    public object[] Content { get; set; }
}

public sealed class UsxMilestone : UsxSidEidBase
{
}

public sealed class UsxLineBreak : IUsxNode
{
}

public sealed class UsxCrossReference : IUsxNode
{
    [XmlAttribute("loc")]
    public string Location { get; set; }

    [XmlElement("char", typeof(UsxChar))]
    [XmlText(typeof(string))]
    public object[] Content { get; set; }

    public override string ToString() => Location;
}

public interface IUsxNode { }

public interface IUsxText : IUsxNode
{
    string Text { get; }
}

public abstract class UsxStyleBase : IUsxNode
{
    [XmlAttribute("style")]
    public string? Style { get; set; }

    public override string ToString() => $"({Style})";
}

public abstract class UsxSidEidBase : UsxStyleBase
{
    [XmlAttribute("sid")]
    public string? StartId { get; set; }

    [XmlAttribute("eid")]
    public string? EndId { get; set; }

    public override string ToString() =>
        $"({Style}, {StartId}{EndId})";
}

public abstract class UsxContent : UsxStyleBase
{
    [XmlElement("char", typeof(UsxChar))]
    [XmlElement("verse", typeof(UsxVerseMarker))]
    [XmlElement("ms", typeof(UsxMilestone))]
    [XmlElement("optbreak", typeof(UsxLineBreak))]
    [XmlElement("note", typeof(UsxFootnote))]
    [XmlElement("ref", typeof(UsxCrossReference))]
    [XmlText(typeof(string))]
    public object[]? Content
    {
        get => _content;
        set
        {
            _content = value;
            _text = new Lazy<string?>(() =>
            {
                if (_content == null) return null;
                return string.Concat(_content.OfType<string>());
            });
        }
    }

    private object[]? _content;

    private Lazy<string?>? _text;

    public string? Text => _text?.Value;
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.