using System.Text.Json.Serialization;
using SOTM.Shared.Models;

namespace SOTM.MissionControl.Models
{
    public enum DeckVariantState
    {
        PICKABLE,
        BANNED
    }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum GameState
    {
        DRAFT = 0,
        PICK = 1,
        PLAY = 2,
        // 3rd bit set == game has started, 4th bit set == win
        WIN = 12,
        LOSS = 4
    }

    public class Game
    {
        [JsonInclude]
        public Dictionary<DeckKind, List<int>> picks = new(); 

        [JsonInclude]
        public GameState state = GameState.DRAFT;

        [JsonInclude]
        public Draft draft;

        [JsonInclude]
        public string id;

        [JsonInclude]
        public long endTimestamp;

        private static Dictionary<DeckKind, List<int>> GetDefaultPicks(Draft draft)
        {
            return new()
            {
                { DeckKind.HERO, draft.heroes.Select((identifier) => 0).ToList() },
                { DeckKind.VILLAIN, draft.villains.Select((identifier) => 0).ToList() },
                { DeckKind.ENVIRONMENT, draft.environments.Select((identifier) => 0).ToList() }
            };
        }

        [JsonConstructor]
        public Game (Draft draft, string id, Dictionary<DeckKind, List<int>> picks, long endTimestamp)
        {
            this.id = id;
            this.draft = draft;
            this.picks = picks;
            this.endTimestamp = endTimestamp;
        }

        public Game (Draft draft): this(
            draft,
            Guid.NewGuid().ToString("N"),
            GetDefaultPicks(draft),
            new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds())
        {
        }

        private int GetIndex(List<GlobalIdentifier> draftList, GlobalIdentifier identifier)
        {
            return draftList.IndexOf(identifier);
        }

        public void Pick(DeckKind kind, GlobalIdentifier identifier, int order)
        {
            int index = GetIndex(this.draft.GetList(kind), identifier);
            if (index >= 0)
            {
                this.picks[kind][index] = (order << 1) | 1;
            }
        }

        // The following functions, used in statistics aggregations, return 1 if true and 0 if false.
        public int WasPicked(DeckKind kind, GlobalIdentifier identifier)
        {
            return this.picks[kind].ElementAtOrDefault(GetIndex(this.draft.GetList(kind), identifier)) & 1;
        }

        public int WasWinner(DeckKind kind, GlobalIdentifier identifier)
        {
            return this.picks[kind].ElementAtOrDefault(GetIndex(this.draft.GetList(kind), identifier)) & ((int) this.state >> 3);
        }

        public IEnumerable<GlobalIdentifier> GetPickedHeroes()
        {
            return this.picks[DeckKind.HERO]
                .Select((pick, index) => (pick, index))
                .Where(tuple => tuple.Item1 != 0)
                .OrderBy(tuple => tuple.Item1)
                .Select(tuple => this.draft.heroes.ElementAtOrDefault(tuple.Item2));
        }

        public IEnumerable<GlobalIdentifier> GetUnpickedHeroes()
        {
            return this.picks[DeckKind.HERO]
                .Select((pick, index) => (pick, index))
                .Where(tuple => tuple.Item1 == 0)
                .Select(tuple => this.draft.heroes.ElementAtOrDefault(tuple.Item2));
        }

        public GlobalIdentifier GetPickedVillain()
        {
            return this.draft.villains
                .ElementAtOrDefault(this.picks[DeckKind.VILLAIN].FindIndex((pick) => pick != 0))
                ?? this.draft.environments.ElementAtOrDefault(0);
        }

        public GlobalIdentifier GetPickedEnvironment()
        {
            return this.draft.environments
                .ElementAtOrDefault(this.picks[DeckKind.ENVIRONMENT].FindIndex((pick) => pick != 0))
                ?? this.draft.environments.ElementAtOrDefault(0);
        }
    }
}
