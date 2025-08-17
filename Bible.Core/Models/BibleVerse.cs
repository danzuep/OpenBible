namespace Bible.Core.Models
{
    public sealed class BibleVerse : IEquatable<BibleVerse>
    {
        public BibleReference Reference { get; set; } = default!;

        public string VerseReference => Reference.ToString();

        public int Id { get; set; }

        private Lazy<string> _text;
        public string Text
        {
            get => _text?.Value ?? string.Empty;
            set
            {
                if (value == null)
                {
                    _text = new Lazy<string>(() => string.Empty);
                    return;
                }
                _text = new Lazy<string>(() => value);
            }
        }

        private BibleWord[] _words;
        public IReadOnlyList<BibleWord> Words
        {
            get => _words ?? Array.Empty<BibleWord>();
            set
            {
                if (value == null || value.Count == 0)
                {
                    _words = Array.Empty<BibleWord>();
                    _text = new Lazy<string>(() => string.Empty);
                    return;
                }
                _text = new Lazy<string>(() => string.Concat(Words));
            }
        }

        public override bool Equals(object other) =>
            other is BibleVerse p && p.Equals(this);

        public bool Equals(BibleVerse other) =>
            other is BibleVerse p && p.Reference.Equals(Reference);

        public override int GetHashCode() =>
            Reference.GetHashCode();

        public override string ToString() =>
            $"{VerseReference} {Text}";
    }
}
