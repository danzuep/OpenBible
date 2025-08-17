using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Bible.Usx.Models;

// Polymorphic interface with discriminator property name and string enums
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(UsjBook), "book")]
[JsonDerivedType(typeof(UsjIdentification), "identification")]
[JsonDerivedType(typeof(UsjChapterMarker), "chapter")]
[JsonDerivedType(typeof(UsjVerseMarker), "verse")]
[JsonDerivedType(typeof(UsjPara), "para")]
[JsonDerivedType(typeof(UsjChar), "char")]
[JsonDerivedType(typeof(UsjFootnote), "footnote")]
[JsonDerivedType(typeof(UsjMilestone), "milestone")]
[JsonDerivedType(typeof(UsjLineBreak), "lineBreak")]
[JsonDerivedType(typeof(UsjCrossReference), "crossReference")]
[JsonDerivedType(typeof(UsjText), "text")]
public interface IUsjNode
{
    [JsonPropertyOrder(-1)] // ensure discriminator is first in JSON
    UsjNodeType Type { get; }
}