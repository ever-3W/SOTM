using System.Text.Json.Serialization;
using Blazored.LocalStorage;
using SOTM.Shared.Models;

namespace SOTM.MissionControl.Services
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum CollectionSource
    {
        PRESET,
        IMPORTED
    }

    public class CollectionMetadata
    {
        [JsonInclude]
        public GlobalIdentifier identifier;
        [JsonInclude]
        public string title;
        [JsonInclude]
        public int heroDeckCount;
        [JsonInclude]
        public int villainDeckCount;
        [JsonInclude]
        public int environmentDeckCount;
        [JsonInclude]
        public int teamVillainDeckCount;
        [JsonInclude]
        public int promoVariantCount;
        [JsonInclude]
        public CollectionSource source;

        public static CollectionMetadata CreateMetadata(CollectionV2 collection, CollectionSource source)
        {
            return new CollectionMetadata()
            {
                identifier = collection.identifier,
                title = collection.title,
                heroDeckCount = collection.heroExpansions.SelectMany(expansion => expansion.GetChildren()).Count(),
                villainDeckCount = collection.villainExpansions.SelectMany(expansion => expansion.GetChildren()).Count(),
                environmentDeckCount = collection.environmentExpansions.SelectMany(expansion => expansion.GetChildren()).Count(),
                teamVillainDeckCount = collection.teamVillainExpansions.SelectMany(expansion => expansion.GetChildren()).Count(),
                promoVariantCount = collection.hangingVariants.Values.SelectMany(variant => variant).Count(),
                source = source
            };
        }
    }

    public class DeckDataModel
    {
        [JsonInclude]
        public List<CollectionV2> presetCollections = new();
        [JsonInclude]
        public List<CollectionV2> importedCollections = new();

        public void AddPreset(CollectionV2 collection)
        {
            string collectionKey = collection.GetIdentifier().ToString();
            int collectionIndex = this.presetCollections.FindIndex(existing => existing.identifier.ToString() == collectionKey);
            if (collectionIndex >= 0)
            {
                this.presetCollections[collectionIndex] = collection;
            }
            else
            {
                this.presetCollections.Add(collection);
            }
        }

        public void AddImported(CollectionV2 collection)
        {
            string collectionKey = collection.GetIdentifier().ToString();
            if (this.presetCollections.FindIndex(existing => existing.identifier.ToString() == collectionKey) >= 0)
            {
                Console.Error.WriteLine($"\"{collection.title}\" is a preset collection, it cannot be imported.");
            }
            else
            {
                int collectionIndex = this.importedCollections.FindIndex(existing => existing.identifier.ToString() == collectionKey);
                if (collectionIndex >= 0)
                {
                    this.importedCollections[collectionIndex] = collection;
                }
                else
                {
                    this.importedCollections.Add(collection);
                }
            }
        }

        public void RemoveImported(GlobalIdentifier collectionIdentifier)
        {
            this.importedCollections = this.importedCollections.Where(collection => !collection.identifier.Equals(collectionIdentifier)).ToList();
        }

        public IEnumerable<CollectionV2> GetAllCollections()
        {
            return this.presetCollections.Concat(this.importedCollections);
        }
    }

    public class DeckDataService
    {
        private const string DECK_DATA_STORAGE_KEY = "DeckDataModel";
        public GenericRepository<DeckDataModel> repo = new(DECK_DATA_STORAGE_KEY, new());

        private const string LOADED_MANIFEST_STORAGE_KEY = "LoadedCollectionManifest";
        public GenericRepository<CollectionManifest> manifestRepo = new(LOADED_MANIFEST_STORAGE_KEY, new());

        public DeckDataModel model
        {
            get => this.repo.value;
            set => this.repo.value = value;
        }

        // Used for data lookup
        private Dictionary<GlobalIdentifier, DeckVariant> deckVariantTable = new();
        private void BuildCollectionDataTables(CollectionV2 collection)
        {
            foreach (DeckVariant variant in collection.GetAllDecks().SelectMany(deck => deck.GetChildren()))
            {
                this.deckVariantTable[variant.identifier] = variant;
            }
            foreach (DeckVariant variant in collection.hangingVariants.Values.SelectMany(variant => variant))
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
            IEnumerable<DeckVariant> promoVariants = this.model.GetAllCollections().SelectMany(collection => collection.hangingVariants.GetValueOrDefault(dne, []));
            return deck.GetChildren().Concat(promoVariants);
        }

        public IEnumerable<IEnumerable<DeckVariant>> VariantsByKindGroupedByDeck(DeckKind kind)
        {
            IEnumerable<Expansion> parentsEnumerable = [];
            if (kind == DeckKind.HERO)
            {
                parentsEnumerable = this.model.GetAllCollections().SelectMany(collection => collection.heroExpansions);
            } else if (kind == DeckKind.VILLAIN)
            {
                parentsEnumerable = this.model.GetAllCollections().SelectMany(collection => collection.villainExpansions);
            } else if (kind == DeckKind.ENVIRONMENT)
            {
                parentsEnumerable = this.model.GetAllCollections().SelectMany(collection => collection.environmentExpansions);
            } else if (kind == DeckKind.VILLAIN_TEAM)
            {
                parentsEnumerable = this.model.GetAllCollections().SelectMany(collection => collection.teamVillainExpansions);
            }
            return parentsEnumerable.SelectMany(expansion => expansion.GetChildren().Select(deck => this.GetAllVariants(deck)));
        }

        public void BuildFromSourceCollections(IEnumerable<CollectionV2?> sourceCollections)
        {
            foreach (CollectionV2? collection in sourceCollections)
            {
                if (collection != null)
                {
                    this.model.AddPreset(collection);
                    this.BuildCollectionDataTables(collection);
                }
            }
        }

        public async Task Load(ILocalStorageService storageService)
        {
            await this.repo.Load(storageService);
            foreach (CollectionV2 collection in this.model.GetAllCollections())
            {
                this.BuildCollectionDataTables(collection);
            }
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
            return this.model.GetAllCollections().SelectMany(collection => collection.heroExpansions);
        }

        public IEnumerable<Expansion> GetVillainExpansions()
        {
            return this.model.GetAllCollections().SelectMany(collection => collection.villainExpansions);
        }

        public IEnumerable<Expansion> GetEnvironmentExpansions()
        {
            return this.model.GetAllCollections().SelectMany(collection => collection.environmentExpansions);
        }

        public IEnumerable<CollectionMetadata> GetAllCollectionMetadata()
        {
            return 
                this.model.presetCollections
                    .Select(collection => CollectionMetadata.CreateMetadata(collection, CollectionSource.PRESET))
                .Concat
                (
                    this.model.importedCollections
                        .Select(collection => CollectionMetadata.CreateMetadata(collection, CollectionSource.IMPORTED))
                );
        }
    }
}
