using System.Text.Json.Serialization;

namespace SOTM.Shared.Models
{
    public class CollectionEntry
    {
        [JsonInclude]
        public GlobalIdentifier identifier;
        [JsonInclude]
        public string title;
        [JsonInclude]
        public List<GlobalIdentifier> expansions = new List<GlobalIdentifier>();
        [JsonInclude]
        public List<Deck> heroes = new List<Deck>();
        [JsonInclude]
        public List<Deck> villains = new List<Deck>();
        [JsonInclude]
        public List<Deck> environments = new List<Deck>();
        [JsonInclude]
        public List<Deck> teamVillains = new List<Deck>();

        public IEnumerable<Deck> GetAllDecks()
        {
            return new List<List<Deck>>
            {
                heroes,
                villains,
                environments,
                teamVillains
            }.SelectMany(deck => deck);
        }
    }
}
