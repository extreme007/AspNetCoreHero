using AspNetCoreHero.Application.Enums.Services;
using AspNetCoreHero.Application.Interfaces.Repositories;
using AspNetCoreHero.Application.Interfaces.Shared;
using AspNetCoreHero.Domain.Entities;
using AspNetCoreHero.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCoreHero.Infrastructure.Persistence.Repositories
{
    public class ProductCategoryRepositoryAsync : GenericRepositoryAsync<ProductCategory>, IProductCategoryRepositoryAsync
    {
        private readonly static CacheTech cacheTech = CacheTech.Memory;
        private readonly string cacheKey = $"{typeof(Product)}";
        private readonly Func<CacheTech, ICacheService> _cacheService;
        private readonly DbSet<ProductCategory>  _category;

        public ProductCategoryRepositoryAsync(ApplicationContext dbContext, Func<CacheTech, ICacheService> cacheService) : base(dbContext, cacheService)
        {
            _category = dbContext.Set<ProductCategory>();
            _cacheService = cacheService;
        }
    }
}
