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

        public IEnumerable<T> Deserialize<T>(string path, string fileType = "usx")
        {
            var files = GetFiles(path, fileType);
            return Deserialize<T>(files);
        }

        public IEnumerable<KeyValuePair<string, IEnumerable<T>>> DeserializeAll<T>(string path, string fileType = "usx")
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

    public enum BibleFileType
    {
        Usx1,
        Usx2,
        Usx3,
        Usx4
    }
}