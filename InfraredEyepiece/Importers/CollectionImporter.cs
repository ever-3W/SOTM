using System.Text.Json;
using System.Text.RegularExpressions;
using ICSharpCode.Decompiler.Metadata;
using SOTM.Shared.Models;
using SOTM.Shared.Models.JSONObjects;

namespace SOTM.InfraredEyepiece.Importers
{
    using JSONPromoCardList = Dictionary<string, Card[]>;

    public abstract class CollectionImporter
    {
        protected static JsonSerializerOptions JSON_SERIALIZER_OPTS = new JsonSerializerOptions()
        { AllowTrailingCommas = true, WriteIndented = true };

        protected abstract GlobalIdentifier collectionIdentifier { get; }

        private (GlobalIdentifier, Deck) ParseDeckList(JSONDeckList dl, string dlIdentifier)
        {
            GlobalIdentifier expansionIdentifier = this.collectionIdentifier.CreateChildIdentifier(
                dl.expansionIdentifier ?? Expansion.BASE_EXPANSION_IDENTIFIER);
            GlobalIdentifier deckIdentifier = expansionIdentifier.CreateChildIdentifier(dlIdentifier);
            Deck deckEntity = new Deck(deckIdentifier)
            {
                title = dl.name,
                kind = Deck.StringToKind(dl.kind),
                sourceExpansionIdentifier = expansionIdentifier
            };

            // Create the base variant
            GlobalIdentifier baseVariantIdentifier = deckEntity.identifier.CreateChildIdentifier(dlIdentifier);
            DeckVariant baseVariant = new DeckVariant(baseVariantIdentifier)
            {
                title = dl.name,
                sourceExpansionIdentifier = expansionIdentifier,
                sourceCollectionIdentifier = this.collectionIdentifier
            };
            deckEntity.AddChild(baseVariant);

            // Find "primary" card identifier, to be used in finding promo variants
            if (dl.initialCardIdentifiers != null)
            {
                Card? primaryCard = Array.Find(dl.cards, card => dl.initialCardIdentifiers.Contains(card.identifier));
                if (primaryCard != null)
                {
                    // baseVariant.title = primaryCard.title;
                    // List promo cards, if any
                    if (dl.promoCards != null)
                    {
                        foreach (Card pc in Array.FindAll(dl.promoCards, (card) => card.identifier == primaryCard.identifier))
                        {
                            DeckVariant variant = new DeckVariant(deckEntity.identifier.CreateChildIdentifier(pc.promoIdentifier))
                            {
                                title = pc.promoTitle,
                                sourceCollectionIdentifier = this.collectionIdentifier,
                                sourceExpansionIdentifier = expansionIdentifier
                            };
                            deckEntity.AddChild(variant);
                        }
                    }
                }
            }

            return (expansionIdentifier, deckEntity);
        }

        private List<HangingDeckVariant> ParseHangingVariants(JSONPromoCardList pcl)
        {
            List<HangingDeckVariant> hvs = new List<HangingDeckVariant>();
            foreach (KeyValuePair<string, Card[]> promoList in pcl)
            {
                foreach (Card card in promoList.Value)
                {
                    hvs.Add(new HangingDeckVariant()
                    {
                        deckNamespacedIdentifier = promoList.Key,
                        promoTitle = card.promoTitle,
                        variantIdentifier = card.promoIdentifier,
                        sourceCollectionIdentifier = this.collectionIdentifier,
                        sourceExpansionIdentifier = this.collectionIdentifier.CreateChildIdentifier(Expansion.PROMO_CARD_LIST_EXPANSION_IDENTIFIER)
                    });
                }
            }
            return hvs;
        }

        public
        (
            List<GlobalIdentifier>,
            List<Deck>,
            Dictionary<string, List<HangingDeckVariant>>
        )
        ParseResourcesFromDLL(string dllPath)
        {
            var hangingDeckVariants = new Dictionary<string, List<HangingDeckVariant>>();
            var expansionIdentifiers = new HashSet<GlobalIdentifier>();
            var decks = new List<Deck>();

            var module = new PEFile(dllPath);
            foreach (Resource r in module.Resources)
            {
                Stream? stream = r.TryOpenStream();
                stream.Position = 0;
                try
                {

                    // Use resource name to identify the type of resource being parsed
                    Match pclMatch = Regex.Match(r.Name, @"DeckLists.PromoCardList.json$");
                    if (pclMatch.Success)
                    {
                        Console.Error.WriteLine("Found PromoCardList {0}", pclMatch.Value);
                        JSONPromoCardList? pcl = JsonSerializer.Deserialize<JSONPromoCardList>(stream, JSON_SERIALIZER_OPTS);
                        if (pcl != null)
                        {
                            foreach(var hdv in this.ParseHangingVariants(pcl))
                            {
                                if (!hangingDeckVariants.ContainsKey(hdv.deckNamespacedIdentifier))
                                {
                                    hangingDeckVariants.Add(hdv.deckNamespacedIdentifier, new List<HangingDeckVariant>());
                                }
                                hangingDeckVariants[hdv.deckNamespacedIdentifier].Add(hdv);
                                expansionIdentifiers.Add(hdv.sourceExpansionIdentifier);
                            }
                        }
                    }
                    else
                    {
                        JSONDeckList? dl = JsonSerializer.Deserialize<JSONDeckList>(stream, JSON_SERIALIZER_OPTS);
                        if (dl != null)
                        {
                            Match dlMatch = Regex.Match(r.Name, @"DeckLists.([A-z0-9]*)DeckList.json$");
                            string dlIdentifier = dlMatch.Success ? dlMatch.Groups[1].Value : r.Name;
                            var (expansionIdentifier, deckEntity) = this.ParseDeckList(dl, dlIdentifier);

                            decks.Add(deckEntity);
                            expansionIdentifiers.Add(expansionIdentifier);
                        }
                    }
                }
                catch (Exception e)
                {
                    stream.Position = 0;
                    Console.Error.WriteLine("Error reading " + r.Name + " from " + dllPath);
                    Console.Error.WriteLine(e.ToString());
                }
            }

            return (expansionIdentifiers.ToList(), decks, hangingDeckVariants);
        }

        public abstract
        (
            GlobalIdentifier,
            string,
            List<GlobalIdentifier>,
            List<Deck>,
            Dictionary<string, List<HangingDeckVariant>>
        )
        ParseResources();
    }
}
