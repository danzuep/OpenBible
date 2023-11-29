namespace Bible.Interfaces
{
    public interface IDataService<T>
    {
        T Load(string fileName, string suffix = ".xml");
    }
}