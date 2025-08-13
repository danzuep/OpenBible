using System.Data;
using System.Text;
using Bible.Backend.Models;
using Bible.Core.Models;
using Bible.Core.Models.Scripture;

namespace Bible.Backend.Services
{
    public class BibleBookBuilder
    {
        private readonly List<ScriptureSegment> _segments = new();

        private BibleBook _bibleBook;
        private BibleReference _bibleReference;
        private List<BibleChapter> _chapters;
        private List<BibleVerse> _verses;
        private BibleChapter _currentChapter = new();
        private BibleVerse _currentVerse = new();
        private StringBuilder _stringBuilder;

        public BibleBookBuilder()
        {
            _bibleBook = new BibleBook();
            _bibleReference = new BibleReference();
            _bibleBook.Reference = _bibleReference;
            _chapters = new List<BibleChapter>();
            _verses = new List<BibleVerse>();
            _stringBuilder = new StringBuilder();
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

        private void AddCurrentChapter()
        {
            if (_currentChapter.Id < 1) return;
            _currentChapter.Verses = [.. _verses];
            _chapters.Add(_currentChapter);
            _verses.Clear();
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

        private static readonly IReadOnlyList<MetadataCategory> _metadataCategoriesToShow =
            [MetadataCategory.Text, MetadataCategory.Markup];

        private void AddCurrentVerse()
        {
            if (_currentVerse.Id < 1) return;
            var sb = new StringBuilder();
            foreach (var segment in _segments)
            {
                if (_metadataCategoriesToShow.Contains(segment.Category))
                {
                    sb.Append(segment.Text);
                }
                else if (segment.Category == MetadataCategory.Footnote)
                {
                    sb.AppendFormat("{0}", segment.Text);
                }
            }
            _currentVerse.Text = sb.ToString();
            //_currentVerse.Pronunciation = string.Concat(_segments
            //    .Where(s => s.Category == MetadataCategory.Pronunciation)
            //    .Select(s => s.Text));
            _verses.Add(_currentVerse);
            _segments.Clear();
        }

        /// <summary>
        /// Adds verses from a UsxPara paragraph with style "p"
        /// </summary>
        /// <param name="paragraph"></param>
        /// <returns></returns>
        public BibleBookBuilder AddVersesFromParagraph(UsxPara paragraph)
        {
            if (paragraph?.Content == null)
                return this;

            foreach (var verse in ToBibleVerses(paragraph, _bibleReference, _stringBuilder))
            {
                _verses.Add(verse);
            }

            return this;
        }

        /// <summary>
        /// Finalize and get the built BibleBook object
        /// </summary>
        /// <returns></returns>
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

        #region Private Helper Methods

        private IEnumerable<BibleVerse> ToBibleVerses(UsxContent paragraph, BibleReference bibleReference, StringBuilder stringBuilder)
        {
            if (paragraph == null || paragraph.Content == null)
            {
                yield break;
            }

            var bibleVerse = new BibleVerse();

            foreach (var item in paragraph.Content)
            {
                switch (item)
                {
                    case UsxHeading textValue:
                        stringBuilder.Append(textValue.Text);
                        break;

                    case UsxMarker verseMarker when verseMarker.Style == "v":
                        if (!string.IsNullOrEmpty(verseMarker.Number))
                        {
                            if (int.TryParse(verseMarker.Number, out var verseNumber))
                            {
                                bibleVerse.Id = verseNumber;
                            }

                            if (!string.IsNullOrEmpty(verseMarker.StartId))
                            {
                                var chapterVerse = verseMarker.StartId.Split(' ').LastOrDefault();
                                if (!string.IsNullOrEmpty(chapterVerse))
                                {
                                    bibleVerse.Reference = new BibleReference(bibleReference) { Reference = chapterVerse };
                                }
                                else
                                {
                                    bibleVerse.Reference = new BibleReference(bibleReference) { Reference = verseMarker.Number };
                                }
                            }
                            else
                            {
                                bibleVerse.Reference = new BibleReference(bibleReference) { Reference = verseMarker.Number };
                            }
                        }

                        bibleVerse.Text = stringBuilder.ToString();

                        if (!string.IsNullOrEmpty(bibleVerse.Text))
                        {
                            yield return bibleVerse;

                            stringBuilder.Clear();
                            bibleVerse = new BibleVerse();
                        }
                        break;

                    case UsxContent nestedContent when nestedContent.Content != null:
                        foreach (var nestedVerse in ToBibleVerses(nestedContent, bibleReference, stringBuilder))
                        {
                            bibleVerse.Text += nestedVerse.Text;
                        }
                        break;

                    case string text:
                        bibleVerse.Text += text;
                        break;
                }
            }

            // Yield last verse if it has text
            if (!string.IsNullOrEmpty(bibleVerse.Text))
            {
                yield return bibleVerse;
            }
        }

        #endregion
    }
}