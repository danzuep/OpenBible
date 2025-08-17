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
            [ChapterParser.Key] = new Lazy<IUsxElementParser>(() => new ChapterParser()),
            [VerseParser.Key] = new Lazy<IUsxElementParser>(() => new VerseParser()),
            [MilestoneParser.Key] = new Lazy<IUsxElementParser>(() => new MilestoneParser()),
            [LineBreakParser.Key] = new Lazy<IUsxElementParser>(() => new LineBreakParser()),
            [CharParser.Key] = new Lazy<IUsxElementParser>(() => new CharParser()),
            [FootnoteParser.Key] = new Lazy<IUsxElementParser>(() => new FootnoteParser(this)),
            [CrossReferenceParser.Key] = new Lazy<IUsxElementParser>(() => new CrossReferenceParser(this)),
            [ParaParser.Key] = new Lazy<IUsxElementParser>(() => new ParaParser(this)),
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
}