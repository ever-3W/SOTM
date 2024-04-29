using System.Text.Json.Serialization;
using Blazored.LocalStorage;

namespace SOTM.MissionControl.Services
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum GameEndAction
    {
        RETURN_TO_DRAFT,
        START_NEW_GAME
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ImportHistoryMode
    {
        MERGE_EXISTING,
        OVERWRITE_ALL
    }

    public class SettingsModel
    {
        [JsonInclude]
        public int draftHeroCount = 8;
        [JsonInclude]
        public RandomizerMethod heroRandomizerMethod = RandomizerMethod.RANDOMIZE_BY_DECK;
        [JsonInclude]
        public RandomizerMethod villainRandomizerMethod = RandomizerMethod.RANDOMIZE_BY_VARIANT;
        [JsonInclude]
        public GameEndAction gameEndAction = GameEndAction.RETURN_TO_DRAFT;
        [JsonInclude]
        public ImportHistoryMode importHistoryMode = ImportHistoryMode.MERGE_EXISTING;
        [JsonInclude]
        public int? draftAutoSaveInterval = 15;
    }
    public class SettingsService
    {
        private const string SETTINGS_STORAGE_KEY = "Settings";
        private GenericRepository<SettingsModel> repo = new(SETTINGS_STORAGE_KEY, new());
        private ILocalStorageService _storageService;

        public async Task Save(ILocalStorageService storageService)
        {
            this._storageService = storageService;
            await repo.Save(this._storageService);
        }

        public async Task Save()
        {
            await repo.Save(this._storageService);
        }

        public async Task Load(ILocalStorageService storageService)
        {
            this._storageService = storageService;
            await repo.Load(this._storageService);
        }

        public RandomizerMethod HeroRandomizerMethod
        {
            get => this.repo.value.heroRandomizerMethod;
            set
            {
                this.repo.value.heroRandomizerMethod = value;
                Task.Run(this.Save);
            }
        }

        public RandomizerMethod VillainRandomizerMethod
        {
            get => this.repo.value.villainRandomizerMethod;
            set
            {
                this.repo.value.villainRandomizerMethod = value;
                Task.Run(this.Save);
            }
        }

        public int DraftHeroCount
        {
            get => this.repo.value.draftHeroCount;
            set
            {
                this.repo.value.draftHeroCount = value;
                Task.Run(this.Save);
            }
        }

        public GameEndAction GameEndAction
        {
            get => this.repo.value.gameEndAction;
            set
            {
                this.repo.value.gameEndAction = value;
                Task.Run(this.Save);
            }
        }

        public ImportHistoryMode ImportHistoryMode
        {
            get => this.repo.value.importHistoryMode;
            set
            {
                this.repo.value.importHistoryMode = value;
                Task.Run(this.Save);
            }
        }

        public int? DraftAutoSaveInterval
        {
            get => this.repo.value.draftAutoSaveInterval;
            set
            {
                this.repo.value.draftAutoSaveInterval = value;
                Task.Run(this.Save);
            }
        }
    }
}