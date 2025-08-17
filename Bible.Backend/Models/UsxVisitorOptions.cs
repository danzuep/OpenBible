using Microsoft.Extensions.Options;
using Unihan.Models;

namespace Bible.Backend.Models;

public sealed class UsxVisitorOptions : IOptions<UsxVisitorOptions>
{
    public bool EnableStrongs { get; set; }
    public bool EnableRedLetters { get; set; }
    public bool EnableFootnotes { get; set; }
    public bool EnableCrossReferences { get; set; }
    public bool EnableChapterLinks { get; set; }
    public bool EnableRubyText { get; set; }
    public UnihanField? EnableRunes { get; set; }

    public UsxVisitorOptions Value => this;
}

internal sealed class UsxVisitorReference
{
    public string Title { get; set; } = "Bible";
    public string? BookCode { get; set; }
    public string? Chapter { get; set; }
    public string? Verse { get; set; }

    public override string ToString()
    {
        if (Verse != null)
            return $"{BookCode}.{Chapter}.{Verse}";
        else if (Chapter != null)
            return $"{BookCode}.{Chapter}";
        else
            return BookCode ?? "_";
    }
}
