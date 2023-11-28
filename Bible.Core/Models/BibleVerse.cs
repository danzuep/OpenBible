using System;

namespace Bible.Core.Models
{
    public sealed class BibleVerse : IEquatable<BibleVerse>
    {
        public BibleReference Reference { get; set; } = default!;

        public string VerseReference => $"{Reference}:{Id}";

        public int Id { get; set; }

        public string Text { get; set; } = default;

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
