using System.Xml;
using Bible.Usx.Models;

namespace Bible.Usx.Parsers;

public class CharParser : IUsxElementParser
{
    public static readonly string Key = "char";

    public string ElementName => Key;

    public async Task<IUsjNode> ParseAsync(XmlReader reader)
    {
        var style = reader.GetAttribute("style") ?? string.Empty;
        var closed = bool.TryParse(reader.GetAttribute("closed"), out var c) && c;
        var strong = reader.GetAttribute("strong");

        string text = string.Empty;

        if (reader.IsEmptyElement)
            return new UsjChar(style, closed, strong, text);

        await reader.ReadAsync();
        if (reader.NodeType == XmlNodeType.Text || reader.NodeType == XmlNodeType.CDATA)
        {
            text = reader.Value ?? string.Empty;
            await reader.ReadAsync();
        }

        while (!(reader.NodeType == XmlNodeType.EndElement && reader.Name == "char"))
        {
            await reader.ReadAsync();
        }

        return new UsjChar(style, closed, strong, text);
    }
}
