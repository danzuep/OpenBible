namespace Bible.Core.Models
{
    public class BibleFootnote
    {
        public string Id { get; }
        public string Content { get; }

        private string _prefix;

        public BibleFootnote(string id, string content, string prefix = "footnote-")
        {
            Id = id;
            Content = content;
            _prefix = prefix;
        }

        public override string ToString() =>
            $"<div id=\"{_prefix}{Id}\">[{Id}] {Content}</div>";
    }

    public class BibleFootnoteList : List<BibleFootnote> { }
}
