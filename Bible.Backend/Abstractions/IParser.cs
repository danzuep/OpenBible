namespace Bible.Backend.Abstractions
{
    public interface IParser<T>
    {
        Task<T?> ParseAsync(string version, string bookName, CancellationToken cancellationToken = default);
    }

    public interface IBulkParser
    {
        IEnumerable<T> Enumerate<T>(string path, string fileType = "usx");
        IEnumerable<KeyValuePair<string, IEnumerable<T>>> EnumerateAll<T>(string path, string fileType = "usx");
    }
}