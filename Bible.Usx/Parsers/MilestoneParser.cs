using System.Xml;
using Bible.Usx.Models;

namespace Bible.Usx.Parsers;

public class MilestoneParser : IUsxElementParser
{
    public static readonly string Key = "ms";

    public string ElementName => Key;

    public async Task<IUsjNode> ParseAsync(XmlReader reader)
    {
        var style = reader.GetAttribute("style") ?? string.Empty;
        var sid = reader.GetAttribute("sid");
        var eid = reader.GetAttribute("eid");

        if (!reader.IsEmptyElement)
        {
            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "ms")
                    break;
            }
        }

        return new UsjMilestone(style, sid, eid);
    }
}
