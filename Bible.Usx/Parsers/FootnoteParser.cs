using System.Xml;
using Bible.Usx.Models;
using Bible.Usx.Services;

namespace Bible.Usx.Parsers;

public class FootnoteParser : IUsxElementParser
{
    public static readonly string Key = "note";

    public string ElementName => Key;

    private readonly UsxParserFactory _parserFactory;

    public FootnoteParser(UsxParserFactory parserFactory)
    {
        _parserFactory = parserFactory;
    }

    public async Task<IUsjNode> ParseAsync(XmlReader reader)
    {
        var caller = reader.GetAttribute("caller") ?? string.Empty;
        var style = reader.GetAttribute("style") ?? string.Empty;
        var content = new List<IUsjNode>();

        if (reader.IsEmptyElement)
            return new UsjFootnote(caller, style, content);

        while (await reader.ReadAsync())
        {
            if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "note")
                break;

            if (reader.NodeType == XmlNodeType.Element)
            {
                if (_parserFactory.TryGetParser(reader.Name, out var parser) && parser != null)
                {
                    content.Add(await parser.ParseAsync(reader));
                }
                else
                {
                    await reader.SkipAsync();
                }
            }
            else if (reader.NodeType == XmlNodeType.Text || reader.NodeType == XmlNodeType.CDATA)
            {
                var text = reader.Value;
                if (!string.IsNullOrWhiteSpace(text))
                    content.Add(new UsjText(text!));
            }
        }

        return new UsjFootnote(caller, style, content);
    }
}
