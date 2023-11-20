namespace Bible.Reader.Models
{
    using System.Xml.Serialization;

    [XmlRoot(ElementName = "XMLBIBLE")]
    //[XmlRoot(ElementName = "XMLBIBLE", Namespace = "http://www.w3.org/2001/XMLSchema")]
    public sealed class XmlZefania
    {
        [XmlAttribute("biblename")]
        public string BibleName { get; set; }

        [XmlAttribute("version")]
        public string Version { get; set; }

        [XmlElement("INFORMATION")]
        public XzInformation Information { get; set; }

        [XmlElement("BIBLEBOOK")]
        public XzBook[] BibleBooks { get; set; }

        //[XmlElement("STYLESETS")]
        //public StyleSets StyleSets { get; set; }

        //[XmlElement("APPINFO")]
        //public AppInfo[] AppInfos { get; set; }

        //[XmlAttribute("revision")]
        //public int Revision { get; set; }

        //[XmlAttribute("status")]
        //public string Status { get; set; }

        //[XmlAttribute("type")]
        //public string Type { get; set; }
    }

    public sealed class XzInformation
    {
        [XmlElement("format")]
        public string Format { get; set; }

        [XmlElement("date")]
        public string Date { get; set; }

        [XmlElement("creator")]
        public string Creator { get; set; }

        [XmlElement("language")]
        public string Language { get; set; }

        [XmlElement("rights")]
        public string Rights { get; set; }
    }

    public sealed class XzBook
    {
        [XmlAttribute("bnumber")]
        public int Number { get; set; }

        [XmlAttribute("bname")]
        public string Name { get; set; }

        //[XmlAttribute("bsname")]
        //public string ShortName { get; set; }

        [XmlElement("CHAPTER")]
        public XzChapter[] Chapters { get; set; }
    }

    public sealed class XzChapter
    {
        [XmlAttribute("cnumber")]
        public int Number { get; set; }

        [XmlElement("VERS")]
        public XzVerse[] Verses { get; set; }

        //[XmlElement("PROLOG")]
        //public Prolog[] Prologs { get; set; }

        //[XmlElement("CAPTION")]
        //public Caption[] Captions { get; set; }

        //[XmlElement("REMARK")]
        //public Remark[] Remarks { get; set; }

        //[XmlElement("MEDIA")]
        //public Media[] Medias { get; set; }
    }

    public sealed class XzVerse
    {
        [XmlAttribute("vnumber")]
        public int Number { get; set; }

        [XmlText]
        public string Text { get; set; }

        //[XmlAttribute("enumber")]
        //public int RangeEnd { get; set; }

        //[XmlAttribute("pfr")]
        //public bool IsProofread { get; set; }

        //[XmlElement("NOTE")]
        //public Note[] Notes { get; set; }

        //[XmlElement("DIV")]
        //public Div[] Divs { get; set; }

        //[XmlAttribute("aix")]
        //public string Aix { get; set; }

        //[XmlAttribute("vg")]
        //public string Vg { get; set; }
    }

    //public sealed class Prolog : Caption
    //{
    //    [XmlText]
    //    public string Value { get; set; }

    //    [XmlAttribute("vref")]
    //    public string Id { get; set; }
    //}

    //public class Caption
    //{
    //    [XmlElement("STYLE")]
    //    public Style[] Styles { get; set; }

    //    [XmlElement("BR")]
    //    public Br[] Breaks { get; set; }

    //    [XmlElement("XREF")]
    //    public Xref[] CrossReferences { get; set; }
    //}

    //public sealed class StyleSets
    //{
    //    [XmlElement("S")]
    //    public Style[] Styles { get; set; }
    //}

    //public sealed class Style
    //{
    //    [XmlAttribute("id")]
    //    public string Id { get; set; }
    //}

    //public sealed class AppInfo
    //{
    //    [XmlText]
    //    public string Value { get; set; }
    //}
}