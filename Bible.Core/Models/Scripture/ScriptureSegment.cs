namespace Bible.Core.Models.Scripture
{
    /// <summary>
    /// Immutable readonly struct representing a single scripture segment.
    /// TODO consider using MessagePack for more compact serialization.
    /// </summary>
    public readonly struct ScriptureSegment
    {
        /// <summary>Text content of this segment.</summary>
        public string Text { get; }

        /// <summary>Metadata category assigned to this segment.</summary>
        public MetadataCategory Category { get; }

        /// <summary>
        /// Creates a new ScriptureSegment instance.
        /// </summary>
        /// <param name="text">Text content (non-null).</param>
        /// <param name="category">Metadata category (default: Text).</param>
        /// <exception cref="ArgumentNullException">If text is null.</exception>
        public ScriptureSegment(string text, MetadataCategory category = MetadataCategory.Text)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
            Category = category;
        }

        /// <summary>
        /// Returns a string representation of this scripture segment.
        /// Omits the Category if it is the default MetadataCategory.Text.
        /// </summary>
        public override string ToString()
        {
            if (Category == MetadataCategory.Text)
                return $"\"{Text}\"";
            return $"\"{Text}\" ({Category})";
        }
    }
}