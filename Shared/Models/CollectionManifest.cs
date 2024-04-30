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

        public static string CalculateHash(byte[] content)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(content);
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

        public static IEnumerable<CollectionManifestDelta> ListDeltas (CollectionManifest? left, CollectionManifest? right)
        {
            if (left is null && right is null)
            {
                return Enumerable.Empty<CollectionManifestDelta>();
            }
            IEnumerable<string> leftKeys = left?.files.Keys ?? Enumerable.Empty<string>();
            IEnumerable<string> rightKeys = right?.files.Keys ?? Enumerable.Empty<string>();

            var leftDeltas = leftKeys
                .Select(key => new CollectionManifestDelta()
                {
                    identifier = key,
                    file = left!.files[key].file,
                    leftHash = left.files[key].hash,
                    rightHash = right?.files.GetValueOrDefault(key)?.hash ?? null
                });
            var rightDeltas = rightKeys
                .Select(key => new CollectionManifestDelta()
                {
                    identifier = key,
                    file = right!.files[key].file,
                    leftHash = left?.files.GetValueOrDefault(key)?.hash ?? null,
                    rightHash = right.files[key].hash
                });
            return leftDeltas.Concat(rightDeltas).Where(delta => delta.leftHash != delta.rightHash);
        }
    }
}
