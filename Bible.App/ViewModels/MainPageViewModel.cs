using Bible.App.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Bible.App.Abstractions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;

namespace Bible.App.ViewModels
{
    public sealed partial class MainPageViewModel : BaseViewModel
    {
        [ObservableProperty]
        private BibleUiModel? bible;

        public ObservableCollection<string> Languages { get; } = [];

        [ObservableProperty]
        private string? selectedLanguage;

        internal static readonly string Eng = "eng";
        internal static readonly string Web = "WEB";
        static readonly string _webName = "World English Bible";
        public IList<TranslationUiModel> Translations { get; internal set; } = [];

        public Dictionary<string, List<TranslationUiModel>> Identifiers { get; } = [];

        [ObservableProperty]
        private int bookIndex = -1;

        [ObservableProperty]
        private int chapterIndex = -1;

        private readonly IUiDataService _readerService;
        private readonly ILogger _logger;

        public MainPageViewModel(IUiDataService readerService, ILogger? logger = null)
        {
            _readerService = readerService;
            // TODO: fix logging injection
            _logger = logger ?? NullLogger<MainPageViewModel>.Instance;
        }

        void InitializeSelections()
        {
            SelectedLanguage ??= Eng;
            Translations = Identifiers[SelectedLanguage];
        }

        void Initialize()
        {
            Identifiers.Add(Eng, [new(Web, _webName)]);
            Languages.Add(Eng);
            InitializeSelections();
        }

        internal async Task RefreshIdentifiersAsync()
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet && !Identifiers.Any())
                Initialize();
            else
            {
                Identifiers.Clear();
                Languages.Clear();
                var translations = _readerService.AsyncGetBibleInfo();
                await foreach (var bibleInfo in translations)
                {
                    var translation = new TranslationUiModel(bibleInfo.Identifier, bibleInfo.Name);
                    if (Identifiers.ContainsKey(bibleInfo.Language))
                        Identifiers[bibleInfo.Language].Add(translation);
                    else
                    {
                        Identifiers.Add(bibleInfo.Language, [translation]);
                        Languages.Add(bibleInfo.Language);
                    }
                }
                if (!Identifiers.Any() || !Languages.Contains(Eng))
                    Initialize();
                else
                    InitializeSelections();
            }
        }

        [RelayCommand]
        private async Task TranslationSelectedAsync(TranslationUiModel? value)
        {
            try
            {
                if (value != null && Identifiers.Count > 1)
                {
                    Bible = await _readerService.GetBibleAsync(value.Identifier);
                }
                if (Bible == null)
                {
                    Bible = await _readerService.LoadFileAsync(string.Join('/', Eng, Web));
                }
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogDebug(ex, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                System.Diagnostics.Debugger.Break();
            }
            ChapterIndex = -1;
            BookIndex = 0;
        }
    }
}