using System.Xml;
using Bible.Usx.Models;

namespace Bible.Usx.Parsers;

public class LineBreakParser : IUsxElementParser
{
    public static readonly string Key = "optbreak";

    public string ElementName => Key;

    public async Task<IUsjNode> ParseAsync(XmlReader reader)
    {
        var style = reader.GetAttribute("style") ?? string.Empty;

        if (!reader.IsEmptyElement)
        {
            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "optbreak")
                    break;
            }
        }

        return new UsjLineBreak(style);
    }
}
