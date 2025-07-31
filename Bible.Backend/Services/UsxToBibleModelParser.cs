using System.Reflection;
using Bible.Backend.Abstractions;
using Bible.Backend.Adapters;
using Bible.Backend.Models;
using Bible.Core.Abstractions;
using Bible.Core.Models;

namespace Bible.Backend.Services
{
    public class UsxToBibleModelParser : IDataService<BibleModel>
    {
        private readonly IBulkParser _parser;

        public UsxToBibleModelParser(IBulkParser parser)
        {
            _parser = parser;
        }

        public BibleModel Load(string version = "eng-WEBBE", string suffix = ".usx")
        {
            var usxPath = UsxToBibleBookParser.GetBibleAssetFolder(version, suffix);
            var usxBooks = _parser.Enumerate<UsxBook>(usxPath);
            var books = usxBooks.Select(b => b.ToBibleFormat()).ToList();
            var bible = new BibleModel
            {
                Books = books,
                Information = new()
                {
                    IsoLanguage = version.Split("-").FirstOrDefault(),
                    Version = version.Split("-").LastOrDefault()
                }
            };
            return bible;
        }
    }
}
