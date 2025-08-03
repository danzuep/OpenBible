namespace Bible.Core.Models.Scripture
{
    public class ScriptureRange
    {
        public ScriptureBookMetadata BookMetadata { get; set; }
        public ScriptureRecord[] ScriptureRecords { get; set; }
    }
}
