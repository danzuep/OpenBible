using System.Runtime.CompilerServices;

namespace Bible.Core.Models.Scripture
{
    public class ScriptureSegmentBuilder
    {
        private readonly List<ScriptureSegment> _segments = new();

        private readonly Dictionary<byte, (int start, int end)> _chapterPositions = new();
        private readonly Dictionary<(byte chapter, byte verse), (int start, int end)> _versePositions = new();
        private readonly Dictionary<ushort, (int start, int end)> _paragraphPositions = new();

        private byte _currentChapter = 0;
        private byte _currentVerse = 0;
        private ushort _currentParagraph = 0;

        private int _currentChapterStart = 0;
        private int _currentVerseStart = 0;
        private int _currentParagraphStart = 0;

        private bool _sealed = false;

        public ScriptureBookMetadata BookMetadata { get; } = new();

        public void HandleChapterChange(byte chapter)
        {
            ThrowIfSealed();
            if (chapter == _currentChapter) return;

            CloseCurrentChapter();
            _currentChapter = chapter;
            _currentChapterStart = _segments.Count;

            // Reset verse state
            _currentVerse = 0;
            _currentVerseStart = _segments.Count;
        }

        public void HandleVerseChange(byte verse)
        {
            ThrowIfSealed();
            if (verse == _currentVerse) return;

            CloseCurrentVerse();
            _currentVerse = verse;
            _currentVerseStart = _segments.Count;
        }

        private void HandleParagraphChange()
        {
            CloseCurrentParagraph();
            _currentParagraph++;
            _currentParagraphStart = _segments.Count;
        }

        public void AddScriptureSegment(string text, MetadataCategory category = MetadataCategory.Text)
        {
            ThrowIfSealed();
            if (text is null) throw new ArgumentNullException(nameof(text));

            // Handle paragraph change
            if (category == MetadataCategory.Style && text == "p")
            {
                HandleParagraphChange();
            }

            _segments.Add(new ScriptureSegment(text, category));
        }

        /// <summary>
        /// Seals the book from further modifications and prepares internal data for efficient slicing.
        /// </summary>
        public ScriptureBook Build()
        {
            if (_sealed) throw new InvalidOperationException("Already sealed");

            CloseCurrentVerse();
            CloseCurrentChapter();
            CloseCurrentParagraph();

            _sealed = true;

            var storage = new ScriptureSegmentStorage(_segments.ToArray());
            var indexManager = new ScriptureIndexManager(storage);

            // Set ranges from builder positions
            foreach (var kvp in _chapterPositions)
                indexManager.SetChapterRange(kvp.Key, kvp.Value.start, kvp.Value.end);

            foreach (var kvp in _versePositions)
                indexManager.SetVerseRange(kvp.Key.chapter, kvp.Key.verse, kvp.Value.start, kvp.Value.end);

            foreach (var kvp in _paragraphPositions)
                indexManager.SetParagraphRange(kvp.Key, kvp.Value.start, kvp.Value.end);

            return new ScriptureBook { Metadata = BookMetadata.GetSealedCopy(), IndexManager = indexManager };
        }

        public ScriptureBook BuildChapter(byte chapter)
        {
            if (_sealed) throw new InvalidOperationException("Already sealed");

            CloseCurrentVerse();
            CloseCurrentChapter();
            CloseCurrentParagraph();

            _sealed = true;

            var storage = new ScriptureSegmentStorage(_segments.ToArray());
            var indexManager = new ScriptureIndexManager(storage);

            // Set ranges from builder positions
            foreach (var kvp in _chapterPositions)
                indexManager.SetChapterRange(kvp.Key, kvp.Value.start, kvp.Value.end);

            foreach (var kvp in _versePositions)
                indexManager.SetVerseRange(kvp.Key.chapter, kvp.Key.verse, kvp.Value.start, kvp.Value.end);

            foreach (var kvp in _paragraphPositions)
                indexManager.SetParagraphRange(kvp.Key, kvp.Value.start, kvp.Value.end);

            return new ScriptureBook { Metadata = BookMetadata.GetSealedCopy(), IndexManager = indexManager };
        }

        // Close helper methods
        private void CloseCurrentChapter()
        {
            if (_currentChapter != 0)
                _chapterPositions[_currentChapter] = (_currentChapterStart, _segments.Count);
        }

        private void CloseCurrentVerse()
        {
            if (_currentVerse != 0)
                _versePositions[(_currentChapter, _currentVerse)] = (_currentVerseStart, _segments.Count);
        }

        private void CloseCurrentParagraph()
        {
            if (_currentParagraph != 0)
                _paragraphPositions[_currentParagraph] = (_currentParagraphStart, _segments.Count);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ThrowIfSealed()
        {
            if (_sealed)
            {
                throw new InvalidOperationException("Cannot add after the book has been sealed.");
            }
        }
    }
}
