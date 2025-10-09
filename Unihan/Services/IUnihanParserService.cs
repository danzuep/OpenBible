namespace Unihan.Services
{
    public interface IUnihanParserService
    {
        Task<Stream> ProcessStreamAsync(Stream inputStream);
    }
}