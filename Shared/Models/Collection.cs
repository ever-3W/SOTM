using System.Text.Json.Serialization;

namespace SOTM.Shared.Models
{
    public class Collection : AncestorGroup<Expansion>
    {
        [JsonInclude]
        public string title;

        [JsonInclude]
        public List<Expansion> expansions {
            get => this.JsonChildPropertyGetter();
            set => this.JsonChildPropertySetter(value);
        }
        public Collection(GlobalIdentifier identifier) : base(identifier) { }

        public static string BASE_COLLECTION_IDENTIFIER = "Vanilla";

    }
}
