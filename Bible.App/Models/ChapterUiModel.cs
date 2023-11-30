namespace BibleApp.Models
{
    public sealed partial class ChapterUiModel : List<VerseUiModel>
    {
        public int Id { get; }

        public string? Copyright { get; set; }

        public ChapterUiModel(int id) : base(new List<VerseUiModel>())
        {
            Id = id;
        }

        public ChapterUiModel(List<VerseUiModel> verses, int id) : base(verses)
        {
            Id = id;
        }

        public override string ToString() =>
            $"Chapter {Id} ({this.Count} verses)";
    }
}