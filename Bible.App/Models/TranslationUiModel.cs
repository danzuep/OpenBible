namespace Bible.App.Models
{
    public sealed partial class TranslationUiModel
    {
        public string Identifier { get; }

        public string Name { get; }

        public TranslationUiModel(string identifier, string name)
        {
            Identifier = identifier;
            Name = name;
        }

        public override string ToString() =>
            $"[{Identifier}] {Name}";
    }
}