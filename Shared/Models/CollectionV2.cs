using System.Collections.Immutable;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;

namespace SOTM.Shared.Models
{
    public class Collection: IIdentifiable
    {
        public const string BASE_COLLECTION_IDENTIFIER = "Vanilla";
        public const string BASE_COLLECTION_TITLE = "Official Content";
        public const string BASE_COLLECTION_COLOR = "e02128";

        [JsonInclude]
        public GlobalIdentifier identifier;

        [JsonInclude]
        public string title;

        private int _color;
        [JsonInclude]
        public string color {
            get => $"#{this._color:X6}";
            set
            {
                // make sure it's a valid hex color
                string colorStr = value;
                if (colorStr.StartsWith("0x"))
                {
                    colorStr = colorStr.Substring(2);
                }
                else if (colorStr.StartsWith('#'))
                {
                    colorStr = colorStr.Substring(1);
                }
                if (!Int32.TryParse(Encoding.UTF8.GetBytes(colorStr), NumberStyles.HexNumber, null, out this._color))
                {
                    this._color = 0;
                }
            }
        }

        private AncestorGroup<Expansion> heroExpansionParent;
        [JsonInclude]
        public List<Expansion> heroExpansions {
            get => this.heroExpansionParent.JsonChildPropertyGetter();
            set => this.heroExpansionParent.JsonChildPropertySetter(value);
        }

        private AncestorGroup<Expansion> villainExpansionParent;
        [JsonInclude]
        public List<Expansion> villainExpansions {
            get => this.villainExpansionParent.JsonChildPropertyGetter();
            set => this.villainExpansionParent.JsonChildPropertySetter(value);
        }

        private AncestorGroup<Expansion> environmentExpansionParent;
        [JsonInclude]
        public List<Expansion> environmentExpansions {
            get => this.environmentExpansionParent.JsonChildPropertyGetter();
            set => this.environmentExpansionParent.JsonChildPropertySetter(value);
        }

        private AncestorGroup<Expansion> teamVillainExpansionParent;
        [JsonInclude]
        public List<Expansion> teamVillainExpansions {
            get => this.teamVillainExpansionParent.JsonChildPropertyGetter();
            set => this.teamVillainExpansionParent.JsonChildPropertySetter(value);
        }


        [JsonInclude]
        public Dictionary<string, List<DeckVariant>> hangingVariants;

        [JsonConstructor]
        public Collection(GlobalIdentifier identifier, string title) {
            this.identifier = identifier;
            this.title = title;

            this.heroExpansionParent = new AncestorGroup<Expansion>(identifier);
            this.villainExpansionParent = new AncestorGroup<Expansion>(identifier);
            this.environmentExpansionParent = new AncestorGroup<Expansion>(identifier);
            this.teamVillainExpansionParent = new AncestorGroup<Expansion>(identifier);

            this.hangingVariants = new();
        }

        public GlobalIdentifier GetIdentifier()
        {
            return this.identifier;
        }
        public void AddDeck(Deck deck)
        {
            AncestorGroup<Expansion>? expansionParent = null;
            if (deck.kind == DeckKind.HERO)
            {
                expansionParent = this.heroExpansionParent;
            }
            else if (deck.kind == DeckKind.VILLAIN)
            {
                expansionParent = this.villainExpansionParent;
            }
            else if (deck.kind == DeckKind.ENVIRONMENT)
            {
                expansionParent = this.environmentExpansionParent;
            }
            else if (deck.kind == DeckKind.VILLAIN_TEAM)
            {
                expansionParent = this.teamVillainExpansionParent;
            }

            if (expansionParent != null)
            {
                Expansion expansion = expansionParent.GetChild(deck.sourceExpansionIdentifier)
                    ?? expansionParent.AddChild(new Expansion(deck.sourceExpansionIdentifier));

                expansion.AddChild(deck);
            }
        }

        public IEnumerable<Expansion> GetAllExpansions()
        {
            return new List<AncestorGroup<Expansion>> {
                this.heroExpansionParent,
                this.villainExpansionParent,
                this.environmentExpansionParent,
                this.teamVillainExpansionParent,
            }
                .SelectMany(parent => parent.GetChildren());
        }

        public IEnumerable<Deck> GetAllDecks()
        {
            return this.GetAllExpansions()
                .SelectMany(expansion => expansion.GetChildren());
        }

        public override int GetHashCode()
        {
            return this.GetIdentifier().GetHashCode();
        }
    }
}
