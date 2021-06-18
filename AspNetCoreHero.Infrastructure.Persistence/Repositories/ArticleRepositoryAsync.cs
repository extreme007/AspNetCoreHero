using AspNetCoreHero.Application.Enums;
using AspNetCoreHero.Application.Interfaces.Repositories;
using AspNetCoreHero.Domain.Entities;
using AspNetCoreHero.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCoreHero.Infrastructure.Persistence.Repositories
{
    public class ArticleRepositoryAsync : GenericRepositoryAsync<Article>, IArticleRepositoryAsync
    {
        private readonly static CacheTech cacheTech = CacheTech.Memory;
        private readonly string cacheKey = $"{typeof(Article)}";
        private readonly DbSet<Article> _category;
        private readonly IDistributedCache _distributedCache;

        public ArticleRepositoryAsync(ApplicationContext dbContext, IDistributedCache distributedCache) : base(dbContext, distributedCache)
        {
            _category = dbContext.Set<Article>();
            _distributedCache = distributedCache;
        }
    }
}
