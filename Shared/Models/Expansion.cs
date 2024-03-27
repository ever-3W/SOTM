using System.Text.Json.Serialization;

namespace SOTM.Shared.Models
{
    public class Expansion : AncestorGroup<Deck>
    {

        [JsonInclude]
        public List<Deck> decks {
            get => this.JsonChildPropertyGetter();
            set => this.JsonChildPropertySetter(value);
        }

        public const string BASE_EXPANSION_IDENTIFIER = "BaseExpansion";
        // Mods introducing new promo cards aren't tied to a specific Expansion, so this is a placeholder
        public const string PROMO_CARD_LIST_EXPANSION_IDENTIFIER = "PromoCardList";
        public Expansion(GlobalIdentifier identifier) : base(identifier) { }

        public IEnumerable<Deck> GetByKind(DeckKind kind)
        {
            return children.Values.Where(deck => deck.kind == kind);
        }
    }
}
