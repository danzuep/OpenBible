using Bible.Core.Models.Scripture;

namespace Bible.ServiceDefaults.Models
{
    public static class ScriptureSegmentDtoExtensions
    {
        public static ScriptureSegmentDto ToDto(this ScriptureSegment segment)
        {
            return new ScriptureSegmentDto
            {
                Text = segment.Text,
                Category = segment.Category
            };
        }

        public static ScriptureSegmentDto ToDto(this ScriptureBookMetadata metadata)
        {
            return new ScriptureSegmentDto
            {
                BookMetadata = metadata.GetSealedCopy(),
            };
        }

        public static ScriptureRange ToDto(this ReadOnlySpan<ScriptureSegment> segments, ScriptureBookMetadata metadata)
        {
            var records = new List<ScriptureSegmentDto>(segments.Length);
            foreach (var segment in segments)
            {
                records.Add(new ScriptureSegmentDto
                {
                    Text = segment.Text,
                    Category = segment.Category
                });
            }
            var scriptureRange = new ScriptureRange
            {
                BookMetadata = metadata.GetSealedCopy(),
                ScriptureRecords = records.ToArray()
            };
            return scriptureRange;
        }

        public static ScriptureRange ToDto(this ScriptureBook scriptureBook)
            => scriptureBook.IndexManager.GetBook().ToDto(scriptureBook.Metadata);

        public static ScriptureRange ToChapterDto(this ScriptureBook scriptureBook, byte chapter)
            => scriptureBook.IndexManager.GetChapter(chapter).ToDto(scriptureBook.Metadata);
    }
}
