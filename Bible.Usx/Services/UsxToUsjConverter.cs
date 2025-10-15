using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Bible.Usx.Models;

namespace Bible.Usx.Services;

public class UsxToUsjConverter
{
    public static readonly string Key = "usx";

    private readonly UsxParserFactory _parserFactory;

    public UsxToUsjConverter(UsxParserFactory? parserFactory = null)
    {
        _parserFactory = parserFactory ?? new UsxParserFactory();
    }

    public void SetTextParser(Func<int, IList<string>> enrich)
    {
        _parserFactory.SetTextParser(enrich);
    }

    public async Task<UsjBook> ParseUsxBookAsync(XmlReader reader, CancellationToken cancellationToken = default)
    {
        var version = reader.GetAttribute("version") ?? string.Empty;
        UsjIdentification? identification = null;
        var content = new List<IUsjNode>();

        if (reader.IsEmptyElement)
            return new UsjBook(version, new UsjIdentification(), content);

        while (await reader.ReadAsync())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (reader.NodeType == XmlNodeType.EndElement && reader.Name == Key)
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

    public async Task<UsjBook> ParseUsxBookAsync(XDocument xDocument)
    {
        if (xDocument.Root == null || xDocument.Root.Name.LocalName != "usx")
            throw new InvalidDataException("No <usx> root element found.");

        var version = (string?)xDocument.Root.Attribute("version") ?? string.Empty;

        UsjIdentification? identification = null;
        var content = new List<IUsjNode>();

        foreach (var element in xDocument.Root.Elements())
        {
            if (_parserFactory.TryGetParser(element.Name.LocalName, out var parser) && parser != null)
            {
                using var reader = element.CreateReader();
                // Move to the element node
                _ = reader.MoveToContent();
                var node = await parser.ParseAsync(reader);
                if (node is UsjIdentification id)
                    identification = id;
                else
                    content.Add(node);
            }
            else
            {
                // Skip unknown elements or handle as needed
            }
        }

        identification ??= new UsjIdentification();
        return new UsjBook(version, identification, content);
    }

    private static readonly LoadOptions _settings = LoadOptions.PreserveWhitespace;

    private static T? Deserialize<T>(XDocument xDocument)
    {
        var serializer = new XmlSerializer(typeof(T));
        using (var reader = xDocument.CreateReader())
        {
            var result = (T?)serializer.Deserialize(reader);
            return result;
        }
    }

    public UsjBook ConvertUsxStreamToUsjBook(Stream usxStream)
    {
        // Load XDocument synchronously but from the stream directly
        var xDocument = XDocument.Load(usxStream, _settings);

        // Verify the root element is <usx>
        if (xDocument.Root == null || xDocument.Root.Name.LocalName != "usx")
        {
            throw new InvalidDataException("No <usx> root element found.");
        }

        // Deserialize the XDocument into UsjBook
        var book = Deserialize<UsjBook>(xDocument);

        if (book == null)
        {
            throw new InvalidDataException("Failed to deserialize UsjBook.");
        }

        return book;
    }

    public async Task<UsjBook> ConvertUsxStreamToUsjBookAsync(Stream usxStream, CancellationToken cancellationToken = default)
    {
        using var reader = XmlReader.Create(usxStream, new XmlReaderSettings { Async = true });

        while (await reader.ReadAsync())
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (reader.NodeType == XmlNodeType.Element && reader.Name == "usx")
            {
                return await ParseUsxBookAsync(reader, cancellationToken);
            }
        }

        throw new InvalidDataException("No <usx> root element found.");
    }

    /// <summary>
    /// Converts a USX stream to UsjBook preserving whitespace.
    /// Tries to use XmlReader with IgnoreWhitespace=false if possible,
    /// otherwise falls back to loading XDocument with PreserveWhitespace.
    /// </summary>
    public async Task<UsjBook> ConvertUsxStreamToUsjBookPreserveWhitespaceAsync(Stream usxStream)
    {
        // Attempt to create XmlReader with whitespace preserved
        var settings = new XmlReaderSettings
        {
            Async = true,
            IgnoreWhitespace = false, // preserve whitespace nodes
        };

        try
        {
            usxStream.Seek(0, SeekOrigin.Begin); // reset stream before reading

            using var reader = XmlReader.Create(usxStream, settings);

            // Read forward to <usx> element
            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "usx")
                {
                    // Use existing async parser that respects the whitespace
                    return await ParseUsxBookAsync(reader);
                }
            }

            throw new InvalidDataException("No <usx> root element found.");
        }
        catch (XmlException)
        {
            // Fallback: if XmlReader fails, or stream does not support seeking, load XDocument preserving whitespace

            usxStream.Seek(0, SeekOrigin.Begin);

            var xDocument = XDocument.Load(usxStream, _settings);

            if (xDocument.Root == null || xDocument.Root.Name.LocalName != "usx")
                throw new InvalidDataException("No <usx> root element found.");

            var book = Deserialize<UsjBook>(xDocument);
            if (book == null) throw new InvalidDataException("Failed to deserialize UsjBook.");

            return book;
        }
    }

    private static readonly JsonSerializerOptions _options = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<string> ConvertUsxStreamToUsjJsonAsync(Stream usxStream)
    {
        var jsonTypeInfo = new UsjJsonContext(_options).UsjBook;
        var book = await ConvertUsxStreamToUsjBookAsync(usxStream);
        var json = JsonSerializer.Serialize(book, jsonTypeInfo);
        return json;
    }
}