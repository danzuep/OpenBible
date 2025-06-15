using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Bible.Backend.Parser
{
    #region Simple POCO classes

    public class Scripture
    {
        public List<Book> Books { get; } = new List<Book>();
    }

    public class Book
    {
        public string Code { get; set; }
        public string Style { get; set; }
        public List<Chapter> Chapters { get; } = new List<Chapter>();
    }

    public class Chapter
    {
        public int Number { get; set; }
        public string Style { get; set; }
        public List<VerseMarker> VerseMarkers { get; } = new List<VerseMarker>();
        public List<Para> Paras { get; } = new List<Para>();
    }

    public class VerseMarker
    {
        public int Number { get; set; }
        public string Style { get; set; }
    }

    public class Para
    {
        public string Style { get; set; }
        public List<object> Content { get; } = new List<object>(); // string or inline elements
    }

    public class Char
    {
        public string Style { get; set; }
        public string OsisID { get; set; }
        public string Text { get; set; }
    }

    public class Note
    {
        public string Caller { get; set; }
        public string Style { get; set; }
        public Char Marker { get; set; }
        public Para Content { get; set; }
    }

    #endregion

    #region USX Parser

    public class UsxPara
    {
        private Scripture scripture = new Scripture();

        private Book currentBook;
        private Chapter currentChapter;
        private Para currentPara;
        private Note currentNote;
        private Char currentChar;

        // Stack to track nested elements inside para/notes
        private Stack<string> elementStack = new Stack<string>();

        // Buffer for accumulating text inside inline elements
        private System.Text.StringBuilder textBuffer = new System.Text.StringBuilder();

        public Scripture Parse(string filePath)
        {
            using XmlReader reader = XmlReader.Create(filePath, new XmlReaderSettings { IgnoreWhitespace = true });

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        StartElement(reader);
                        if (reader.IsEmptyElement)
                            EndElement(reader.Name);
                        break;

                    case XmlNodeType.Text:
                    case XmlNodeType.CDATA:
                        HandleText(reader.Value);
                        break;

                    case XmlNodeType.EndElement:
                        EndElement(reader.Name);
                        break;
                }
            }

            return scripture;
        }

        private void StartElement(XmlReader reader)
        {
            string name = reader.Name;
            elementStack.Push(name);

            switch (name)
            {
                case "book":
                    currentBook = new Book
                    {
                        Code = reader.GetAttribute("code"),
                        Style = reader.GetAttribute("style")
                    };
                    scripture.Books.Add(currentBook);
                    break;

                case "chapter":
                    currentChapter = new Chapter
                    {
                        Style = reader.GetAttribute("style")
                    };
                    if (int.TryParse(reader.GetAttribute("number"), out int chNum))
                        currentChapter.Number = chNum;
                    currentBook.Chapters.Add(currentChapter);
                    break;

                case "verse":
                    var verseMarker = new VerseMarker
                    {
                        Style = reader.GetAttribute("style")
                    };
                    if (int.TryParse(reader.GetAttribute("number"), out int vNum))
                        verseMarker.Number = vNum;
                    currentChapter.VerseMarkers.Add(verseMarker);
                    break;

                case "para":
                    currentPara = new Para
                    {
                        Style = reader.GetAttribute("style")
                    };
                    currentChapter.Paras.Add(currentPara);
                    break;

                case "char":
                    currentChar = new Char
                    {
                        Style = reader.GetAttribute("style"),
                        OsisID = reader.GetAttribute("osisID")
                    };
                    textBuffer.Clear();
                    break;

                case "note":
                    currentNote = new Note
                    {
                        Caller = reader.GetAttribute("caller"),
                        Style = reader.GetAttribute("style")
                    };
                    break;

                case "milestone":
                    // Milestones are often empty elements marking events inside paras
                    // You could extend Para.Content with a Milestone class if needed
                    break;

                case "link":
                    // Could add Link class for hyperlinks if desired
                    break;

                case "w":
                    // Word-level markup, can be handled like Char if needed
                    break;

                default:
                    // Other elements ignored or logged
                    break;
            }
        }

        private void HandleText(string text)
        {
            if (elementStack.Count == 0)
                return;

            string currentElement = elementStack.Peek();

            switch (currentElement)
            {
                case "char":
                    textBuffer.Append(text);
                    break;

                case "para":
                    currentPara.Content.Add(text);
                    break;

                case "note":
                    // Notes typically contain paragraphs, so text usually inside para (handled separately)
                    break;

                case "note-marker":
                case "link":
                case "w":
                    // Extend as needed
                    break;

                default:
                    // If no special element, text inside para or elsewhere
                    if (currentPara != null)
                        currentPara.Content.Add(text);
                    break;
            }
        }

        private void EndElement(string name)
        {
            if (elementStack.Count == 0)
                return;

            string currentElement = elementStack.Pop();

            if (name != currentElement)
                throw new InvalidOperationException($"Mismatched end element: expected {currentElement}, got {name}");

            switch (name)
            {
                case "char":
                    if (currentChar != null)
                    {
                        currentChar.Text = textBuffer.ToString();
                        textBuffer.Clear();

                        // Add char element to currentPara or currentNote content
                        if (currentNote != null && currentNote.Content == null)
                            currentNote.Content = new Para { Style = "footnote" }; // default style

                        if (currentNote != null)
                        {
                            // If Marker not set, assume first char is marker
                            if (currentNote.Marker == null)
                                currentNote.Marker = currentChar;
                            else
                                currentNote.Content.Content.Add(currentChar);
                        }
                        else if (currentPara != null)
                        {
                            currentPara.Content.Add(currentChar);
                        }

                        currentChar = null;
                    }
                    break;

                case "note":
                    // Attach note content to currentPara content
                    if (currentNote != null && currentPara != null)
                    {
                        currentPara.Content.Add(currentNote);
                        currentNote = null;
                    }
                    break;

                case "para":
                    currentPara = null;
                    break;

                case "chapter":
                    currentChapter = null;
                    break;

                case "book":
                    currentBook = null;
                    break;
            }
        }
    }

    #endregion

    #region Example usage
    public static class Program
    {
        public static void Main()
        {
            var parser = new UsxPara();

            // Replace "input.usx" with your USX file path
            Scripture usx = parser.Parse("input.usx");

            // Example: print structure
            foreach (var book in usx.Books)
            {
                Console.WriteLine($"Book {book.Code}");
                foreach (var chapter in book.Chapters)
                {
                    Console.WriteLine($" Chapter {chapter.Number}");
                    foreach (var verse in chapter.VerseMarkers)
                    {
                        Console.WriteLine($"  Verse {verse.Number}");
                    }
                    foreach (var para in chapter.Paras)
                    {
                        Console.WriteLine($"  Para ({para.Style}):");
                        foreach (var content in para.Content)
                        {
                            switch (content)
                            {
                                case string s:
                                    Console.Write(s);
                                    break;
                                case Char c:
                                    Console.Write($"[Char:{c.Text} osisID={c.OsisID}]");
                                    break;
                                case Note n:
                                    Console.Write("[Note:");
                                    if (n.Marker != null)
                                        Console.Write(n.Marker.Text);
                                    if (n.Content != null)
                                        foreach (var c2 in n.Content.Content)
                                            Console.Write(c2 is string cs ? cs : "");
                                    Console.Write("]");
                                    break;
                            }
                        }
                        Console.WriteLine();
                    }
                }
            }
        }
    }
    #endregion
}