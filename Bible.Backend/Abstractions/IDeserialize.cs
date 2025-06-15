namespace Bible.Backend.Abstractions
{
    public interface IDeserialize
    {
        T? Deserialize<T>(string filePath);
    }
}