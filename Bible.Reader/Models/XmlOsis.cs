namespace Bible.Reader.Models
{
    using System.Xml.Serialization;

    [XmlRoot(ElementName = "osis", Namespace = "http://www.w3.org/2001/XMLSchema")]
    public class Osis
    {
        [XmlElement(ElementName = "osisCorpus")]
        public OsisCorpus[] OsisCorpus { get; set; }

        [XmlElement(ElementName = "osisText")]
        public OsisText[] OsisText { get; set; }

        [XmlAttribute(AttributeName = "TEIform")]
        public string TEIform { get; set; }
    }

    public class OsisCorpus
    {
        [XmlElement(ElementName = "header")]
        public CorpusHeader Header { get; set; }

        [XmlElement(ElementName = "titlePage")]
        public TitlePage TitlePage { get; set; }

        [XmlElement(ElementName = "osisText")]
        public OsisText[] OsisText { get; set; }

        [XmlAttribute(AttributeName = "TEIform")]
        public string TEIform { get; set; }
    }

    public class CorpusHeader
    {
        [XmlElement(ElementName = "revisionDesc")]
        public RevisionDesc[] RevisionDesc { get; set; }

        [XmlElement(ElementName = "work")]
        public Work[] Work { get; set; }

        [XmlAttribute(AttributeName = "TEIform")]
        public string TEIform { get; set; }

        [XmlAttribute(AttributeName = "canonical")]
        public bool Canonical { get; set; }
    }

    public class OsisText
    {
        [XmlElement(ElementName = "header")]
        public Header Header { get; set; }

        [XmlElement(ElementName = "titlePage")]
        public TitlePage TitlePage { get; set; }

        [XmlElement(ElementName = "div")]
        public Div[] Div { get; set; }

        [XmlAttribute(AttributeName = "annotateRef")]
        public string AnnotateRef { get; set; }

        [XmlAttribute(AttributeName = "canonical")]
        public bool Canonical { get; set; }

        [XmlAttribute(AttributeName = "ID")]
        public string ID { get; set; }

        [XmlAttribute(AttributeName = "osisID")]
        public string OsisID { get; set; }

        [XmlAttribute(AttributeName = "osisIDWork")]
        public string OsisIDWork { get; set; }

        [XmlAttribute(AttributeName = "osisRefWork")]
        public string OsisRefWork { get; set; }

        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }

        [XmlAttribute(AttributeName = "subType")]
        public string SubType { get; set; }

        [XmlAttribute(AttributeName = "lang")]
        public string Lang { get; set; }

        [XmlAttribute(AttributeName = "space")]
        public string Space { get; set; }

        [XmlAttribute(AttributeName = "TEIform")]
        public string TEIform { get; set; }
    }

    public class Header
    {
        [XmlElement(ElementName = "revisionDesc")]
        public RevisionDesc[] RevisionDesc { get; set; }

        [XmlElement(ElementName = "work")]
        public Work[] Work { get; set; }

        [XmlElement(ElementName = "workPrefix")]
        public WorkPrefix[] WorkPrefix { get; set; }

        [XmlAttribute(AttributeName = "TEIform")]
        public string TEIform { get; set; }

        [XmlAttribute(AttributeName = "canonical")]
        public bool Canonical { get; set; }
    }

    public class RevisionDesc
    {
        // TODO: add properties for revisionDesc
    }

    public class Work
    {
        // TODO: add properties for work
    }

    public class WorkPrefix
    {
        // TODO: add properties for workPrefix
    }

    public class TitlePage
    {
        // TODO: add properties for titlePage
    }

    public class Div
    {
        // TODO: add properties for div
    }
}