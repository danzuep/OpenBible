namespace Bible.Core.Models.Scripture
{
    public class ScriptureSegmentStorage
    {
        private readonly ScriptureSegment[] _segments;

        public ScriptureSegmentStorage(IEnumerable<ScriptureSegment> segments)
        {
            _segments = segments?.ToArray() ?? throw new ArgumentNullException(nameof(segments));
        }

        public ReadOnlySpan<ScriptureSegment> AsSpan() => _segments.AsSpan();

        public ReadOnlySpan<ScriptureSegment> Slice(int start, int length)
            => _segments.AsSpan(start, length);

        public int Length => _segments.Length;
    }
}
