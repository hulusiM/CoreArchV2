using CoreArchV2.Services.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace CoreArchV2.Services.Services
{
    public class CacheService : ICacheService
    {

        private readonly IDistributedCache _distributedCache;
        public CacheService(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        public async Task<T> Get<T>(string key) where T : class
        {
            var cachedResponse = await _distributedCache.GetStringAsync(key);
            var result = cachedResponse != null ? JsonConvert.DeserializeObject<T>(cachedResponse) : null;
            return result;

        }

        public async Task Save<T>(string key, T obj, double expirationMinutes) where T : class
        {
            if (expirationMinutes > 480) throw new ApplicationException("Cache expiration time limit is exceeded. ");
            var cacheEntryOptions = new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(expirationMinutes));
            await _distributedCache.SetStringAsync(key, JsonConvert.SerializeObject(obj), cacheEntryOptions);
        }

        public async Task Clear(string key)
        {
            await _distributedCache.RemoveAsync(key);
        }

        public async Task<string> GetAsync(string key)
        {
            return await _distributedCache.GetStringAsync(key);
        }

        public async Task<bool> AddCacheAsync<T>(string key, T data, TimeSpan expiry = default)
        {
            if (data == null) return false;
            if (expiry == default) expiry = TimeSpan.FromHours(24);
            try
            {
                var jsonStringData = System.Text.Json.JsonSerializer.Serialize(data);
                var options = new DistributedCacheEntryOptions().SetAbsoluteExpiration(DateTime.UtcNow.Add(expiry));
                await _distributedCache.SetStringAsync(key, jsonStringData, options);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> func, TimeSpan expiry = default)
        {
            T value = await GetFromCacheAsync<T>(key);
            if (value == null)
            {
                value = await func();
                await AddCacheAsync(key, value, expiry);
            }
            return value;
        }

        public async Task<T> GetFromCacheAsync<T>(string key)
        {
            try
            {
                var stringData = await GetAsync(key);
                if (!string.IsNullOrWhiteSpace(stringData))
                {
                    return System.Text.Json.JsonSerializer.Deserialize<T>(stringData);
                }
            }
            catch
            {
            }
            return default(T);
        }
    }
}
