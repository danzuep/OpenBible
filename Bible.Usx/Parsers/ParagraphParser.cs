using System.Xml;
using Bible.Usx.Models;
using Bible.Usx.Services;

namespace Bible.Usx.Parsers;
public class ParagraphParser : IUsxElementParser
{
    public static readonly string Key = "para";

    public string ElementName => Key;

    private readonly UsxParserFactory _factory;

    public ParagraphParser(UsxParserFactory factory)
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
                var hasParser = _factory.TryGetParser(reader.Name, out var parser);
                if (hasParser && parser is CharacterParser cpar)
                {
                    var usjChar = (UsjChar)await cpar.ParseAsync(reader);
                    content.AddRange(_factory.TextParser.ParseEnriched(usjChar));
                }
                else if (hasParser && parser != null)
                    content.Add(await parser.ParseAsync(reader));
                else
                    await reader.SkipAsync();
            }
            else if (reader.NodeType == XmlNodeType.Text ||
                reader.NodeType == XmlNodeType.Whitespace ||
                reader.NodeType == XmlNodeType.SignificantWhitespace ||
                reader.NodeType == XmlNodeType.CDATA)
            {
                content.AddRange(_factory.TextParser.ParseEnriched(reader.Value));
            }
        }

        return new UsjPara(style, content);
    }
}