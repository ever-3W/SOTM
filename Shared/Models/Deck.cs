using System.Text.Json.Serialization;

namespace SOTM.Shared.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DeckKind
    {
        HERO,
        VILLAIN,
        ENVIRONMENT,
        VILLAIN_TEAM,
        VILLAIN_OBLIVAEON
    }

    public class Deck : AncestorGroup<DeckVariant>
    {
        [JsonInclude]
        public string? title;
        [JsonInclude]
        public DeckKind kind;
        [JsonInclude]
        public GlobalIdentifier sourceExpansionIdentifier;

        [JsonInclude]
        public List<DeckVariant> variants {
            get => this.JsonChildPropertyGetter();
            set => this.JsonChildPropertySetter(value);
        }

        public Deck(GlobalIdentifier identifier) : base(identifier) { }

        // For checking whether deck is listed in manifest
        public string GetDeckListIdentifier()
        {
            return this.GetIdentifier().identifiers.GetValueOrDefault(DeckTreeLevel.DECK);
        }

        // For resolving hanging variants
        public string GetNamespacedIdentifier()
        {
            if (identifier.identifiers[DeckTreeLevel.COLLECTION] == Collection.BASE_COLLECTION_IDENTIFIER) return identifier.LocalIdentifier();
            return identifier.identifiers[DeckTreeLevel.COLLECTION] + "." + identifier.LocalIdentifier();
        }
        public static DeckKind StringToKind(string kind)
        {
            if (kind.Equals("Hero")) return DeckKind.HERO;
            if (kind.Equals("Villain")) return DeckKind.VILLAIN;
            if (kind.Equals("Environment")) return DeckKind.ENVIRONMENT;
            if (kind.Equals("VillainTeam")) return DeckKind.VILLAIN_TEAM;
            throw new Exception("\"" + kind + "\" is not a valid deck type");
        }
    }
}
