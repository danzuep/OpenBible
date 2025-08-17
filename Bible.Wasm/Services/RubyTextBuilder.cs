using Microsoft.AspNetCore.Components;
using Bible.Core.Models;
using Bible.Core.Models.Scripture;

namespace Bible.Wasm.Services
{
    public class RubyTextBuilder
    {
        private readonly BibleParagraphList _paragraphs = new();
        private readonly BibleFootnoteList _footnotes = new();
        private readonly ScriptureBookMetadata _bookMetadata = new();
        private BibleVerseList? _currentParagraph;
        private BibleWordList? _currentVerse;

        public RubyTextBuilder SetBookCode(string bookCode)
        {
            if (!string.IsNullOrEmpty(bookCode))
                _bookMetadata.Id = bookCode;
            return this;
        }

        public RubyTextBuilder SetBookName(string bookName)
        {
            if (!string.IsNullOrEmpty(bookName))
                _bookMetadata.Name = bookName;
            return this;
        }

        public RubyTextBuilder SetVersionName(string version)
        {
            if (!string.IsNullOrEmpty(version))
                _bookMetadata.Version = version.Trim('-').Trim();
            return this;
        }

        public RubyTextBuilder SetLanguage(string language)
        {
            if (!string.IsNullOrEmpty(language))
                _bookMetadata.IsoLanguage = language;
            return this;
        }

        public RubyTextBuilder HandleParagraphChange()
        {
            EndParagraph();
            StartParagraph();
            return this;
        }

        public RubyTextBuilder HandleChapterChange(string chapterNumber, string? startId)
        {
            EndVerse();
            StartVerse(null, chapterNumber);
            return this;
        }

        public RubyTextBuilder HandleVerseChange(string verseNumber, string? startId)
        {
            EndVerse();
            StartVerse(verseNumber);
            return this;
        }

        /// <summary>
        /// Add a word to the current verse.
        /// </summary>
        /// <param name="text">The content (render fragment) of the word</param>
        /// <param name="pronunciation">Optional ruby pronunciation text</param>
        /// <param name="footnoteId">Optional footnote ID</param>
        public RubyTextBuilder AddWord(string? text, string? pronunciation = null, string? footnoteId = null)
        {
            if (_currentVerse == null)
                StartVerse(null);

            var word = new BibleWord
            {
                Text = text,
                Pronunciation = pronunciation,
                FootnoteId = footnoteId
            };
            _currentVerse!.Add(word);

            return this;
        }

        /// <summary>
        /// Adds a footnote with the given ID and content.
        /// </summary>
        /// <param name="id">1-based footnote ID by convention</param>
        /// <param name="content">RenderFragment content of the footnote</param>
        public RubyTextBuilder AddFootnote(string id, string content)
        {
            _footnotes.Add(new BibleFootnote
            (
                id,
                content
            ));
            return this;
        }

        /// <summary>
        /// Finalizes the builder and returns the list of paragraphs.
        /// Also ends any open verses or paragraphs.
        /// </summary>
        public BibleParagraphList Build()
        {
            EndVerse();
            EndParagraph();
            return _paragraphs;
        }

        /// <summary>
        /// Gets the list of footnotes added to the builder.
        /// </summary>
        public IReadOnlyList<BibleFootnote> Footnotes => _footnotes.AsReadOnly();

        /// <summary>
        /// Start a new paragraph. Ends the previous paragraph if any.
        /// </summary>
        private RubyTextBuilder StartParagraph()
        {
            EndVerse();
            EndParagraph();
            _currentParagraph = new BibleVerseList();
            return this;
        }

        /// <summary>
        /// Ends the current paragraph and adds it to the list.
        /// </summary>
        private RubyTextBuilder EndParagraph()
        {
            if (_currentParagraph != null)
            {
                EndVerse();
                _paragraphs.Add(_currentParagraph);
                _currentParagraph = null;
            }
            return this;
        }

        /// <summary>
        /// Start a new verse inside the current paragraph.
        /// </summary>
        private RubyTextBuilder StartVerse(string? verseNumber, string? chapterNumber = null)
        {
            if (_currentParagraph == null)
                StartParagraph();

            EndVerse();
            _currentVerse = new BibleWordList();
            if (!string.IsNullOrEmpty(verseNumber))
            {
                _currentVerse.Number = verseNumber;
            }
            if (!string.IsNullOrEmpty(chapterNumber))
            {
                _currentVerse.ChapterNumber = chapterNumber;
            }
            return this;
        }

        /// <summary>
        /// Ends the current verse and adds it to the current paragraph.
        /// </summary>
        private RubyTextBuilder EndVerse()
        {
            if (_currentVerse != null && _currentParagraph != null)
            {
                _currentParagraph.Add(_currentVerse);
                _currentVerse = null;
            }
            return this;
        }
    }
}