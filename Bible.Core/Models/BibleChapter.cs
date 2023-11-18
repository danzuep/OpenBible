using System.Collections.Generic;

namespace Bible.Core.Models
{
    public class BibleChapter
    {
        public BibleReference Reference { get; set; } = default!;

        public int ChapterNumber { get; set; }

        public IReadOnlyList<BibleVerse> Verses { get; set; } = default!;

        public override bool Equals(object other) =>
            other is BibleChapter p && p.Reference.Equals(Reference);

        public override int GetHashCode() =>
            Reference.GetHashCode();

        public override string ToString() =>
            Reference.ToString();
    }
}
