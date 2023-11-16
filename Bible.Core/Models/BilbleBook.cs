using System;
using System.Collections.Generic;

namespace Bible.Core.Models
{
    public class BibleBook : IComparable<BibleBook>
    {
        private static int _createdOrder;
        private readonly int _order;

        /// <summary>
        /// Bible book constructor.
        /// </summary>
        /// <param name="bookName">Book name</param>
        /// <param name="bibleVersion">Bible version</param>
        /// <param name="synonyms">Alternative names</param>
        /// <param name="content">Book content</param>
        internal BibleBook(string bookName, string bibleVersion, IEnumerable<string> synonyms, BookContent content)
        {
            _order = _createdOrder++;
            Name = bookName;
            Version = bibleVersion;
            Synonyms = synonyms;
            Content = content;
        }

        /// <summary>
        /// Version of the bible used.
        /// </summary>
        public string Version { get; }

        /// <summary>
        /// Unabreviated name of the book.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Abreviated names or common mis-spellings of the book.
        /// Standard abbreviations and Thompson Chain references pulled from the 5th edition
        /// of "The Christian Writer's Manual of Style", 2004 edition (ISBN: 9780310487715).
        /// </summary>
        public IEnumerable<string> Synonyms { get; set; }

        /// <summary>
        /// Chapters and verses.
        /// </summary>
        public BookContent Content { get; }

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
            if (book1 == null || book2 == null)
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

        public override bool Equals(object other) =>
            other is BibleBook b && (b.Version, b.Name).Equals((Version, Name));

        /// <inheritdoc cref="IComparable" />
        public int CompareTo(BibleBook other) =>
            _order.CompareTo(other._order);

        public override int GetHashCode() =>
            (Version, Name).GetHashCode();

        public override string ToString() =>
            $"{Name} ({Content.ChapterCount} Chapters)";

        public bool Equals(BibleBook other)
        {
            throw new NotImplementedException();
        }
    }
}