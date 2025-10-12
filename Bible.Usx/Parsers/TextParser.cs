using System.Xml;
using Bible.Usx.Models;

namespace Bible.Usx.Parsers;

public class TextParser : IUsxElementParser
{
    private readonly Func<int, IList<string>>? _enrich;
    public TextParser(Func<int, IList<string>>? enrich = null)
    {
        _enrich = enrich;
    }

    public static readonly string Key = "text";

    public string ElementName => Key;

    public Task<IUsjNode> ParseAsync(XmlReader reader)
    {
        throw new NotImplementedException("Use ParseEnriched method instead.");
    }

    public IList<IUsjNode> ParseEnriched(UsjChar usjChar)
    {
        return ParseEnriched(usjChar.Text);
    }

    public IList<IUsjNode> ParseEnriched(string? text)
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

        foreach (var rune in text.EnumerateRunes())
        {
            if (rune.IsAscii)
            {
                usjNodes.Add(new UsjText(rune.ToString()));
            }
            else
            {
                var metadata = string.Join(", ", _enrich(rune.Value));
                usjNodes.Add(new UsjChar("c", false, metadata, rune.ToString()));
            }
        }

        return usjNodes;
    }
}
