using SOTM.Shared.Models;
using SOTM.MissionControl.Models;

namespace SOTM.MissionControl.Services
{
    public class DraftSelectionsService
    {
        public const string DRAFT_SELECTIONS_STORAGE_KEY = "DraftSelections";
        public GenericRepository<Dictionary<string, DeckVariantState>> repo = new(DRAFT_SELECTIONS_STORAGE_KEY, new());

        public Dictionary<string, DeckVariantState> variantStates
        {
            get => this.repo.value;
            set => this.repo.value = value;
        }
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

        public HashSet<GlobalIdentifier> GetPickableVariantSet()
        {
            return this.variantStates.Where(kv => kv.Value == DeckVariantState.PICKABLE).Select(kv => new GlobalIdentifier(kv.Key)).ToHashSet();
        }
    }
}
