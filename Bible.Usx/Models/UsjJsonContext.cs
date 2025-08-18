using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bible.Usx.Models;

[JsonSerializable(typeof(UsjBook))]
[JsonSerializable(typeof(UsjIdentification))]
[JsonSerializable(typeof(UsjChapterMarker))]
[JsonSerializable(typeof(UsjVerseMarker))]
[JsonSerializable(typeof(UsjPara))]
[JsonSerializable(typeof(UsjChar))]
[JsonSerializable(typeof(UsjFootnote))]
[JsonSerializable(typeof(UsjMilestone))]
[JsonSerializable(typeof(UsjLineBreak))]
[JsonSerializable(typeof(UsjCrossReference))]
[JsonSerializable(typeof(UsjText))]
public partial class UsjJsonContext : JsonSerializerContext
{
    // This class is used to generate the JSON serialization metadata for the Usj models.
    // It allows for polymorphic serialization and deserialization of UsjNode types.
    // The [JsonSerializable] attributes specify which types should be included in the context.
}