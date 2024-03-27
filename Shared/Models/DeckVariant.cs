using System.Text.Json.Serialization;

namespace SOTM.Shared.Models {
    public class DeckVariant: IIdentifiable
    {
        public const string BASE_VARIANT = "BaseVariant";
        [JsonInclude]
        public GlobalIdentifier identifier;
        [JsonInclude]
        public string? title;
        public DeckVariant(GlobalIdentifier identifier) { 
            this.identifier = identifier;
        }

        [JsonInclude]
        public GlobalIdentifier sourceExpansionIdentifier;
        [JsonInclude]
        public GlobalIdentifier sourceCollectionIdentifier;

        public GlobalIdentifier GetIdentifier()
        {
            return identifier;
        }
    }
}
