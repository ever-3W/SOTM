using System.Text.Json.Serialization;
using SOTM.Shared.Models;

namespace SOTM.MissionControl.Models
{
    public class Draft
    {
        [JsonInclude]
        public List<GlobalIdentifier> heroes = new();
        [JsonInclude]
        public List<GlobalIdentifier> villains = new();
        [JsonInclude]
        public List<GlobalIdentifier> environments = new();

        public List<GlobalIdentifier> GetList(DeckKind kind)
        {
            if (kind == DeckKind.HERO) return this.heroes;
            if (kind == DeckKind.VILLAIN) return this.villains;
            if (kind == DeckKind.ENVIRONMENT) return this.environments;
            return this.heroes;
        }

        public IEnumerable<GlobalIdentifier> GetAllVariants()
        {
            return this.villains.Concat(this.heroes).Concat(this.environments);
        }
    }
}
