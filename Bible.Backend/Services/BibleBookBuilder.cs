using System.Text;
using Bible.Core.Models;
using Bible.Core.Models.Scripture;
using Unihan.Models;

namespace Bible.Backend.Services
{
    public class BibleBookBuilder
    {
        private BibleVerse _currentVerse = new();
        private BibleChapter _currentChapter = new();
        private readonly BibleBook _bibleBook = new();
        private readonly BibleReference _bibleReference = new();
        private readonly List<BibleChapter> _chapters = new();
        private readonly List<BibleVerse> _verses = new();
        private readonly List<ScriptureSegment> _segments = new();

        public BibleBookBuilder()
        {
            _bibleBook.Reference = _bibleReference;
        }

        public UnihanLookup? Unihan { get; set; }

        public BibleBookBuilder SetBookCode(string bookCode)
        {
            if (!string.IsNullOrEmpty(bookCode))
                _bibleReference.BookCode = bookCode;
            return this;
        }

        public BibleBookBuilder SetBookName(string bookName)
        {
            if (!string.IsNullOrEmpty(bookName))
                _bibleReference.BookName = bookName;
            return this;
        }

        public BibleBookBuilder SetVersionName(string version)
        {
            if (!string.IsNullOrEmpty(version) && string.IsNullOrEmpty(_bibleReference.Version))
                _bibleReference.Version = version;
            else if (!string.IsNullOrEmpty(_bibleReference.Version))
                _bibleReference.VersionName = version.Trim('-').Trim();
            return this;
        }

        public BibleBookBuilder SetLanguage(string language)
        {
            if (!string.IsNullOrEmpty(language))
                _bibleReference.Language = language;
            return this;
        }

        public BibleBookBuilder AddScriptureSegment(string? text, MetadataCategory category = MetadataCategory.Text)
        {
            if (category == MetadataCategory.Style && text == "p")
            {
                _segments.Add(new ScriptureSegment("\n", MetadataCategory.Markup));
            }
            else
            {
                _segments.Add(new ScriptureSegment(text, category));
            }
            return this;
        }

        private static readonly IReadOnlyList<MetadataCategory> _metadataCategoriesToShow =
            [MetadataCategory.Text, MetadataCategory.Markup];

        private void AddCurrentVerse()
        {
            if (_currentVerse.Id < 1) return;
            var _stringBuilder = new StringBuilder();
            foreach (var segment in _segments)
            {
                if (_metadataCategoriesToShow.Contains(segment.Category))
                {
                    _stringBuilder.Append(segment.Text);
                }
                else if (segment.Category == MetadataCategory.Footnote)
                {
                    _stringBuilder.AppendFormat("{0}", segment.Text);
                }
            }
            _currentVerse.Text = _stringBuilder.ToString();
            //_currentVerse.Pronunciation = string.Concat(_segments
            //    .Where(s => s.Category == MetadataCategory.Pronunciation)
            //    .Select(s => s.Text));
            _verses.Add(_currentVerse);
            _segments.Clear();
        }

        private void AddCurrentChapter()
        {
            if (_currentChapter.Id < 1) return;
            _currentChapter.Verses = [.. _verses];
            _chapters.Add(_currentChapter);
            _verses.Clear();
        }

        public BibleBookBuilder HandleChapterChange(string chapterNumber, string? startId = null)
        {
            if (string.IsNullOrEmpty(chapterNumber)) return this;

            AddCurrentChapter();

            var reference = new BibleReference(_bibleReference);

            if (!string.IsNullOrEmpty(startId))
            {
                var chapterRef = startId.Split(' ').LastOrDefault();
                if (!string.IsNullOrEmpty(chapterRef))
                {
                    reference.Reference = chapterRef;
                }
                else
                {
                    reference.Reference = chapterNumber;
                }
            }
            else
            {
                reference.Reference = chapterNumber;
            }

            int.TryParse(chapterNumber, out var chapterId);
            reference.Chapter = chapterId;

            _currentChapter = new BibleChapter
            {
                Id = chapterId,
                Reference = reference
            };

            return this;
        }

        public BibleBookBuilder HandleVerseChange(string verseNumber, string? startId = null)
        {
            if (string.IsNullOrEmpty(verseNumber)) return this;

            AddCurrentVerse();

            var reference = new BibleReference(_bibleReference);

            if (!string.IsNullOrEmpty(startId))
            {
                var chapterRef = startId.Split(' ').LastOrDefault();
                if (!string.IsNullOrEmpty(chapterRef))
                {
                    reference.Reference = chapterRef;
                }
                else
                {
                    reference.Reference = verseNumber;
                }
            }
            else
            {
                reference.Reference = verseNumber;
            }

            int.TryParse(verseNumber, out var verseId);
            reference.Verse = verseNumber;

            _currentVerse = new BibleVerse
            {
                Id = verseId,
                Reference = reference
            };

            return this;
        }

        /// <summary>
        /// Finalize and get the built BibleBook object
        /// </summary>
        public BibleBook Build()
        {
            AddCurrentVerse();
            _currentVerse = null!;
            AddCurrentChapter();
            _currentChapter = null!;
            _bibleBook.Chapters = [.. _chapters];
            _chapters.Clear();

            return _bibleBook;
        }
    }
}