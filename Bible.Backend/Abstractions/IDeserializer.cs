
namespace Bible.Backend.Abstractions
{
    public interface IDeserializer
    {
        T? Deserialize<T>(string filePath);
        Task<TOut?> DeserializeAsync<TIn, TOut>(string filePath, Func<TIn?, TOut?> transform, CancellationToken cancellationToken = default);
    }
}