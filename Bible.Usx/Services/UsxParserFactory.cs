using Bible.Usx.Parsers;

namespace Bible.Usx.Services;

public class UsxParserFactory
{
    private readonly Dictionary<string, Lazy<IUsxElementParser>> _lazyParsers;

    public UsxParserFactory()
    {
        _lazyParsers = new Dictionary<string, Lazy<IUsxElementParser>>(StringComparer.OrdinalIgnoreCase)
        {
            [BookParser.Key] = new Lazy<IUsxElementParser>(() => new BookParser()),
            [ChapterMarkerParser.Key] = new Lazy<IUsxElementParser>(() => new ChapterMarkerParser()),
            [VerseMarkerParser.Key] = new Lazy<IUsxElementParser>(() => new VerseMarkerParser()),
            [MilestoneParser.Key] = new Lazy<IUsxElementParser>(() => new MilestoneParser()),
            [LineBreakParser.Key] = new Lazy<IUsxElementParser>(() => new LineBreakParser()),
            [FootnoteParser.Key] = new Lazy<IUsxElementParser>(() => new FootnoteParser(this)),
            [CrossReferenceParser.Key] = new Lazy<IUsxElementParser>(() => new CrossReferenceParser(this)),
            [ParagraphParser.Key] = new Lazy<IUsxElementParser>(() => new ParagraphParser(this)),
            [CharacterParser.Key] = new Lazy<IUsxElementParser>(() => new CharacterParser()),
            [TextParser.Key] = new Lazy<IUsxElementParser>(() => new TextParser()),
        };
    }

    public bool TryGetParser(string elementName, out IUsxElementParser? parser)
    {
        if (_lazyParsers.TryGetValue(elementName, out var lazyParser))
        {
            parser = lazyParser.Value;
            return true;
        }
        parser = null;
        return false;
    }

    public bool HasTextEnrichment { get; private set; }
    public void SetTextParser(Func<int, IList<string>>? enrich = null)
    {
        _lazyParsers[TextParser.Key] = new Lazy<IUsxElementParser>(() => new TextParser(enrich));
        HasTextEnrichment = true;
    }

    public TextParser TextParser => TryGetParser(TextParser.Key, out var parser) && parser is TextParser tp ? tp: new TextParser();
}