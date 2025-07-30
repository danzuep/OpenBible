using System.Collections.Generic;

namespace Bible.Core.Models
{
    public sealed class BibleChapter
    {
        public BibleReference Reference { get; set; } = new();

        public int Id { get; set; }

        public IReadOnlyList<BibleVerse> Verses { get; set; } = default!;
        //public List<BibleVerse> Verses { get; set; } = new();

        public override bool Equals(object other) =>
            other is BibleChapter p && (p.Reference?.Equals(Reference) ?? false);

        public override int GetHashCode() =>
            Reference.GetHashCode();

        public override string ToString() =>
            Reference.ToReference();
    }
}
