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
        private const string DECK_DATA_STORAGE_KEY = "DeckDataModel";
        public GenericRepository<List<CollectionV2>> repo = new(DECK_DATA_STORAGE_KEY, new());

        private const string LOADED_MANIFEST_STORAGE_KEY = "LoadedCollectionManifest";
        public GenericRepository<CollectionManifest> manifestRepo = new(LOADED_MANIFEST_STORAGE_KEY, new());

        public List<CollectionV2> model
        {
            get => this.repo.value;
            set => this.repo.value = value;
        }

        // Used for data lookup
        private Dictionary<GlobalIdentifier, DeckVariant> deckVariantTable = new();

        private void buildDataTables()
        {
            foreach (Deck deck in this.model.SelectMany(collection => collection.GetAllDecks()))
            {
                foreach (DeckVariant variant in deck.GetChildren())
                {
                    this.deckVariantTable[variant.identifier] = variant;
                }
            }
            foreach (DeckVariant variant in this.model.SelectMany(collection => collection.hangingVariants.Values.SelectMany(variant => variant)))
            {
                this.deckVariantTable[variant.identifier] = variant;
            }
        }

        public IEnumerable<CollectionManifestDelta> ListManifestDeltas (CollectionManifest other)
        {
            return CollectionManifest.ListDeltas(this.manifestRepo.value, other);
        }

        public IEnumerable<DeckVariant> GetAllVariants(Deck deck)
        {
            string dne = deck.GetNamespacedIdentifier();
            IEnumerable<DeckVariant> promoVariants = this.model.SelectMany(collection => collection.hangingVariants.GetValueOrDefault(dne, []));
            return new List<IEnumerable<DeckVariant>>()
            {
                deck.GetChildren(),
                promoVariants
            }.SelectMany(variant => variant);
        }

        public void BuildFromSourceCollections(IEnumerable<CollectionV2?> sourceCollections)
        {
            foreach (CollectionV2? collection in sourceCollections)
            {
                if (collection != null)
                {
                    this.model.Add(collection);
                }
            }
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
            return this.model.SelectMany(collection => collection.heroExpansions);
        }

        public IEnumerable<Expansion> GetVillainExpansions()
        {
            return this.model.SelectMany(collection => collection.villainExpansions);
        }

        public IEnumerable<Expansion> GetEnvironmentExpansions()
        {
            return this.model.SelectMany(collection => collection.environmentExpansions);
        }
    }
}
