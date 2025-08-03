namespace Bible.ServiceDefaults.Models;

public record UnihanCharacter(string Character, IReadOnlyDictionary<string, IList<string>> Metadata)
{
}
