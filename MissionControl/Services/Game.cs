using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Forms;
using SOTM.MissionControl.Models;

namespace SOTM.MissionControl.Services
{
    public class GameService
    {
        public const string GAME_STORAGE_KEY = "CurrentGame";
        public const string GAME_LOG_STORAGE_KEY = "GameLog";
        public Game? game;

        public Dictionary<string, Game> gameLog = new();

        public void StartNewGame(Draft draft)
        {
            this.game = new Game(draft);
        }

        public void MergeGameLogs(Dictionary<string, Game> newGameLog)
        {
            foreach (var (id, game) in newGameLog)
            {
                this.gameLog[id] = game;
            }
        }

        public async Task LoadGameLog(ILocalStorageService storageService)
        {
            if (await storageService.ContainKeyAsync(GAME_LOG_STORAGE_KEY))
            {
                var newGameLog = await storageService.GetItemAsync<Dictionary<string, Game>>(GAME_LOG_STORAGE_KEY);
                this.MergeGameLogs(newGameLog);
            }
        }

        public async Task SaveGameLog(ILocalStorageService storageService)
        {
            await storageService.SetItemAsync(GAME_LOG_STORAGE_KEY, this.gameLog);
        }

        public void SaveCurrentGameToLog()
        {
            this.gameLog[this.game.id] = this.game;
        }

        public async Task SaveGame(ILocalStorageService storageService)
        {
            await storageService.SetItemAsync(GAME_STORAGE_KEY, this.game);
        }

        public async Task LoadGame(ILocalStorageService storageService)
        {
            if (await storageService.ContainKeyAsync(GAME_STORAGE_KEY))
            {
                this.game = await storageService.GetItemAsync<Game>(GAME_STORAGE_KEY);
            }
        }

        public IEnumerable<Game> GetLoggedGames()
        {
            return this.gameLog.Values.OrderByDescending(game => game.endTimestamp);
        }

        public byte[] GetGameLogBytes()
        {
            return JsonSerializer.SerializeToUtf8Bytes(this.gameLog, new JsonSerializerOptions() { WriteIndented = true });
        }

        public async Task LoadGameLogFile(IBrowserFile file, ImportHistoryMode mode)
        {
            var fileGameLog = await JsonSerializer.DeserializeAsync<Dictionary<string, Game>>(file.OpenReadStream());
            if (fileGameLog == null)
            {
                throw new Exception("File is null.");
            }
            else if (mode == ImportHistoryMode.MERGE_EXISTING)
            {
                this.MergeGameLogs(fileGameLog);
            }
            else if (mode == ImportHistoryMode.OVERWRITE_ALL)
            {
                Console.WriteLine("Overwriting all...");
                this.gameLog = fileGameLog;
            }
        }
    }
}