using System.Text.Json;
using Microsoft.Extensions.Configuration;
using SOTM.Shared.Models;
using SOTM.Shared.Models.JSONObjects;

namespace SOTM.InfraredEyepiece.Importers
{
    public class ModImporter : CollectionImporter
    {
        private GlobalIdentifier _collectionIdentifier;
        private string collectionTitle;
        private string dllPath;
        // Unreleased decks and variants will have DeckList files associated with them,
        // but if they aren't listed in the manifest they won't be playable in game
        private HashSet<string> manifestListedHeroes;
        private HashSet<string> manifestListedVillains;
        private HashSet<string> manifestListedEnvironments;
        private HashSet<string> manifestListedTeamVillains;
        private Dictionary<string, HashSet<string>> manifestListedVariantsByDeck;

        protected override GlobalIdentifier collectionIdentifier { get => _collectionIdentifier; }

        public ModImporter(string publishedFileId, IConfigurationRoot config) : base(config)
        {
            string modDir = Path.Combine(this.config["ModDirectoryPath"], publishedFileId);
            string manifestPath = Path.Combine(modDir, "manifest.json");
            JSONModManifest manifest = JsonSerializer.Deserialize<JSONModManifest>(File.ReadAllText(manifestPath), JSON_SERIALIZER_OPTS);

            this.dllPath = Path.Combine(modDir, manifest.dll);
            this._collectionIdentifier = new GlobalIdentifier(manifest.@namespace);
            this.collectionTitle = manifest.title;
            this.manifestListedHeroes = new HashSet<string>(manifest?.decks?.heroes ?? new string[]{});
            this.manifestListedVillains = new HashSet<string>(manifest?.decks?.villains ?? new string[]{});
            this.manifestListedEnvironments = new HashSet<string>(manifest?.decks?.environments ?? new string[]{});
            this.manifestListedTeamVillains = new HashSet<string>(manifest?.decks?.teamVillains ?? new string[]{});
            this.manifestListedVariantsByDeck = new Dictionary<string, HashSet<string>>(
                (manifest?.variants ?? new Dictionary<string, string[]>())
                    .Select(kv => new KeyValuePair<string, HashSet<string>>
                        (kv.Key, new HashSet<string>(kv.Value)))
            );
        }

        private Dictionary<string, List<HangingDeckVariant>>
        ExcludeHangingVariantsNotListed(Dictionary<string, List<HangingDeckVariant>> hangingVariants)
        {
            var listedHangingVariants = new Dictionary<string, List<HangingDeckVariant>>();
            foreach (KeyValuePair<string, List<HangingDeckVariant>> kv in hangingVariants)
            {
                if (this.manifestListedVariantsByDeck.ContainsKey(kv.Key)) {
                    var listedVariantsInDeck = kv.Value.FindAll(hdv => 
                        this.manifestListedVariantsByDeck[kv.Key].Contains(hdv.variantIdentifier));
                    
                    if (listedVariantsInDeck.Count() > 0)
                    {
                        listedHangingVariants.Add(kv.Key, listedVariantsInDeck.ToList());
                    }
                }
            }
            return listedHangingVariants;
        }

        private List<Deck> ExcludeDecksNotListed(List<Deck> decks)
        {
            return decks.FindAll(deck => 
                deck.kind == DeckKind.HERO && manifestListedHeroes.Contains(deck.GetDeckListIdentifier()) ||
                deck.kind == DeckKind.VILLAIN && manifestListedVillains.Contains(deck.GetDeckListIdentifier()) ||
                deck.kind == DeckKind.ENVIRONMENT && manifestListedEnvironments.Contains(deck.GetDeckListIdentifier()) ||
                deck.kind == DeckKind.VILLAIN_TEAM && manifestListedTeamVillains.Contains(deck.GetDeckListIdentifier())
            );
        }

        public override CollectionV2 ParseResourcesV2()
        {
            CollectionV2 result = new CollectionV2(this._collectionIdentifier, this.collectionTitle);
            // the core DLL shouldn't contain any hanging variants
            var (decks, hangingVariants) = this.ParseResourcesFromDLL(this.dllPath);
            result.hangingVariants = this.ExcludeHangingVariantsNotListed(hangingVariants);
            foreach (Deck deck in this.ExcludeDecksNotListed(decks))
            {
                result.AddDeck(deck);
            }
            return result;
        }
    }
}
