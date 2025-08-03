
namespace Bible.Core.Models.Scripture
{
    [Serializable]
    public class ScriptureRecord
    {
        /// <summary>Text content of this segment.</summary>
        public string Text { get; set; }

        /// <summary>Metadata category assigned to this segment.</summary>
        public MetadataCategory Category { get; set; } = MetadataCategory.Text;

        public override string ToString()
        {
            if (Category == MetadataCategory.Text)
                return Text;
            return $"{Text} ({Category})";
        }
    }
}