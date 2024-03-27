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

    public class SettingsData
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
    }
    public class SettingsService
    {
        public const string SETTINGS_STORAGE_KEY = "Settings";
        public SettingsData settings = new();

        public async Task SaveData(ILocalStorageService storageService)
        {
            await storageService.SetItemAsync(SETTINGS_STORAGE_KEY, this.settings);
        }

        public async Task LoadData(ILocalStorageService localStorage)
        {
            if (await localStorage.ContainKeyAsync(SETTINGS_STORAGE_KEY))
            {
                this.settings = await localStorage.GetItemAsync<SettingsData>(SETTINGS_STORAGE_KEY);
            }
        }
    }
}