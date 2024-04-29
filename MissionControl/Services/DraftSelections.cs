using SOTM.Shared.Models;
using SOTM.MissionControl.Models;
using System.Text.Json.Serialization;

namespace SOTM.MissionControl.Services
{
    public class DraftSelectionsModel
    {
        [JsonInclude]
        public Dictionary<string, DeckVariantState> variantStates = new();
    }

    public class DraftSelectionsService
    {
        public const string DRAFT_SELECTIONS_STORAGE_KEY = "DraftSelections";
        public GenericRepository<DraftSelectionsModel> repo = new(DRAFT_SELECTIONS_STORAGE_KEY, new());

        public void SetPickableVariants(HashSet<GlobalIdentifier> pickableVariants)
        {
            foreach (string identifier in this.repo.value.variantStates.Keys)
            {
                if (!pickableVariants.Contains(new GlobalIdentifier(identifier)))
                {
                    this.repo.value.variantStates[identifier] = DeckVariantState.BANNED;
                }
            }
            foreach (GlobalIdentifier identifier in pickableVariants)
            {
                string identifierKey = identifier.ToString();
                this.repo.value.variantStates[identifierKey] = DeckVariantState.PICKABLE;
            }
        }

        public bool VariantIsPickable(GlobalIdentifier identifier)
        {
            string identifierKey = identifier.ToString();
            if (!this.repo.value.variantStates.ContainsKey(identifierKey))
            {
                // New decks should be pickable by default
                this.repo.value.variantStates[identifierKey] = DeckVariantState.PICKABLE;
            }
            return this.repo.value.variantStates[identifierKey] == DeckVariantState.PICKABLE;
        }

        public HashSet<GlobalIdentifier> GetPickableVariantSet()
        {
            return this.repo.value.variantStates.Where(kv => kv.Value == DeckVariantState.PICKABLE).Select(kv => new GlobalIdentifier(kv.Key)).ToHashSet();
        }
    }
}
