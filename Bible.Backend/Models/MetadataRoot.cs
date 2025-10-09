using System.Text.Json.Serialization;

namespace Bible.Backend.Models;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
public class MetadataRoot
{
    [JsonPropertyName("metadata")]
    public Dictionary<string, MetadataItem> Metadata { get; set; }
}

public class MetadataItem
{
    [JsonPropertyName("path")]
    public string Path { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("scope")]
    public string Scope { get; set; }

    [JsonPropertyName("revision")]
    public int Revision { get; set; }

    [JsonPropertyName("licenseId")]
    public int LicenseId { get; set; }

    [JsonPropertyName("completed")]
    public string Completed { get; set; }

    [JsonPropertyName("updated")]
    public DateTime Updated { get; set; }

    [JsonPropertyName("rights")]
    public string Rights { get; set; }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.