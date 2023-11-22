using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bible.Reader.Models
{
    /// <summary>
    /// <see href="https://ubsicap.github.io/usx/"/>
    /// </summary>
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    [XmlRoot(ElementName = "usx", IsNullable = false)]
    public class XmlUsx3 : IXmlUsx3Base
    {
        [XmlAttribute("version")]
        public decimal Version { get; set; }

        [XmlElement("book", typeof(XmlUsx3Book))]
        [XmlElement("chapter", typeof(XmlUsx3Chapter))]
        [XmlElement("para", typeof(XmlUsx3Paragraph))]
        public XmlUsx3CollationBase[] Items { get; set; }
    }

    /// <summary>
    /// <see href="https://ubsicap.github.io/usx/elements.html#book"/>
    /// </summary>
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class XmlUsx3Book : XmlUsx3CollationBase
    {
        [XmlAttribute("code")]
        public string Id { get; set; }

        [XmlText]
        public string Value { get; set; }
    }

    /// <summary>
    /// <see href="https://ubsicap.github.io/usx/elements.html#chapter"/>
    /// </summary>
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class XmlUsx3Chapter : XmlUsx3StartEndBase
    {
        [XmlAttribute("number")]
        public int Number { get; set; }
    }

    /// <summary>
    /// <see href="https://ubsicap.github.io/usx/elements.html#para"/>
    /// </summary>
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class XmlUsx3Paragraph : XmlUsx3CollationBase
    {
        [XmlElement("char", typeof(XmlUsx3Char))]
        [XmlElement("note", typeof(XmlUsx3Note))]
        [XmlElement("verse", typeof(XmlUsx3Verse))]
        [XmlElement("optbreak", typeof(XmlUsx3OptionalBreak))]
        public XmlUsx3StyleBase[] Items { get; set; }

        [XmlText]
        public string[] Text { get; set; }

        [XmlAttribute("vid")]
        public string VerseId { get; set; }
    }

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class XmlUsx3Char : XmlUsx3CharBase
    {
        [XmlText]
        public string Value { get; set; }
    }

    /// <summary>
    /// The USX note element is used to contain the content for any footnotes or cross references.
    /// Different note types are distinguished by the note style attribute. The inner content for
    /// notes are marked using &lt;char&gt; with a specific subset of style types for the current note type.
    /// Some footnote style types are: f (footnote) | fe (endnote) | ef (extended / study note).
    /// Some cross reference style types are: x (cross reference) | ex (extended / study cross reference).
    /// <see href="https://ubsicap.github.io/usx/elements.html#note"/>
    /// </summary>
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class XmlUsx3Note : XmlUsx3StyleBase
    {
        [XmlElement("char", typeof(XmlUsx3ParaNoteChar))]
        public XmlUsx3CharBase[] Chars { get; set; }

        [XmlAttribute("caller")]
        public string Caller { get; set; }

        /// <summary>
        /// Optional attribute used to tag the Footnote <note> as belonging to a specific
        /// category of study content (e.g. Ideas, People, Places, Objects etc.).
        /// The category attribute is normally only applied to <note> @style="ef".
        /// </summary>
        [XmlAttribute("category")]
        public string Category { get; set; }
    }

    /// <summary>
    /// <see href="https://ubsicap.github.io/usx/notes.html#cross-reference-note"/>
    /// </summary>
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class XmlUsx3ParaNoteChar : XmlUsx3CharBase
    {
        [XmlElement("char")]
        public XmlUsx3CrossReferenceChar[] Char { get; set; }
    }

    /// <summary>
    /// <see href="https://ubsicap.github.io/usx/notes.html#cross-reference-char"/>
    /// </summary>
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class XmlUsx3CrossReferenceChar : XmlUsx3CharBase
    {
        [XmlAttribute("closed")]
        public bool Closed { get; set; }

        [XmlElement("ref")]
        public XmlUsx3Reference[] Reference { get; set; }

        [XmlText]
        public string[] Text { get; set; }
    }

    /// <summary>
    /// <see href="https://ubsicap.github.io/usx/elements.html#ref"/>
    /// </summary>
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class XmlUsx3Reference : IXmlUsx3Base
    {
        [XmlAttribute("loc")]
        public string Location { get; set; }

        [XmlText]
        public string Value { get; set; }
    }

    /// <summary>
    /// <see href="https://ubsicap.github.io/usx/elements.html#verse"/>
    /// </summary>
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class XmlUsx3Verse : XmlUsx3StartEndBase
    {
        [XmlAttribute("number")]
        public string Number { get; set; }
    }

    /// <summary>
    /// <see href="https://ubsicap.github.io/usx/elements.html#ms"/>
    /// </summary>
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class XmlUsx3Milestone : XmlUsx3StartEndBase { }

    /// <summary>
    /// <see href="https://ubsicap.github.io/usx/elements.html#optbreak"/>
    /// </summary>
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class XmlUsx3OptionalBreak : XmlUsx3StyleBase { }

    public abstract class XmlUsx3StartEndBase : XmlUsx3CollationBase
    {
        [XmlAttribute("sid")]
        public string StartId { get; set; }

        [XmlAttribute("eid")]
        public string EndId { get; set; }
    }

    public abstract class XmlUsx3CollationBase : XmlUsx3StyleBase { }

    public abstract class XmlUsx3CharBase : XmlUsx3StyleBase { }

    public abstract class XmlUsx3StyleBase : IXmlUsx3Base
    {
        [XmlAttribute("style")]
        public string Style { get; set; }
    }

    public interface IXmlUsx3Base { }
}
