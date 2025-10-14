using System.Xml;
using Bible.Usx.Models;
using Bible.Usx.Services;

namespace Bible.Usx.Parsers;

public class CrossReferenceParser : IUsxElementParser
{
    public static readonly string Key = "ref";

    public string ElementName => Key;

    private readonly UsxParserFactory _parserFactory;

    public CrossReferenceParser(UsxParserFactory parserFactory)
    {
        _parserFactory = parserFactory;
    }

    public async Task<IUsjNode> ParseAsync(XmlReader reader, CancellationToken cancellationToken = default)
    {
        var style = reader.GetAttribute("style") ?? string.Empty;
        var location = reader.GetAttribute("loc") ?? string.Empty;
        var content = new List<IUsjNode>();

        if (reader.IsEmptyElement)
            return new UsjCrossReference(style, location, content);

        while (await reader.ReadAsync())
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (reader.NodeType == XmlNodeType.EndElement && reader.Name == Key)
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

        return new UsjCrossReference(style, location, content);
    }
}
