namespace Bible.Core.Models.Scripture
{
    /// <summary>
    /// Represents a book of the Bible in Unified Scripture XML format,
    /// optimized for memory-efficient storage and fast slicing of scripture segments.
    /// </summary>
    /// <remarks>
    /// ScriptureBook (Facade)
    ///  ├─ ScriptureSegmentStorage (Immutable segment array)
    ///  ├─ ScriptureIndexManager (Manages ranges and mappings)
    ///  ├─ ScriptureMetadata (Book-level metadata)
    ///  └─ ScriptureSegmentBuilder (Builds segments, manages state before sealing)
    /// </remarks>
    public sealed class ScriptureBook
    {
        public ScriptureIndexManager IndexManager { get; set; }

        public ScriptureBookMetadata Metadata { get; set; }

        public ReadOnlySpan<ScriptureSegment> GetFromReference(string reference)
        {
            if (string.IsNullOrEmpty(reference))
                throw new ArgumentNullException(nameof(reference));
            var chVs = reference.Split(' ').LastOrDefault() ??
                throw new ArgumentException("Reference must contain chapter and verse", nameof(reference));
            var parts = chVs.Split(':');
            if (parts.Length < 2 || !byte.TryParse(parts[0], out var chapter) || !byte.TryParse(parts[1], out var verse))
                throw new ArgumentException("Invalid reference format. Expected 'Book Chapter:Verse'", nameof(reference));
            if (chapter > 0 && verse > 0)
                return IndexManager.GetVerse(chapter, verse);
            if (chapter > 0)
                return IndexManager.GetChapter(chapter);
            throw new ArgumentException("Reference must specify a valid chapter and verse", nameof(reference));
        }

        public ReadOnlySpan<ScriptureSegment> GetChapter(byte chapter)
            => IndexManager.GetChapter(chapter);

        public ReadOnlySpan<ScriptureSegment> GetVerse(byte chapter, byte verse)
            => IndexManager.GetVerse(chapter, verse);

        public ReadOnlySpan<ScriptureSegment> GetParagraph(ushort paragraph)
            => IndexManager.GetParagraph(paragraph);

        public IReadOnlyDictionary<byte, Range> GetAllChapterRanges()
            => IndexManager.ChapterRanges;

        public IReadOnlyDictionary<(byte chapter, byte verse), Range> GetAllVerseRanges()
            => IndexManager.VerseRanges;

        public IReadOnlyDictionary<byte, Range> GetChapterVerseRanges(byte chapter)
            => IndexManager.VerseRanges.Where(r => r.Key.chapter == chapter)
                .ToDictionary(v => v.Key.verse, v => v.Value);

        public IReadOnlyDictionary<ushort, Range> GetAllParagraphRanges()
            => IndexManager.ParagraphRanges;

        public string ToMarkdown()
            => IndexManager.GetBook().ToMarkdown(Metadata);

        public string ToMarkdownChapter(byte chapter)
            => IndexManager.GetChapter(chapter).ToMarkdown(Metadata);
    }
}