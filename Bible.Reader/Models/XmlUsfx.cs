using System.Xml.Serialization;

namespace Bible.Reader.Models
{
    [XmlRoot(ElementName = "usfx", Namespace = "http://www.w3.org/2001/XMLSchema")]
    public class XmlUsfx
    {
        [XmlElement("languageCode")]
        public string[] LanguageCodes { get; set; }

        [XmlElement("rem")]
        public string[] Remarks { get; set; }

        [XmlElement("book")]
        public Book[] Books { get; set; }
    }

    public class Book
    {
        [XmlElement("id")]
        public Id Id { get; set; }

        [XmlElement("usfm")]
        public string Usfm { get; set; }

        [XmlElement("rem")]
        public string[] Remarks { get; set; }

        [XmlElement("h")]
        public Heading[] Headings { get; set; }

        [XmlElement("cl")]
        public string[] ChapterLabels { get; set; }

        [XmlElement("p")]
        public PType[] Paragraphs { get; set; }

        [XmlElement("q")]
        public PType[] PoetryLines { get; set; }

        [XmlElement("mt")]
        public PType[] MainTitles { get; set; }

        [XmlElement("d")]
        public PType[] DescriptiveTitles { get; set; }

        [XmlElement("s")]
        public PType[] SectionHeadings { get; set; }

        [XmlElement("sectionBoundary")]
        public SectionBoundary[] SectionBoundaries { get; set; }

        [XmlElement("b")]
        public B[] BlankLines { get; set; }

        [XmlElement("generated")]
        public string[] GeneratedContent { get; set; }

        [XmlElement("c")]
        public Chapter[] Chapters { get; set; }

        [XmlElement("ca")]
        public string[] AlternateChapters { get; set; }
    }

    public class Id
    {
        [XmlAttribute("id")]
        public string Value { get; set; }
    }

    public class Heading
    {
        [XmlAttribute("level")]
        public byte Level { get; set; }

        [XmlText]
        public string Value { get; set; }
    }

    public class PType
    {
        [XmlAttribute("sfm")]
        public string Sfm { get; set; }

        [XmlText]
        public string Value { get; set; }
    }

    public class SectionBoundary
    {
        [XmlAttribute("sfm")]
        public string Sfm { get; set; }

        [XmlAttribute("level")]
        public byte Level { get; set; }

        [XmlText]
        public string Value { get; set; }
    }

    public class B
    {
        [XmlAttribute("sfm")]
        public string Sfm { get; set; }

        [XmlAttribute("style")]
        public string Style { get; set; }

        [XmlText]
        public string Value { get; set; }
    }

    public class Chapter
    {
        [XmlAttribute("id")]
        public string Id { get; set; }

        [XmlText]
        public string[] Values { get; set; }
    }
}