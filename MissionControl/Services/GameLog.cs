using System.Text.Json;
using System.Text.Json.Serialization;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Forms;
using SOTM.MissionControl.Models;
using SOTM.Shared.Models;

namespace SOTM.MissionControl.Services
{
    // Log the deck variant data plus additional data
    public class DeckVariantLogModel
    {
        [JsonInclude]
        public DeckVariant variant;
        [JsonInclude]
        public GlobalIdentifier sourceCollection;
    }

    public class CollectionLogModel
    {
        [JsonInclude]
        public GlobalIdentifier identifier;
        [JsonInclude]
        public string title;
        [JsonInclude]
        public string color;
    }

    public class GameLogModel
    {
        [JsonInclude]
        public Dictionary<string, Game> games = new();
        [JsonInclude]
        public Dictionary<string, DeckVariantLogModel> variantLog = new();
        [JsonInclude]
        public Dictionary<string, CollectionLogModel> collectionLog = new();

        public void AddGame(Game game, DeckDataService deckData)
        {
            this.games[game.id] = game;
            HashSet<Collection> collections = new();
            foreach (GlobalIdentifier identifier in game.draft.GetAllVariants())
            {
                var collection = deckData.GetVariantCollection(identifier);
                collections.Add(collection);
                this.variantLog[identifier.ToString()] = new DeckVariantLogModel
                {
                    variant = deckData.GetVariantData(identifier),
                    sourceCollection = collection.identifier
                };
            }
            foreach (Collection collection in collections)
            {
                this.collectionLog[collection.identifier.ToString()] = new CollectionLogModel
                {
                    identifier = collection.identifier,
                    color = collection.color,
                    title = collection.title
                };
            }
        }

        public void AddGame(Game game, GameLogModel otherModel)
        {
            this.games[game.id] = game;
            HashSet<GlobalIdentifier> collections = new();
            foreach (GlobalIdentifier identifier in game.draft.GetAllVariants())
            {
                collections.Add(otherModel.variantLog[identifier.ToString()].sourceCollection);
                this.variantLog[identifier.ToString()] = otherModel.variantLog[identifier.ToString()];
            }
            foreach (GlobalIdentifier identifier in collections)
            {
                this.collectionLog[identifier.ToString()] = otherModel.collectionLog[identifier.ToString()];
            }
        }
    }

    public class GameLogService
    {
        private const string GAME_LOG_STORAGE_KEY = "GameLog";
        private readonly GenericRepository<GameLogModel> repo = new(GAME_LOG_STORAGE_KEY, new());
        public Func<ILocalStorageService, Task> Save;
        public Func<ILocalStorageService, Task<GameLogModel>> Load;
        public Func<byte[]> GetBytes;

        public GameLogService()
        {
            this.Save = this.repo.Save;
            this.Load = this.repo.Load;
            this.GetBytes = this.repo.GetBytes;
        }
        public void MergeGameLogs(GameLogModel newGameLog)
        {
            foreach (var game in newGameLog.games.Values)
            {
                this.repo.value.AddGame(game, newGameLog);
            }
        }

        public void AddGame(Game game, DeckDataService deckData)
        {
            this.repo.value.AddGame(game, deckData);
        }

        public DeckVariantViewModel? GetVariantMetadata(GlobalIdentifier identifier)
        {
            var variant = this.repo.value.variantLog[identifier.ToString()].variant;
            var collectionKey = this.repo.value.variantLog[identifier.ToString()].sourceCollection.ToString();
            if (variant == null)
            {
                return null;
            }
            return new DeckVariantViewModel(variant) 
            { color = this.repo.value.collectionLog[collectionKey].color };
        }

        public IEnumerable<Game> GetLoggedGames()
        {
            return this.repo.value.games.Values.OrderByDescending(game => game.endTimestamp);
        }

        public async Task LoadGameLogFile(IBrowserFile file, ImportHistoryMode mode)
        {
            var fileGameLog = await JsonSerializer.DeserializeAsync<GameLogModel>(file.OpenReadStream());
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
                this.repo.value = fileGameLog;
            }
        }
    }
}