namespace Bible.Console;

using System;
using System.Threading.Tasks;
using Bible.Backend;
using Bible.Backend.Adapters;
using Bible.Backend.Models;
using Bible.Backend.Services;
using Microsoft.Extensions.Logging;

public class Program
{
    public static async Task Main()
    {
        //Example1();
        Example2();

        Console.WriteLine();
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    private static void Example1()
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
            builder.AddDebug().AddConsole());
        var jsonLogger = loggerFactory.CreateLogger<JsonParser>();
        var jsonParser = new JsonParser(jsonLogger);
        var xmlLogger = loggerFactory.CreateLogger<XmlParser>();
        var xmlParser = new XmlParser(xmlLogger);
        var downloadLogger = loggerFactory.CreateLogger<DownloadExtractor>();
        var downloadExtractor = new DownloadExtractor(null, downloadLogger);
        var usxParser = new UsxParser(xmlParser);

        string xmlFilePath = "release/USX_4/JUD.usx"; // Path to your USX XML file
        string jsonOutputPath = "JUD.json"; // Path to save the JSON output
        var usxFileName = "7142879509583d59-rev146-2025-06-08-release";
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var usxFilePath = Path.Combine(userProfile, "Downloads", usxFileName);
        var files = usxParser.Deserialize<UsxScriptureBook>(usxFilePath).ToList();
        var jude = files.Single(f => f.Translation.BookCode == "JUD");
        var p = jude.Content.Where(b => b.Style == "p");
        var ignore = new string[] { "f" };
        var content = p.Where(c => !ignore.Contains(c.Style)).ToList();
        var text = content.Select(c => string.Join("", c)).ToList();

