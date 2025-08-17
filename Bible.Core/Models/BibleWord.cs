namespace Bible.Core.Models
{
    public sealed class BibleWord
    {
        public string Text { get; set; } = default;

        /// <summary>Optional pronunciation</summary>
        public string Pronunciation { get; set; }

        /// <summary>Optional 1-based footnote ID by convention</summary>
        public string FootnoteId { get; set; }

        public override string ToString() => Text;
    }
}
