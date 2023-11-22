using System.ComponentModel;
using System.Xml.Serialization;

namespace Bible.Reader.Models
{
    public enum BibleBookId
    {
        Unknown = 0,
        [XmlEnum("GEN")]
        [Description("Genesis")]
        Genesis = 1,
        [XmlEnum("EXO")]
        [Description("Exodus")]
        Exodus = 2,
        [XmlEnum("LEV")]
        [Description("Leviticus")]
        Leviticus = 3,
        [XmlEnum("NUM")]
        [Description("Numbers")]
        Numbers = 4,
        [XmlEnum("DEU")]
        [Description("Deuteronomy")]
        Deuteronomy = 5,
        [XmlEnum("JOS")]
        [Description("Joshua")]
        Joshua = 6,
        [XmlEnum("JDG")]
        [Description("Judges")]
        Judges = 7,
        [XmlEnum("RUT")]
        [Description("Ruth")]
        Ruth = 8,
        [XmlEnum("1SA")]
        [Description("1 Samuel")]
        FirstSamuel = 9,
        [XmlEnum("2SA")]
        [Description("2 Samuel")]
        SecondSamuel = 10,
        [XmlEnum("1KI")]
        [Description("1 Kings")]
        FirstKings = 11,
        [XmlEnum("2KI")]
        [Description("2 Kings")]
        SecondKings = 12,
        [XmlEnum("1CH")]
        [Description("1 Chronicles")]
        FirstChronicles = 13,
        [XmlEnum("2CH")]
        [Description("2 Chronicles")]
        SecondChronicles = 14,
        [XmlEnum("EZR")]
        [Description("Ezra")]
        Ezra = 15,
        [XmlEnum("NEH")]
        [Description("Nehemiah")]
        Nehemiah = 16,
        [XmlEnum("EST")]
        [Description("Esther")]
        Esther = 17,
        [XmlEnum("JOB")]
        [Description("Job")]
        Job = 18,
        [XmlEnum("PSA")]
        [Description("Psalms")]
        Psalms = 19,
        [XmlEnum("PRO")]
        [Description("Proverbs")]
        Proverbs = 20,
        [XmlEnum("ECC")]
        [Description("Ecclesiastes")]
        Ecclesiastes = 21,
        [XmlEnum("SNG")]
        [Description("Song of Solomon")]
        SongOfSolomon = 22,
        [XmlEnum("ISA")]
        [Description("Isaiah")]
        Isaiah = 23,
        [XmlEnum("JER")]
        [Description("Jeremiah")]
        Jeremiah = 24,
        [XmlEnum("LAM")]
        [Description("Lamentations")]
        Lamentations = 25,
        [XmlEnum("EZK")]
        [Description("Ezekiel")]
        Ezekiel = 26,
        [XmlEnum("DAN")]
        [Description("Daniel")]
        Daniel = 27,
        [XmlEnum("HOS")]
        [Description("Hosea")]
        Hosea = 28,
        [XmlEnum("JOL")]
        [Description("Joel")]
        Joel = 29,
        [XmlEnum("AMO")]
        [Description("Amos")]
        Amos = 30,
        [XmlEnum("OBA")]
        [Description("Obadiah")]
        Obadiah = 31,
        [XmlEnum("JON")]
        [Description("Jonah")]
        Jonah = 32,
        [XmlEnum("MIC")]
        [Description("Micah")]
        Micah = 33,
        [XmlEnum("NAM")]
        [Description("Nahum")]
        Nahum = 34,
        [XmlEnum("HAB")]
        [Description("Habakkuk")]
        Habakkuk = 35,
        [XmlEnum("ZEP")]
        [Description("Zephaniah")]
        Zephaniah = 36,
        [XmlEnum("HAG")]
        [Description("Haggai")]
        Haggai = 37,
        [XmlEnum("ZEC")]
        [Description("Zechariah")]
        Zechariah = 38,
        [XmlEnum("MAL")]
        [Description("Malachi")]
        Malachi = 39,
        [XmlEnum("MAT")]
        [Description("Matthew")]
        Matthew = 40,
        [XmlEnum("MRK")]
        [Description("Mark")]
        Mark = 41,
        [XmlEnum("LUK")]
        [Description("Luke")]
        Luke = 42,
        [XmlEnum("JHN")]
        [Description("John")]
        John = 43,
        [XmlEnum("ACT")]
        [Description("Acts")]
        Acts = 44,
        [XmlEnum("ROM")]
        [Description("Romans")]
        Romans = 45,
        [XmlEnum("1CO")]
        [Description("1 Corinthians")]
        FirstCorinthians = 46,
        [XmlEnum("2CO")]
        [Description("2 Corinthians")]
        SecondCorinthians = 47,
        [XmlEnum("GAL")]
        [Description("Galatians")]
        Galatians = 48,
        [XmlEnum("EPH")]
        [Description("Ephesians")]
        Ephesians = 49,
        [XmlEnum("PHP")]
        [Description("Philippians")]
        Philippians = 50,
        [XmlEnum("COL")]
        [Description("Colossians")]
        Colossians = 51,
        [XmlEnum("1TH")]
        [Description("1 Thessalonians")]
        FirstThessalonians = 52,
        [XmlEnum("2TH")]
        [Description("2 Thessalonians")]
        SecondThessalonians = 53,
        [XmlEnum("1TI")]
        [Description("1 Timothy")]
        FirstTimothy = 54,
        [XmlEnum("2TI")]
        [Description("2 Timothy")]
        SecondTimothy = 55,
        [XmlEnum("TIT")]
        [Description("Titus")]
        Titus = 56,
        [XmlEnum("PHM")]
        [Description("Philemon")]
        Philemon = 57,
        [XmlEnum("HEB")]
        [Description("Hebrews")]
        Hebrews = 58,
        [XmlEnum("JAS")]
        [Description("James")]
        James = 59,
        [XmlEnum("1PE")]
        [Description("1 Peter")]
        FirstPeter = 60,
        [XmlEnum("2PE")]
        [Description("2 Peter")]
        SecondPeter = 61,
        [XmlEnum("1JN")]
        [Description("1 John")]
        FirstJohn = 62,
        [XmlEnum("2JN")]
        [Description("2 John")]
        SecondJohn = 63,
        [XmlEnum("3JN")]
        [Description("3 John")]
        ThirdJohn = 64,
        [XmlEnum("JUD")]
        [Description("Jude")]
        Jude = 65,
        [XmlEnum("REV")]
        [Description("Revelation")]
        Revelation = 66,
        [XmlEnum("JDT")]
        [Description("Judith")]
        Judith = 67,
        [XmlEnum("ESG")]
        [Description("Esther Greek")]
        EstherGreek = 68,
        [XmlEnum("WIS")]
        [Description("Wisdom of Solomon")]
        WisdomOfSolomon = 69,
        [XmlEnum("SIR")]
        [Description("Sirach (Ecclesiasticus)")]
        Sirach = 70,
        [XmlEnum("BAR")]
        [Description("Baruch")]
        Baruch = 71,
        [XmlEnum("LJE")]
        [Description("Letter of Jeremiah")]
        LetterOfJeremiah = 72,
        [XmlEnum("S3Y")]
        [Description("Song of 3 Young Men")]
        SongOfThreeYoungMen = 73,
        [XmlEnum("SUS")]
        [Description("Susanna")]
        Susanna = 74,
        [XmlEnum("BEL")]
        [Description("Bel and the Dragon")]
        BelAndTheDragon = 75,
        [XmlEnum("1MA")]
        [Description("1 Maccabees")]
        FirstMaccabees = 76,
        [XmlEnum("2MA")]
        [Description("2 Maccabees")]
        SecondMaccabees = 77,
        [XmlEnum("3MA")]
        [Description("3 Maccabees")]
        ThirdMaccabees = 78,
        [XmlEnum("4MA")]
        [Description("4 Maccabees")]
        FourthMaccabees = 79,
        [XmlEnum("1ES")]
        [Description("1 Esdras (Greek)")]
        FirstEsdrasGreek = 80,
        [XmlEnum("2ES")]
        [Description("2 Esdras (Latin)")]
        SecondEsdrasLatin = 81,
        [XmlEnum("MAN")]
        [Description("Prayer of Manasseh")]
        PrayerOfManasseh = 82,
        [XmlEnum("PS2")]
        [Description("Psalm 151")]
        Psalm151 = 83,
        [XmlEnum("ODA")]
        [Description("Odes")]
        Odes = 84,
        [XmlEnum("PSS")]
        [Description("Psalms of Solomon")]
        PsalmsOfSolomon = 85,
        [XmlEnum("EZA")]
        [Description("Apocalypse of Ezra")]
        ApocalypseOfEzra = 86,
        [XmlEnum("5EZ")]
        [Description("5 Ezra")]
        FifthEzra = 87,
        [XmlEnum("6EZ")]
        [Description("6 Ezra")]
        SixthEzra = 88,
        [XmlEnum("DAG")]
        [Description("Daniel Greek")]
        DanielGreek = 89,
        [XmlEnum("PS3")]
        [Description("Psalms 152-155")]
        Psalms152_155 = 90,
        [XmlEnum("2BA")]
        [Description("2 Baruch (Apocalypse)")]
        SecondBaruch = 91,
        [XmlEnum("LBA")]
        [Description("Letter of Baruch")]
        LetterOfBaruch = 92,
        [XmlEnum("JUB")]
        [Description("Jubilees")]
        Jubilees = 93,
        [XmlEnum("ENO")]
        [Description("Enoch")]
        Enoch = 94,
        [XmlEnum("1MQ")]
        [Description("1 Meqabyan")]
        FirstMeqabyan = 95,
        [XmlEnum("2MQ")]
        [Description("2 Meqabyan")]
        SecondMeqabyan = 96,
        [XmlEnum("3MQ")]
        [Description("3 Meqabyan")]
        ThirdMeqabyan = 97,
        [XmlEnum("REP")]
        [Description("Reproof")]
        Reproof = 98,
        [XmlEnum("4BA")]
        [Description("4 Baruch")]
        FourthBaruch = 99,
        [XmlEnum("LAO")]
        [Description("Laodiceans")]
        Laodiceans = 100
    }

    public enum BibleBookNumber
    {
        First = 1,
        Second = 2,
        Third = 3
    }
}