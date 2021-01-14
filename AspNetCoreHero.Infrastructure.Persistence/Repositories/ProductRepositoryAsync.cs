using AspNetCoreHero.Application.Enums;
using AspNetCoreHero.Application.Interfaces.Repositories;
using AspNetCoreHero.Application.Interfaces.Shared;
using AspNetCoreHero.Domain.Entities;
using AspNetCoreHero.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCoreHero.Infrastructure.Persistence.Repositories
{
    public class ProductRepositoryAsync : GenericRepositoryAsync<Product>, IProductRepositoryAsync
    {
        private readonly string cacheKey = $"{typeof(Product)}";
        private readonly IDistributedCache _distributedCache;

        public ProductRepositoryAsync(ApplicationContext dbContext, IDistributedCache distributedCache) : base(dbContext, distributedCache)
        {
            _distributedCache = distributedCache;
        }

        public async Task<IReadOnlyList<Product>> GetAllWithCategoriesAsync(int pageNumber, int pageSize, bool isCached = false)
        {
            var encodedResult = _distributedCache.GetString(cacheKey);
            if (string.IsNullOrEmpty(encodedResult))
            {
                var data = Entities.Include(a => a.ProductCategory).AsQueryable();
                var cachedList = await data.Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking().ToListAsync();
                await _distributedCache.SetStringAsync(cacheKey,JsonConvert.SerializeObject(cachedList));
                return cachedList;
            }
            return JsonConvert.DeserializeObject<IReadOnlyList<Product>>(encodedResult);
        }

        public async Task<IReadOnlyList<Product>> GetAllWithCategoriesWithoutImagesAsync(int pageNumber, int pageSize, bool isCached = false)
        {
            var cacheKeyCustom = $"{cacheKey}WithoutImages{pageNumber}{pageSize}";
            var encodedResult = _distributedCache.GetString(cacheKeyCustom);
            if (string.IsNullOrEmpty(encodedResult))
            {
                var data = Entities.Include(a => a.ProductCategory).AsQueryable();
                var cachedList = await data.Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking().Select(a =>
                new Product
                {
                    Id = a.Id,
                    Description = a.Description,
                    Name = a.Name,
                    ProductCategory = a.ProductCategory,
                    ProductCategoryId = a.ProductCategoryId,
                    Price = a.Price,
                    Barcode = a.Barcode
                }).ToListAsync();             
                await _distributedCache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(cachedList));
                return cachedList;
            }
            return JsonConvert.DeserializeObject<IReadOnlyList<Product>>(encodedResult);
        }

        public Task<bool> IsUniqueBarcodeAsync(string barcode)
        {
            return Entities.AllAsync(p => p.Barcode != barcode);
        }
    }
}
