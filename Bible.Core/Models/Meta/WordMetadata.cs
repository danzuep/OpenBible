namespace Bible.Core.Models.Meta
{
    public struct WordMetadata
    {
        public WordMetadata(string key, IList<string> values = null)
        {
            Text = key ?? throw new ArgumentNullException(nameof(key));
            Metadata = values;
        }

        public string Text { get; }

        public IList<string> Metadata { get; }

        //public static implicit operator string(TextMetadata text) => text;

        //public static implicit operator TextMetadata(string value) => new(value);

        public override string ToString()
        {
            if (Metadata == null || Metadata.Count == 0)
                return Text;
            return $"{Text}【{string.Join("; ", Metadata)}】";
        }
    }
}