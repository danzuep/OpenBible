using Bible.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bible.Interfaces
{
    public interface IBibleService
    {
        Task<IList<BibleBook>> GetAllBooks();
    }
}
