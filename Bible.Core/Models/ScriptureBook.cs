using System;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Bible.Core.Models
{
    /// <summary>
    /// Categories for metadata used in scripture segments.
    /// </summary>
    public enum MetadataCategory
    {
        Meta,
        Text,
        Style,
        Marker,
        Footnote,
        Reference,
        Pronunciation
    }

    /// <summary>
    /// Immutable readonly struct representing a single scripture segment.
    /// Chapter and Verse are no longer stored in this struct.
    /// </summary>
    public readonly struct ScriptureSegment
    {
        /// <summary>Text content of this segment.</summary>
        public string Text { get; }
        /// <summary>Metadata category assigned to this segment.</summary>
        public MetadataCategory Category { get; }

        /// <summary>
        /// Creates a new ScriptureSegment instance.
        /// </summary>
        /// <param name="text">Text content (non-null).</param>
        /// <param name="category">Metadata category (default: Text).</param>
        /// <exception cref="ArgumentNullException">If text is null.</exception>
        public ScriptureSegment(string text, MetadataCategory category = MetadataCategory.Text)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
            Category = category;
        }

        /// <summary>
        /// Returns a string representation of this scripture segment.
        /// Omits the Category if it is the default MetadataCategory.Text.
        /// </summary>
        public override string ToString()
        {
            if (Category == MetadataCategory.Text)
                return $"\"{Text}\"";
            return $"\"{Text}\" ({Category})";
        }
    }

    /// <summary>
    /// Represents a book of the Bible in Unified Scripture XML format,
    /// optimized for memory-efficient storage and fast slicing of scripture segments.
    /// </summary>
    public sealed class ScriptureBook
    {
        /// <summary>
        /// The 3-letter code of the book, e.g. "GEN".
        /// </summary>
        public string Code
        {
            get
            {
                //var span = GetBookMetadata(BookMetadataCodeIndex);
                var span = GetVerse(0, 0);
                return span.ToText();
            }
            set
            {
                AddBookMetadata(value);
            }
        }

        /// <summary>
        /// The name of the book, e.g. "Genesis".
        /// </summary>
        public string Name
        {
            get
            {
                //var span = GetBookMetadata(BookMetadataNameIndex);
                var span = GetVerse(0, 0);
                return span.ToText();
            }
            set
            {
                AddBookMetadata(value);
            }
        }

        private const byte BookMetadataIndex = 0;

        private const byte BookMetadataCodeIndex = 0;

        private const byte BookMetadataNameIndex = 1;

        // Internal mutable storage while building
        private List<ScriptureSegment> _verseSegmentsList = new List<ScriptureSegment>();

        // Final sealed array for efficient slicing
        private ScriptureSegment[] _verseSegments;

        /// <summary>
        /// Backing field for _verseSegments exposed as property.
        /// </summary>
        private ScriptureSegment[] _verseSegmentsArray => _verseSegments ?? throw new InvalidOperationException("Book must be sealed before accessing scripture segments.");

        /// <summary>
        /// Dictionary mapping chapter number to Range of scripture segment indices.
        /// Chapter 0 is reserved for book-level metadata.
        /// </summary>
        private readonly Dictionary<byte, Range> _chapterIndexRanges = new();

        /// <summary>
        /// Dictionary mapping (chapter, verse) to Range of scripture segment indices.
        /// </summary>
        private readonly Dictionary<(byte chapter, byte verse), Range> _verseIndexRanges = new();

        /// <summary>
        /// Dictionary mapping paragraph number to Range of scripture segment indices.
        /// </summary>
        private readonly Dictionary<ushort, Range> _paragraphIndexRanges = new();

        /// <summary>
        /// Dictionary mapping section name to Range of scripture segment indices.
        /// </summary>
        private readonly Dictionary<string, Range> _sectionIndexRanges = new(StringComparer.Ordinal);

        // Track current paragraph and section to build ranges
        private ushort _currentParagraph = 0;
        private string _currentSection = null;

        // Track start indices for chapter, verse, paragraph, section ranges.
        private int _currentChapterStartIndex = 0;
        private byte _currentChapter = 0;

        private int _currentVerseStartIndex = 0;
        private byte _currentVerse = 0;

        private int _currentParagraphStartIndex = 0;
        private ushort _lastParagraphNumber = 0;

        private int _currentSectionStartIndex = 0;

        // Flag to prevent adding after Seal()
        private bool _sealed = false;

        /// <summary>
        /// Book-level metadata stored as chapter BookMetadata segments.
        /// </summary>
        /// <inheritdoc cref="AddScriptureSegment(string, MetadataCategory)"/>
        public void AddBookMetadata(string text, MetadataCategory category = MetadataCategory.Meta)
        {
            // chapter BookMetadata is reserved for book-level metadata
            var segment = new ScriptureSegment(text, category);
            _verseSegmentsList!.Add(segment);
        }

        /// <summary>
        /// Adds a scripture segment to the book.
        /// Handles paragraph and section ranges based on style metadata and text content..
        /// </summary>
        /// <param name="text">Text content.</param>
        /// <param name="category">Metadata category.</param>
        /// <exception cref="InvalidOperationException">If called after Seal().</exception>
        /// <exception cref="ArgumentNullException">If text is null.</exception>
        public void AddScriptureSegment(string text, MetadataCategory category = MetadataCategory.Text)
        {
            if (_sealed)
                ThrowSealed();

            if (text is null)
                throw new ArgumentNullException(nameof(text));

            if (category == MetadataCategory.Style && text == "p") // paragraph start
            {
                // Close previous paragraph range if any
                if (_currentParagraph != 0)
                {
                    SetRange(_paragraphIndexRanges, _currentParagraph, _currentParagraphStartIndex, _verseSegmentsList!.Count);
                }
                _currentParagraph = (ushort)(_lastParagraphNumber + 1);
                _lastParagraphNumber = _currentParagraph;
                _currentParagraphStartIndex = _verseSegmentsList!.Count;
            }
            else if (category == MetadataCategory.Style && text == "s") // section start
            {
                // Close previous section range if any
                if (_currentSection != null)
                {
                    SetRange(_sectionIndexRanges, _currentSection, _currentSectionStartIndex, _verseSegmentsList!.Count);
                }
                // Assign new section name
                _currentSection = $"Section{_sectionIndexRanges.Count + 1}";
                _currentSectionStartIndex = _verseSegmentsList!.Count;
            }

            var segment = new ScriptureSegment(text, category);
            _verseSegmentsList!.Add(segment);
        }

        /// <summary>
        /// Handles closing and starting ranges for chapter changes.
        /// </summary>
        /// <param name="chapter">New chapter number.</param>
        public void HandleChapterChange(byte chapter)
        {
            if (_sealed)
                ThrowSealed();

            if (chapter == _currentChapter)
                return;

            if (_currentChapter != 0 || _verseSegmentsList!.Count > 0)
            {
                SetRange(_chapterIndexRanges, _currentChapter, _currentChapterStartIndex, _verseSegmentsList!.Count);
            }
            _currentChapter = chapter;
            _currentChapterStartIndex = _verseSegmentsList!.Count;

            // Reset verse tracking for new chapter
            _currentVerse = 0;
            _currentVerseStartIndex = _verseSegmentsList!.Count;
        }

        /// <summary>
        /// Handles closing and starting ranges for verse changes.
        /// </summary>
        /// <param name="verse">New verse number.</param>
        public void HandleVerseChange(byte verse)
        {
            if (_sealed)
                ThrowSealed();

            if (verse == _currentVerse)
                return;

            if (_currentVerse != 0 || _verseSegmentsList!.Count > 0)
            {
                SetRange(_verseIndexRanges, (_currentChapter, _currentVerse), _currentVerseStartIndex, _verseSegmentsList!.Count);
            }
            _currentVerse = verse;
            _currentVerseStartIndex = _verseSegmentsList!.Count;
        }

        /// <summary>
        /// Seals the book from further modifications and prepares internal data for efficient slicing.
        /// </summary>
        /// <exception cref="InvalidOperationException">If called multiple times.</exception>
        public void Seal()
        {
            if (_sealed) return;

            _sealed = true;

            // Close last open ranges for chapter, verse, paragraph, section
            if (_currentVerse != 0)
                SetRange(_verseIndexRanges, (_currentChapter, _currentVerse), _currentVerseStartIndex, _verseSegmentsList!.Count);
            if (_currentChapter != 0)
                SetRange(_chapterIndexRanges, _currentChapter, _currentChapterStartIndex, _verseSegmentsList!.Count);
            if (_currentParagraph != 0)
                SetRange(_paragraphIndexRanges, _currentParagraph, _currentParagraphStartIndex, _verseSegmentsList!.Count);
            if (_currentSection != null)
                SetRange(_sectionIndexRanges, _currentSection, _currentSectionStartIndex, _verseSegmentsList!.Count);

            // Convert list to array for efficient ReadOnlySpan slicing
            _verseSegments = _verseSegmentsList!.ToArray();

            // Clear the list to free memory
            _verseSegmentsList = null;
        }

        /// <summary>
        /// Gets book metadata segments as a ReadOnlySpan.
        /// </summary>
        /// <param name="index">Optional index number.</param>
        /// <returns>ReadOnlySpan of book metadata, or empty if not found.</returns>
        public ReadOnlySpan<ScriptureSegment> GetBookMetadata(byte? index = null)
        {
            if (!_sealed)
                return _verseSegmentsList.Take(1).ToArray() ?? ReadOnlySpan<ScriptureSegment>.Empty;
            if (index.HasValue && _verseIndexRanges.TryGetValue((BookMetadataIndex, index.Value), out var verseRange))
                return _verseSegmentsArray.AsSpan(verseRange.Start.Value, verseRange.End.Value - verseRange.Start.Value);
            if (_chapterIndexRanges.TryGetValue(BookMetadataIndex, out var chapterRange))
                return _verseSegmentsArray.AsSpan(chapterRange.Start.Value, chapterRange.End.Value - chapterRange.Start.Value);
            return ReadOnlySpan<ScriptureSegment>.Empty;
        }

        /// <summary>
        /// Gets the scripture segments for a given chapter as a ReadOnlySpan.
        /// </summary>
        /// <param name="chapter">Chapter number.</param>
        /// <returns>ReadOnlySpan of ScriptureSegments for the chapter, or empty if not found.</returns>
        public ReadOnlySpan<ScriptureSegment> GetChapter(byte chapter)
        {
            if (!_sealed)
                ThrowNotSealed();

            if (_chapterIndexRanges.TryGetValue(chapter, out var range))
                return _verseSegmentsArray.AsSpan(range.Start.Value, range.End.Value - range.Start.Value);
            return ReadOnlySpan<ScriptureSegment>.Empty;
        }

        /// <summary>
        /// Gets the scripture segments for a given chapter and verse as a ReadOnlySpan.
        /// </summary>
        /// <param name="chapter">Chapter number.</param>
        /// <param name="verse">Verse number.</param>
        /// <returns>ReadOnlySpan of ScriptureSegments for the verse, or empty if not found.</returns>
        public ReadOnlySpan<ScriptureSegment> GetVerse(byte chapter, byte verse)
        {
            if (!_sealed)
                ThrowNotSealed();

            if (_verseIndexRanges.TryGetValue((chapter, verse), out var range))
                return _verseSegmentsArray.AsSpan(range.Start.Value, range.End.Value - range.Start.Value);
            return ReadOnlySpan<ScriptureSegment>.Empty;
        }

        /// <summary>
        /// Gets the scripture segments for a given paragraph number as a ReadOnlySpan.
        /// </summary>
        /// <param name="paragraphNumber">Paragraph number.</param>
        /// <returns>ReadOnlySpan of ScriptureSegments for the paragraph, or empty if not found.</returns>
        public ReadOnlySpan<ScriptureSegment> GetParagraph(ushort paragraphNumber)
        {
            if (!_sealed)
                ThrowNotSealed();

            if (_paragraphIndexRanges.TryGetValue(paragraphNumber, out var range))
                return _verseSegmentsArray.AsSpan(range.Start.Value, range.End.Value - range.Start.Value);
            return ReadOnlySpan<ScriptureSegment>.Empty;
        }

        /// <summary>
        /// Gets the scripture segments for a given section name as a ReadOnlySpan.
        /// </summary>
        /// <param name="sectionName">Section name.</param>
        /// <returns>ReadOnlySpan of ScriptureSegments for the section, or empty if not found.</returns>
        public ReadOnlySpan<ScriptureSegment> GetSection(string sectionName)
        {
            if (!_sealed)
                ThrowNotSealed();

            if (sectionName is null)
                throw new ArgumentNullException(nameof(sectionName));

            if (_sectionIndexRanges.TryGetValue(sectionName, out var range))
                return _verseSegmentsArray.AsSpan(range.Start.Value, range.End.Value - range.Start.Value);
            return ReadOnlySpan<ScriptureSegment>.Empty;
        }

        /// <summary>
        /// Returns a string representation of the ScriptureBook,
        /// including its Name and Code.
        /// </summary>
        public override string ToString()
            => $"Name=\"{Name}\"";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetRange<T>(Dictionary<T, Range> dict, T key, int start, int end) where T : notnull
        {
            // Defensive: end must be >= start
            if (end < start) throw new ArgumentOutOfRangeException(nameof(end), "Range end must be >= start");
            dict[key] = start..end;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ThrowNotSealed()
            => throw new InvalidOperationException("Book must be sealed before accessing data.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ThrowSealed()
            => throw new InvalidOperationException("Cannot add scripture segments after the book has been sealed.");
    }

    public static class Sample
    {
        /// <summary>
        /// Sample usage demonstrating creation, adding scripture segments, sealing, and retrieving data.
        /// </summary>
        /// <param name="logger">Optional logger for output. If null, creates a console logger.</param>
        public static void UnifiedScripture(ILogger logger = null)
        {
            using var loggerFactory = logger == null
                ? LoggerFactory.Create(builder => builder.AddConsole())
                : null;
            logger ??= loggerFactory.CreateLogger("SampleUnifiedScripture");

            var book = new ScriptureBook
            {
                Name = "SampleBook"
            };

            // Add book-level metadata (chapter BookMetadata, verse 0)
            book.AddBookMetadata("Book metadata: Author info");

            book.HandleChapterChange(1);
            book.AddScriptureSegment("# ", MetadataCategory.Marker);

            // Add some paragraphs and sections using style metadata
            // Paragraph 1 start
            book.AddScriptureSegment("p", MetadataCategory.Style);

            // Section 1 start
            book.AddScriptureSegment("s", MetadataCategory.Style);

            // Add scripture segments for chapter 1, verse 1
            book.AddScriptureSegment("In the beginning God created the heavens and the earth.");
            book.AddScriptureSegment("This is the first verse.", MetadataCategory.Text);

            book.HandleVerseChange(2);
            book.AddScriptureSegment("# ", MetadataCategory.Marker);

            // Add scripture segment for chapter 1, verse 2
            book.AddScriptureSegment("Now the earth was formless and empty.", MetadataCategory.Text);

            // Paragraph 2 start
            book.AddScriptureSegment("p", MetadataCategory.Style);

            // Section 2 start (new section)
            book.AddScriptureSegment("s", MetadataCategory.Style);

            book.HandleVerseChange(3);
            book.AddScriptureSegment("# ", MetadataCategory.Marker);

            // Add scripture segment for chapter 1, verse 3
            book.AddScriptureSegment("And God said, 'Let there be light,' and there was light.");

            // Seal the book to finalize internal structures
            book.Seal();

            // Log the ScriptureBook ToString output
            logger.LogInformation(book.ToString());

            // Retrieve and print all scripture segments for chapter 1
            var chapter1 = book.GetChapter(1);
            logger.LogInformation("Chapter 1 segments:");
            foreach (var seg in chapter1)
            {
                logger.LogInformation(seg.ToString());
            }

            // Retrieve and print all scripture segments for chapter 1, verse 1
            var verse1_1 = book.GetVerse(1, 1);
            logger.LogInformation("Chapter 1, Verse 1 segments:");
            foreach (var seg in verse1_1)
            {
                logger.LogInformation(seg.ToString());
            }

            // Retrieve and print paragraph 1
            var paragraph1 = book.GetParagraph(1);
            logger.LogInformation("Paragraph 1 segments:");
            foreach (var seg in paragraph1)
            {
                logger.LogInformation(seg.ToString());
            }

            // Retrieve and print section 1
            var section1 = book.GetSection("Section1");
            logger.LogInformation("Section 1 segments:");
            foreach (var seg in section1)
            {
                logger.LogInformation(seg.ToString());
            }
        }
    }

    public static class ScriptureSegmentExtensions
    {
        private static readonly string[] _markup = ["w", "p"];

        public static string ToText(this ReadOnlySpan<ScriptureSegment> segments)
        {
            var sb = new StringBuilder();
            foreach (var segment in segments)
            {
                if (segment.Category == MetadataCategory.Marker)
                    sb.AppendFormat("**{0}** ", segment.Text);
                else if (segment.Category == MetadataCategory.Text)
                    sb.Append(segment.Text);
                else if (segment.Category != MetadataCategory.Style)
                    sb.AppendFormat("({0})", segment.Text);
                else if (!_markup.Contains(segment.Text))
                    sb.AppendFormat("<{0} />", segment.Text);
            }
            return sb.ToString();
        }
    }
}