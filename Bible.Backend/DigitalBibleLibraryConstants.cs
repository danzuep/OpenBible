public static class DigitalBibleLibraryConstants
{
    public const string BaseUrl = "https://app.library.bible/";

    internal const string WebId = "9879dbb7cfe39e4d";
    public const string WebbeId = "7142879509583d59";
    internal const string NivId = "78a9f6124f344018";
    internal const string Esv16Id = "f421fe261da7624f";
    internal const string CevId = "555fef9a6cb31151";
    internal const string GntdId = "61fd76eafa1577c2";
    internal const string NrsvId = "1fd99b0d5841e19b";
    internal const string AmpId = "a81b73293d3080c9";
    internal const string cmnOccbId = "a6e06d2c5b90ad89";

    public static readonly int AgreementIdPublicDomain1 = 242201;
    public static readonly int AgreementIdPublicDomain2 = 240016;

    public static string GetContentUrl(string id) =>
        $"content/{id}/files";

    public static string GetDownloadUrl(string id, int? agreement = null) =>
        $"content/{id}/download?agreementId={agreement ?? AgreementIdPublicDomain2}";

    public static readonly HttpClient HttpClient = new()
    {
        BaseAddress = new Uri(BaseUrl)
    };
}