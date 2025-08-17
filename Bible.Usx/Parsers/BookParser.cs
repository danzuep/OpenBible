using System.Xml;
using Bible.Usx.Models;

namespace Bible.Usx.Parsers;

public class BookParser : IUsxElementParser
{
    public static readonly string Key = "book";

    public string ElementName => Key;

    public async Task<IUsjNode> ParseAsync(XmlReader reader)
    {
        var code = reader.GetAttribute("code") ?? string.Empty;
        string versionName = string.Empty;

        if (reader.IsEmptyElement)
        {
            return new UsjIdentification(code, versionName, "book");
        }

        await reader.ReadAsync();
        if (reader.NodeType == XmlNodeType.Text)
        {
            versionName = reader.Value ?? string.Empty;
            await reader.ReadAsync();
        }

        while (!(reader.NodeType == XmlNodeType.EndElement && reader.Name == "book"))
        {
            await reader.ReadAsync();
        }

        return new UsjIdentification(code, versionName, "book");
    }
}
