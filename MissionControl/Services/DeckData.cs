using System.Text.RegularExpressions;
using Blazored.LocalStorage;
using SOTM.Shared.Models;

namespace SOTM.MissionControl.Services
{
    public class FetchCollectionResult
    {
        public CollectionV2 collection;
        public bool hashMatch;
    }
    public class DeckDataService
    {
        private const string DECK_DATA_STORAGE_KEY = "DeckData";
        public GenericRepository<CollectionV2> repo = new(DECK_DATA_STORAGE_KEY, new(new GlobalIdentifier("Root"), "Root"));

        private const string LOADED_MANIFEST_STORAGE_KEY = "LoadedCollectionManifest";
        public GenericRepository<CollectionManifest> manifestRepo = new(LOADED_MANIFEST_STORAGE_KEY, new());

        public CollectionV2 collection
        {
            get => this.repo.value;
            set => this.repo.value = value;
        }

        // Used for data lookup
        private Dictionary<GlobalIdentifier, DeckVariant> deckVariantTable = new();

        private void buildDataTables()
        {
            foreach (Deck deck in this.collection.GetAllDecks())
            {
                foreach (DeckVariant variant in deck.GetChildren())
                {
                    this.deckVariantTable[variant.identifier] = variant;
                }
            }
        }

        public IEnumerable<CollectionManifestDelta> ListManifestDeltas (CollectionManifest other)
        {
            return CollectionManifest.ListDeltas(this.manifestRepo.value, other);
        }

        public void BuildFromSourceCollections(IEnumerable<CollectionV2?> sourceCollections)
        {
            this.collection = new(new GlobalIdentifier("Root"), "Root");
            foreach (CollectionV2? collection in sourceCollections)
            {
                if (collection != null)
                {
                    this.collection.MergeWith(collection);
                }
            }
            this.collection.ResolveHangingVariants();
            this.buildDataTables();
        }

        public async Task Load(ILocalStorageService storageService)
        {
            await this.repo.Load(storageService);
            this.buildDataTables();
        }

        public DeckVariant? GetVariantData(GlobalIdentifier? identifier)
        {
            if (identifier == null)
            {
                return null;
            }
            return this.deckVariantTable.GetValueOrDefault(identifier);
        }

        public IEnumerable<Expansion> GetHeroExpansions()
        {
            return this.collection.heroExpansions;
        }

        public IEnumerable<Expansion> GetVillainExpansions()
        {
            return this.collection.villainExpansions;
        }

        public IEnumerable<Expansion> GetEnvironmentExpansions()
        {
            return this.collection.environmentExpansions;
        }
    }
}
