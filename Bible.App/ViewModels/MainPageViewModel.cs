using System.Collections.ObjectModel;
using Bible.App.Abstractions;
using Bible.App.Models;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bible.App.ViewModels
{
    public sealed partial class MainPageViewModel : BaseViewModel
    {
        internal static readonly string Eng = "eng";
        internal static readonly string Web = "WEB";
        static readonly string _webName = "World English Bible";

        private WeakReference<BibleUiModel?> _bibleReference = new(default);

        public BibleUiModel? Bible
        {
            get
            {
                _bibleReference.TryGetTarget(out BibleUiModel? bible);
                return bible;
            }
        }

        public ObservableCollection<string> Languages { get; } = new();

        private string _selectedLanguage = Eng;
        public string SelectedLanguage
        {
            get => _selectedLanguage;
            set => SetProperty(ref _selectedLanguage, value);
        }

        public ObservableCollection<TranslationUiModel> Translations { get; } = new();
        private string _selectedTranslation = Web;
        public string SelectedTranslation
        {
            get => _selectedTranslation;
            set => SetProperty(ref _selectedTranslation, value);
        }

        public ObservableCollection<LanguageUiModel> Identifiers { get; } = new();

        private int _bookIndex = -1;
        public int BookIndex
        {
            get => _bookIndex;
            set => SetProperty(ref _bookIndex, value);
        }

        private int _chapterIndex = -1;
        public int ChapterIndex
        {
            get => _chapterIndex;
            set => SetProperty(ref _chapterIndex, value);
        }

        private readonly IUiDataService _readerService;
        private readonly ILogger<MainPageViewModel> _logger;

        public MainPageViewModel(IUiDataService readerService, ILogger<MainPageViewModel>? logger = null)
        {
            _readerService = readerService;
            _logger = logger ?? NullLogger<MainPageViewModel>.Instance;
        }

        void InitializeSelections()
        {
            SelectedLanguage ??= Eng;
            Translations.Clear();
            foreach (var translation in Identifiers.First(i => i.Language == SelectedLanguage))
            {
                Translations.Add(translation);
            }
            SelectedTranslation ??= Web;
        }

        void Initialize()
        {
            Identifiers.Add(new(Eng, [new(Web, _webName, isDownloaded: true)]));
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
                await foreach (var bibleInfo in translations.ConfigureAwait(false))
                {
                    var translation = new TranslationUiModel(bibleInfo.Identifier, bibleInfo.Name);
                    var identifier = Identifiers.FirstOrDefault(i => i.Language == bibleInfo.Language);
                    if (identifier?.Language != null)
                        identifier.Add(translation);
                    else
                    {
                        Identifiers.Add(new LanguageUiModel(bibleInfo.Language, new List<TranslationUiModel> { translation }));
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
                if (!_bibleReference.TryGetTarget(out BibleUiModel? bible) || bible == null || bible.Translation != value?.Identifier)
                {
                    // The object has been garbage collected, so we need to recreate it
                    if (value != null && Identifiers.Count > 1)
                    {
                        bible = await _readerService.GetBibleAsync(value.Identifier).ConfigureAwait(false);
                        _bibleReference.SetTarget(bible);
                        Translations.First(t => t.Identifier == value.Identifier).IsDownloaded = true;
                    }
                    if (bible == null)
                    {
                        bible = await _readerService.LoadFileAsync(string.Join('/', Eng, Web)).ConfigureAwait(false);
                        _bibleReference.SetTarget(bible);
                    }
                    OnPropertyChanged(nameof(Bible));
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