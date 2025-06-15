public static class DigitalBibleLibraryConstants
{
    public const string BaseUrl = "https://app.thedigitalbiblelibrary.org/";
    public const string Entry = "entry?id=";
    public const string Download = "entry/download_archive?id=";
    // https://app.thedigitalbiblelibrary.org/entry/download_archive?id=7142879509583d59&license=4015&revision=146&type=release

    internal const string WebId = "9879dbb7cfe39e4d";
    public const string WebbeId = "7142879509583d59";
    internal const string NivId = "78a9f6124f344018";
    internal const string Esv16Id = "f421fe261da7624f";
    internal const string CevId = "555fef9a6cb31151";
    internal const string GntdId = "61fd76eafa1577c2";
    internal const string NrsvId = "1fd99b0d5841e19b";
    internal const string AmpId = "a81b73293d3080c9";

    public static readonly string Licence1Suffix = "4015";
    public static readonly string Licence2Suffix = "6232";
    public static readonly string SampleRevision = "146";
    public static readonly string QuerySuffix = $"&license={Licence1Suffix}&revision={SampleRevision}&type=release";
    public static readonly string SampleSuffix = $"{Download}{WebbeId}{QuerySuffix}";
    public static readonly string SampleFull = $"{BaseUrl}{SampleSuffix}";

    public static readonly HttpClient HttpClient = new()
    {
        BaseAddress = new Uri(BaseUrl)
    };
}