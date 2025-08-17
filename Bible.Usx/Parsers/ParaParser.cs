using System.Xml;
using Bible.Usx.Models;
using Bible.Usx.Services;

namespace Bible.Usx.Parsers;
public class ParaParser : IUsxElementParser
{
    public static readonly string Key = "para";

    public string ElementName => Key;

    private readonly UsxParserFactory _factory;

    public ParaParser(UsxParserFactory factory)
    {
        _factory = factory;
    }

    public async Task<IUsjNode> ParseAsync(XmlReader reader)
    {
        var style = reader.GetAttribute("style") ?? string.Empty;
        var content = new List<IUsjNode>();

        if (reader.IsEmptyElement)
            return new UsjPara(style, content);

        while (await reader.ReadAsync())
        {
            if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "para")
                break;

            if (reader.NodeType == XmlNodeType.Element)
            {
                if (_factory.TryGetParser(reader.Name, out var parser) && parser != null)
                    content.Add(await parser.ParseAsync(reader));
                else
                    await reader.SkipAsync();
            }
            else if (reader.NodeType == XmlNodeType.Text || reader.NodeType == XmlNodeType.CDATA)
            {
                var text = reader.Value;
                if (!string.IsNullOrWhiteSpace(text))
                    content.Add(new UsjText(text!));
            }
        }

        return new UsjPara(style, content);
    }
}