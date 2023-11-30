namespace BibleApp.Models
{
    public sealed partial class VerseUiModel
    {
        public int Id { get; }

        public string Text { get; }

        public VerseUiModel(int id, string text)
        {
            Id = id;
            Text = text;
        }

        public override string ToString() =>
            $"{Id} {Text}";
    }
}