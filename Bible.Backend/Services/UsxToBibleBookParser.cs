using Bible.Backend.Abstractions;
using Bible.Backend.Adapters;
using Bible.Backend.Models;
using Bible.Core.Models;

namespace Bible.Backend.Services
{
    public class UsxToBibleBookParser : ParsingStrategy, IParser<BibleBook>
    {
        private readonly IDeserializer _deserializer;

        public UsxToBibleBookParser(IDeserializer deserializer)
        {
            _deserializer = deserializer;
        }

        private static readonly string _fileType = "usx";

        public async Task<BibleBook?> ParseAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            var usxBook = await _deserializer.DeserializeAsync<UsxBook>(stream, cancellationToken);
            var bibleBook = usxBook.ToBibleFormat();
            return bibleBook;
        }

        public async Task<BibleBook?> ParseFilesAsync(string version, string bookName, CancellationToken cancellationToken = default)
        {
            var files = GetFiles(GetBibleAssetFolder(version, _fileType), _fileType, bookName);
            var usxPath = files.FirstOrDefault(f => f.EndsWith($"{bookName}.{_fileType}"));
            if (usxPath == null) return null;
            var bibleBook = await _deserializer.DeserializeAsync<UsxBook, BibleBook>(
                usxPath, b => b.ToBibleFormat(), cancellationToken);
            return bibleBook;
        }

        internal static string GetBibleAssetFolder(string version, string? fileType = null, string? basePath = null)
        {
            basePath ??= GetPathAbove("Bible.Data");
            fileType ??= _fileType;
            var subfolder = fileType.StartsWith(".") ? fileType[1..] : fileType;
            var usxPath = Path.Combine(basePath, subfolder);
            return usxPath;
        }

        internal static string GetPathAbove(string project)
        {
            var count = 0;
            while (!Directory.Exists(project))
            {
                if (++count > 4)
                {
                    project = string.Empty;
                    break;
                }
                project = Path.Combine("..", project);
            }

            return project;
        }
    }
}
