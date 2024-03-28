using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Forms;
using SOTM.MissionControl.Models;

namespace SOTM.MissionControl.Services
{
    public class GameLogService
    {
        public const string GAME_LOG_STORAGE_KEY = "GameLog";
        public GenericRepository<Dictionary<string, Game>> repo = new(GAME_LOG_STORAGE_KEY, new());
        public Dictionary<string, Game> gameLog
        {
            get => this.repo.value;
            set => this.repo.value = value;
        }

        public void MergeGameLogs(Dictionary<string, Game> newGameLog)
        {
            foreach (var (id, game) in newGameLog)
            {
                this.gameLog[id] = game;
            }
        }

        public void AddGame(Game game)
        {
            this.gameLog[game.id] = game;
        }

        public IEnumerable<Game> GetLoggedGames()
        {
            return this.gameLog.Values.OrderByDescending(game => game.endTimestamp);
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