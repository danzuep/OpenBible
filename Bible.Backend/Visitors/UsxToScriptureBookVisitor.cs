namespace Bible.Backend.Visitors;

using Bible.Backend.Abstractions;
using Bible.Backend.Models;
using Bible.Core.Models;
using Microsoft.Extensions.Options;

public sealed class UsxToScriptureBookVisitor : IUsxVisitor
{
    public static ScriptureBook GetBook(UsxBook? usxScriptureBook, UnihanLookup? unihan = null, UsxVisitorOptions? options = null)
    {
        var visitor = new UsxToScriptureBookVisitor(options);
        visitor.Unihan = unihan;
        visitor.Accept(usxScriptureBook);
        return visitor.GetBibleBook();
    }

    public UsxToScriptureBookVisitor(IOptions<UsxVisitorOptions>? options = null)
    {
        _options = options?.Value ?? new UsxVisitorOptions();
    }

    public UnihanLookup? Unihan { get; set; }

    private readonly BibleReference _bibleReference = new();

    private readonly List<UsxFootnote> _footnotes = new();

    private readonly UsxVisitorOptions _options;

    private readonly ScriptureBook _bibleBook = new();

    private readonly IReadOnlyList<string> _headingParaStyles =
        ["h", .. UsxToMarkdownVisitor.ParaStylesToHide];

    public void Visit(UsxIdentification identification)
    {
        _bibleBook.AddBookMetadata(identification.Style, MetadataCategory.Style);
        if (!string.IsNullOrEmpty(identification.BookName) ||
            !string.IsNullOrEmpty(identification.BookCode))
        {
            _bibleBook.Name = identification.BookName;
            _bibleReference.BookName = identification.BookName;
            _bibleReference.BookCode = identification.BookCode;
        }
    }

    public void Visit(UsxPara para)
    {
        _bibleBook.AddScriptureSegment(para.Style, MetadataCategory.Style);
        if (_headingParaStyles.Any(h => !string.IsNullOrEmpty(para.Style) &&
            para.Style.StartsWith(h, StringComparison.OrdinalIgnoreCase)) &&
            para.Text is string heading)
        {
            if (string.IsNullOrEmpty(_bibleReference.BookName))
                _bibleReference.BookName = heading;
            _bibleBook.AddBookMetadata(heading);
        }
        else
        {
            _bibleBook.AddScriptureSegment("\n", MetadataCategory.Text);
            this.Accept(para.Content);
        }
    }

    public void Visit(UsxChapterMarker chapterMarker)
    {
        if (int.TryParse(chapterMarker.Number, out var chapterNumber))
        {
            _bibleReference.Chapter = chapterNumber;
            _bibleReference.Verse = null;
            _bibleBook.HandleChapterChange((byte)chapterNumber);
            _bibleBook.AddScriptureSegment(chapterMarker.StartId, MetadataCategory.Marker);
        }
    }

    public void Visit(UsxVerseMarker verseMarker)
    {
        if (int.TryParse(verseMarker.Number, out var verseNumber))
        {
            _bibleReference.Verse = verseMarker.Number;
            _bibleBook.HandleVerseChange((byte)verseNumber);
            _bibleBook.AddScriptureSegment(verseMarker.Number, MetadataCategory.Marker);
        }
    }

    public void Visit(UsxChar usxChar)
    {
        _bibleBook.AddScriptureSegment(usxChar.Style, MetadataCategory.Style);
        this.Accept(usxChar.Content);
    }

    public void Visit(string text)
    {
        _bibleBook.AddScriptureSegment(text);
    }

    public void Visit(UsxMilestone milestone)
    {
        _bibleBook.AddScriptureSegment(milestone.Style, MetadataCategory.Style);
    }

    public void Visit(UsxLineBreak lineBreak)
    {
        _bibleBook.AddScriptureSegment("\n", MetadataCategory.Text);
    }

    public void Visit(UsxCrossReference reference)
    {
        if (_options.EnableCrossReferences)
        {
            _bibleBook.AddScriptureSegment(reference.Location, MetadataCategory.Reference);
            this.Accept(reference.Content);
        }
    }

    public void Visit(UsxFootnote footnote)
    {
        _bibleBook.AddScriptureSegment(footnote.Style, MetadataCategory.Style);
        if (_options.EnableFootnotes)
        {
            _footnotes.Add(footnote);
            _bibleBook.AddScriptureSegment(footnote.Caller, MetadataCategory.Footnote);
            this.Accept(footnote.Content);
        }
    }

    public ScriptureBook GetBibleBook()
    {
        _bibleBook.Seal();
        return _bibleBook;
    }
}