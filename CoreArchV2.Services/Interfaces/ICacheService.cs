namespace CoreArchV2.Services.Interfaces
{
    public interface ICacheService
    {
        Task<T> Get<T>(string key) where T : class;
        Task Save<T>(string key, T obj, double expirationMinutes) where T : class;
        Task Clear(string key);
        Task<string> GetAsync(string key);

        Task<bool> AddCacheAsync<T>(string key, T data, TimeSpan expiry = default);
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> func, TimeSpan expiry = default);
        Task<T> GetFromCacheAsync<T>(string key);
    }
}
