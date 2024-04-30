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
        public string color;
        // the *Variant fields store the number of non-base variants
        [JsonInclude]
        public int heroDeckCount;
        [JsonInclude]
        public int heroVariantCount;
        [JsonInclude]
        public int villainDeckCount;
        [JsonInclude]
        public int villainVariantCount;
        [JsonInclude]
        public int environmentDeckCount;
        [JsonInclude]
        public int environmentVariantCount;
        [JsonInclude]
        public int teamVillainDeckCount;
        [JsonInclude]
        public int teamVillainVariantCount;
        [JsonInclude]
        public int promoVariantCount;
        [JsonInclude]
        public CollectionSource source;

        private static (int, int) GetDeckAndVariantCounts(IEnumerable<Expansion> expansions)
        {
            var decks = expansions.SelectMany(expansion => expansion.GetChildren());
            return (decks.Count(), Enumerable.Sum(decks.Select(deck => deck.GetChildren().Count() - 1)));
        }

        public static CollectionViewModel CreateViewModel(Collection collection, CollectionSource source)
        {
            var (heroDeckCount, heroVariantCount) = GetDeckAndVariantCounts(collection.heroExpansions);
            var (villainDeckCount, villainVariantCount) = GetDeckAndVariantCounts(collection.villainExpansions);
            var (environmentDeckCount, environmentVariantCount) = GetDeckAndVariantCounts(collection.environmentExpansions);
            var (teamVillainDeckCount, teamVillainVariantCount) = GetDeckAndVariantCounts(collection.teamVillainExpansions);
            
            return new CollectionViewModel()
            {
                identifier = collection.identifier,
                title = collection.title,
                color = collection.color,
                heroDeckCount = heroDeckCount,
                heroVariantCount = heroVariantCount,
                villainDeckCount = villainDeckCount,
                villainVariantCount = villainVariantCount,
                environmentDeckCount = environmentDeckCount,
                environmentVariantCount = environmentVariantCount,
                teamVillainDeckCount = teamVillainDeckCount,
                teamVillainVariantCount = teamVillainVariantCount,
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

        public bool AddImported(Collection collection)
        {
            string collectionKey = collection.GetIdentifier().ToString();
            if (this.presetCollections.FindIndex(existing => existing.identifier.ToString() == collectionKey) >= 0)
            {
                Console.Error.WriteLine($"\"{collection.title}\" is a server-side collection, it cannot be imported.");
                return false;
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
                return true;
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
        private readonly GenericRepository<DeckDataModel> repo = new(DECK_DATA_STORAGE_KEY, new());
        public Func<ILocalStorageService, Task> SaveDeckData;
        public Func<ILocalStorageService, Task<DeckDataModel>> LoadDeckData;
        private const string LOADED_MANIFEST_STORAGE_KEY = "LoadedCollectionManifest";
        private readonly GenericRepository<CollectionManifest> manifestRepo = new(LOADED_MANIFEST_STORAGE_KEY, new());
        public Func<ILocalStorageService, Task> SaveManifestData;
        public Func<ILocalStorageService, Task<CollectionManifest>> LoadManifestData;

        public DeckDataService()
        {
            this.SaveDeckData = repo.Save;
            this.LoadDeckData = repo.Load;
            this.SaveManifestData = manifestRepo.Save;
            this.LoadManifestData = manifestRepo.Load;
        }

        public IEnumerable<Collection> Collections => this.repo.value.GetAllCollections();

        public CollectionManifest CollectionManifest
        {
            set => this.manifestRepo.value = value;
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

        public IEnumerable<DeckVariant> GetAllVariants(Deck deck)
        {
            string dne = deck.GetNamespacedIdentifier();
            IEnumerable<DeckVariant> promoVariants = this.Collections.SelectMany(collection => collection.hangingVariants.GetValueOrDefault(dne, []));
            return deck.GetChildren().Concat(promoVariants);
        }

        public Collection GetVariantCollection(GlobalIdentifier identifier)
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
            { color = this.GetVariantCollection(variant.identifier).color };
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
                parentsEnumerable = this.Collections.SelectMany(collection => collection.heroExpansions);
            } else if (kind == DeckKind.VILLAIN)
            {
                parentsEnumerable = this.Collections.SelectMany(collection => collection.villainExpansions);
            } else if (kind == DeckKind.ENVIRONMENT)
            {
                parentsEnumerable = this.Collections.SelectMany(collection => collection.environmentExpansions);
            } else if (kind == DeckKind.VILLAIN_TEAM)
            {
                parentsEnumerable = this.Collections.SelectMany(collection => collection.teamVillainExpansions);
            }
            return parentsEnumerable.SelectMany(expansion => expansion.GetChildren().Select(deck => this.GetAllVariants(deck)));
        }

        public void BuildFromSourceCollections(IEnumerable<Collection?> sourceCollections)
        {
            foreach (Collection? collection in sourceCollections)
            {
                if (collection != null)
                {
                    this.repo.value.AddPreset(collection);
                    this.BuildCollectionDataTables(collection);
                }
            }
        }

        public async Task Load(ILocalStorageService storageService)
        {
            await this.repo.Load(storageService);
            foreach (Collection collection in this.Collections)
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

        public IEnumerable<CollectionViewModel> CollectionViewModels =>
            this.repo.value.presetCollections
                .Select(collection => CollectionViewModel.CreateViewModel(collection, CollectionSource.PRESET))
                .Concat
                (
                    this.repo.value.importedCollections
                        .Select(collection => CollectionViewModel.CreateViewModel(collection, CollectionSource.IMPORTED))
                );

        public void ImportCollection(Collection collection)
        {
            if (this.repo.value.AddImported(collection))
            {
                this.BuildCollectionDataTables(collection);
            }
        }

        public void RemoveImportedCollection(GlobalIdentifier collectionIdentifier)
        {
            this.repo.value.RemoveImported(collectionIdentifier);
        }
    }
}
