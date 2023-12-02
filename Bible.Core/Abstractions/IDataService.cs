namespace Bible.Core.Abstractions
{
    public interface IDataService<T>
    {
        T Load(string fileName, string suffix = ".xml");
    }
}