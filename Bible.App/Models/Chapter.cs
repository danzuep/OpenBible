namespace BibleApp.Models
{
    public sealed class Chapter
    {
        public int Id { get; set; }
        public IList<string>? Verses { get; set; }
    }
}
