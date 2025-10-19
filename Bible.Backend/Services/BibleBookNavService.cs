using Bible.Backend.Models;

namespace Bible.Backend.Services
{
    public interface IBibleBookNavService
    {
        public IReadOnlyList<BibleBookNav> OldTestamentBooks { get; }
        public IReadOnlyList<BibleBookNav> NewTestamentBooks { get; }
    }

    public class BibleBookNavService : IBibleBookNavService
    {
        public IReadOnlyList<BibleBookNav> OldTestamentBooks => _oldTestamentBooks;

        public IReadOnlyList<BibleBookNav> NewTestamentBooks => _newTestamentBooks;

        private static IReadOnlyList<BibleBookNav> _oldTestamentBooks =
        [
            new() { Id = "GEN", Name = "Genesis", ChapterCount = 50 },
            new() { Id = "EXO", Name = "Exodus", ChapterCount = 40 },
            new() { Id = "LEV", Name = "Leviticus", ChapterCount = 27 },
            new() { Id = "NUM", Name = "Numbers", ChapterCount = 36 },
            new() { Id = "DEU", Name = "Deuteronomy", ChapterCount = 34 },
            new() { Id = "JOS", Name = "Joshua", ChapterCount = 24 },
            new() { Id = "JDG", Name = "Judges", ChapterCount = 21 },
            new() { Id = "RUT", Name = "Ruth", ChapterCount = 4 },
            new() { Id = "1SA", Name = "1 Samuel", ChapterCount = 31 },
            new() { Id = "2SA", Name = "2 Samuel", ChapterCount = 24 },
            new() { Id = "1KI", Name = "1 Kings", ChapterCount = 22 },
            new() { Id = "2KI", Name = "2 Kings", ChapterCount = 25 },
            new() { Id = "1CH", Name = "1 Chronicles", ChapterCount = 29 },
            new() { Id = "2CH", Name = "2 Chronicles", ChapterCount = 36 },
            new() { Id = "EZR", Name = "Ezra", ChapterCount = 10 },
            new() { Id = "NEH", Name = "Nehemiah", ChapterCount = 13 },
            new() { Id = "EST", Name = "Esther", ChapterCount = 10 },
            new() { Id = "JOB", Name = "Job", ChapterCount = 42 },
            new() { Id = "PSA", Name = "Psalms", ChapterCount = 150 },
            new() { Id = "PRO", Name = "Proverbs", ChapterCount = 31 },
            new() { Id = "ECC", Name = "Ecclesiastes", ChapterCount = 12 },
            new() { Id = "SNG", Name = "Song of Solomon", ChapterCount = 8 },
            new() { Id = "ISA", Name = "Isaiah", ChapterCount = 66 },
            new() { Id = "JER", Name = "Jeremiah", ChapterCount = 52 },
            new() { Id = "LAM", Name = "Lamentations", ChapterCount = 5 },
            new() { Id = "EZK", Name = "Ezekiel", ChapterCount = 48 },
            new() { Id = "DAN", Name = "Daniel", ChapterCount = 12 },
            new() { Id = "HOS", Name = "Hosea", ChapterCount = 14 },
            new() { Id = "JOL", Name = "Joel", ChapterCount = 3 },
            new() { Id = "AMO", Name = "Amos", ChapterCount = 9 },
            new() { Id = "OBA", Name = "Obadiah", ChapterCount = 1 },
            new() { Id = "JON", Name = "Jonah", ChapterCount = 4 },
            new() { Id = "MIC", Name = "Micah", ChapterCount = 7 },
            new() { Id = "NAM", Name = "Nahum", ChapterCount = 3 },
            new() { Id = "HAB", Name = "Habakkuk", ChapterCount = 3 },
            new() { Id = "ZEP", Name = "Zephaniah", ChapterCount = 3 },
            new() { Id = "HAG", Name = "Haggai", ChapterCount = 2 },
            new() { Id = "ZEC", Name = "Zechariah", ChapterCount = 14 },
            new() { Id = "MAL", Name = "Malachi", ChapterCount = 4 }
        ];

