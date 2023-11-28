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
    public class XmlUsx : IXmlUsxBase
    {
        [XmlAttribute("version")]
        public decimal Version { get; set; }

        [XmlElement("book", typeof(XmlUsxBook))]
        [XmlElement("para", typeof(XmlUsxParagraph))]
        [XmlElement("chapter", typeof(XmlUsxChapter))]
        public XmlUsxCollationBase[] Items { get; set; }
    }

    /// <summary>
    /// <see href="https://ubsicap.github.io/usx/elements.html#book"/>
    /// </summary>
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class XmlUsxBook : XmlUsxCollationBase
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
    public class XmlUsxChapter : XmlUsxStartEndBase
    {
        [XmlAttribute("number")]
        public int Number { get; set; }


        /// FYI - this is not part of the USX standard
        [XmlElement("para")]
        public XmlUsxParagraph[] Paragraphs { get; set; }
    }

    /// <summary>
    /// <see href="https://ubsicap.github.io/usx/elements.html#para"/>
    /// </summary>
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class XmlUsxParagraph : XmlUsxCollationBase
    {
        [XmlElement("char", typeof(XmlUsxChar))]
        [XmlElement("note", typeof(XmlUsxNote))]
        [XmlElement("verse", typeof(XmlUsxVerse))]
        [XmlElement("optbreak", typeof(XmlUsxOptionalBreak))]
        public XmlUsxStyleBase[] Items { get; set; }

        [XmlAttribute("vid")]
        public string VerseId { get; set; }
    }

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class XmlUsxChar : XmlUsxCharBase
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
    public class XmlUsxNote : XmlUsxStyleBase
    {
        [XmlElement("char", typeof(XmlUsxParaNoteChar))]
        public XmlUsxCharBase[] Chars { get; set; }

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
    public class XmlUsxParaNoteChar : XmlUsxCharBase
    {
        [XmlElement("char")]
        public XmlUsxCrossReferenceChar[] Char { get; set; }
    }

    /// <summary>
    /// <see href="https://ubsicap.github.io/usx/notes.html#cross-reference-char"/>
    /// </summary>
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class XmlUsxCrossReferenceChar : XmlUsxCharBase
    {
        [XmlAttribute("closed")]
        public bool Closed { get; set; }

        [XmlElement("ref")]
        public XmlUsxReference[] Reference { get; set; }

        [XmlText]
        public string[] Text { get; set; }
    }

    /// <summary>
    /// <see href="https://ubsicap.github.io/usx/elements.html#ref"/>
    /// </summary>
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class XmlUsxReference : IXmlUsxBase
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
    public class XmlUsxVerse : XmlUsxStartEndBase
    {
        [XmlAttribute("number")]
        public string Number { get; set; }

        [XmlElement("char", typeof(XmlUsxChar))]
        [XmlElement("note", typeof(XmlUsxNote))]
        [XmlElement("optbreak", typeof(XmlUsxOptionalBreak))]
        public XmlUsxStyleBase[] Items { get; set; }

        [XmlText]
        public string[] Text { get; set; } = Array.Empty<string>();

        private static readonly char[] _trimChars = new char[] { '\r', '\n' };
        public string VerseText => string.Join("", Text).Trim(_trimChars);
    }

    /// <summary>
    /// <see href="https://ubsicap.github.io/usx/elements.html#ms"/>
    /// </summary>
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class XmlUsxMilestone : XmlUsxStartEndBase { }

    /// <summary>
    /// <see href="https://ubsicap.github.io/usx/elements.html#optbreak"/>
    /// </summary>
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class XmlUsxOptionalBreak : XmlUsxStyleBase { }

    public abstract class XmlUsxStartEndBase : XmlUsxCollationBase
    {
        [XmlAttribute("sid")]
        public string StartId { get; set; }

        [XmlAttribute("eid")]
        public string EndId { get; set; }
    }

    public abstract class XmlUsxCollationBase : XmlUsxStyleBase { }

    public abstract class XmlUsxCharBase : XmlUsxStyleBase { }

    public abstract class XmlUsxStyleBase : IXmlUsxBase
    {
        [XmlAttribute("style")]
        public string Style { get; set; }
    }

    public interface IXmlUsxBase { }
}
