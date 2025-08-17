using System.Xml;
using Bible.Usx.Models;

namespace Bible.Usx.Parsers;

public class ChapterParser : IUsxElementParser
{
    public static readonly string Key = "chapter";

    public string ElementName => Key;

    public async Task<IUsjNode> ParseAsync(XmlReader reader)
    {
        var number = reader.GetAttribute("number") ?? string.Empty;
        var style = "chapter";
        var sid = reader.GetAttribute("sid");
        var eid = reader.GetAttribute("eid");

        if (!reader.IsEmptyElement)
        {
            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "chapter")
                    break;
            }
        }

        return new UsjChapterMarker(number, style, sid, eid);
    }
}
