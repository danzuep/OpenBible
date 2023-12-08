namespace Bible.App.Models
{
    public sealed partial class LanguageUiModel : List<TranslationUiModel>
    {
        /// <summary>
        /// ISO 3 language code
        /// </summary>
        public string Language { get; set; }

        public LanguageUiModel(string language, List<TranslationUiModel>? translations = null) : base(translations ?? new())
        {
            Language = language;
        }

        public override string ToString() =>
            $"[{Language}] ({this.Count} translations)";
    }
}
