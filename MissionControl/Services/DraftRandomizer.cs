using SOTM.Shared.Models;
using SOTM.MissionControl.Models;

namespace SOTM.MissionControl.Services
{
    public enum RandomizerMethod
    {
        RANDOMIZE_BY_DECK,
        RANDOMIZE_BY_VARIANT
    }
    public class DraftRandomizerService
    {
        private Random random = new();

        // Select k items randomly from a source enumerable.
        // Each item has an equal probability to be selected.
        private IEnumerable<T> SelectKFrom<T>(IEnumerable<T> enumerable, int k)
        {
            int remainingIndices = enumerable.Count();
            SortedSet<int> selectedIndices = new();

            while (remainingIndices > 0 && selectedIndices.Count() < k)
            {
                int nextIndex = this.random.Next(remainingIndices);
                int trueIndex = nextIndex;

                int offset = selectedIndices.GetViewBetween(0, trueIndex).Count();
                while (trueIndex - offset != nextIndex)
                {
                    trueIndex += nextIndex - (trueIndex - offset);
                    offset = selectedIndices.GetViewBetween(0, trueIndex).Count();
                };
                selectedIndices.Add(trueIndex);
                remainingIndices--;
            }

            return selectedIndices.Select(index => enumerable.ElementAt(index)).ToList();
        }
        public List<GlobalIdentifier> DraftRandomVariants (DeckDataService deckData, DeckKind kind, RandomizerMethod method, HashSet<GlobalIdentifier> pickableVariants, int length)
        {
            var groupedVariants = deckData.VariantsByKindGroupedByDeck(kind);
            if (method == RandomizerMethod.RANDOMIZE_BY_DECK)
            {
                // Each deck has an equal probability to be selected.
                // Once the decks are selected, a non-banned variant is selected from it at random.
                return SelectKFrom
                (
                    groupedVariants
                        .Select(deckVariants => deckVariants.Where(variant => pickableVariants.Contains(variant.identifier)))
                        .Where(deckVariants => deckVariants.Count() > 0), 
                    length
                ).SelectMany(deckVariants => SelectKFrom(deckVariants, 1)
                    .Select(variant => variant.identifier)
                ).ToList();
            }
            else
            {
                // Each non-banned variant has an equal probability to be selected.
                return SelectKFrom
                (
                    groupedVariants
                        .SelectMany(deckVariants => deckVariants)
                        .Where(variant => pickableVariants.Contains(variant.identifier))
                        .Select(variant => variant.identifier),
                    length
                ).ToList();
            }
        }

        public Draft DraftRandomClassicGame(DeckDataService deckData, SettingsService settingsSvc, HashSet<GlobalIdentifier> pickableVariants)
        {
            return new Draft()
            {
                heroes = DraftRandomVariants(deckData, DeckKind.HERO, settingsSvc.HeroRandomizerMethod, pickableVariants, settingsSvc.DraftHeroCount),
                villains = DraftRandomVariants(deckData, DeckKind.VILLAIN, settingsSvc.VillainRandomizerMethod, pickableVariants, 1),
                environments = DraftRandomVariants(deckData, DeckKind.ENVIRONMENT, RandomizerMethod.RANDOMIZE_BY_VARIANT, pickableVariants, 1)
            };
        }
    }
}