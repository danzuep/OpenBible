//namespace Bible.Data.Usx.Converter;

//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Text;
//using System.Text.Json;
//using System.Xml;

//class UsxToUsjConverter
//{
//    // Define classes to represent the USJ structure (simplified example)
//    public class Usj
//    {
//        public string Version { get; set; }
//        public Book Book { get; set; }
//        public List<Para> Paras { get; set; } = new();
//        public List<Chapter> Chapters { get; set; } = new();
//    }

//    public class Book
//    {
//        public string Code { get; set; }
//        public string Style { get; set; }
//        public string Title { get; set; }
//    }

//    public class Para
//    {
//        public string Style { get; set; }
//        public string Vid { get; set; }
//        public List<object> Content { get; set; } = new();
//        // Content can be strings or Verse or Char objects
//    }

//    public class Verse
//    {
//        public string Number { get; set; }
//        public string Style { get; set; }
//        public string Sid { get; set; }
//        public string Eid { get; set; }
//    }

//    public class Char
//    {
//        public string Style { get; set; }
//        public string Strong { get; set; }
//        public string Text { get; set; }
//    }

//    public class Chapter
//    {
//        public string Number { get; set; }
//        public string Style { get; set; }
//        public string Sid { get; set; }
//        public string Eid { get; set; }
//    }

//    public static Usj ParseUsx(string xmlFilePath)
//    {
//        var settings = new XmlReaderSettings
//        {
//            IgnoreWhitespace = false, // Preserve whitespace inside para
//            DtdProcessing = DtdProcessing.Ignore
//        };

//        Usj usj = new Usj();

//        using (var reader = XmlReader.Create(xmlFilePath, settings))
//        {
//            Para currentPara = null;

//            while (reader.Read())
//            {
//                if (reader.NodeType == XmlNodeType.Element)
//                {
//                    switch (reader.Name)
//                    {
//                        case "usx":
//                            usj.Version = reader.GetAttribute("version");
//                            break;

//                        case "book":
//                            usj.Book = new Book
//                            {
//                                Code = reader.GetAttribute("code"),
//                                Style = reader.GetAttribute("style"),
//                                Title = reader.ReadElementContentAsString()
//                            };
//                            break;

//                        case "chapter":
//                            var chapter = new Chapter
//                            {
//                                Number = reader.GetAttribute("number"),
//                                Style = reader.GetAttribute("style"),
//                                Sid = reader.GetAttribute("sid"),
//                                Eid = reader.GetAttribute("eid")
//                            };
//                            usj.Chapters.Add(chapter);
//                            if (!reader.IsEmptyElement)
//                                reader.Read(); // advance past chapter element content if any
//                            break;

//                        case "para":
//                            currentPara = new Para
//                            {
//                                Style = reader.GetAttribute("style"),
//                                Vid = reader.GetAttribute("vid")
//                            };
//                            usj.Paras.Add(currentPara);
//                            if (reader.IsEmptyElement)
//                            {
//                                currentPara = null;
//                            }
//                            break;

//                        case "verse":
//                            if (currentPara != null)
//                            {
//                                var verse = new Verse
//                                {
//                                    Number = reader.GetAttribute("number"),
//                                    Style = reader.GetAttribute("style"),
//                                    Sid = reader.GetAttribute("sid"),
//                                    Eid = reader.GetAttribute("eid")
//                                };
//                                currentPara.Content.Add(verse);
//                                if (reader.IsEmptyElement)
//                                {
//                                    // empty verse tag, no inner text
//                                }
//                                else
//                                {
//                                    reader.Read(); // move inside verse if any content
//                                }
//                            }
//                            break;

//                        case "char":
//                            if (currentPara != null)
//                            {
//                                var strong = reader.GetAttribute("strong");
//                                var style = reader.GetAttribute("style");

//                                // Read inner text of <char>
//                                string text = reader.ReadElementContentAsString();

//                                var ch = new Char
//                                {
//                                    Style = style,
//                                    Strong = strong,
//                                    Text = text
//                                };
//                                currentPara.Content.Add(ch);
//                            }
//                            break;
//                    }
//                }
//                else if (reader.NodeType == XmlNodeType.Text || reader.NodeType == XmlNodeType.Whitespace || reader.NodeType == XmlNodeType.SignificantWhitespace)
//                {
//                    if (currentPara != null)
//                    {
//                        // Preserve spaces inside para text nodes
//                        currentPara.Content.Add(reader.Value);
//                    }
//                }
//                else if (reader.NodeType == XmlNodeType.EndElement)
//                {
//                    if (reader.Name == "para")
//                    {
//                        currentPara = null;
//                    }
//                }
//            }
//        }

//        return usj;
//    }

//    static void Main()
//    {
//        string inputPath = "3jn.usx.xml"; // your input USX xml file
//        string outputPath = "3jn.usj.json";

//        Usj usj = ParseUsx(inputPath);

//        var options = new JsonSerializerOptions
//        {
//            WriteIndented = true,
//            // You can customize converters if needed to serialize polymorphic objects in Para.Content
//        };

//        string json = JsonSerializer.Serialize(usj, options);

//        File.WriteAllText(outputPath, json);

//        Console.WriteLine($"Converted USX to USJ JSON saved to {outputPath}");
//    }
//}