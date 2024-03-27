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

        private List<T> SelectKFrom<T>(IEnumerable<T> enumerable, int k)
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
        public List<GlobalIdentifier> DraftRandomVariants (RandomizerMethod method, HashSet<GlobalIdentifier> pickableVariants, IEnumerable<Expansion> expansions, int length)
        {
            if (method == RandomizerMethod.RANDOMIZE_BY_DECK)
            {
                // Each deck has an equal probability to be selected.
                // Once the decks are selected, a non-banned variant is selected from it at random.
                return SelectKFrom
                (
                    expansions
                        .SelectMany(expansion => expansion.GetChildren())
                        .Where(deck => 
                            deck
                                .GetChildren()
                                .Where(variant => pickableVariants.Contains(variant.identifier))
                                .Count() > 0)
                        .ToArray(), 
                    length
                ).SelectMany(deck => SelectKFrom(
                    deck
                        .GetChildren()
                        .Where(variant => pickableVariants.Contains(variant.identifier))
                        .Select(variant => variant.identifier)
                        .ToArray(),
                    1
                )).ToList();
            }
            else
            {
                // Each non-banned variant has an equal probability to be selected.
                return SelectKFrom
                (
                    expansions
                        .SelectMany(expansion => expansion.GetChildren())
                        .SelectMany(deck => deck.GetChildren())
                        .Where(variant => pickableVariants.Contains(variant.identifier))
                        .Select(variant => variant.identifier)
                        .ToArray(),
                    length
                ).ToList();
            }
        }

        public Draft DraftRandomClassicGame(DeckDataService deckData, SettingsService settingsSvc, HashSet<GlobalIdentifier> pickableVariants)
        {
            return new Draft()
            {
                heroes = DraftRandomVariants(settingsSvc.settings.heroRandomizerMethod, pickableVariants, deckData.GetHeroExpansions(), settingsSvc.settings.draftHeroCount),
                villains = DraftRandomVariants(settingsSvc.settings.villainRandomizerMethod, pickableVariants, deckData.GetVillainExpansions(), 1),
                environments = DraftRandomVariants(RandomizerMethod.RANDOMIZE_BY_VARIANT, pickableVariants, deckData.GetEnvironmentExpansions(), 1)
            };
        }
    }
}