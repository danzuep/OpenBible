namespace Bible.Core.Models
{
    /// <summary>
    /// Book-level metadata.
    /// </summary>
    public class BibleBookMetadata : IEquatable<BibleBookMetadata>
    {
        /// <summary>
        /// The 3-letter code of the book, e.g. "GEN".
        /// </summary>
        public string BookCode { get; set; } = string.Empty;

        /// <summary>
        /// The version of scripture, e.g. "ESV", "NIV".
        /// </summary>
        public string BibleVersion { get; set; } = string.Empty;

        /// <summary>
        /// ISO 639-3 three-letter language code.
        /// </summary>
        public string IsoLanguage { get; set; } = string.Empty;

        public BibleBookMetadata Copy()
        {
            var copy = (BibleBookMetadata)MemberwiseClone();
            return copy;
        }

        public bool Equals(BibleBookMetadata other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return BookCode == other.BookCode && BibleVersion == other.BibleVersion && IsoLanguage == other.IsoLanguage;
        }
    }
}