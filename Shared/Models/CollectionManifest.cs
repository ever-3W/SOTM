using System.ComponentModel.DataAnnotations;
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
    public class CollectionManifestDelta
    {
        [JsonInclude]
        public string identifier;
        [JsonInclude]
        public string file;
        [JsonInclude]
        public string? leftHash;
        [JsonInclude]
        public string? rightHash;
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

        public void Add(string identifier, string file, string hash)
        {
            if (this.files.ContainsKey(identifier))
            {
                this.files[identifier].file = file;
                this.files[identifier].hash = hash;
            }
            else
            {
                this.files[identifier] = new CollectionManifestEntry()
                {
                    file = file,
                    hash = hash,
                    sortOrder = this.files.Values.Count() > 0
                        ? Enumerable.Max(this.files.Values.Select(entry => entry.sortOrder)) + 1
                        : 0
                };
            }
        }

        public static IEnumerable<CollectionManifestDelta> ListDeltas (CollectionManifest left, CollectionManifest right)
        {
            return left.files.Keys
                .Union(right.files.Keys)
                .Select(key => new CollectionManifestDelta()
                {
                    identifier = key,
                    file = left.files.GetValueOrDefault(key)?.file ?? right.files.GetValueOrDefault(key)?.file,
                    leftHash = left.files.GetValueOrDefault(key)?.hash ?? null,
                    rightHash = right.files.GetValueOrDefault(key)?.hash ?? null
                })
                .Where(delta => delta.leftHash != delta.rightHash);
        }
    }
}
