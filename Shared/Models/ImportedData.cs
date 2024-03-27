using System.Text.Json.Serialization;

namespace SOTM.Shared.Models
{
    public class ImportedData
    {
        [JsonInclude]
        public List<CollectionEntry> collections = new();
        [JsonInclude]
        public Dictionary<string, List<HangingDeckVariant>> hangingDeckVariants = new();
    }
}
