namespace Bible.Web.Models
{
    public sealed class BibleBookNav
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        private int _chapterCount;
        public int ChapterCount
        {
            get => _chapterCount;
            set
            {
                _chapterCount = value;
                _chapters = Enumerable.Range(1, _chapterCount).ToArray();
            }
        }

        private int[] _chapters = [1];
        public IReadOnlyList<int> Chapters => _chapters;

        public override string ToString() =>
            $"{Id}-{Name}-{ChapterCount}";
    }
}