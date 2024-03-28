using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace SOTM.Shared.Models
{
    public class CollectionManifestEntry
    {
        [JsonInclude]
        public string file;
        [JsonInclude]
        public string hash;
        [JsonInclude]
        public int sortOrder;
    }
    public class CollectionManifest
    {
        [JsonInclude]
        public Dictionary<string, CollectionManifestEntry> files = new();

        public static string CalculateHash(string content)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(content));
                return Convert.ToHexString(hashBytes);
            }
        }
    }
}
