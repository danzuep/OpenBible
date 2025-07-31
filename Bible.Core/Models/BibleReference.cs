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
        /// Three-letter abbreviation of the book name.
        /// </summary>
        public string BookCode { get; set; } = default!;

        /// <summary>
        /// Optional chapter and verse reference.
        /// TODO: replace with chapter verse.
        /// </summary>
        public string Reference { get; set; }

        /// <summary>
        /// Optional chapter reference.
        /// </summary>
        public int Chapter { get; set; }

        /// <summary>
        /// Optional verse reference.
        /// </summary>
        public string Verse { get; set; }

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

        public string ToReference()
        {
            if (string.IsNullOrEmpty(Reference))
                return $"{BookName} ({Translation})";
            return $"{BookName} {Reference} ({Translation})";
        }

        public string ToSearch()
        {
            if (!string.IsNullOrEmpty(Reference))
                return $"{BookCode}.{Reference}?v={Translation}";
            if (!string.IsNullOrEmpty(Verse))
                return $"{BookCode}.{Chapter}.{Verse}?v={Translation}";
            else if (Chapter > 0)
                return $"{BookCode}.{Chapter}?v={Translation}";
            else if (!string.IsNullOrEmpty(BookCode))
                return $"{BookCode}?v={Translation}";
            else if (!string.IsNullOrEmpty(BookName))
                return $"{BookName}?v={Translation}";
            else
                return Translation ?? "_";
        }

        public string ToPath()
        {
            if (string.IsNullOrEmpty(Reference))
                return BookName;
            var ch = Reference.Split(new char[] { ':', ' ' });
            if (ch.Length > 1)
                return $"{BookName}/{Reference[0]}";
            return $"{BookName}/{Reference}";
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Reference))
                return $"{Translation} {BookName}";
            return $"{Translation} {BookName} {Reference}";
        }
    }
}
