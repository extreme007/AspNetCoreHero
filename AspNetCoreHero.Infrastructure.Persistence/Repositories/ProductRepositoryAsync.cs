using AspNetCoreHero.Application.Enums;
using AspNetCoreHero.Application.Interfaces.Repositories;
using AspNetCoreHero.Application.Interfaces.Shared;
using AspNetCoreHero.Domain.Entities;
using AspNetCoreHero.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCoreHero.Infrastructure.Persistence.Repositories
{
    public class ProductRepositoryAsync : GenericRepositoryAsync<Product>, IProductRepositoryAsync
    {
        private readonly static CacheTech cacheTech = CacheTech.Memory;
        private readonly string cacheKey = $"{typeof(Product)}";
        private readonly Func<CacheTech, ICacheService> _cacheService;
        private readonly DbSet<Product> _products;

        public ProductRepositoryAsync(ApplicationContext dbContext, Func<CacheTech, ICacheService> cacheService) : base(dbContext, cacheService)
        {
            _products = dbContext.Set<Product>();
            _cacheService = cacheService;
        }

        public async Task<IReadOnlyList<Product>> GetAllWithCategoriesAsync(int pageNumber, int pageSize, bool isCached = false)
        {
            if (!_cacheService(cacheTech).TryGet(cacheKey, out IReadOnlyList<Product> cachedList))
            {
                var data = _products.Include(a => a.ProductCategory).AsQueryable();
                cachedList = await data.Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking().ToListAsync();
                _cacheService(cacheTech).Set(cacheKey, cachedList);
            }
            return cachedList;
        }

        public async Task<IReadOnlyList<Product>> GetAllWithCategoriesWithoutImagesAsync(int pageNumber, int pageSize, bool isCached = false)
        {
            if (!_cacheService(cacheTech).TryGet($"{cacheKey}WithoutImages{pageNumber}{pageSize}", out IReadOnlyList<Product> cachedList))
            {
                var data = _products.Include(a => a.ProductCategory).AsQueryable();
                cachedList = await data.Skip((pageNumber - 1) * pageSize)
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
                _cacheService(cacheTech).Set($"{cacheKey}WithoutImages{pageNumber}{pageSize}", cachedList);
            }
            return cachedList;
        }

        public Task<bool> IsUniqueBarcodeAsync(string barcode)
        {
            return _products.AllAsync(p => p.Barcode != barcode);
        }
    }
}
