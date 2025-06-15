using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Bible.Backend.Usx
{
    public partial class UsxPara : UsxStyleBase, IXmlSerializable
    {
        [XmlElement("verse")]
        public UsxMarker VerseMarker { get; set; }

        [XmlIgnore]
        public List<object> Content { get; set; } = new();
        //public List<IUsxBase> Content { get; set; } = new();

        public override string ToString() =>
            $"{VerseMarker.Number} ({Style}, {Content.Count})";

        public XmlSchema GetSchema() => null;

        public void ReadXml(XmlReader reader)
        {
            Content.Clear();

            // Read style attribute
            if (reader.MoveToAttribute("style"))
            {
                Style = reader.Value;
            }

            // Move to content inside <para>
            reader.MoveToElement();

            if (reader.IsEmptyElement)
            {
                reader.ReadStartElement(); // <para/>
                return;
            }

            reader.ReadStartElement(); // consume <para>

            while (!(reader.NodeType == XmlNodeType.EndElement && reader.Name == "para"))
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "verse":
                            // Deserialize VerseMarker
                            var verseSerializer = new XmlSerializer(typeof(UsxMarker));
                            VerseMarker = (UsxMarker)verseSerializer.Deserialize(reader);
                            break;

                        case "char":
                            var charSerializer = new XmlSerializer(typeof(UsxChar));
                            var ch = (UsxChar)charSerializer.Deserialize(reader);
                            Content.Add(ch);
                            break;

                        case "note":
                            var noteSerializer = new XmlSerializer(typeof(UsxNote));
                            var note = (UsxNote)noteSerializer.Deserialize(reader);
                            Content.Add(note);
                            break;

                        case "ms":
                            var milestoneSerializer = new XmlSerializer(typeof(UsxMilestone));
                            var milestone = (UsxMilestone)milestoneSerializer.Deserialize(reader);
                            Content.Add(milestone);
                            break;

                        case "optbreak":
                            var linkSerializer = new XmlSerializer(typeof(UsxLineBreak));
                            var link = (UsxLineBreak)linkSerializer.Deserialize(reader);
                            Content.Add(link);
                            break;

                        default:
                            // Unknown element, skip or handle as needed
                            reader.Skip();
                            break;
                    }
                }
                else if (reader.NodeType == XmlNodeType.Text
                    || reader.NodeType == XmlNodeType.CDATA
                    || reader.NodeType == XmlNodeType.SignificantWhitespace
                    || reader.NodeType == XmlNodeType.Whitespace)
                {
                    string text = reader.Value;
                    if (!string.IsNullOrEmpty(text))
                    {
                        Content.Add(text);
                    }
                    reader.Read();
                }
                else
                {
                    reader.Read();
                }
            }

            reader.ReadEndElement(); // consume </para>
        }

        public void WriteXml(XmlWriter writer)
        {
            if (!string.IsNullOrEmpty(Style))
            {
                writer.WriteAttributeString("style", Style);
            }

            // Write verse marker if present
            if (VerseMarker != null)
            {
                var verseSerializer = new XmlSerializer(typeof(UsxMarker));
                verseSerializer.Serialize(writer, VerseMarker);
            }

            // Write content list
            foreach (var item in Content)
            {
                switch (item)
                {
                    case string s:
                        writer.WriteString(s);
                        break;

                    case UsxChar ch:
                        var charSerializer = new XmlSerializer(typeof(UsxChar));
                        charSerializer.Serialize(writer, ch);
                        break;

                    case UsxNote note:
                        var noteSerializer = new XmlSerializer(typeof(UsxNote));
                        noteSerializer.Serialize(writer, note);
                        break;

                    case UsxMilestone milestone:
                        var milestoneSerializer = new XmlSerializer(typeof(UsxMilestone));
                        milestoneSerializer.Serialize(writer, milestone);
                        break;

                    case UsxLineBreak optbreak:
                        var linkSerializer = new XmlSerializer(typeof(UsxLineBreak));
                        linkSerializer.Serialize(writer, optbreak);
                        break;

                    default:
                        // Unknown type, ignore or throw
                        break;
                }
            }
        }
    }
}
