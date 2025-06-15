namespace Bible.Data.Usx;

using System;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Xml.Parser;
using AngleSharp.XPath;

class UsxXPathExample
{
    public static async Task Main()
    {
        // Sample USX XML
        var usx = @"
<usx version='3'>
  <book code='GEN' style='book'>Genesis</book>
  <chapter number='1' style='chapter'/>
  <para style='p'>
    <verse number='1' style='v'/>
    In the beginning God created the heaven and the earth.
  </para>
</usx>";

        // Configure AngleSharp for XML parsing with XPath support
        var config = Configuration.Default
            .WithXml()          // XML parser
            .WithXPath();       // XPath extension

        var context = BrowsingContext.New(config);
        var parser = context.GetService<IXmlParser>();

        // Parse USX XML into document
        var document = await parser.ParseDocumentAsync(usx);

        // Use XPath to locate the <chapter> element with number=1
        var chapter = document.Body.SelectSingleNode("//chapter[@number='1']");

        if (chapter == null)
        {
            Console.WriteLine("Chapter 1 not found!");
            return;
        }

        // Create a new <para> element with a verse and text
        var newPara = document.CreateElement("para");
        newPara.SetAttribute("style", "p");

        var newVerse = document.CreateElement("verse");
        newVerse.SetAttribute("number", "2");
        newVerse.SetAttribute("style", "v");

        newPara.AppendChild(newVerse);
        newPara.AppendChild(document.CreateTextNode("And the earth was without form, and void; and darkness was upon the face of the deep."));

        // Insert the new paragraph after the chapter element
        chapter.Parent.InsertBefore(newPara, chapter.NextSibling);

        // Serialize DOM back to string
        var updatedUsx = document.DocumentElement.OuterHtml;

        Console.WriteLine("Modified USX XML:\n");
        Console.WriteLine(updatedUsx);
    }
}