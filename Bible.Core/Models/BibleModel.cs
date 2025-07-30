using System.Collections;

namespace Bible.Core.Models
{
    public sealed class BibleModel : IEnumerable<BibleBook>
    {
        /// <summary>
        /// Information about the bible.
        /// </summary>
        public BibleInformation Information { get; set; }

        /// <summary>
        /// Books of the bible.
        /// </summary>
        public IReadOnlyList<BibleBook> Books { get; set; }
        //public List<BibleBook> Books { get; set; }

        /// <summary>
        /// Number of books.
        /// </summary>
        public int BookCount => Books.Count;

        public IEnumerator<BibleBook> GetEnumerator() =>
            Books.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();

        public override string ToString() =>
            $"{Information} ({BookCount} Books)";
    }

    public sealed class BibleInformation
    {
        public string Name { get; set; }
        public string IsoLanguage { get; set; }
        public string Translation { get; set; }
        public string Publisher { get; set; }
        public string Version { get; set; }
        public int? PublishedYear { get; set; }

        public override string ToString() =>
            $"({IsoLanguage} {Translation} {PublishedYear}) {Name}";
    }
}