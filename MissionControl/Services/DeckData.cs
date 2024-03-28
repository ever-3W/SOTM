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

        private const int VARIANT_LINE_CHAR_LIMIT = 25;

        private Dictionary<string, string> cardDisplaySpecialCases = new()
        {
            { "Accelerated Evolution Anathema", "Acc. Evolution Anathema" },
            { "Swarm Eater: Distributed Hivemind", "Swarm Eater: Hivemind" },
            { "Tiamat, The Jaws of Winter", "Hydra Tiamat" },
            { "America's Newest Legacy", "Newest Legacy" },
            { "America's Greatest Legacy", "Greatest Legacy" },
            { "Prime Wardens Argent Adept", "Prime Wardens" },
            { "Xtreme Prime Wardens Argent Adept", "Xtreme Prime Wardens" },
            { "Dark Conductor Argent Adept", "Dark Conductor" },
            { "One With Freedom Doctor Metropolis", "One With Freedom"},
            { "Tsukiko Tanner: The Game is Rigged", "The Game is Rigged" },
            { "Madame Mittermeier's Fantastical Festival of Conundrums and Curiosities", "MMFFCC"},
            { "F.S.C. Continuance Wanderer", "Continuance Wanderer"},
            { "Halberd Experimental Research Center", "Halberd Research Center"},
            { "The Chasm of a Thousand Nights", "Chasm of 1,000 Nights"},
            { "Catchwater Harbor 1929", "Catchwater Harbor, 1929" }
        };
        public string GetVariantDisplayTitle (string variantTitle, string deckTitle)
        {
            if (cardDisplaySpecialCases.ContainsKey(variantTitle))
            {
                return cardDisplaySpecialCases[variantTitle];
            }
            if (variantTitle.Length >= VARIANT_LINE_CHAR_LIMIT)
            {
                if (variantTitle.StartsWith("The "))
                {
                    return variantTitle.Replace("The ", "");
                }
                if (variantTitle.StartsWith("Ministry Of Strategic Science", StringComparison.CurrentCultureIgnoreCase))
                {
                    return variantTitle.Replace("Ministry Of Strategic Science", "MSS");
                }
                if (variantTitle.StartsWith(deckTitle, StringComparison.CurrentCultureIgnoreCase))
                {
                    return Regex.Replace(variantTitle, $"{deckTitle}(\\:?)(,?)( ?)", "", RegexOptions.IgnoreCase);
                }
                if (variantTitle.EndsWith(deckTitle, StringComparison.CurrentCultureIgnoreCase))
                {
                    return Regex.Replace(variantTitle, $"{deckTitle}( ?)", "");
                }
            }
            return variantTitle;
        }
    }
}
