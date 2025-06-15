using System;
using System.Collections.Generic;
using System.Text;

namespace Bible.Backend.Passage
{
    public class PassageResponse
    {
        public Data data { get; set; }
        public Meta meta { get; set; }
    }

    public class Data
    {
        public string id { get; set; }
        public string orgId { get; set; }
        public string bibleId { get; set; }
        public string bookId { get; set; }
        public string[] chapterIds { get; set; }
        public string reference { get; set; }
        public string content { get; set; }
        public int verseCount { get; set; }
        public string copyright { get; set; }
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
