using System.Xml;
using Bible.Usx.Models;

namespace Bible.Usx.Parsers;

public class LineBreakParser : IUsxElementParser
{
    public static readonly string Key = "optbreak";

    public string ElementName => Key;

    public async Task<IUsjNode> ParseAsync(XmlReader reader, CancellationToken cancellationToken = default)
    {
        var style = reader.GetAttribute("style") ?? string.Empty;

        if (!reader.IsEmptyElement)
        {
            while (await reader.ReadAsync())
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == Key)
                    break;
            }
        }

        return new UsjLineBreak(style);
    }
}
