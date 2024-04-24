using SOTM.Shared.Models;

namespace SOTM.MissionControl.Models
{
    public class DeckVariantMetadata
    {
        public string? title;
        public string? shortTitle;
        public GlobalIdentifier identifier;
        public string? color;

        public DeckVariantMetadata(DeckVariant variant)
        {
            this.identifier = variant.identifier;
            this.title = variant.title;
            this.shortTitle = variant.shortTitle;
        }
    }
}
