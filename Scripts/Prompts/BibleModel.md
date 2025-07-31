Your response must be in English. The goal is to efficiently store Unified Scripture XML data in memory (https://ubsicap.github.io/usx/structure.html).

---

Please generate a complete .NET Standard 2.1 compatible C# implementation of a Bible book model with the following specifications and best practices:

1. Define an enum `MetadataCategory` with these values:
   - `Meta`
   - `Text`
   - `Style`
   - `Markup`
   - `Footnote`
   - `Reference`
   - `Pronunciation`

2. Define a readonly struct `VerseSegment` that represents a single verse segment with:
   - A `byte Chapter` property.
   - A `byte Verse` property.
   - A `string Text` property.
   - A `MetadataCategory Category` property.
   - A constructor that takes:
     - `byte chapter`
     - `byte verse`
     - `string text`
     - `MetadataCategory category` (`MetadataCategory.Text` by default)
     The constructor should build an immutable lookup from the metadata.
   - Override `ToString()` to show all properties, except MetadataCategory when it is default.

3. Use the `Range` struct to represent a range of indices with:
   - `using System.Runtime.CompilerServices;` at the top for `Range`.
   This will be used for chapter, verse, paragraph, and section index ranges.

4. Define a class `UnifiedScriptureBook` representing a book of the Bible with:
   - A `string Name` property.
   - Book-level metadata stored as chapter 0 verse 0.
   - `private readonly VerseSegment[]? _verseSegments;` as the backing storage for verse segments.
   - Dictionaries mapping:
     - `byte chapter` → `Range` (range of verse segment indices in that chapter)
     - `(byte chapter, byte verse)` → `Range` (range of verse segment indices in that verse)
     - `ushort paragraphNumber` → `Range`
     - `string sectionName` → `Range`
   - A method `AddVerseSegment` defined as:
     ```csharp
     void AddVerseSegment(byte chapter, byte verse, string text, MetadataCategory category = MetadataCategory.Text)
     ```
     This adds a verse segment internally (initially stored in a list), updates chapter and verse index ranges automatically.
     Paragraph and section tracking is managed explicitly via MetadataCategory.Style ('p' and 's').
   - A method `Seal()` that:
     - Converts the internal list to the `_verseSegments` array for efficient span slicing.
     - Frees the memory from the now finished fields like _verseSegmentsList.
     - Prevents further additions after sealing.
   - Methods returning `ReadOnlySpan<VerseSegment>` slices for:
     - A whole chapter: `ReadOnlySpan<VerseSegment> GetChapter(byte chapter)`
     - A single verse: `ReadOnlySpan<VerseSegment> GetVerse(byte chapter, byte verse)`
     - A whole paragraph: `ReadOnlySpan<VerseSegment> GetParagraph(ushort paragraphNumber)`
     - A whole section: `ReadOnlySpan<VerseSegment> GetSection(string sectionName)`

5. Add input validation where appropriate e.g., null checks.

6. Provide a `public static class Sample` `UnifiedScripture(ILogger logger)` method demonstrating:
   - Creating a `UnifiedScriptureBook` instance with a name.
   - Adding several verse segments with various metadata.
   - Adding several verse segments accross paragraphs and sections.
   - Calling `Seal()` after all additions.
   - Retrieving and printing verse segments for:
     - A full chapter
     - A specific verse
   - Demonstrate the overridden `ToString()` of `VerseSegment` printing text and metadata.
   - If ILogger is null: `using var loggerFactory = LoggerFactory.Create(builder => builder.AddDebug().AddConsole());

Make sure the code is well-structured, uses immutable and readonly patterns where suitable, and clearly documents key parts. Use idiomatic C# 10+ features, and ensure the design cleanly separates metadata handling and range tracking.

Return the full C# code with all these requirements implemented inside the namespace `Bible.Core.Models.UnifiedScriptureXmlModel`.