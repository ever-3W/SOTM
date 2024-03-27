using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Blazored.LocalStorage;
using SOTM.Shared.Models;

namespace SOTM.MissionControl.Services
{
    public class DeckDataService
    {
        private ImportedData? data;
        private Dictionary<GlobalIdentifier, DeckVariant> deckVariantTable = new();
        private const string DECK_DATA_STORAGE_KEY = "DeckData";

        private void buildDataTables()
        {
            if (this.data != null)
            {
                foreach (CollectionEntry ce in this.data.collections)
                {
                    foreach (Deck deck in ce.GetAllDecks())
                    {
                        foreach (DeckVariant variant in deck.GetChildren())
                        {
                            this.deckVariantTable[variant.identifier] = variant;
                        }
                    }
                }
            }
        }
        private async Task LoadDataFromServer(HttpClient httpClient)
        {
            this.data = await httpClient.GetFromJsonAsync<ImportedData>("data/collections.json");
            this.buildDataTables();
        }
        private async Task TryLoadDataFromLocal(ILocalStorageService localStorage)
        {
            if (await localStorage.ContainKeyAsync(DECK_DATA_STORAGE_KEY))
            {
                this.data = await localStorage.GetItemAsync<ImportedData>(DECK_DATA_STORAGE_KEY);
                this.buildDataTables();
            }
        }

        private async Task SaveDataToLocal(ILocalStorageService storageService)
        {
            await storageService.SetItemAsync(DECK_DATA_STORAGE_KEY, this.data);
        }

        public async Task LoadData(ILocalStorageService localStorage, HttpClient httpClient)
        {
            if (this.data == null)
            {
                await this.TryLoadDataFromLocal(localStorage);
                if (this.data == null)
                {
                    await this.LoadDataFromServer(httpClient);
                    await this.SaveDataToLocal(localStorage);
                }
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

        private List<Expansion> GroupByDeckExpansion(List<Deck> decks)
        {
            var definedExpansions = new Dictionary<GlobalIdentifier, Expansion>();
            foreach (Deck deck in decks)
            {
                if (!definedExpansions.ContainsKey(deck.sourceExpansionIdentifier))
                {
                    definedExpansions[deck.sourceExpansionIdentifier] = new Expansion(deck.sourceExpansionIdentifier);
                }
                definedExpansions[deck.sourceExpansionIdentifier].AddChild(deck);
            }
            return definedExpansions.Values.ToList();
        }

        public List<Expansion> GetHeroExpansions()
        {
            return this.data?.collections.SelectMany(collection => this.GroupByDeckExpansion(collection.heroes)).ToList() ?? [];
        }

        public List<Expansion> GetVillainExpansions()
        {
            return this.data?.collections.SelectMany(collection => this.GroupByDeckExpansion(collection.villains)).ToList() ?? [];
        }

        public List<Expansion> GetEnvironmentExpansions()
        {
            return this.data?.collections.SelectMany(collection => this.GroupByDeckExpansion(collection.environments)).ToList() ?? [];
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
