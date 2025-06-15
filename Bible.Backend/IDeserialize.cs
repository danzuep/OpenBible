namespace Bible.Backend
{
    public interface IDeserialize
    {
        T? Deserialize<T>(string filePath);
    }
}