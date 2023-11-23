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
            var bibleFile = GetFromFile<XmlUsx3>(fileName, FileType.Usx);
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
                FileType.Usx => GetFromUsxFile<T>(fileName),
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
        private static T GetFromUsxFile<T>(string fileName) where T : class
        {
            var filePath = ExpandPath(fileName, FileType.Usx);
            using var fileStream = File.OpenRead(filePath);
            var serializer = new XmlSerializer(typeof(T));
            var result = serializer.Deserialize(fileStream) as T;
            return result;
        }

        /// <inheritdoc cref="GetFromFile{T}"/>
        public static async Task<BibleChapter> GetFromUsxFileAsync(string fileName, string bookId = "GEN", string chapterNumber = "1")
        {
            var verses = new List<BibleVerse>();
            var filePath = ExpandPath(fileName, FileType.Usx);
            using var xmlStream = File.OpenRead(filePath);
            var doc = await XDocument.LoadAsync(xmlStream, LoadOptions.None, CancellationToken.None);
            //var bookElement = doc.Descendants("book")
            //    .FirstOrDefault(e => e.Attribute("code")?.Value == bookId);
            var chapterElement = doc.Descendants("chapter")
                .FirstOrDefault(c => c.Attribute("style")?.Value == "c" &&
                    c.Attribute("number")?.Value == chapterNumber);
            var chapterReference = new BibleReference
            {
                Translation = "OCCB",
                BookName = chapterElement.Attribute("sid").Value
            };
            var paragraphs = chapterElement?.ElementsAfterSelf()
                .Where(p => p.Name == "para" && p.Attribute("style")?.Value == "p");
            foreach (var paragraph in paragraphs)
            {
                //var verse = paragraph.Descendants("verse").FirstOrDefault(v => v.Attribute("number")?.Value == verseNum && v.Attribute("style")?.Value == "v");
                //var verseElements = paragraph.Descendants("verse").ToList();
                var paragraphNodes = paragraph.DescendantNodes(); //.ToList();

                bool isVerseNode = false;
                XElement usxClosingTag = null;
                var verse = new BibleVerse();
                foreach (var node in paragraphNodes)
                {
                    if (node is XElement verseElement)
                    {
                        if (verseElement.Name == "verse")
                        {
                            isVerseNode = true;
                            usxClosingTag = paragraphNodes.OfType<XElement>()
                                .LastOrDefault(d => d.NodeType == XmlNodeType.EndElement);
                            if (verseElement.Attribute("style")?.Value == "v" &&
                                verseElement.Attribute("sid") != null &&
                                int.TryParse(verseElement.Attribute("number")?.Value, out int verseNumber))
                            {
                                verse.Number = verseNumber;
                                verse.Reference = chapterReference;
                            }
                            else if (verseElement.Attribute("eid") != null)
                            {
                                verses.Add(verse);
                                verse = new BibleVerse();
                            }
                            else
                            {
                                System.Diagnostics.Debugger.Break();
                            }
                        }
                        else
                        {
                            // metadata like notes etc.
                            //isVerseNode = verseElement == usxClosingTag;
                            //if (verseElement.Name.LocalName.StartsWith("</"))
                            //    System.Diagnostics.Debugger.Break();
                        }
                    }
                    else if (isVerseNode && node is XText textNode && string.IsNullOrEmpty(verse.Text))
                    {
                        verse.Text = textNode.Value;
                        isVerseNode = false;
                        // TODO - filter out metadata like notes etc. then add subsequent text, c.f. GEN 1:5
                    }
                    else
                    {
                        // metadata contents
                    }
                }
                break;
            }
            var chapter = new BibleChapter { Verses = verses };
            //var book = new BibleBook { Chapters = new List<BibleChapter> { chapter } };
            return chapter;
        }

        private const string _relativeDirectory = "..\\..\\..\\..\\Bible.Data\\";
        private static string ExpandXsltPath(string fileName) =>
            Path.Combine(_baseDirectory, _relativeDirectory, Path.GetExtension(fileName).TrimStart('.'), fileName);

        /// <inheritdoc cref="GetFromFile{T}"/>
        public static void TransformUsx2Xml(string inputFile, string xsltFile = "usx2xml.xslt", string outputSuffix = ".xml")
        {
            // Load the input XML document
            var xmlDoc = new XmlDocument();
            var inPath = ExpandXsltPath(inputFile);
            xmlDoc.Load(inPath);

            // Load the XSLT stylesheet
            var xslt = new XslCompiledTransform();
            var xsltPath = ExpandXsltPath(xsltFile);
            xslt.Load(xsltPath);

            // Perform the transformation
            var inputExtension = Path.GetExtension(inputFile).ToCharArray();
            var outputFileName = $"{inputFile.TrimEnd(inputExtension)}{outputSuffix}";
            var outPath = ExpandXsltPath(outputFileName);
            var outDirectory = Path.GetDirectoryName(outPath);
            Directory.CreateDirectory(outDirectory);
            xslt.Transform(xmlDoc, null, new XmlTextWriter(outPath, null));

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
