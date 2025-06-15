using System;
using System.Collections.Generic;
using System.Text;

namespace Bible.Backend.Verse
{
    public class VerseResponse
    {
        public Data data { get; set; }
        public Meta meta { get; set; }
    }

    public class Data
    {
        public string id { get; set; }
        public string orgId { get; set; }
        public string bookId { get; set; }
        public string chapterId { get; set; }
        public string bibleId { get; set; }
        public string reference { get; set; }
        public Content[] content { get; set; }
        public int verseCount { get; set; }
        public string copyright { get; set; }
        public Next next { get; set; }
        public Previous previous { get; set; }
    }

    public class Next
    {
        public string id { get; set; }
        public string number { get; set; }
    }

    public class Previous
    {
        public string id { get; set; }
        public string number { get; set; }
    }

    public class Content
    {
        public string name { get; set; }
        public string type { get; set; }
        public Attrs attrs { get; set; }
        public Item[] items { get; set; }
    }

    public class Attrs
    {
        public string style { get; set; }
    }

    public class Item
    {
        public string text { get; set; }
        public string type { get; set; }
        public Attrs1 attrs { get; set; }
    }

    public class Attrs1
    {
        public string verseId { get; set; }
        public string[] verseOrgIds { get; set; }
    }

    public class Meta
    {
        public string fums { get; set; }
        public string fumsId { get; set; }
        public string fumsJsInclude { get; set; }
        public string fumsJs { get; set; }
        public string fumsNoScript { get; set; }
    }
}
