namespace BibleApp.Models
{
    public sealed partial class BibleUiModel : List<BookUiModel>
    {
        public string Translation { get; } = default!;

        public BibleUiModel(string translation) : base(new List<BookUiModel>())
        {
            Translation = translation;
        }

        public BibleUiModel(List<BookUiModel> books, string translation) : base(books)
        {
            Translation = translation;
        }

        public override string ToString() =>
            $"Bible: {Translation} ({this.Count} books)";
    }
}