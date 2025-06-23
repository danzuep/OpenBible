public static class DigitalBibleLibraryConstants
{
    public const string BaseUrl = "https://app.thedigitalbiblelibrary.org/";

    public static string GetEntryUrl(string id) =>
        $"entry?id={id}";

    public static string GetDowloadUrl(string id, int license) =>
        $"entry/download_archive?id={id}&license={license}&type=release";

    public static readonly HttpClient HttpClient = new()
    {
        BaseAddress = new Uri(BaseUrl),
        Timeout = TimeSpan.FromMinutes(10)
    };
}