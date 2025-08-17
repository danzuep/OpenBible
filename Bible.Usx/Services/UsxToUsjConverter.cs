using System.Text.Json;
using System.Xml;
using Bible.Usx.Models;

namespace Bible.Usx.Services;

public class UsxToUsjConverter
{
    private readonly UsxParserFactory _parserFactory;

    public UsxToUsjConverter(UsxParserFactory? parserFactory = null)
    {
        _parserFactory = parserFactory ?? new UsxParserFactory();
    }

    public async Task<UsjBook> ParseUsxBookAsync(XmlReader reader)
    {
        var version = reader.GetAttribute("version") ?? string.Empty;
        UsjIdentification? identification = null;
        var content = new List<IUsjNode>();

        if (reader.IsEmptyElement)
            return new UsjBook(version, new UsjIdentification(), content);

        while (await reader.ReadAsync())
        {
            if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "usx")
                break;

            if (reader.NodeType == XmlNodeType.Element)
            {
                if (_parserFactory.TryGetParser(reader.Name, out var parser) && parser != null)
                {
                    var node = await parser.ParseAsync(reader);
                    if (node is UsjIdentification id)
                        identification = id;
                    else
                        content.Add(node);
                }
                else
                {
                    await reader.SkipAsync();
                }
            }
        }

        identification ??= new UsjIdentification();
        return new UsjBook(version, identification, content);
    }

    public async Task<UsjBook> ConvertUsxStreamToUsjBookAsync(Stream usxStream)
    {
        using var reader = XmlReader.Create(usxStream, new XmlReaderSettings { Async = true });

        while (await reader.ReadAsync())
        {
            if (reader.NodeType == XmlNodeType.Element && reader.Name == "usx")
            {
                return await ParseUsxBookAsync(reader);
            }
        }

        throw new InvalidDataException("No <usx> root element found.");
    }

    public async Task<string> ConvertUsxStreamToUsjJsonAsync(Stream usxStream)
    {
        var book = await ConvertUsxStreamToUsjBookAsync(usxStream);
        return JsonSerializer.Serialize(book, UsjJsonContext.Default.UsjBook);
    }
}
