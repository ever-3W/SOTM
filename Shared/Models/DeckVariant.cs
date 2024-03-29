using System.Text.Json.Serialization;

namespace SOTM.Shared.Models {
    public class DeckVariant: IIdentifiable
    {
        public const string BASE_VARIANT = "BaseVariant";
        [JsonInclude]
        public GlobalIdentifier identifier;
        [JsonInclude]
        public string? title;
        [JsonInclude]
        public string? shortTitle;
        public DeckVariant(GlobalIdentifier identifier) { 
            this.identifier = identifier;
        }

        public GlobalIdentifier GetIdentifier()
        {
            return identifier;
        }
    }
}
