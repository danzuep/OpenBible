namespace BibleApp.Models
{
    public sealed class VerseUiModel
    {
        public int Id { get; set; }

        public string Text { get; set; } = default!;

        public override string ToString() =>
            $"{Id} {Text}";
    }
}