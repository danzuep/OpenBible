using System.Collections.Generic;

namespace Bible.Core.Models
{
    public sealed class BibleModel
    {
        /// <summary>
        /// Information about the bible.
        /// </summary>
        public BibleInformation Information { get; set; }

        /// <summary>
        /// Books of the bible.
        /// </summary>
        public IReadOnlyList<BibleBook> Books { get; set; }

        /// <summary>
        /// Number of books.
        /// </summary>
        public int BookCount => Books.Count;

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