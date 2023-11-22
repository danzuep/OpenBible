namespace Bible.Reader.Models.My
{
    using System.Xml.Serialization;

    [XmlRoot("xmlbible", Namespace = "http://www.w3.org/2001/XMLSchema")]
    public class XmlZefania14
    {
        [XmlAttribute("language")]
        public string Language { get; set; }

        [XmlElement("prolog")]
        public Xz14Prolog Prolog { get; set; }

        [XmlElement("biblebook")]
        public Xz14Book[] BibleBooks { get; set; }
    }

    public class Xz14Prolog
    {
        public object Value { get; set; }
    }

    public class Xz14Book
    {
        [XmlAttribute("book")]
        public string Book { get; set; }

        [XmlElement("prolog")]
        public Xz14Prolog Prolog { get; set; }

        [XmlElement("chapter")]
        public Xz14Chapter[] Chapters { get; set; }
    }

    public class Xz14Chapter
    {
        [XmlAttribute("book")]
        public string Book { get; set; }

        [XmlAttribute("cn")]
        public int ChapterNumber { get; set; }

        [XmlElement("prolog")]
        public Xz14Prolog Prolog { get; set; }

        [XmlElement("headline")]
        public Xz14Headline[] Headlines { get; set; }

        [XmlElement("verse")]
        public Xz14Verse[] Verses { get; set; }

        [XmlElement("paragraph")]
        public Xz14Paragraph[] Paragraphs { get; set; }

        [XmlElement("remark")]
        public Xz14Remark[] Remarks { get; set; }
    }

    public class Xz14Paragraph
    {
        [XmlElement("headline")]
        public Xz14Headline[] Headlines { get; set; }

        [XmlElement("verse")]
        public Xz14Verse[] Verses { get; set; }

        [XmlElement("remark")]
        public Xz14Remark[] Remarks { get; set; }
    }

    public class Xz14Verse
    {
        [XmlAttribute("book")]
        public string Book { get; set; }

        [XmlAttribute("cn")]
        public int ChapterNumber { get; set; }

        [XmlAttribute("vn")]
        public int VerseNumber { get; set; }

        [XmlAttribute("ix")]
        public int Connector { get; set; }

        [XmlElement("emphasis")]
        public Xz14Emphasis[] Emphases { get; set; }
    }

    public class Xz14Headline
    {
        public object Value { get; set; }
    }

    public class Xz14Remark
    {
        public object Value { get; set; }
    }

    public class Xz14Emphasis
    {
        [XmlAttribute("id")]
        public string Id { get; set; }

        [XmlAttribute("xd")]
        public int ExternalReference { get; set; }

        [XmlElement("emphasis")]
        public Xz14Emphasis[] Emphases { get; set; }
    }
}