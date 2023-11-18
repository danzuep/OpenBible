namespace Bible.Core.Models
{
    public class BibleVerse
    {
        public BibleReference Reference { get; set; } = default!;

        public int VerseNumber { get; set; }

        public string Text { get; set; } = default!;

        public override bool Equals(object other) =>
            other is BibleVerse p && p.Reference.Equals(Reference);

        public override int GetHashCode() =>
            Reference.GetHashCode();

        public override string ToString() =>
            $"\"{Text}\" {Reference}";
    }
}
