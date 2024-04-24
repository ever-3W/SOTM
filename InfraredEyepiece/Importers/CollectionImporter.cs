using System.Text.Json;
using System.Text.RegularExpressions;
using ICSharpCode.Decompiler.Metadata;
using Microsoft.Extensions.Configuration;
using SOTM.InfraredEyepiece.Utilities;
using SOTM.Shared.Models;
using SOTM.Shared.Models.JSONObjects;

namespace SOTM.InfraredEyepiece.Importers
{
    using JSONPromoCardList = Dictionary<string, Card[]>;

    public abstract class CollectionImporter
    {
        protected IConfigurationRoot config;
        protected static JsonSerializerOptions JSON_SERIALIZER_OPTS = new JsonSerializerOptions()
        { AllowTrailingCommas = true, WriteIndented = true };

        protected abstract GlobalIdentifier collectionIdentifier { get; }

        public CollectionImporter(IConfigurationRoot config)
        {
            this.config = config;
        }

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
                title = VariantTitleUtils.GetVariantFullTitle(deckEntity.title),
                shortTitle = VariantTitleUtils.GetVariantShortTitle(deckEntity.title, deckEntity.title)
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
                                title = VariantTitleUtils.GetVariantFullTitle(pc.promoTitle),
                                shortTitle = VariantTitleUtils.GetVariantShortTitle(pc.promoTitle, deckEntity.title)
                            };
                            deckEntity.AddChild(variant);
                        }
                    }
                }
            }

            return (expansionIdentifier, deckEntity);
        }

        private Dictionary<string, List<DeckVariant>> ParseHangingVariants(JSONPromoCardList pcl)
        {
            return new Dictionary<string, List<DeckVariant>> (
                pcl.Select((promoList) => 
                    new KeyValuePair<string, List<DeckVariant>>(
                        promoList.Key,
                        promoList.Value.Select(card => 
                        {
                            GlobalIdentifier identifier = this.collectionIdentifier
                                .CreateChildIdentifier(Expansion.PROMO_CARD_LIST_EXPANSION_IDENTIFIER)
                                .CreateChildIdentifier(promoList.Key.Split('.').Last())
                                .CreateChildIdentifier(card.promoIdentifier);

                            return new DeckVariant(identifier)
                            {
                                title = VariantTitleUtils.GetVariantFullTitle(card.promoTitle),
                                // Reuse full title since short title function requires deck title
                                shortTitle = VariantTitleUtils.GetVariantFullTitle(card.promoTitle)
                            };
                        }).ToList()
                    ))
            );
        }

        public
        (
            List<Deck>,
            Dictionary<string, List<DeckVariant>>
        )
        ParseResourcesFromDLL(string dllPath)
        {
            var hangingDeckVariants = new Dictionary<string, List<DeckVariant>>();
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
                        JSONPromoCardList? pcl = JsonSerializer.Deserialize<JSONPromoCardList>(stream, JSON_SERIALIZER_OPTS);
                        if (pcl != null)
                        {
                            hangingDeckVariants = this.ParseHangingVariants(pcl);
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

            return (decks, hangingDeckVariants);
        }


        public string GetOutputFileName()
        {
            return $"{this.collectionIdentifier}.json";
        }
        public string GetOutputFilePath()
        {
            return Path.Combine(
                this.config["OutputPath"],
                this.GetOutputFileName()
            );
        }
        public abstract CollectionV2 ParseResourcesV2();
    }
}
