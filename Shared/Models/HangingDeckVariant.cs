using System.Text.Json.Serialization;

namespace SOTM.Shared.Models {
    public class HangingDeckVariant
    {
        [JsonInclude]
        public string deckNamespacedIdentifier;
        [JsonInclude]
        public string variantIdentifier;
        [JsonInclude]
        public string promoTitle;
        [JsonInclude]
        public GlobalIdentifier sourceExpansionIdentifier;
        [JsonInclude]
        public GlobalIdentifier sourceCollectionIdentifier;
    }
}
