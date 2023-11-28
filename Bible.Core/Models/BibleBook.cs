using System;
using System.Collections.Generic;
using System.Linq;

namespace Bible.Core.Models
{
    public sealed class BibleBook : IComparable<BibleBook>, IEquatable<BibleBook>
    {
        private static int _createdOrder;
        private readonly int _order;

        public BibleBook() { }

        /// <summary>
        /// Bible book constructor.
        /// </summary>
        /// <param name="reference">Translation and book name</param>
        /// <param name="content">Book content</param>
        public BibleBook(BibleReference reference, IEnumerable<BibleChapter> chapters)
        {
            _order = _createdOrder++;
            Reference = reference;
            Chapters = chapters.ToArray();
        }

        public BibleReference Reference { get; set; } = default!;

        /// <summary>
        /// Abreviated or alternative names of the book.
        /// </summary>
        public IList<string> Aliases { get; set; }

        public int Id { get; set; }

        /// <summary>
        /// Chapters and verses.
        /// </summary>
        public IReadOnlyList<BibleChapter> Chapters { get; set; }

        /// <summary>
        /// Number of chapters.
        /// </summary>
        public int ChapterCount => Chapters.Count;

        /// <summary>
        /// Determines whether two objects are equal.
        /// </summary>
        /// <param name="book1">The first object to compare.</param>
        /// <param name="book2">The second object to compare.</param>
        /// <returns>true if the objects are equal; otherwise, false.</returns>
        private static bool EqualityTest(BibleBook book1, BibleBook book2)
        {
            // both are null, or both are same instance
            if (ReferenceEquals(book1, book2))
                return true;

            // one is null, but not both
            if (book1 is null || book2 is null)
                return false;

            return book1.Equals(book2);
        }

        /// <inheritdoc cref="EqualityTest(BibleBook, BibleBook)" />
        public static bool operator == (BibleBook book1, BibleBook book2) =>
            EqualityTest(book1, book2);

        /// <summary>
        /// Determines whether two objects are not equal.
        /// </summary>
        /// <param name="book1">The first object to compare.</param>
        /// <param name="book2">The second object to compare.</param>
        /// <returns>true if the objects are not equal; otherwise, false.</returns>
        public static bool operator != (BibleBook book1, BibleBook book2) =>
            !EqualityTest(book1, book2);

        public bool Equals(BibleBook other) =>
            other is BibleBook b && b.Reference.Equals(Reference);

        public override bool Equals(object other) =>
            other is BibleBook b && b.Reference.Equals(Reference);

        /// <inheritdoc cref="IComparable" />
        public int CompareTo(BibleBook other) =>
            _order.CompareTo(other._order);

        public override int GetHashCode() =>
            Reference.GetHashCode();

        public override string ToString() =>
            $"{Reference} ({ChapterCount} Chapters)";
    }
}