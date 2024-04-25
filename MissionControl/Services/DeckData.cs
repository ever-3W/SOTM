using System.Text.Json.Serialization;
using Blazored.LocalStorage;
using SOTM.MissionControl.Models;
using SOTM.Shared.Models;

namespace SOTM.MissionControl.Services
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum CollectionSource
    {
        PRESET,
        IMPORTED
    }

    public class CollectionViewModel
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

        public static CollectionViewModel CreateMetadata(Collection collection, CollectionSource source)
        {
            return new CollectionViewModel()
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
        public List<Collection> presetCollections = new();
        [JsonInclude]
        public List<Collection> importedCollections = new();

        public void AddPreset(Collection collection)
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

        public void AddImported(Collection collection)
        {
            string collectionKey = collection.GetIdentifier().ToString();
            if (this.presetCollections.FindIndex(existing => existing.identifier.ToString() == collectionKey) >= 0)
            {
                Console.Error.WriteLine($"\"{collection.title}\" is a server-side collection, it cannot be imported.");
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

        public IEnumerable<Collection> GetAllCollections()
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
        private Dictionary<GlobalIdentifier, DeckVariant> variantIdentifierEntityTable = new();
        private Dictionary<GlobalIdentifier, Collection> variantIdentifierCollectionTable = new();
        private void BuildCollectionDataTables(Collection collection)
        {
            foreach (DeckVariant variant in collection.GetAllDecks().SelectMany(deck => deck.GetChildren()))
            {
                this.variantIdentifierEntityTable[variant.identifier] = variant;
                this.variantIdentifierCollectionTable[variant.identifier] = collection;
            }
            foreach (DeckVariant variant in collection.hangingVariants.Values.SelectMany(variant => variant))
            {
                this.variantIdentifierEntityTable[variant.identifier] = variant;
                this.variantIdentifierCollectionTable[variant.identifier] = collection;
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

        public Collection GetSourceCollection(GlobalIdentifier identifier)
        {
            return this.variantIdentifierCollectionTable[identifier];
        }

        public DeckVariantViewModel? GetVariantMetadata(DeckVariant? variant)
        {
            if (variant == null)
            {
                return null;
            }
            return new DeckVariantViewModel(variant) 
            { color = this.GetSourceCollection(variant.identifier).color };
        }

        public IEnumerable<DeckVariantViewModel> GetAllVariantViewModels(Deck deck)
        {
            return this.GetAllVariants(deck).Select(this.GetVariantMetadata);
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

        public void BuildFromSourceCollections(IEnumerable<Collection?> sourceCollections)
        {
            foreach (Collection? collection in sourceCollections)
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
            foreach (Collection collection in this.model.GetAllCollections())
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
            return this.variantIdentifierEntityTable.GetValueOrDefault(identifier);
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

        public IEnumerable<CollectionViewModel> GetAllCollectionViewModel()
        {
            return 
                this.model.presetCollections
                    .Select(collection => CollectionViewModel.CreateMetadata(collection, CollectionSource.PRESET))
                .Concat
                (
                    this.model.importedCollections
                        .Select(collection => CollectionViewModel.CreateMetadata(collection, CollectionSource.IMPORTED))
                );
        }
    }
}
