namespace Bible.Backend.Visitors;

using Bible.Backend.Abstractions;
using Bible.Backend.Models;
using Bible.Core.Models;
using Microsoft.Extensions.Options;

public sealed class UsxToBibleBookVisitor : IUsxVisitor
{
    public static BibleBook GetBibleBook(UsxScriptureBook? usxScriptureBook, UnihanLookup? unihan = null, UsxVisitorOptions? options = null)
    {
        var visitor = new UsxToBibleBookVisitor(options);
        visitor.Unihan = unihan;
        visitor.Accept(usxScriptureBook);
        return visitor.GetBibleBook();
    }

    public UsxToBibleBookVisitor(IOptions<UsxVisitorOptions>? options = null)
    {
        _options = options?.Value ?? new UsxVisitorOptions();
    }

    public UnihanLookup? Unihan { get; set; }

    private readonly List<UsxFootnote> _footnotes = new();

    private readonly UsxVisitorOptions _options;

    private readonly BibleBook _bibleBook = new();

    private readonly IReadOnlyList<string> _headingParaStyles =
        ["h", .. UsxToMarkdownVisitor.ParaStylesToHide];

    public void Visit(UsxIdentification identification)
    {
        if (!string.IsNullOrEmpty(identification.BookName) ||
            !string.IsNullOrEmpty(identification.BookCode))
        {
            _bibleBook.Reference = new BibleReference
            {
                BookName = identification.BookName,
                BookCode = identification.BookCode
            };
        }
    }

    public void Visit(UsxPara para)
    {
        if (_headingParaStyles.Any(h => para.Style.StartsWith(h, StringComparison.OrdinalIgnoreCase)) &&
            para.Text is string heading)
        {
            if (string.IsNullOrEmpty(_bibleBook.Reference.BookName))
                _bibleBook.Reference.BookName = heading;
            //else _bibleBook.Aliases.Add(heading);
        }
        else
        {
            this.Accept(para.Content);
        }
    }

    public void Visit(UsxChapterMarker chapterMarker)
    {
        if (int.TryParse(chapterMarker.Number, out var chapterNumber))
        {
            _bibleBook.Reference.Chapter = chapterMarker.Number;
            _bibleBook.Reference.Verse = null;
            var bibleChapter = new BibleChapter
            {
                Id = chapterNumber,
                Reference = new BibleReference(_bibleBook.Reference)
                {
                    Reference = chapterMarker.Number,
                    Chapter = chapterMarker.Number
                }
            };
            //_bibleBook.Chapters.Add(bibleChapter);
        }
    }

    public void Visit(UsxVerseMarker verseMarker)
    {
        if (int.TryParse(verseMarker.Number, out var verseNumber))
        {
            _bibleBook.Reference.Verse = verseMarker.Number;
            var bibleVerse = new BibleVerse
            {
                Id = verseNumber,
                Reference = new BibleReference(_bibleBook.Reference)
                {
                    Reference = $"{_bibleBook.Reference}:{verseMarker.Number}",
                    Verse = verseMarker.Number
                }
            };
            //_bibleBook.Chapters.LastOrDefault()?.Verses.Add(bibleVerse);
        }
    }

    public void Visit(UsxChar usxChar)
    {
        this.Accept(usxChar.Content);
    }

    public void Visit(string text)
    {
        //_bibleBook.Chapters.LastOrDefault()?.Verses.LastOrDefault()?.Add(text);
    }

    public void Visit(UsxMilestone milestone)
    {
        // Can be treated as special markup if needed
    }

    public void Visit(UsxLineBreak lineBreak)
    {
    }

    public void Visit(UsxCrossReference reference)
    {
        if (_options.EnableCrossReferences)
        {
        }
    }
    
    public void Visit(UsxFootnote note)
    {
        if (_options.EnableFootnotes)
        {
        }
    }

    public BibleBook GetBibleBook()
    {
        return _bibleBook;
    }
}