using System.Xml.Serialization;

// "https://ubsicap.github.io/";
namespace Bible.Data.Usx
{
    [XmlRoot("usx")]
    public class UsxScriptureBook
    {
        [XmlAttribute("version")]
        public string UsxVersion { get; set; }

        [XmlElement("book")]
        public UsxIdentification BookMetadata { get; set; }

        [XmlElement("chapter")]
        public UsxMarker ChapterMarker { get; set; }

        [XmlElement("para")]
        public List<UsxPara> Paragraphs { get; set; }

        public override string ToString() =>
            $"{BookMetadata.Code}";
    }

    public partial class UsxPara : UsxStyleBase
    {
        // Mixed content: text + multiple inline elements
        [XmlElement("char", typeof(UsxChar))]
        [XmlElement("verse", typeof(UsxMarker))]
        [XmlElement("note", typeof(Note))]
        [XmlElement("milestone", typeof(Milestone))]
        [XmlElement("link", typeof(Link))]
        [XmlElement("w", typeof(W))]
        [XmlText(typeof(string))]
        public List<object> Content { get; set; }
    }

    public class UsxIdentification : UsxStyleBase
    {
        [XmlAttribute("code")]
        public string Code { get; set; }

        [XmlText]
        public string Name { get; set; }

        public override string ToString() =>
            $"{Name} ({Style}, {Code})";
    }

    public class UsxMarker : UsxStyleBase
    {
        [XmlAttribute("number")]
        public int Number { get; set; }

        [XmlAttribute("sid")]
        public string? StartId { get; set; }

        [XmlAttribute("eid")]
        public string? EndId { get; set; }

        public override string ToString() =>
            $"{Number} ({Style}, {StartId}{EndId})";
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

    public class Note : UsxStyleBase
    {
        [XmlAttribute("caller")]
        public string Caller { get; set; }

        [XmlElement("char")]
        public List<UsxChar> References { get; set; } = new();

        public override string ToString()
        {
            var text = References.Select(r => r.Text);
            return $"{string.Join("", text)}, ({Style}, {Caller})";
        }
    }

    public sealed class Milestone : UsxStyleBase
    {
    }

    public class Link : UsxTextBase
    {
        [XmlAttribute("href")]
        public string Href { get; set; }

        public override string ToString() =>
            $"[{Text}]({Href})";
    }

    public class W : UsxTextBase
    {
        [XmlAttribute("lemma")]
        public string Lemma { get; set; }

        public override string ToString() => Text;
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

    public abstract class UsxTextBase : UsxBase, IUsxTextBase
    {
        [XmlText]
        public string Text { get; set; }

        public override string ToString() => Text;
    }
}
