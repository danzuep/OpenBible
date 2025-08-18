namespace Bible.Core.Models.Meta
{
    public sealed class WordsWithMetadata
    {
        public WordsWithMetadata(IEnumerable<WordMetadata> values = null)
        {
            Words = values?.ToArray() ?? Array.Empty<WordMetadata>();
        }

        public IList<WordMetadata> Words { get; set; }

        public override string ToString()
        {
            return string.Concat(Words);
        }
    }
}