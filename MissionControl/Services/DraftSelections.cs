using System.Text.Json.Serialization;
using Blazored.LocalStorage;
using SOTM.Shared.Models;
using SOTM.MissionControl.Models;

namespace SOTM.MissionControl.Services
{
    public class DraftSelections
    {
        [JsonInclude]
        public Dictionary<string, DeckVariantState> variantStates = new();
    }

    public class DraftSelectionsService: DraftSelections
    {
        public const string DRAFT_SELECTIONS_STORAGE_KEY = "DraftSelections";
        public void SetPickableVariants(HashSet<GlobalIdentifier> pickableVariants)
        {
            foreach (string identifier in this.variantStates.Keys)
            {
                if (!pickableVariants.Contains(new GlobalIdentifier(identifier)))
                {
                    this.variantStates[identifier] = DeckVariantState.BANNED;
                }
            }
            foreach (GlobalIdentifier identifier in pickableVariants)
            {
                string identifierKey = identifier.ToString();
                this.variantStates[identifierKey] = DeckVariantState.PICKABLE;
            }
        }

        public bool VariantIsPickable(GlobalIdentifier identifier)
        {
            string identifierKey = identifier.ToString();
            if (!this.variantStates.ContainsKey(identifierKey))
            {
                // New decks should be pickable by default
                this.variantStates[identifierKey] = DeckVariantState.PICKABLE;
            }
            return this.variantStates[identifierKey] == DeckVariantState.PICKABLE;
        }

        public async Task SavePickableVariants(ILocalStorageService storageService)
        {
            await storageService.SetItemAsync(DRAFT_SELECTIONS_STORAGE_KEY, this);
        }

        public async Task LoadPickableVariants(ILocalStorageService storageService)
        {
            if (await storageService.ContainKeyAsync(DRAFT_SELECTIONS_STORAGE_KEY))
            {
                this.variantStates = (await storageService.GetItemAsync<DraftSelections>(DRAFT_SELECTIONS_STORAGE_KEY))?.variantStates ?? this.variantStates;
            }
        }

        public HashSet<GlobalIdentifier> GetPickableVariantSet()
        {
            return this.variantStates.Where(kv => kv.Value == DeckVariantState.PICKABLE).Select(kv => new GlobalIdentifier(kv.Key)).ToHashSet();
        }
    }
}
