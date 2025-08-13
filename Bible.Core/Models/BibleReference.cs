using System;

namespace Bible.Core.Models
{
    public sealed class BibleReference : IEquatable<BibleReference>, IComparable<BibleReference>
    {
        public BibleReference() { }

        public BibleReference(BibleReference bibleReference)
        {
            Language = bibleReference.Language;
            Version = bibleReference.Version;
            VersionName = bibleReference.VersionName;
            BookName = bibleReference.BookName;
            Reference = bibleReference.Reference;
        }

        /// <summary>
        /// ISO language of the bible used.
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Translation version of the bible used.
        /// </summary>
        public string Version { get; set; } = default!;

        /// <summary>
        /// Translation version name of the bible used.
        /// </summary>
        public string VersionName { get; set; } = default!;

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
            return (other.Version, other.BookName, other.Reference).CompareTo((Version, BookName, Reference));
        }

        public override bool Equals(object other) =>
            other is BibleReference p && p.Equals(this);

        public bool Equals(BibleReference other) => other is BibleReference p &&
            (p.Version, p.BookName, p.Reference).Equals((Version, BookName, Reference));

        public override int GetHashCode() =>
            (Version, BookName, Reference).GetHashCode();

        public string ToReference()
        {
            if (string.IsNullOrEmpty(Reference))
                return $"{BookName} - {Version}";
            return $"{BookName} {Reference} - {Version}";
        }

        public string ToSearch()
        {
            if (!string.IsNullOrEmpty(Reference))
                return $"{BookCode}.{Reference}?v={Version}";
            if (!string.IsNullOrEmpty(Verse))
                return $"{BookCode}.{Chapter}.{Verse}?v={Version}";
            else if (Chapter > 0)
                return $"{BookCode}.{Chapter}?v={Version}";
            else if (!string.IsNullOrEmpty(BookCode))
                return $"{BookCode}?v={Version}";
            else if (!string.IsNullOrEmpty(BookName))
                return $"{BookName}?v={Version}";
            else
                return Version ?? "_";
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
                return $"{Version} {BookName}";
            return $"{Version} {BookName} {Reference}";
        }
    }
}
