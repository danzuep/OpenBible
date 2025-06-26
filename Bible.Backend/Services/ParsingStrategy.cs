using System.Globalization;
using Bible.Backend.Abstractions;
using Bible.Backend.Services;

namespace Bible.Backend
{
    public abstract class ParsingStrategy
    {
        private static int GetVersion(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return -1;
            }
            var splitName = name.Split('_').Last();
            if (int.TryParse(splitName, out var version))
            {
                return version;
            }
            foreach (var letter in name)
            {
                if (char.IsDigit(letter))
                {
                    return CharUnicodeInfo.GetDecimalDigitValue(letter);
                }
            }
            return -1;
        }

        private static string? GetLatestVersionFolder(string path, string searchPattern)
        {
            var folders = Directory.EnumerateDirectories(path, searchPattern, SearchOption.AllDirectories);
            var sorted = folders?.OrderByDescending(GetVersion);
            return sorted?.FirstOrDefault();
        }

        protected static IEnumerable<string> GetFiles(string path, string fileType)
        {
            var folder = GetLatestVersionFolder(path, $"*{fileType}*");
            if (string.IsNullOrEmpty(folder))
            {
                return Array.Empty<string>();
            }
            return Directory.EnumerateFiles(folder, $"*.{fileType}");
        }
    }

    public class UsxParser : ParsingStrategy
    {
        private readonly IDeserialize _deserializer;

        public UsxParser(IDeserialize? deserializer = null)
        {
            _deserializer = deserializer ?? new XDocParser();
        }

        public IEnumerable<T> Deserialize<T>(string path) where T : class
        {
            var files = GetFiles(path, "usx");
            foreach (var file in files)
            {
                var deserialized = _deserializer.Deserialize<T>(file);
                if (deserialized != null)
                {
                    yield return deserialized;
                }
            }
        }
    }

    public enum BibleFileType
    {
        Usx1,
        Usx2,
        Usx3,
        Usx4
    }
}