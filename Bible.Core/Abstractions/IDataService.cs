using System.Threading.Tasks;

namespace Bible.Core.Abstractions
{
    public interface IDataService<T>
    {
        T Load(string fileName, string suffix = ".xml");
        //Task<T> LoadFileAsync(string fileName);
    }
}