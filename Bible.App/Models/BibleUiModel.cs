namespace Bible.App.Models
{
    public sealed partial class BibleUiModel : List<BookUiModel>
    {
        public BibleUiModel(string? translation, List<BookUiModel>? books = null) : base(books ?? new())
        {
            Translation = translation ?? string.Empty;
        }

        public string Translation { get; }

        public override string ToString() =>
            $"Bible: {Translation} ({this.Count} books)";
    }
}