

namespace Bible.Backend.Abstractions
{
    public interface IDeserializer
    {
        T? Deserialize<T>(string filePath);
        Task<TOut?> DeserializeAsync<TIn, TOut>(string filePath, Func<TIn?, TOut?> transform, CancellationToken cancellationToken = default);
        Task<T?> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default);
        Task<TOut?> DeserializeResourceAsync<TIn, TOut>(string resourceName, Func<TIn?, TOut?> transform, CancellationToken cancellationToken = default);
    }
}