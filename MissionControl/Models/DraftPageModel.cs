// using SOTM.Shared.Models;

// namespace SOTM.MissionControl.Models
// {

//     public class DraftPageModel
//     {
//         public List<Expansion> heroExpansions = new();
//         public List<Expansion> villainExpansions = new();
//         public List<Expansion> environmentExpansions = new();

//         public Dictionary<GlobalIdentifier, DeckVariantState> variantStates = new();

//         private List<Expansion> GroupByDeckExpansion(List<Deck> decks)
//         {
//             var definedExpansions = new Dictionary<GlobalIdentifier, Expansion>();
//             foreach (Deck deck in decks)
//             {
//                 if (!definedExpansions.ContainsKey(deck.sourceExpansionIdentifier))
//                 {
//                     definedExpansions[deck.sourceExpansionIdentifier] = new Expansion(deck.sourceExpansionIdentifier);
//                 }
//                 definedExpansions[deck.sourceExpansionIdentifier].AddChild(deck);
//                 foreach (DeckVariant variant in deck.GetChildren())
//                 {
//                     this.variantStates.Add(variant.identifier, DeckVariantState.PICKABLE);
//                 }
//             }
//             return definedExpansions.Values.ToList();
//         }

//         public void ImportCollectionEntry(CollectionEntry ce)
//         {
//             this.heroExpansions.AddRange(GroupByDeckExpansion(ce.heroes));
//             this.villainExpansions.AddRange(GroupByDeckExpansion(ce.villains));
//             this.environmentExpansions.AddRange(GroupByDeckExpansion(ce.environments));
//         }
//     }
// }
