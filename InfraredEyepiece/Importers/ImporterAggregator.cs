using System.Text.Json;
using System.Text.Json.Serialization;
using SOTM.Shared.Models;

namespace SOTM.InfraredEyepiece.Importers
{
    public class ImporterAggregator : ImportedData
    {

        public ImporterAggregator AddFromImporter(CollectionImporter ci)
        {
            var (
                collectionIdentifier,
                collectionTitle,
                expansions,
                decks,
                hangingVariants
            ) = ci.ParseResources();
            
            this.collections.Add(new CollectionEntry()
            {
                identifier = collectionIdentifier,
                title = collectionTitle,
                expansions = expansions,
                heroes = decks.FindAll(deck => deck.kind == DeckKind.HERO).ToList(),
                villains = decks.FindAll(deck => deck.kind == DeckKind.VILLAIN).ToList(),
                environments = decks.FindAll(deck => deck.kind == DeckKind.ENVIRONMENT).ToList(),
                teamVillains = decks.FindAll(deck => deck.kind == DeckKind.VILLAIN_TEAM).ToList()
            });

            foreach (var kv in hangingVariants)
            {
                if (this.hangingDeckVariants.ContainsKey(kv.Key))
                {
                    this.hangingDeckVariants[kv.Key].AddRange(kv.Value);
                }
                else
                {
                    this.hangingDeckVariants[kv.Key] = kv.Value;
                }
            }
            return this;
        }

        public ImporterAggregator ResolveHangingVariants()
        {
            foreach (CollectionEntry collection in this.collections)
            {
                foreach (Deck deck in collection.GetAllDecks())
                {
                    string dne = deck.GetNamespacedIdentifier();
                    if (this.hangingDeckVariants.ContainsKey(dne))
                    {
                        Console.Error.WriteLine($"Resolved {dne} as {deck.identifier}");
                        foreach (HangingDeckVariant hdv in this.hangingDeckVariants[dne])
                        {
                            deck.AddChild(new DeckVariant(deck.identifier.CreateChildIdentifier(hdv.variantIdentifier))
                            {
                                title = hdv.promoTitle,
                                sourceExpansionIdentifier = hdv.sourceExpansionIdentifier,
                                sourceCollectionIdentifier = hdv.sourceCollectionIdentifier
                            });
                        }
                        this.hangingDeckVariants.Remove(dne);
                    }
                }
            }
            return this;
        }
    }
}
