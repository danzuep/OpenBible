public static class BibleApiConstants
{
    public const string Key = "de8227330fbf44f266d779e29db6b48a";

    public const string BaseUrl = "https://api.scripture.api.bible/";
    public const string Version = "v1/";
    public const string Bibles = "bibles/";
    public const string WEBBE = "7142879509583d59-01";
    public const string Passages = "/passages/";
    public const string Chapters = "/chapters/";
    public const string Jud1 = "JUD.1";
    public const string Verses = "/verses/";
    public const string Jud11 = "JUD.1.1";
    public const string JsonSuffix = "?content-type=json";
    public const string ChapterSuffix = $"{JsonSuffix}&include-notes=false&include-titles=true&include-chapter-numbers=true&include-verse-numbers=true&include-verse-spans=false";
    public const string VerseSuffix = $"{JsonSuffix}&include-notes=false&include-titles=false&include-chapter-numbers=false&include-verse-numbers=false&include-verse-spans=false&use-org-id=false";
    public const string PassageSuffix = "?content-type=text&include-notes=false&include-titles=false&include-chapter-numbers=false&include-verse-numbers=false&include-verse-spans=false&use-org-id=false";

    public static readonly string SampleSuffix = $"{Version}{Bibles}{WEBBE}{Passages}{Jud1}{PassageSuffix}";
    public static readonly string SampleFull = $"{BaseUrl}{SampleSuffix}";
}