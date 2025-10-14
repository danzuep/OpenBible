using System.Xml;
using Bible.Usx.Models;

namespace Bible.Usx.Parsers;

public class ChapterMarkerParser : IUsxElementParser
{
    public static readonly string Key = "chapter";

    public string ElementName => Key;

    public async Task<IUsjNode> ParseAsync(XmlReader reader, CancellationToken cancellationToken = default)
    {
        var style = reader.GetAttribute("style") ?? string.Empty;
        var number = reader.GetAttribute("number") ?? string.Empty;
        var sid = reader.GetAttribute("sid");
        var eid = reader.GetAttribute("eid");

        if (!reader.IsEmptyElement)
        {
            while (await reader.ReadAsync())
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == Key)
                    break;
            }
        }

        return new UsjChapterMarker(style, number, sid, eid);
    }
}
