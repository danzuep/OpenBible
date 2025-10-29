using Bible.Core.Models.Scripture;

namespace Bible.Razor.Models
{
    public class ScriptureRange
    {
        public ScriptureBookMetadata? BookMetadata { get; set; }
        public IList<ScriptureSegmentDto>? ScriptureRecords { get; set; }
    }
}
