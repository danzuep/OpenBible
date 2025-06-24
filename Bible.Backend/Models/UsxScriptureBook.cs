namespace Bible.Backend.Models;

using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Extensions.Options;

/// <see href="https://ubsicap.github.io/"/>
[XmlRoot("usx")]
public sealed class UsxScriptureBook : IUsxBase
{
    [XmlAttribute("version")]
    public string UsxVersion { get; set; }

    [XmlElement("book")]
    public UsxIdentification Translation { get; set; }

    [XmlElement("chapter", typeof(UsxChapterMarker))]
    [XmlElement("para", typeof(UsxPara))]
    public UsxStyleBase[] Content { get; set; }

    public override string ToString() =>
        $"{Translation?.BookCode}";
}

public sealed class UsxIdentification : UsxStyleBase
{
    [XmlAttribute("code")]
    public string BookCode { get; set; }

    [XmlText]
    public string Name { get; set; }

    public static implicit operator string?(UsxIdentification text) =>
        text?.Name;

    public static implicit operator UsxIdentification(string value) =>
        new UsxIdentification { Name = value };

    public override string ToString() =>
        $"{Name} ({Style}, {BookCode})";
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

public sealed class UsxLineBreak : IUsxBase
{
}

public sealed class UsxCrossReference : IUsxBase
{
    [XmlAttribute("loc")]
    public string Location { get; set; }

    [XmlElement("char", typeof(UsxChar))]
    [XmlText(typeof(string))]
    public object[] Content { get; set; }

    public override string ToString() => Location;
}

public interface IUsxBase { }

public interface IUsxText : IUsxBase
{
    string Text { get; }
}

public abstract class UsxStyleBase : IUsxBase
{
    [XmlAttribute("style")]
    public string Style { get; set; }

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


[XmlRoot("usx")]
public sealed class UsxParaBaseTest : IUsxBase
{
    [XmlElement("para", typeof(UsxPara))]
    public UsxPara Para { get; set; }
}

public sealed class UsxVisitorOptions : IOptions<UsxVisitorOptions>
{
    public bool EnableStrongs { get; set; }
    public bool EnableRedLetters { get; set; }
    public bool EnableFootnotes { get; set; }
    public bool EnableCrossReferences { get; set; }
    public bool EnableChapterLinks { get; set; }

    public UsxVisitorOptions Value => this;
}

internal sealed class UsxVisitorReference
{
    public string Title { get; set; } = "Bible";
    public string? BookCode { get; set; }
    public string? Chapter { get; set; }
    public string? Verse { get; set; }

    public override string ToString()
    {
        if (Verse != null)
            return $"{BookCode}.{Chapter}.{Verse}";
        else if (Chapter != null)
            return $"{BookCode}.{Chapter}";
        else
            return BookCode ?? "_";
    }
}


