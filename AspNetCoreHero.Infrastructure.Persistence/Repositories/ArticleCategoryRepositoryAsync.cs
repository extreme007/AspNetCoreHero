using AspNetCoreHero.Application.Enums;
using AspNetCoreHero.Application.Interfaces.Repositories;
using AspNetCoreHero.Application.Interfaces.Shared;
using AspNetCoreHero.Domain.Entities;
using AspNetCoreHero.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCoreHero.Infrastructure.Persistence.Repositories
{
    public class ArticleCategoryRepositoryAsync : GenericRepositoryAsync<ArticleCategory>, IArticleCategoryRepositoryAsync
    {
        private readonly static CacheTech cacheTech = CacheTech.Memory;
        private readonly string cacheKey = $"{typeof(ArticleCategory)}";
        private readonly DbSet<ArticleCategory>  _category;
        private readonly IDistributedCache _distributedCache;

        public ArticleCategoryRepositoryAsync(ApplicationContext dbContext, IDistributedCache distributedCache) : base(dbContext, distributedCache)
        {
            _category = dbContext.Set<ArticleCategory>();
            _distributedCache = distributedCache;
        }
    }
}
