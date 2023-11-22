namespace Bible.Core.Models
{
    public class BibleReference
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

        public override bool Equals(object other) => other is BibleReference p &&
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
