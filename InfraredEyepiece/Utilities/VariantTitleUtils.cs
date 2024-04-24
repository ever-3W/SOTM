using System.Text.RegularExpressions;

namespace SOTM.InfraredEyepiece.Utilities
{
    public class VariantTitleUtils
    {
        private static Dictionary<string, string> FULL_TITLE_SPECIAL_CASES = new()
        {
            { "Tiamat, The Jaws of Winter", "Hydra Tiamat" },
            { "Catchwater Harbor 1929", "Catchwater Harbor, 1929" }
        };

        public static string GetVariantFullTitle (string variantTitle)
        {
            if (FULL_TITLE_SPECIAL_CASES.ContainsKey(variantTitle))
            {
                return FULL_TITLE_SPECIAL_CASES[variantTitle];
            }
            return variantTitle;
        }

        private const int VARIANT_LINE_CHAR_LIMIT = 23;
        private static Dictionary<string, string> SHORT_TITLE_SPECIAL_CASES = new()
        {
            { "Accelerated Evolution Anathema", "Acc. Evolution Anathema" },
            { "Swarm Eater: Distributed Hivemind", "Swarm Eater: Hivemind" },
            { "Tiamat, The Jaws of Winter", "Hydra Tiamat" },
            { "America's Newest Legacy", "Newest Legacy" },
            { "America's Greatest Legacy", "Greatest Legacy" },
            { "Prime Wardens Argent Adept", "Prime Wardens" },
            { "Xtreme Prime Wardens Argent Adept", "Xtreme Prime Wardens" },
            { "Dark Conductor Argent Adept", "Dark Conductor" },
            { "One With Freedom Doctor Metropolis", "One With Freedom"},
            { "Nitro Boost Absolute Zero", "Nitro Boost" },
            { "Urban Warfare Expatriette", "Urban Warfare" },
            { "The Knights: Wasteland Ronin", "Wasteland Ronin" },
            { "Last of The Forgotten Order Necro", "Forgotten Order" },
            { "Tsukiko Tanner: The Game is Rigged", "The Game is Rigged" },
            { "Madame Mittermeier's Fantastical Festival of Conundrums and Curiosities", "MMFFCC"},
            { "Catchwater Harbor 1929", "Catchwater Harbor, 1929" },
            { "F.S.C. Continuance Wanderer", "Continuance Wanderer"},
            { "Halberd Experimental Research Center", "Halberd Research Center"},
            { "The Chasm of a Thousand Nights", "Chasm of 1,000 Nights"},
            { "Secret Origins Alycia Chin", "Secret Origins" }
        };

        public static string GetVariantShortTitle (string variantTitle, string? deckTitle)
        {
            if (SHORT_TITLE_SPECIAL_CASES.ContainsKey(variantTitle))
            {
                return SHORT_TITLE_SPECIAL_CASES[variantTitle];
            }
            if (variantTitle.Length >= VARIANT_LINE_CHAR_LIMIT)
            {
                if (variantTitle.StartsWith("The "))
                {
                    return variantTitle.Replace("The ", "");
                }
                if (variantTitle.StartsWith("Ministry Of Strategic Science", StringComparison.CurrentCultureIgnoreCase))
                {
                    return variantTitle.Replace("Ministry Of Strategic Science", "MSS");
                }
                if (deckTitle is not null)
                {
                    if (variantTitle == deckTitle)
                    {
                        return variantTitle;
                    }
                    if (variantTitle.StartsWith(deckTitle, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return Regex.Replace(variantTitle, $"{deckTitle}(\\:?)(,?)( ?)", "", RegexOptions.IgnoreCase);
                    }
                    if (variantTitle.EndsWith(deckTitle, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return Regex.Replace(variantTitle, $"{deckTitle}( ?)", "");
                    }
                }
            }
            return variantTitle;
        }
    }
}
