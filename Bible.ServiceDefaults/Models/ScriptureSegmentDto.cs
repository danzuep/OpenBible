using Bible.Core.Models.Scripture;

namespace Bible.ServiceDefaults.Models
{
    [Serializable]
    public class ScriptureSegmentDto
    {
        /// <summary>Text content of this segment.</summary>
        public string? Text { get; set; }

        /// <summary>Metadata category assigned to this segment.</summary>
        public MetadataCategory Category { get; set; } = MetadataCategory.Text;

        public ScriptureBookMetadata? BookMetadata { get; set; }

        public override string ToString()
        {
            if (Category == MetadataCategory.Text)
                return Text ?? string.Empty;
            return $"{Text} ({Category})";
        }
    }
}