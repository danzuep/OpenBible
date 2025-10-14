using System.Xml;
using Bible.Usx.Models;

namespace Bible.Usx.Parsers;

public class TextParser : IUsxElementParser
{
    public static readonly string Key = "text";

    public string ElementName => Key;

    private readonly Func<int, IList<string>>? _enrich;
    public TextParser(Func<int, IList<string>>? enrich = null)
    {
        _enrich = enrich;
        HasTextEnrichment = enrich != null;
    }

    internal bool HasTextEnrichment { get; private set; }

    public UsjChar Enrich(UsjChar usjChar)
    {
        var usjNodes = ParseEnriched(usjChar.Text, false);
        if (!usjNodes.Any())
        {
            return usjChar;
        }
        return usjChar with
        {
            Text = string.Empty,
            Content = usjNodes
        };
    }

    public IList<IUsjNode> ParseEnriched(string? text, bool addTextAfterClear = true)
    {
        var usjNodes = new List<IUsjNode>();
        if (string.IsNullOrEmpty(text))
        {
            return usjNodes;
        }
        if (_enrich == null)
        {
            usjNodes.Add(new UsjText(text));
            return usjNodes;
        }

        var hasUnicode = false;
        foreach (var rune in text.EnumerateRunes())
        {
            if (rune.IsAscii)
            {
                usjNodes.Add(new UsjText(rune.ToString()));
            }
            else
            {
                hasUnicode = true;
                var metadata = string.Join(", ", _enrich(rune.Value));
                usjNodes.Add(new UsjChar("rune", rune.ToString(), metadata, null, null));
            }
        }

        // If there are only ASCII characters, UsjText is more efficient
        if (!hasUnicode)
        {
            usjNodes.Clear();
            if (addTextAfterClear) usjNodes.Add(new UsjText(text));
        }

        return usjNodes;
    }

    public Task<IUsjNode> ParseAsync(XmlReader reader, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Use ParseEnriched method instead.");
    }
}
