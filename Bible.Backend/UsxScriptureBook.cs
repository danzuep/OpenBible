namespace Bible.Backend;

using System.Collections.Generic;
using System.Xml.Serialization;

// "https://ubsicap.github.io/";

[XmlRoot("usx")]
public class UsxScriptureBook
{
    [XmlAttribute("version")]
    public string UsxVersion { get; set; }

    [XmlElement("book")]
    public UsxTranslation Translation { get; set; }

    [XmlElement("chapter")]
    public UsxMarker ChapterMarker { get; set; }

    [XmlElement("para")]
    public List<UsxPara> ParagraphContent { get; set; }

    public override string ToString() =>
        $"{Translation.BookCode}";
}

public class UsxTranslation : UsxStyleBase
{
    [XmlAttribute("code")]
    public string BookCode { get; set; }

    [XmlText]
    public string Name { get; set; }

    public override string ToString() =>
        $"{Name} ({Style}, {BookCode})";
}

public class UsxMarker : UsxSidEidBase
{
    [XmlAttribute("number")]
    public int Number { get; set; }

    public override string ToString() =>
        $"{Number} ({Style}, {StartId}{EndId})";
}

public class UsxPara : UsxStyleBase
{
    [XmlElement("char", typeof(UsxChar))]
    [XmlElement("verse", typeof(UsxMarker))]
    [XmlElement("ms", typeof(UsxMilestone))]
    [XmlElement("optbreak", typeof(UsxLineBreak))]
    [XmlElement("note", typeof(UsxNote))]
    [XmlElement("ref", typeof(UsxReference))]
    [XmlText(typeof(string))]
    public List<object> Content { get; set; }
}

public class UsxChar : UsxStyleBase, IUsxTextBase
{
    [XmlAttribute("strong")]
    public string Strong { get; set; }

    [XmlText]
    public string Text { get; set; }

    public override string ToString() =>
        $"{Text} ({Style}, {Strong})";
}

public class UsxNote : UsxStyleBase
{
    [XmlAttribute("caller")]
    public string Caller { get; set; }

    [XmlElement("char")]
    public List<UsxChar> References { get; set; } = new();

    public override string ToString()
    {
        var text = References.Select(r => string.Join("", r.Text));
        return $"{text}, ({Style}, {Caller})";
    }
}

public sealed class UsxMilestone : UsxSidEidBase
{
}

public class UsxLineBreak : IUsxBase
{
}

public class UsxReference : IUsxBase
{
    [XmlAttribute("loc")]
    public string Location { get; set; }

    public override string ToString() => Location;
}

public interface IUsxBase { }

public interface IUsxTextBase : IUsxBase
{
    string Text { get; }
}

public abstract class UsxBase : IUsxBase { }

public abstract class UsxStyleBase : UsxBase
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

public abstract class UsxTextBase : UsxBase, IUsxTextBase
{
    [XmlText]
    public string Text { get; set; }

    public override string ToString() => Text;
}