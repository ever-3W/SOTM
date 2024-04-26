using System.Text.Json;
using System.Text.Json.Serialization;

namespace SOTM.Shared.Models {

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DeckTreeLevel
    {
        COLLECTION = 0,
        EXPANSION = 1,
        DECK = 2,
        VARIANT = 3
    }

    [JsonConverter(typeof(GlobalIdentifierJsonConverter))]
    public class GlobalIdentifier
    {
        public Dictionary<DeckTreeLevel, string> identifiers;

        public DeckTreeLevel level;

        public GlobalIdentifier()
        {
            identifiers = new Dictionary<DeckTreeLevel, string>();
        }

        public GlobalIdentifier(string? key)
        {
            identifiers = new Dictionary<DeckTreeLevel, string>();

            if (key != null) {
                string[] identifiersToAdd = key.Split('.');
                foreach (string identifier in identifiersToAdd)
                {
                    AppendIdentifier(identifier);
                }
            }
        }

        public GlobalIdentifier(GlobalIdentifier other)
        {
            level = other.level;
            identifiers = new Dictionary<DeckTreeLevel, string>(other.identifiers);
        }

        public override string ToString()
        {
            return string.Join('.', identifiers.Values);
        }

        private void AppendIdentifier(string identifier)
        {
            DeckTreeLevel childLevel = level + 1;
            identifiers.Add(this.level, identifier);
            this.level = childLevel;
        }

        public GlobalIdentifier CreateChildIdentifier(string identifier)
        {
            GlobalIdentifier childIdentifier = new GlobalIdentifier(this);
            childIdentifier.AppendIdentifier(identifier);
            return childIdentifier;
        }
        public string LocalIdentifier()
        {
            return identifiers[level-1];
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (obj is GlobalIdentifier otherIdentifier) {
                return this.identifiers.SequenceEqual(otherIdentifier.identifiers);
            }
            return false;
        }
    }

    class GlobalIdentifierJsonConverter : JsonConverter<GlobalIdentifier>
    {
        public override GlobalIdentifier Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) => new GlobalIdentifier(reader.GetString());

        public override void Write(
            Utf8JsonWriter writer,
            GlobalIdentifier identifier,
            JsonSerializerOptions options) =>
                writer.WriteStringValue(identifier.ToString());
    }
}
