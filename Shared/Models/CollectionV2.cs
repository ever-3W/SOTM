using System.Text.Json.Serialization;

namespace SOTM.Shared.Models
{
    public class CollectionV2
    {
        public const string BASE_COLLECTION_IDENTIFIER = "Vanilla";

        [JsonInclude]
        public GlobalIdentifier identifier;

        [JsonInclude]
        public string title;

        [JsonInclude]
        public int sortOrder = 0;

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
        public Dictionary<string, List<HangingDeckVariant>> hangingVariants;

        [JsonConstructor]
        public CollectionV2(GlobalIdentifier identifier, string title) {
            this.identifier = identifier;
            this.title = title;

            this.heroExpansionParent = new AncestorGroup<Expansion>(identifier);
            this.villainExpansionParent = new AncestorGroup<Expansion>(identifier);
            this.environmentExpansionParent = new AncestorGroup<Expansion>(identifier);
            this.teamVillainExpansionParent = new AncestorGroup<Expansion>(identifier);

            this.hangingVariants = new();
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

        public IEnumerable<Deck> GetAllDecks()
        {
            return new List<AncestorGroup<Expansion>> {
                this.heroExpansionParent,
                this.villainExpansionParent,
                this.environmentExpansionParent,
                this.teamVillainExpansionParent,
            }
                .SelectMany(parent => parent.GetChildren())
                .SelectMany(expansion => expansion.GetChildren());
        }

        public void MergeWith(CollectionV2 other)
        {
            this.heroExpansionParent.AddChildren(other.heroExpansionParent.GetChildren());
            this.villainExpansionParent.AddChildren(other.villainExpansionParent.GetChildren());
            this.environmentExpansionParent.AddChildren(other.environmentExpansionParent.GetChildren());
            this.teamVillainExpansionParent.AddChildren(other.teamVillainExpansionParent.GetChildren());

            foreach (var kv in other.hangingVariants)
            {
                if (this.hangingVariants.ContainsKey(kv.Key))
                {
                    this.hangingVariants[kv.Key].AddRange(kv.Value);
                }
                else
                {
                    this.hangingVariants[kv.Key] = kv.Value;
                }
            }
        }

        public void ResolveHangingVariants()
        {
            foreach (Deck deck in this.GetAllDecks())
            {
                string dne = deck.GetNamespacedIdentifier();
                if (this.hangingVariants.ContainsKey(dne))
                {
                    Console.Error.WriteLine($"Resolved {dne} as {deck.identifier}");
                    foreach (HangingDeckVariant hdv in this.hangingVariants[dne])
                    {
                        deck.AddChild(new DeckVariant(deck.identifier.CreateChildIdentifier(hdv.variantIdentifier))
                        {
                            title = hdv.promoTitle,
                            sourceExpansionIdentifier = hdv.sourceExpansionIdentifier,
                            sourceCollectionIdentifier = hdv.sourceCollectionIdentifier
                        });
                    }
                    this.hangingVariants.Remove(dne);
                }
            }
        }
    }
}
