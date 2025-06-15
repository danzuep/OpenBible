using System.Globalization;
using System.IO;

namespace Bible.Backend
{
    public abstract class ParsingStrategy
    {
        protected static string? FindFile(string folderPath, string fileToFind = "1CO.")
        {
            if (!Directory.Exists(folderPath))
            {
                return null;
            }
            var files = Directory.EnumerateFiles(folderPath);
            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                if (fileName.StartsWith(fileToFind, StringComparison.OrdinalIgnoreCase))
                {
                    return file;
                }
            }
            return null;
        }

        protected static IEnumerable<string> FindFolders(string folderPath, string searchPattern = "*.usx")
        {
            if (!Directory.Exists(folderPath))
            {
                yield break;
            }
            var folders = new List<string>();
            var files = Directory.EnumerateFiles(folderPath, searchPattern);
            foreach (var file in files)
            {
                var folderName = Path.GetDirectoryName(file);
                if (!string.IsNullOrEmpty(folderName) && !folders.Contains(folderName))
                {
                    folders.Add(folderName);
                    yield return folderName;
                }
            }
        }

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

        private static IReadOnlyList<BibleFileType> _usxFileTypes =
        [
            BibleFileType.Usx1, BibleFileType.Usx2, BibleFileType.Usx3, BibleFileType.Usx4
        ];

        protected IReadOnlyList<BibleFileType> GetTypes(string? filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return Array.Empty<BibleFileType>();
            }
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension switch
            {
                ".usx" => _usxFileTypes,
                _ => Array.Empty<BibleFileType>(),
            };
        }

        protected bool CanHandle(string? fileExtension, params string[] validExtensions)
        {
            if (string.IsNullOrEmpty(fileExtension) || validExtensions == null)
            {
                return false;
            }
            return validExtensions.Contains(fileExtension);
        }
    }

    public class UsxParser : ParsingStrategy
    {
        private readonly IDeserialize _deserializer;

        public UsxParser(IDeserialize? deserializer = null)
        {
            _deserializer = deserializer ?? new XmlParser();
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
        //public IEnumerable<T> Deserialize<T>(string folderPath) where T : class
        //{
        //    var filePath = FindFile(folderPath);
        //    var folder = Path.GetDirectoryName(folderPath);
        //    if (!string.IsNullOrEmpty(folder) &&
        //        //GetTypes(filePath).Contains(BibleFileType.Usx4) &&
        //        CanHandle(Path.GetExtension(filePath), ".usx"))
        //    {
        //        var deserializer = new XmlParser();
        //        var files = Directory.EnumerateFiles(folder);
        //        foreach (var file in files)
        //        {
        //            var deserialized = deserializer.Deserialize<T>(file);
        //            if (deserialized != null)
        //            {
        //                yield return deserialized;
        //            }
        //        }
        //    }
        //}
    }

    public enum BibleFileType
    {
        Usx1,
        Usx2,
        Usx3,
        Usx4
    }
}