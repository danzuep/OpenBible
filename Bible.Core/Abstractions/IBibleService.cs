using Bible.Core.Models;

namespace Bible.Interfaces
{
    public interface IBibleService
    {
        BibleModel LoadBible(string fileName, string suffix = ".xml");
    }
}