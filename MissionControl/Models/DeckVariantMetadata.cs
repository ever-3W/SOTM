using SOTM.Shared.Models;

namespace SOTM.MissionControl.Models
{
    public class DeckVariantMetadata
    {
        public string? shortTitle;
        public GlobalIdentifier identifier;
        public string? color;

        public DeckVariantMetadata(DeckVariant variant)
        {
            this.identifier = variant.identifier;
            this.shortTitle = variant.shortTitle;
        }
    }
}
