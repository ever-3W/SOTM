using Microsoft.Extensions.Configuration;
using SOTM.Shared.Models;

namespace SOTM.InfraredEyepiece.Importers
{
    public class CoreGameImporter : CollectionImporter
    {
        private GlobalIdentifier _collectionIdentifier = new GlobalIdentifier(Collection.BASE_COLLECTION_IDENTIFIER);
        protected override GlobalIdentifier collectionIdentifier { get => _collectionIdentifier; }

        public CoreGameImporter(IConfigurationRoot config): base(config)
        {}

        public override CollectionV2 ParseResourcesV2()
        {
            CollectionV2 result = new CollectionV2(this._collectionIdentifier, Collection.BASE_COLLECTION_IDENTIFIER);
            // the core DLL shouldn't contain any hanging variants
            var (expansions, decks, hangingVariants) = this.ParseResourcesFromDLL(this.config["VanillaDLLPath"]);
            result.hangingVariants = hangingVariants;
            foreach (Deck deck in decks)
            {
                // Override OblivAeon's "kind" to prevent him from being selected for drafting Clasic games
                if (deck.title != null && deck.title.EndsWith("OblivAeon"))
                {
                    deck.kind = DeckKind.VILLAIN_OBLIVAEON;
                }
                result.AddDeck(deck);
            }
            return result;
        }

        public override
        (
            GlobalIdentifier,
            string,
            List<GlobalIdentifier>,
            List<Deck>,
            Dictionary<string, List<HangingDeckVariant>>
        ) ParseResources()
        {
            // the core DLL shouldn't contain any hanging variants
            var (expansions, decks, hangingVariants) = this.ParseResourcesFromDLL(this.config["VanillaDLLPath"]);
            return
            (
                this._collectionIdentifier,
                Collection.BASE_COLLECTION_IDENTIFIER,
                expansions,
                // Override OblivAeon's "kind" to prevent him from being selected for drafting Clasic games
                decks.Select(deck =>
                    {
                        if (deck.title != null && deck.title.EndsWith("OblivAeon"))
                        {
                            deck.kind = DeckKind.VILLAIN_OBLIVAEON;
                        }
                        return deck;
                    }).ToList(),
                hangingVariants
            );
        }
    }
}