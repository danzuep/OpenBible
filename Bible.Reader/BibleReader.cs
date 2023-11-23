using Bible.Core.Models;
using Bible.Reader.Adapters;
using Bible.Reader.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.Xsl;

namespace Bible.Reader
{
    public class BibleReader
    {
        //https://raw.githubusercontent.com/kohelet-net-admin/zefania-xml-bibles/master/Bibles/

        public static BibleModel LoadZefBible(string fileName, string suffix = ".xml")
        {
            var info = fileName.Split('/');
            FileType? fileType = null;
            if (suffix.EndsWith(".usx"))
                fileType = FileType.Usx;
            else if (suffix.EndsWith(".xml"))
                fileType = FileType.Xml;
            else if (suffix.EndsWith(".json"))
                fileType = FileType.Json;
            //Type type = fileName == "zho/OCCB/GEN" ? typeof(XmlUsx3) : typeof(XmlZefania05);
            var bibleFile = GetFromFile<XmlZefania05>($"{fileName}{suffix}", fileType);
            var bible = bibleFile.ToBibleFormat(info[0], info[1]);
            return bible;
        }

        public static BibleModel LoadUsxBible(string fileName)
        {
            var info = fileName.Split('/');
            //var fileType = fileName == "zho/OCCB/GEN" ? FileType.Usx : FileType.Xml;
            var bibleFile = GetFromFile<XmlUsx>(fileName, FileType.Usx);
            var bible = bibleFile.ToBibleFormat(info[0], info[1]);
            return bible;
        }

        /// <summary>
        /// Map file data to an object.
        /// </summary>
        /// <typeparam name="T">Object to map to.</typeparam>
        /// <param name="fileName">File name.</param>
        /// <param name="fileType">File type.</param>
        /// <returns>Mapped object.</returns>
        /// <inheritdoc cref="File.OpenRead"/>
        public static T GetFromFile<T>(string fileName, FileType? fileType) where T : class
        {
            T result = fileType switch
            {
                FileType.Json => GetFromJsonFile<T>(fileName),
                FileType.Xml => GetFromXmlFile<T>(fileName),
                FileType.Usx => GetFromXmlFile<T>(fileName),
                _ => throw new NotImplementedException()
            };
            return result;
        }

        private static readonly string _baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private static string ExpandPath(string fileName, FileType fileType)
        {
            string prefix, suffix;
            var typeName = fileType.ToString().ToLowerInvariant();
            (prefix, suffix) = (typeName, $".{typeName}");
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
            return Path.Combine(_baseDirectory, "..", prefix, fileName);
        }

        /// <inheritdoc cref="GetFromFile{T}"/>
        public static T GetFromUsxFile<T>(string fileName) where T : class
        {
            var filePath = ExpandPath(fileName, FileType.Usx);
            using var fileStream = File.OpenRead(filePath);
            var serializer = new XmlSerializer(typeof(T));
            var result = serializer.Deserialize(fileStream) as T;
            return result;
        }

        private const string _relativeDirectory = "..\\..\\..\\..\\Bible.Data\\";
        private static string ExpandXsltPath(string fileName) =>
            Path.Combine(_baseDirectory, _relativeDirectory, Path.GetExtension(fileName).TrimStart('.'), fileName);
        private static readonly string[] _xsltFiles = new string[] { "usx2xml01verses.xslt", "usx2xml02chapters.xslt" };

        /// <inheritdoc cref="GetFromFile{T}"/>
        public static void TransformUsx2Xml(string inputFile, string outputSuffix = ".xml")
        {
            // Get the output path
            var inputExtension = Path.GetExtension(inputFile).ToCharArray();
            var outputFileName = $"{inputFile.TrimEnd(inputExtension)}{outputSuffix}";
            var outPath = ExpandXsltPath(outputFileName);
            var outDirectory = Path.GetDirectoryName(outPath);

            // Initialise variables
            var xmlDoc = new XmlDocument();
            var inPath = ExpandXsltPath(inputFile);
            var xslt = new XslCompiledTransform();

            foreach (var xsltFile in _xsltFiles)
            {
                // Load the input XML document
                xmlDoc.Load(inPath);

                // Load the XSLT stylesheet
                var xsltPath = ExpandXsltPath(xsltFile);
                xslt.Load(xsltPath);

                // Perform the transformation
                Directory.CreateDirectory(outDirectory);
                using var writer = new XmlTextWriter(outPath, null);
                xslt.Transform(xmlDoc, null, writer);
                inPath = outPath;
            }

            System.Diagnostics.Debug.WriteLine($"File transformed to {outPath}.");
        }

        /// <summary>
        /// Map XML file data to an object.
        /// </summary>
        /// <typeparam name="T">Object to map to.</typeparam>
        /// <param name="xmlFileName">XML file name.</param>
        /// <inheritdoc cref="GetFromFile{T}"/>
        private static T GetFromXmlFile<T>(string xmlFileName) where T : class
        {
            var xmlFilePath = ExpandPath(xmlFileName, FileType.Xml);
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
        private static T GetFromJsonFile<T>(string jsonFileName) where T : class
        {
            var jsonFilePath = ExpandPath(jsonFileName, FileType.Json);
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
