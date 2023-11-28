using System;

namespace Bible.Core.Models
{
    public sealed class BibleReference : IEquatable<BibleReference>, IComparable<BibleReference>
    {
        public BibleReference() { }

        public BibleReference(BibleReference bibleReference)
        {
            Translation = bibleReference.Translation;
            BookName = bibleReference.BookName;
            Reference = bibleReference.Reference;
        }

        /// <summary>
        /// Version of the bible used.
        /// </summary>
        public string Translation { get; set; } = default!;

        /// <summary>
        /// Unabreviated name of the book.
        /// </summary>
        public string BookName { get; set; } = default!;

        /// <summary>
        /// Optional chapter and verse reference.
        /// </summary>
        public string Reference { get; set; }

        public int CompareTo(BibleReference other)
        {
            if (other == null) return 1;
            return (other.Translation, other.BookName, other.Reference).CompareTo((Translation, BookName, Reference));
        }

        public override bool Equals(object other) =>
            other is BibleReference p && p.Equals(this);

        public bool Equals(BibleReference other) => other is BibleReference p &&
            (p.Translation, p.BookName, p.Reference).Equals((Translation, BookName, Reference));

        public override int GetHashCode() =>
            (Translation, BookName, Reference).GetHashCode();

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Reference))
                return $"{Translation} {BookName}";
            return $"{Translation} {BookName} {Reference}";
        }
    }
}
