using AspNetCoreHero.Application.DTOs.Settings;
using AspNetCoreHero.Application.Interfaces.Shared;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;

namespace AspNetCoreHero.Infrastructure.Shared.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _redisCache;
        private readonly CacheSettings _cacheConfig;
        private DistributedCacheEntryOptions _cacheOptions;

        public RedisCacheService(IDistributedCache redisCache, IOptions<CacheSettings> cacheConfig)
        {
            _redisCache = redisCache;
            _cacheConfig = cacheConfig.Value;
            if (_cacheConfig != null)
            {
                _cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = DateTime.Now.AddHours(_cacheConfig.AbsoluteExpirationInHours),
                    SlidingExpiration = TimeSpan.FromMinutes(_cacheConfig.SlidingExpirationInMinutes)
                };
            }
        }

        public T Get<T>(string key)
        {
            var value = _redisCache.GetString(key);

            if (value != null)
            {
                return JsonConvert.DeserializeObject<T>(value);
            }

            return default;
        }

        public void Remove(string cacheKey)
        {
            _redisCache.RemoveAsync(cacheKey);
        }

        public T Set<T>(string key, T value)
        {
            _redisCache.SetString(key, JsonConvert.SerializeObject(value), _cacheOptions);

            return value;
        }     
    }
}