        private static IReadOnlyList<BibleBookNav> _newTestamentBooks =
        [
            new() { Id = "MAT", Name = "Matthew", ChapterCount = 28 },
            new() { Id = "MRK", Name = "Mark", ChapterCount = 16 },
            new() { Id = "LUK", Name = "Luke", ChapterCount = 24 },
            new() { Id = "JHN", Name = "John", ChapterCount = 21 },
            new() { Id = "ACT", Name = "Acts", ChapterCount = 28 },
            new() { Id = "ROM", Name = "Romans", ChapterCount = 16 },
            new() { Id = "1CO", Name = "1 Corinthians", ChapterCount = 16 },
            new() { Id = "2CO", Name = "2 Corinthians", ChapterCount = 13 },
            new() { Id = "GAL", Name = "Galatians", ChapterCount = 6 },
            new() { Id = "EPH", Name = "Ephesians", ChapterCount = 6 },
            new() { Id = "PHP", Name = "Philippians", ChapterCount = 4 },
            new() { Id = "COL", Name = "Colossians", ChapterCount = 4 },
            new() { Id = "1TH", Name = "1 Thessalonians", ChapterCount = 5 },
            new() { Id = "2TH", Name = "2 Thessalonians", ChapterCount = 3 },
            new() { Id = "1TI", Name = "1 Timothy", ChapterCount = 6 },
            new() { Id = "2TI", Name = "2 Timothy", ChapterCount = 4 },
            new() { Id = "TIT", Name = "Titus", ChapterCount = 3 },
            new() { Id = "PHM", Name = "Philemon", ChapterCount = 1 },
            new() { Id = "HEB", Name = "Hebrews", ChapterCount = 13 },
            new() { Id = "JAS", Name = "James", ChapterCount = 5 },
            new() { Id = "1PE", Name = "1 Peter", ChapterCount = 5 },
            new() { Id = "2PE", Name = "2 Peter", ChapterCount = 3 },
            new() { Id = "1JN", Name = "1 John", ChapterCount = 5 },
            new() { Id = "2JN", Name = "2 John", ChapterCount = 1 },
            new() { Id = "3JN", Name = "3 John", ChapterCount = 1 },
            new() { Id = "JUD", Name = "Jude", ChapterCount = 1 },
            new() { Id = "REV", Name = "Revelation", ChapterCount = 22 }
        ];

        public static readonly string DefaultLanguage = "zho-Hant";

        public static string GetVersion(string isoLanguage) =>
            BibleVersions.TryGetValue(isoLanguage, out var versions) ? versions.First() : "OCCB";

        // ISO 639-3 language, ISO 15924 script, English name, native name
        public static readonly Dictionary<string, IReadOnlyList<string>> IsoLanguages = new()
        {
            ["eng"] = ["English"],
            // ["tha"] = ["ไทย", "Thai"],
            ["zho-Hans"] = ["中文（简体）", "Chinese (Simplified)"],
            ["zho-Hant"] = ["中文（繁體）", "Chinese (Traditional)"],
        };

        // ISO 639-3 language, bible version acronyms
        public static readonly Dictionary<string, IReadOnlyList<string>> BibleVersions = new()
        {
            ["eng"] = ["WEBBE", "WEBU", "T4T"],
            ["tha"] = ["KJV"],
            ["zho-Hans"] = ["OCCB"],
            ["zho-Hant"] = ["OCCB"],
        };

        // Bible version acronyms, English name, native name, copyright
        public static readonly Dictionary<string, IReadOnlyList<string>> BibleAcronyms = new()
        {
            { "eng-WEBU", ["World English Bible USA"] },
            { "eng-WEBBE", ["World English Bible British Edition"] },
            { "eng-T4T", ["Translation for Translators (ENG-USA)"] },
            { "eng-KJV", ["King James Version"] },
            { "tha-KJV", ["Thai King James Version"] },
            { "zho-Hans-OCCB", ["Biblica® Open Chinese Contemporary Bible 2022 (Simplified)", "圣经当代译本开放资源", "Copyright © 1979,2005,2007,2011,2022 Biblica, Inc." ] },
            { "zho-Hant-OCCB", ["Biblica® Open Chinese Contemporary Bible 2023 (Traditional)", "聖經，當代譯本開放資源", "Copyright © 1979,2005,2007,2012,2023 Biblica, Inc."] }
        };
    }
}