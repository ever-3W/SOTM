using SOTM.Shared.Models;

namespace SOTM.InfraredEyepiece.Importers
{
    public class CoreGameImporter : CollectionImporter
    {
        // For Windows
        private const string CORE_DLL_PATH = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Sentinels of the Multiverse\\Sentinels_Data\\Managed\\SentinelsEngine.dll";

        private GlobalIdentifier _collectionIdentifier = new GlobalIdentifier(Collection.BASE_COLLECTION_IDENTIFIER);
        protected override GlobalIdentifier collectionIdentifier { get => _collectionIdentifier; }

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
            var (expansions, decks, hangingVariants) = this.ParseResourcesFromDLL(CORE_DLL_PATH);
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