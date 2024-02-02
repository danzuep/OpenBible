using System.Collections.Generic;

namespace Bible.Core.Models
{
    public sealed class BibleChapter
    {
        public BibleReference Reference { get; set; } = default!;

        public int Id { get; set; }

        public IReadOnlyList<BibleVerse> Verses { get; set; } = default!;

        public override bool Equals(object other) =>
            other is BibleChapter p && (p.Reference?.Equals(Reference) ?? false);

        public override int GetHashCode() =>
            Reference.GetHashCode();

        public override string ToString() =>
            Reference.ToReference();
    }
}
