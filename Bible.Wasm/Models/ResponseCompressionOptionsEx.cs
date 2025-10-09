namespace Bible.Wasm.Models
{
    public sealed class ResponseCompressionOptionsEx
    {
        public bool EnableBrotli { get; set; } = true;
        public bool EnableGzip { get; set; } = true;
        public string BrotliLevel { get; set; } = "Optimal"; // "Optimal" or "Fastest" or "NoCompression"
        public string GzipLevel { get; set; } = "Fastest";
        public string[] MimeTypes { get; set; } = Array.Empty<string>();
    }
}