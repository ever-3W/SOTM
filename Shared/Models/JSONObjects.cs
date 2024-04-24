namespace SOTM.Shared.Models.JSONObjects
{
    #pragma warning disable CS8618
    public class JSONDeckList
    {
        public string name { get; set; }
        public string kind { get; set; }
        public string expansionIdentifier { get; set; }
        public string[] initialCardIdentifiers { get; set; }
        public Card[] cards { get; set; }
        public Card[] promoCards { get; set; }
    }
    public class Card
    {
        public string identifier { get; set; }
        public string promoIdentifier { get; set; }
        public string title { get; set; }
        public string promoTitle { get; set; }
        public bool character { get; set; }

    }
    public class JSONModManifest
    {
        public string title { get; set; }
        public string color { get; set; }
        public string @namespace { get; set; }
        public string dll { get; set; }
        public DeckLists decks { get; set; }
        public Dictionary<string, string[]> variants { get; set; }
    }
    public class DeckLists
    {
        public string[] heroes { get; set; }
        public string[] villains { get; set; }
        public string[] environments { get; set; }
        public string[] teamVillains { get; set; }
    }
    #pragma warning restore CS8618
}
