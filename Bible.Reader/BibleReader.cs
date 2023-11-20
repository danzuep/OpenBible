using Bible.Core.Models;
using Bible.Reader.Adapters;
using Bible.Reader.Models;
using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bible.Reader
{
    public class BibleReader
    {
        public static BibleModel LoadBible<T>(string fileName, string suffix = ".zefania.xml") where T : class
        {
            FileType? fileType = null;
            if (suffix.EndsWith(".xml"))
                fileType = FileType.Xml;
            else if (suffix.EndsWith(".json"))
                fileType = FileType.Json;
            var bibleFile = GetFromFile<XmlZefania>($"{fileName}{suffix}", fileType);
            var info = fileName.Split('-');
            var bible = bibleFile.ToBibleFormat(info[0], info[1]);
            return bible;
        }

        /// <summary>
        /// Map file data to an object.
        /// </summary>
        /// <typeparam name="T">Object to map to.</typeparam>
        /// <param name="filePath">File path.</param>
        /// <param name="fileType">File type.</param>
        /// <returns>Mapped object.</returns>
        /// <inheritdoc cref="File.OpenRead"/>
        public static T GetFromFile<T>(string filePath, FileType? fileType) where T : class
        {
            T result = fileType switch
            {
                FileType.Json => GetFromJsonFile<T>(filePath),
                FileType.Xml => GetFromXmlFile<T>(filePath),
                _ => throw new NotImplementedException()
            };
            return result;
        }

        private static readonly string _baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private static string ExpandPath(string fileName, string prefix, string suffix)
        {
            if (!Path.HasExtension(fileName) || !fileName.EndsWith(suffix))
                fileName += suffix;
            if (fileName.StartsWith(_baseDirectory[..15]))
            {
                var directory = Path.GetDirectoryName(fileName);
                if (directory.EndsWith(prefix))
                {
                    return fileName;
                }
                var name = Path.GetFileName(fileName);
                return Path.Combine(directory, prefix, name);
            }
            return Path.Combine(_baseDirectory, prefix, fileName);
        }

        /// <summary>
        /// Map XML file data to an object.
        /// </summary>
        /// <typeparam name="T">Object to map to.</typeparam>
        /// <param name="xmlFileName">XML file name.</param>
        /// <inheritdoc cref="GetFromFile{T}"/>
        private static T GetFromXmlFile<T>(string xmlFileName, string prefix = "Xml", string suffix = ".xml") where T : class
        {
            var xmlFilePath = ExpandPath(xmlFileName, prefix, suffix);
            using var fileStream = File.OpenRead(xmlFilePath);
            var serializer = new XmlSerializer(typeof(T));
            var result = serializer.Deserialize(fileStream) as T;
            return result;
        }

        /// <summary>
        /// Map JSON file data to an object.
        /// </summary>
        /// <typeparam name="T">Object to map to.</typeparam>
        /// <param name="jsonFileName">JSON file name.</param>
        /// <inheritdoc cref="GetFromFile{T}"/>
        private static T GetFromJsonFile<T>(string jsonFileName, string prefix = "Json", string suffix = ".json") where T : class
        {
            var jsonFilePath = ExpandPath(jsonFileName, prefix, suffix);
            using var utf8Json = File.OpenRead(jsonFilePath);
            var result = JsonSerializer.Deserialize<T>(utf8Json);
            return result;
        }

        /// <summary>
        /// Map JSON file data to an object.
        /// </summary>
        /// <typeparam name="T">Object to map to.</typeparam>
        /// <param name="jsonFilePath">JSON file path.</param>
        /// <param name="options">Options to control the behavior during reading.</param>
        /// <param name="cancellationToken">
        /// The <see cref="CancellationToken"/> that can be used to cancel the read operation.
        /// </param>
        /// <inheritdoc cref="GetFromFile{T}"/>
        /// <inheritdoc cref="JsonSerializer.DeserializeAsync{T}"/>
        public static async Task<T> GetFromJsonFileAsync<T>(string jsonFilePath, JsonSerializerOptions options = null, CancellationToken cancellationToken = default)
        {
            using var utf8Json = File.OpenRead(jsonFilePath);
            var result = await JsonSerializer.DeserializeAsync<T>(utf8Json, options, cancellationToken);
            return result;
        }
    }
}
