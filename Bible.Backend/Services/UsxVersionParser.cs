using Bible.Backend.Abstractions;

namespace Bible.Backend.Services
{
    public class UsxVersionParser : ParsingStrategy, IBulkParser
    {
        private readonly IDeserializer _deserializer;

        public UsxVersionParser(IDeserializer? deserializer = null)
        {
            _deserializer = deserializer ?? new XDocDeserializer();
        }

        private IEnumerable<T> Deserialize<T>(IEnumerable<string>? files)
        {
            if (files == null)
            {
                yield break;
            }

            foreach (var file in files)
            {
                var deserialized = _deserializer.Deserialize<T>(file);
                if (deserialized != null)
                {
                    yield return deserialized;
                }
            }
        }

        public IEnumerable<T> Enumerate<T>(string path, string fileType = "usx")
        {
            var files = GetFiles(path, fileType);
            return Deserialize<T>(files);
        }

        public IEnumerable<KeyValuePair<string, IEnumerable<T>>> EnumerateAll<T>(string path, string fileType = "usx")
        {
            var folders = Directory.EnumerateDirectories(path, $"*{fileType}*", SearchOption.AllDirectories);
            if (folders == null)
            {
                yield break;
            }
            foreach (var folder in folders)
            {
                var versions = Directory.EnumerateFiles(folder, $"*.{fileType}");
                if (versions == null)
                {
                    continue;
                }
                yield return new(folder, Deserialize<T>(versions));
            }
        }
    }
}