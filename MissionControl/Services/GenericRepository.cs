using System.Text.Json;
using Blazored.LocalStorage;

namespace SOTM.MissionControl.Services
{
    public class GenericRepository<T>
    {
        private string storageKey;
        public T value;
        public GenericRepository(string storageKey, T value)
        {
            this.storageKey = storageKey;
            this.value = value;
        }

        public async Task Save(ILocalStorageService storageService)
        {
            await storageService.SetItemAsync(storageKey, this.value);
        }

        public async Task<T> Load(ILocalStorageService storageService)
        {
            this.value = (await storageService.GetItemAsync<T>(storageKey)) ?? this.value;
            return this.value;
        }

        public byte[] GetBytes()
        {
            return JsonSerializer.SerializeToUtf8Bytes(this.value, new JsonSerializerOptions() { WriteIndented = true });
        }
    }
}
