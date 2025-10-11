using System.Text.Json;

namespace Bible.Core.Models.Scripture
{
    public class ScriptureIndexManager
    {
        private readonly Dictionary<byte, Range> _chapterRanges = new();
        private readonly Dictionary<(byte chapter, byte verse), Range> _verseRanges = new();
        private readonly Dictionary<ushort, Range> _paragraphRanges = new();

        private readonly ScriptureSegmentStorage _segmentStorage;

        public ScriptureIndexManager(ScriptureSegmentStorage segmentStorage)
        {
            _segmentStorage = segmentStorage ?? throw new ArgumentNullException(nameof(segmentStorage));
        }

        // Methods to add ranges during building
        public void SetChapterRange(byte chapter, int start, int end)
            => _chapterRanges[chapter] = start..end;

        public void SetVerseRange(byte chapter, byte verse, int start, int end)
            => _verseRanges[(chapter, verse)] = start..end;

        public void SetParagraphRange(ushort paragraph, int start, int end)
            => _paragraphRanges[paragraph] = start..end;

        // Read-only accessors for ranges
        public IReadOnlyDictionary<byte, Range> ChapterRanges => _chapterRanges;
        public IReadOnlyDictionary<(byte chapter, byte verse), Range> VerseRanges => _verseRanges;
        public IReadOnlyDictionary<ushort, Range> ParagraphRanges => _paragraphRanges;

        // Span accessors
        public ReadOnlySpan<ScriptureSegment> GetBook()
            => _segmentStorage.AsSpan();

        public ReadOnlySpan<ScriptureSegment> GetChapter(byte chapter)
            => _chapterRanges.TryGetValue(chapter, out var r) ? _segmentStorage.Slice(r.Start.Value, r.End.Value - r.Start.Value) : ReadOnlySpan<ScriptureSegment>.Empty;

        public ReadOnlySpan<ScriptureSegment> GetVerse(byte chapter, byte verse)
            => _verseRanges.TryGetValue((chapter, verse), out var r) ? _segmentStorage.Slice(r.Start.Value, r.End.Value - r.Start.Value) : ReadOnlySpan<ScriptureSegment>.Empty;

        public ReadOnlySpan<ScriptureSegment> GetParagraph(ushort paragraph)
            => _paragraphRanges.TryGetValue(paragraph, out var r) ? _segmentStorage.Slice(r.Start.Value, r.End.Value - r.Start.Value) : ReadOnlySpan<ScriptureSegment>.Empty;

        public async Task<Stream> SerializeAsync()
        {
            return await _segmentStorage.SerializeAsync().ConfigureAwait(false);
        }
    }
}