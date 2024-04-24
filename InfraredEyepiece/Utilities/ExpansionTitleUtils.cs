using System.Text.RegularExpressions;

namespace SOTM.InfraredEyepiece.Utilities
{
    public class ExpansionTitleUtils
    {
        private static Dictionary<string, string> FULL_TITLE_SPECIAL_CASES = new()
        {
            { "BaseExpansion", "Base Game" },
            { "SpookyGhostwriterComics", "Spooky Ghostwriter"},
            { "Menagerie", "Menagerie of the Multiverse"}
        };

        public static string GetExpansionFullTitle (string expansionIdentifier)
        {
            if (FULL_TITLE_SPECIAL_CASES.ContainsKey(expansionIdentifier))
            {
                return FULL_TITLE_SPECIAL_CASES[expansionIdentifier];
            }
            return Regex.Replace(expansionIdentifier, "([a-z])([A-Z0-9])", "$1 $2")
                .Replace("Cauldron ", "Cauldron:")
                .Replace("Obliv Aeon", "OblivAeon");
        }

        private const int VARIANT_LINE_CHAR_LIMIT = 25;

        private static Dictionary<string, string> SHORT_TITLE_SPECIAL_CASES = new()
        {
            { "VillainsOfTheMultiverse", "VOTM"},
            { "Menagerie", "MOTM"}
        };

        public static string GetExpansionShortTitle (string expansionIdentifier)
        {
            if (SHORT_TITLE_SPECIAL_CASES.ContainsKey(expansionIdentifier))
            {
                return SHORT_TITLE_SPECIAL_CASES[expansionIdentifier];
            }
            return GetExpansionFullTitle(expansionIdentifier);
        }
    }
}
