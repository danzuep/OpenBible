namespace Bible.Core.Models
{
    public class BookContent
    {
        public BookContent(string[][] content)
        {
            Content = content;
        }

        /// <summary>
        /// Chapters and verses.
        /// </summary>
        public string[][] Content { get; }

        public int ChapterCount => Content?.Length ?? 0;

        public override bool Equals(object other) =>
            other is BookContent p && p.Content.Equals(Content);

        public override int GetHashCode() => Content.GetHashCode();
    }
}
