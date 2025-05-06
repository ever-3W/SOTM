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

        private const int VARIANT_LINE_CHAR_LIMIT = 18;
        private static Dictionary<string, string> SHORT_TITLE_SPECIAL_CASES = new()
        {
            { "Accelerated Evolution Anathema", "Acc. Evolution" },
            { "Swarm Eater: Distributed Hivemind", "Hive Swarm Eater" },
            { "Tiamat, The Jaws of Winter", "Hydra Tiamat" },
            { "America's Newest Legacy", "Newest Legacy" },
            { "America's Greatest Legacy", "Greatest Legacy" },
            { "One With Freedom Doctor Metropolis", "One With Freedom"},
            { "Enlightened Mr. Fixer", "Enlightened" },
            { "Northern Wind Mr. Fixer", "Northern Wind" },
            { "La Comodora: Curse of the Black Spot", "Black Spot" },
            { "The Knights: Wasteland Ronin", "Wasteland Ronin" },
            { "Wasteland Ronin The Stranger", "Wasteland Ronin" },
            { "Last of The Forgotten Order Necro", "Forgotten Order" },
            { "Pike Industrial Complex", "Pike Ind. Complex" },
            { "Mobile Defense Platform", "Mobile Def. Platform" },
            { "Tsukiko Tanner: The Game is Rigged", "Game is Rigged" },
            { "The Enclave of the Endlings", "Endling Enclave" },
            { "Madame Mittermeier's Fantastical Festival of Conundrums and Curiosities", "Mdm. Mittermeier's"},
            { "Catchwater Harbor 1929", "Catchwater Harbor" },
            { "St. Simeon's Catacombs", "St Simeon's" },
            { "F.S.C. Continuance Wanderer", "F.S.C. Wanderer"},
            { "Halberd Experimental Research Center", "Halberd Research"},
            { "The Chasm of a Thousand Nights", "1000 Nights"},
            { "Secret Origins Alycia Chin", "Secret Origins" },
            { "Ultra Kart Super Slam Escarlata", "Ultra Kart" }
        };

        public static string GetVariantShortTitle (string variantTitle, string? deckTitle)
        {
            if (SHORT_TITLE_SPECIAL_CASES.ContainsKey(variantTitle))
            {
                return SHORT_TITLE_SPECIAL_CASES[variantTitle];
            }
            string result = variantTitle;
            if (variantTitle.Length >= VARIANT_LINE_CHAR_LIMIT)
            {
                result = _dropLeading(result);
                if (result.Contains("Ministry Of Strategic Science", StringComparison.CurrentCultureIgnoreCase))
                {
                    result = "Strategic Science";
                }
                if (result.Contains("Test Subject", StringComparison.CurrentCultureIgnoreCase))
                {
                    result = result.Replace("Test Subject", "");
                }
                if (deckTitle is not null && variantTitle != deckTitle)
                {
                    deckTitle = _dropLeading(deckTitle);
                    if (result.StartsWith(deckTitle, StringComparison.CurrentCultureIgnoreCase) ||
                    result.EndsWith(deckTitle, StringComparison.CurrentCultureIgnoreCase))
                    {
                        result = Regex.Replace(result, $"{deckTitle}", "", RegexOptions.IgnoreCase);
                    }
                }
            }
            return _safeTrim(_dropLeading(result));
        }

        private static string _safeTrim(string input)
        {
            return Regex.Replace(input, $"^\\W*(.*?)\\W*$", "$1");
        }

        private static string _dropLeading(string input)
        {
            return Regex.Replace(input, $"^(\u0027s|\\W|the|at|Void Guard)*", "", RegexOptions.IgnoreCase);
        }
    }
}
