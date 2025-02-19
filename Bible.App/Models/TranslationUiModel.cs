namespace Bible.App.Models
{
    public sealed partial class TranslationUiModel
    {
        public TranslationUiModel(string identifier, string name, bool isDownloaded = false)
        {
            Identifier = identifier;
            Name = name;
            IsDownloaded = isDownloaded;
        }

        public string Identifier { get; }

        public string Name { get; }

        public bool IsDownloaded { get; set; }

        public string DisplayText =>
            IsDownloaded ? $"{this} ✓" : ToString();

        public override string ToString() =>
            $"[{Identifier}] {Name}";
    }
}