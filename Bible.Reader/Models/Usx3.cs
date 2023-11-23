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
    public class Usx3 : IUsx3Base
    {
        [XmlAttribute("version")]
        public decimal Version { get; set; }

        [XmlElement("book", typeof(Usx3Book))]
        [XmlElement("para", typeof(Usx3Paragraph))]
        [XmlElement("chapter", typeof(Usx3Chapter))]
        public Usx3CollationBase[] Items { get; set; }
    }

    /// <summary>
    /// <see href="https://ubsicap.github.io/usx/elements.html#book"/>
    /// </summary>
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class Usx3Book : Usx3CollationBase
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
    public class Usx3Chapter : Usx3StartEndBase
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
    public class Usx3Paragraph : Usx3CollationBase
    {
        [XmlElement("char", typeof(Usx3Char))]
        [XmlElement("note", typeof(Usx3Note))]
        [XmlElement("verse", typeof(Usx3Verse))]
        [XmlElement("optbreak", typeof(Usx3OptionalBreak))]
        public Usx3StyleBase[] Items { get; set; }

        [XmlText]
        public string[] Text { get; set; }

        [XmlAttribute("vid")]
        public string VerseId { get; set; }
    }

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class Usx3Char : Usx3CharBase
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
    public class Usx3Note : Usx3StyleBase
    {
        [XmlElement("char", typeof(Usx3ParaNoteChar))]
        public Usx3CharBase[] Chars { get; set; }

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
    public class Usx3ParaNoteChar : Usx3CharBase
    {
        [XmlElement("char")]
        public Usx3CrossReferenceChar[] Char { get; set; }
    }

    /// <summary>
    /// <see href="https://ubsicap.github.io/usx/notes.html#cross-reference-char"/>
    /// </summary>
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class Usx3CrossReferenceChar : Usx3CharBase
    {
        [XmlAttribute("closed")]
        public bool Closed { get; set; }

        [XmlElement("ref")]
        public Usx3Reference[] Reference { get; set; }

        [XmlText]
        public string[] Text { get; set; }
    }

    /// <summary>
    /// <see href="https://ubsicap.github.io/usx/elements.html#ref"/>
    /// </summary>
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class Usx3Reference : IUsx3Base
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
    public class Usx3Verse : Usx3StartEndBase
    {
        [XmlAttribute("number")]
        public string Number { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    /// <summary>
    /// <see href="https://ubsicap.github.io/usx/elements.html#ms"/>
    /// </summary>
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class Usx3Milestone : Usx3StartEndBase { }

    /// <summary>
    /// <see href="https://ubsicap.github.io/usx/elements.html#optbreak"/>
    /// </summary>
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class Usx3OptionalBreak : Usx3StyleBase { }

    public abstract class Usx3StartEndBase : Usx3CollationBase
    {
        [XmlAttribute("sid")]
        public string StartId { get; set; }

        [XmlAttribute("eid")]
        public string EndId { get; set; }
    }

    public abstract class Usx3CollationBase : Usx3StyleBase { }

    public abstract class Usx3CharBase : Usx3StyleBase { }

    public abstract class Usx3StyleBase : IUsx3Base
    {
        [XmlAttribute("style")]
        public string Style { get; set; }
    }

    public interface IUsx3Base { }
}
