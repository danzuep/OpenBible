using System.Text.Json;

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

        public async Task<Stream> SerializeAsync()
        {
            var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, _segments).ConfigureAwait(false);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        public static async Task<ScriptureSegmentStorage> DeserializeAsync(Stream stream, JsonSerializerOptions options, CancellationToken cancellationToken = default)
        {
            stream.Seek(0, SeekOrigin.Begin);
            var segments = await JsonSerializer.DeserializeAsync<ScriptureSegmentStorage>(stream, options, cancellationToken).ConfigureAwait(false);
            return new ScriptureSegmentStorage(segments?._segments ?? Array.Empty<ScriptureSegment>());
        }
    }
}
