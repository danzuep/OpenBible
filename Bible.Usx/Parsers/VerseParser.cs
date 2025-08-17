using System.Xml;
using Bible.Usx.Models;

namespace Bible.Usx.Parsers;

public class VerseParser : IUsxElementParser
{
    public static readonly string Key = "verse";

    public string ElementName => Key;

    public async Task<IUsjNode> ParseAsync(XmlReader reader)
    {
        var number = reader.GetAttribute("number") ?? string.Empty;
        var style = "verse";
        var sid = reader.GetAttribute("sid");
        var eid = reader.GetAttribute("eid");

        if (!reader.IsEmptyElement)
        {
            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "verse")
                    break;
            }
        }

        return new UsjVerseMarker(number, style, sid, eid);
    }
}
