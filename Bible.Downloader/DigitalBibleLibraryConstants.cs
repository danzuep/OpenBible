public static class DigitalBibleLibraryConstants
{
    public const string BaseUrl = "https://app.thedigitalbiblelibrary.org/";

    public static string GetEntryUrl(string id) =>
        $"entry?id={id}";

    public static string GetDownloadUrl(string id, int license) =>
        $"entry/download_archive?id={id}&license={license}&type=release";

    public static string GetDownloadUrl(string path) =>
        $"{BaseUrl}/{path}";

    public static readonly HttpClient HttpClient = new()
    {
        BaseAddress = new Uri(BaseUrl),
        Timeout = TimeSpan.FromMinutes(10)
    };
}