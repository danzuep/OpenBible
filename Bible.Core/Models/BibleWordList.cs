namespace Bible.Core.Models
{
    public class BibleWordList : List<BibleWord>
    {
        public string Number { get; set; }
        public string ChapterNumber { get; set; }
    }

    public class BibleVerseList : List<BibleWordList> { }

    public class BibleParagraphList : List<BibleVerseList> { }
}