        //await downloadExtractor.DownloadExtractAsync(xmlFilePath, jsonOutputPath);
        //var jsonData = jsonParser.Deserialize<Usx>(jsonOutputPath);
        //var xmlData = xmlParser.Deserialize<Usx>(xmlFilePath);
    }

    private static void Example2()
    {
        var usx = @"<?xml version=""1.0"" encoding=""utf-8""?>
<usx version=""3.0"">
  <book code=""3JN"" style=""id"">64-3JN-web.sfm World English Bible British Edition (WEBBE)</book>
  <para style=""ide"">UTF-8</para>
  <para style=""h"">3 John</para>
  <para style=""toc1"">John’s Third Letter</para>
  <para style=""toc2"">3 John</para>
  <para style=""toc3"">3Jn</para>
  <para style=""mt1"">John’s Third Letter</para>
  <chapter number=""1"" style=""c"" sid=""3JN 1"" />
  <para style=""p"">
    <verse number=""1"" style=""v"" sid=""3JN 1:1"" />
    <char style=""w"" strong=""G1722"">The</char> <char style=""w"" strong=""G4245"">elder</char> <char style=""w"" strong=""G1722"">to</char> <char style=""w"" strong=""G1050"">Gaius</char> <char style=""w"" strong=""G1722"">the</char> beloved, <char style=""w"" strong=""G3739"">whom</char> <char style=""w"" strong=""G1473"">I</char> love <char style=""w"" strong=""G1722"">in</char> truth.<verse eid=""3JN 1:1"" /></para>
  <para style=""p"">
    <verse number=""2"" style=""v"" sid=""3JN 1:2"" />Beloved, <char style=""w"" strong=""G2532"">I</char> <char style=""w"" strong=""G2172"">pray</char> <char style=""w"" strong=""G3588"">that</char> <char style=""w"" strong=""G4771"">you</char> <char style=""w"" strong=""G2532"">may</char> <char style=""w"" strong=""G2137"">prosper</char> <char style=""w"" strong=""G2532"">in</char> <char style=""w"" strong=""G3956"">all</char> <char style=""w"" strong=""G3956"">things</char> <char style=""w"" strong=""G2532"">and</char> <char style=""w"" strong=""G2532"">be</char> healthy, <char style=""w"" strong=""G2532"">even</char> <char style=""w"" strong=""G2531"">as</char> <char style=""w"" strong=""G2532"">your</char> <char style=""w"" strong=""G5590"">soul</char> <char style=""w"" strong=""G2137"">prospers</char>. <verse eid=""3JN 1:2"" /><verse number=""3"" style=""v"" sid=""3JN 1:3"" /><char style=""w"" strong=""G1063"">For</char> <char style=""w"" strong=""G2532"">I</char> <char style=""w"" strong=""G5463"">rejoiced</char> <char style=""w"" strong=""G3029"">greatly</char> <char style=""w"" strong=""G2532"">when</char> brothers <char style=""w"" strong=""G2064"">came</char> <char style=""w"" strong=""G2532"">and</char> <char style=""w"" strong=""G3140"">testified</char> <char style=""w"" strong=""G1722"">about</char> <char style=""w"" strong=""G2532"">your</char> truth, <char style=""w"" strong=""G2532"">even</char> <char style=""w"" strong=""G2531"">as</char> <char style=""w"" strong=""G4771"">you</char> <char style=""w"" strong=""G4043"">walk</char> <char style=""w"" strong=""G1722"">in</char> truth. <verse eid=""3JN 1:3"" /><verse number=""4"" style=""v"" sid=""3JN 1:4"" /><char style=""w"" strong=""G3778"">I</char> <char style=""w"" strong=""G2192"">have</char> <char style=""w"" strong=""G3756"">no</char> <char style=""w"" strong=""G3173"">greater</char> <char style=""w"" strong=""G5479"">joy</char> <char style=""w"" strong=""G3173"">than</char> <char style=""w"" strong=""G3778"">this</char>: <char style=""w"" strong=""G2443"">to</char> hear <char style=""w"" strong=""G1722"">about</char> <char style=""w"" strong=""G1699"">my</char> <char style=""w"" strong=""G5043"">children</char> <char style=""w"" strong=""G4043"">walking</char> <char style=""w"" strong=""G1722"">in</char> truth.<verse eid=""3JN 1:4"" /></para>
  <para style=""p"">
    <verse number=""5"" style=""v"" sid=""3JN 1:5"" />Beloved, <char style=""w"" strong=""G3739"">you</char> <char style=""w"" strong=""G4160"">do</char> <char style=""w"" strong=""G2532"">a</char> <char style=""w"" strong=""G4103"">faithful</char> <char style=""w"" strong=""G2038"">work</char> <char style=""w"" strong=""G1519"">in</char> <char style=""w"" strong=""G3739"">whatever</char> <char style=""w"" strong=""G3739"">you</char> <char style=""w"" strong=""G2038"">accomplish</char> <char style=""w"" strong=""G1519"">for</char> <char style=""w"" strong=""G3588"">those</char> <char style=""w"" strong=""G3739"">who</char> <char style=""w"" strong=""G3588"">are</char> brothers <char style=""w"" strong=""G2532"">and</char> <char style=""w"" strong=""G3581"">strangers</char>. <verse eid=""3JN 1:5"" /><verse number=""6"" style=""v"" sid=""3JN 1:6"" /><char style=""w"" strong=""G3588"">They</char> <char style=""w"" strong=""G4160"">have</char> <char style=""w"" strong=""G3140"">testified</char> <char style=""w"" strong=""G3588"">about</char> <char style=""w"" strong=""G4160"">your</char> love <char style=""w"" strong=""G1799"">before</char> <char style=""w"" strong=""G3588"">the</char> <char style=""w"" strong=""G1577"">assembly</char>. <char style=""w"" strong=""G4771"">You</char> <char style=""w"" strong=""G2316"">will</char> <char style=""w"" strong=""G4160"">do</char> <char style=""w"" strong=""G2573"">well</char> <char style=""w"" strong=""G4160"">to</char> <char style=""w"" strong=""G4311"">send</char> <char style=""w"" strong=""G3588"">them</char> forward <char style=""w"" strong=""G4311"">on</char> <char style=""w"" strong=""G3588"">their</char> <char style=""w"" strong=""G4311"">journey</char> <char style=""w"" strong=""G2316"">in</char> <char style=""w"" strong=""G4160"">a</char> <char style=""w"" strong=""G4311"">way</char> worthy <char style=""w"" strong=""G2316"">of</char> <char style=""w"" strong=""G2316"">God</char>, <verse eid=""3JN 1:6"" /><verse number=""7"" style=""v"" sid=""3JN 1:7"" /><char style=""w"" strong=""G1063"">because</char> <char style=""w"" strong=""G1063"">for</char> <char style=""w"" strong=""G3588"">the</char> <char style=""w"" strong=""G5228"">sake</char> <char style=""w"" strong=""G3686"">of</char> <char style=""w"" strong=""G3588"">the</char> <char style=""w"" strong=""G3686"">Name</char> <char style=""w"" strong=""G3588"">they</char> <char style=""w"" strong=""G1831"">went</char> <char style=""w"" strong=""G1831"">out</char>, <char style=""w"" strong=""G2983"">taking</char> <char style=""w"" strong=""G3367"">nothing</char> <char style=""w"" strong=""G3588"">from</char> <char style=""w"" strong=""G3588"">the</char> <char style=""w"" strong=""G1482"">Gentiles</char>. <verse eid=""3JN 1:7"" /><verse number=""8"" style=""v"" sid=""3JN 1:8"" /><char style=""w"" strong=""G2249"">We</char> <char style=""w"" strong=""G3767"">therefore</char> <char style=""w"" strong=""G3784"">ought</char> <char style=""w"" strong=""G2443"">to</char> receive <char style=""w"" strong=""G5108"">such</char>, <char style=""w"" strong=""G2443"">that</char> <char style=""w"" strong=""G2249"">we</char> <char style=""w"" strong=""G2443"">may</char> <char style=""w"" strong=""G1096"">be</char> <char style=""w"" strong=""G4904"">fellow</char> <char style=""w"" strong=""G4904"">workers</char> <char style=""w"" strong=""G2443"">for</char> <char style=""w"" strong=""G3588"">the</char> truth.<verse eid=""3JN 1:8"" /></para>
  <para style=""p"">
    <verse number=""9"" style=""v"" sid=""3JN 1:9"" />
    <char style=""w"" strong=""G1473"">I</char> <char style=""w"" strong=""G1125"">wrote</char> <char style=""w"" strong=""G3756"">to</char> <char style=""w"" strong=""G3588"">the</char> <char style=""w"" strong=""G1577"">assembly</char>, <char style=""w"" strong=""G3588"">but</char> <char style=""w"" strong=""G1361"">Diotrephes</char>, <char style=""w"" strong=""G3588"">who</char> <char style=""w"" strong=""G5383"">loves</char> <char style=""w"" strong=""G3756"">to</char> <char style=""w"" strong=""G3756"">be</char> <char style=""w"" strong=""G3588"">first</char> amongst <char style=""w"" strong=""G3588"">them</char>, doesn’<char style=""w"" strong=""G3588"">t</char> <char style=""w"" strong=""G1926"">accept</char> <char style=""w"" strong=""G3588"">what</char> <char style=""w"" strong=""G2249"">we</char> <char style=""w"" strong=""G1473"">say</char>. <verse eid=""3JN 1:9"" /><verse number=""10"" style=""v"" sid=""3JN 1:10"" /><char style=""w"" strong=""G1223"">Therefore</char>, <char style=""w"" strong=""G1437"">if</char> <char style=""w"" strong=""G1473"">I</char> <char style=""w"" strong=""G2064"">come</char>, <char style=""w"" strong=""G1473"">I</char> <char style=""w"" strong=""G2532"">will</char> <char style=""w"" strong=""G2532"">call</char> <char style=""w"" strong=""G5279"">attention</char> <char style=""w"" strong=""G2532"">to</char> <char style=""w"" strong=""G1223"">his</char> <char style=""w"" strong=""G2041"">deeds</char> <char style=""w"" strong=""G3739"">which</char> <char style=""w"" strong=""G2532"">he</char> <char style=""w"" strong=""G4160"">does</char>, <char style=""w"" strong=""G5396"">unjustly</char> <char style=""w"" strong=""G5396"">accusing</char> <char style=""w"" strong=""G4160"">us</char> <char style=""w"" strong=""G1223"">with</char> <char style=""w"" strong=""G4190"">wicked</char> <char style=""w"" strong=""G3056"">words</char>. <char style=""w"" strong=""G3361"">Not</char> <char style=""w"" strong=""G4160"">content</char> <char style=""w"" strong=""G1223"">with</char> <char style=""w"" strong=""G3778"">this</char>, <char style=""w"" strong=""G2532"">he</char> doesn’<char style=""w"" strong=""G3588"">t</char> <char style=""w"" strong=""G1926"">receive</char> <char style=""w"" strong=""G2532"">the</char> brothers himself, <char style=""w"" strong=""G2532"">and</char> <char style=""w"" strong=""G3588"">those</char> <char style=""w"" strong=""G3739"">who</char> <char style=""w"" strong=""G1014"">would</char>, <char style=""w"" strong=""G2532"">he</char> <char style=""w"" strong=""G2967"">forbids</char> <char style=""w"" strong=""G2532"">and</char> <char style=""w"" strong=""G1544"">throws</char> <char style=""w"" strong=""G1544"">out</char> <char style=""w"" strong=""G3056"">of</char> <char style=""w"" strong=""G2532"">the</char> <char style=""w"" strong=""G1577"">assembly</char>.<verse eid=""3JN 1:10"" /></para>
  <para style=""p"">
    <verse number=""11"" style=""v"" sid=""3JN 1:11"" />Beloved, don’<char style=""w"" strong=""G3588"">t</char> <char style=""w"" strong=""G3401"">imitate</char> <char style=""w"" strong=""G3588"">that</char> <char style=""w"" strong=""G3588"">which</char> <char style=""w"" strong=""G1510"">is</char> <char style=""w"" strong=""G2556"">evil</char>, <char style=""w"" strong=""G3361"">but</char> <char style=""w"" strong=""G3588"">that</char> <char style=""w"" strong=""G3588"">which</char> <char style=""w"" strong=""G1510"">is</char> <char style=""w"" strong=""G3588"">good</char>. <char style=""w"" strong=""G3588"">He</char> <char style=""w"" strong=""G3588"">who</char> <char style=""w"" strong=""G1510"">does</char> <char style=""w"" strong=""G3588"">good</char> <char style=""w"" strong=""G1510"">is</char> <char style=""w"" strong=""G1537"">of</char> <char style=""w"" strong=""G2316"">God</char>. <char style=""w"" strong=""G3588"">He</char> <char style=""w"" strong=""G3588"">who</char> <char style=""w"" strong=""G1510"">does</char> <char style=""w"" strong=""G2556"">evil</char> hasn’<char style=""w"" strong=""G3588"">t</char> <char style=""w"" strong=""G3708"">seen</char> <char style=""w"" strong=""G2316"">God</char>. <verse eid=""3JN 1:11"" /><verse number=""12"" style=""v"" sid=""3JN 1:12"" /><char style=""w"" strong=""G1216"">Demetrius</char> <char style=""w"" strong=""G3748"">has</char> <char style=""w"" strong=""G2532"">the</char> <char style=""w"" strong=""G3141"">testimony</char> <char style=""w"" strong=""G5259"">of</char> <char style=""w"" strong=""G3956"">all</char>, <char style=""w"" strong=""G2532"">and</char> <char style=""w"" strong=""G5259"">of</char> <char style=""w"" strong=""G2532"">the</char> truth itself; <char style=""w"" strong=""G1161"">yes</char>, <char style=""w"" strong=""G2249"">we</char> <char style=""w"" strong=""G2532"">also</char> <char style=""w"" strong=""G3140"">testify</char>, <char style=""w"" strong=""G2532"">and</char> <char style=""w"" strong=""G3754"">you</char> <char style=""w"" strong=""G1492"">know</char> <char style=""w"" strong=""G3754"">that</char> <char style=""w"" strong=""G2532"">our</char> <char style=""w"" strong=""G3141"">testimony</char> <char style=""w"" strong=""G1510"">is</char> <char style=""w"" strong=""G3588"">true</char>.<verse eid=""3JN 1:12"" /></para>
  <para style=""p"">
    <verse number=""13"" style=""v"" sid=""3JN 1:13"" />
    <char style=""w"" strong=""G2532"">I</char> <char style=""w"" strong=""G2192"">had</char> <char style=""w"" strong=""G4183"">many</char> <char style=""w"" strong=""G4183"">things</char> <char style=""w"" strong=""G2532"">to</char> <char style=""w"" strong=""G1125"">write</char> <char style=""w"" strong=""G2532"">to</char> <char style=""w"" strong=""G4771"">you</char>, <char style=""w"" strong=""G2532"">but</char> <char style=""w"" strong=""G2532"">I</char> <char style=""w"" strong=""G2532"">am</char> <char style=""w"" strong=""G3756"">unwilling</char> <char style=""w"" strong=""G2532"">to</char> <char style=""w"" strong=""G1125"">write</char> <char style=""w"" strong=""G2532"">to</char> <char style=""w"" strong=""G4771"">you</char> <char style=""w"" strong=""G1223"">with</char> <char style=""w"" strong=""G3188"">ink</char> <char style=""w"" strong=""G2532"">and</char> <char style=""w"" strong=""G2563"">pen</char>; <verse eid=""3JN 1:13"" /><verse number=""14"" style=""v"" sid=""3JN 1:14"" /><char style=""w"" strong=""G1161"">but</char> <char style=""w"" strong=""G2532"">I</char> <char style=""w"" strong=""G1679"">hope</char> <char style=""w"" strong=""G4314"">to</char> <char style=""w"" strong=""G3708"">see</char> <char style=""w"" strong=""G4771"">you</char> soon. <char style=""w"" strong=""G2532"">Then</char> <char style=""w"" strong=""G2532"">we</char> <char style=""w"" strong=""G2532"">will</char> <char style=""w"" strong=""G2980"">speak</char> <char style=""w"" strong=""G4750"">face</char> <char style=""w"" strong=""G4314"">to</char> <char style=""w"" strong=""G4750"">face</char>.</para>
  <para style=""p"" vid=""3JN 1:14"">Peace <char style=""w"" strong=""G2532"">be</char> <char style=""w"" strong=""G4314"">to</char> <char style=""w"" strong=""G4771"">you</char>. <char style=""w"" strong=""G2532"">The</char> friends greet <char style=""w"" strong=""G4771"">you</char>. Greet <char style=""w"" strong=""G2532"">the</char> friends <char style=""w"" strong=""G2532"">by</char> name.<verse eid=""3JN 1:14"" /></para>
  <chapter eid=""3JN 1"" />
</usx>";

        var deserializer = new XDocParser();
        var book = deserializer.DeserializeXml<UsxScriptureBook>(usx);
        var text = BibleParser.WriteToMarkdown(book);

        using var loggerFactory = LoggerFactory.Create(builder =>
            builder.AddDebug().AddConsole());
        var logger = loggerFactory.CreateLogger<Program>();
        //logger.LogInformation(text);

        var bibleBook = book?.ToBibleFormat();
        foreach (var chapter in bibleBook!.Chapters)
        {
            foreach (var verse in chapter.Verses)
            {
                logger.LogInformation(verse.Text);
            }
        }

        var html = book?.ToHtml();
        logger.LogInformation(html);
    }
}