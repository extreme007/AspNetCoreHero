using AspNetCoreHero.Application.Interfaces.Shared;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCoreHero.Infrastructure.Shared.Services
{
    public class RedisCacheService : ICacheService
    {
        //private readonly IDistributedCache _redisCache;
        //private readonly CacheConfiguration _cacheConfig;
        //private DistributedCacheEntryOptions _cacheOptions;

        //public RedisCacheService(IDistributedCache redisCache, IOptions<CacheConfiguration> cacheConfig)
        //{
        //    _redisCache = redisCache;
        //    _cacheConfig = cacheConfig.Value;
        //    if (_cacheConfig != null)
        //    {
        //        _cacheOptions = new DistributedCacheEntryOptions
        //        {
        //            AbsoluteExpiration = DateTime.Now.AddHours(_cacheConfig.AbsoluteExpirationInHours),
        //            SlidingExpiration = TimeSpan.FromMinutes(_cacheConfig.SlidingExpirationInMinutes)
        //        };
        //    }
        //}
        public void Remove(string cacheKey)
        {
            throw new NotImplementedException();

        }

        public T Set<T>(string cacheKey, T value)
        {
            throw new NotImplementedException();
        }

        public bool TryGet<T>(string cacheKey, out T value)
        {
            throw new NotImplementedException();
        }
    }
}
