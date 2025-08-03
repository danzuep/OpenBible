namespace Bible.Core.Models.Scripture
{
    /// <summary>
    /// Book-level metadata.
    /// </summary>
    public class ScriptureBookMetadata : IEquatable<ScriptureBookMetadata>
    {
        /// <summary>
        /// The 3-letter code of the book, e.g. "GEN".
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// The name of the book, e.g. "Genesis".
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The version of scripture, e.g. "ESV", "NIV".
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// ISO 639-3 three-letter language code.
        /// </summary>
        public string IsoLanguage { get; set; } = string.Empty;

        public IList<ScriptureSegment> Segments { get; set; } = new List<ScriptureSegment>();

        public ScriptureBookMetadata GetSealedCopy()
        {
            var copy = (ScriptureBookMetadata)MemberwiseClone();
            copy.Segments = Segments.ToArray();
            return copy;
        }

        public bool Equals(ScriptureBookMetadata other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id && Version == other.Version && IsoLanguage == other.IsoLanguage;
        }
    }
}