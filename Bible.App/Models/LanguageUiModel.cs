namespace Bible.App.Models
{
    public sealed partial class LanguageUiModel : List<TranslationUiModel>
    {
        public LanguageUiModel(string language, List<TranslationUiModel>? translations = null) : base(translations ?? new())
        {
            Language = language;
        }

        /// <summary>
        /// ISO 3 language code
        /// </summary>
        public string Language { get; set; }

        public override string ToString() =>
            $"[{Language}] ({this.Count} translations)";
    }
}
