using System.Xml;
using Bible.Usx.Models;

namespace Bible.Usx.Parsers;

public interface IParser<TIn, TOut>
{
    /// <inheritdoc cref="Convert"/>
    Task<TOut> ParseAsync(TIn input, CancellationToken cancellationToken = default);
}

public interface IXmlReaderParser<T> : IParser<XmlReader, T>
{
    new Task<T> ParseAsync(XmlReader reader, CancellationToken cancellationToken = default);
}

public interface IUsxElementParser : IXmlReaderParser<IUsjNode>
{
    /// <summary>
    /// Element name this parser handles.
    /// </summary>
    string ElementName { get; }

    /// <summary>
    /// Parse the current element from the XmlReader (positioned at start element).
    /// Returns a USJ node representing the element.
    /// </summary>
    new Task<IUsjNode> ParseAsync(XmlReader reader, CancellationToken cancellationToken = default);
}
