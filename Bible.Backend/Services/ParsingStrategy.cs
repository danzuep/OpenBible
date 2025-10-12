using System.Globalization;

namespace Bible.Backend.Services
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

        internal static IEnumerable<string> GetFiles(string path, string fileType, string filter = "*")
        {
            var folder = GetLatestVersionFolder(path, $"*{fileType}*");
            if (string.IsNullOrEmpty(folder))
            {
                return Directory.EnumerateFiles(path, $"{filter}.{fileType}");
            }
            return Directory.EnumerateFiles(folder, $"{filter}.{fileType}");
        }
    }
}