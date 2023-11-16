namespace Bible.Core.Models
{
    public class BibleReference
    {
        public string Book { get; set; } = string.Empty;

        public int Chapter { get; set; } = 1;

        public int[]? Verses { get; set; }
    }
}
