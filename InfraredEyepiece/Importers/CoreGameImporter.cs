using Microsoft.Extensions.Configuration;
using SOTM.Shared.Models;

namespace SOTM.InfraredEyepiece.Importers
{
    public class CoreGameImporter : CollectionImporter
    {
        private GlobalIdentifier _collectionIdentifier = new GlobalIdentifier(CollectionV2.BASE_COLLECTION_IDENTIFIER);
        protected override GlobalIdentifier collectionIdentifier { get => _collectionIdentifier; }

        public CoreGameImporter(IConfigurationRoot config): base(config)
        {}

        public override CollectionV2 ParseResourcesV2()
        {
            CollectionV2 result = new CollectionV2(this._collectionIdentifier, CollectionV2.BASE_COLLECTION_IDENTIFIER);
            // the core DLL shouldn't contain any hanging variants
            var (decks, hangingVariants) = this.ParseResourcesFromDLL(this.config["VanillaDLLPath"]);
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
    }
}