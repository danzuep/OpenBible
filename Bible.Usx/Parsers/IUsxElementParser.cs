using System.Xml;
using Bible.Usx.Models;

namespace Bible.Usx.Parsers;

public interface IUsxElementParser
{
    /// <summary>
    /// Element name this parser handles.
    /// </summary>
    string ElementName { get; }

    /// <summary>
    /// Parse the current element from the XmlReader (positioned at start element).
    /// Returns a USJ node representing the element.
    /// </summary>
    Task<IUsjNode> ParseAsync(XmlReader reader, CancellationToken cancellationToken = default);
}
