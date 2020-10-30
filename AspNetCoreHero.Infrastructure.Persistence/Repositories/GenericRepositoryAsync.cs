using AspNetCoreHero.Application.Enums.Services;
using AspNetCoreHero.Application.Interfaces.Repositories;
using AspNetCoreHero.Application.Interfaces.Shared;
using AspNetCoreHero.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AspNetCoreHero.Infrastructure.Persistence.Repositories
{
    public class GenericRepositoryAsync<T> : IGenericRepositoryAsync<T> where T : class
    {
        private readonly static CacheTech cacheTech = CacheTech.Memory;
        private readonly string cacheKey = $"{typeof(T)}";
        private readonly ApplicationContext _dbContext;
        private readonly Func<CacheTech, ICacheService> _cacheService;
        private DbSet<T> dbSet;

        public GenericRepositoryAsync(ApplicationContext dbContext, Func<CacheTech, ICacheService> cacheService)
        {
            _dbContext = dbContext;
            _cacheService = cacheService;
            dbSet = _dbContext.Set<T>();
        }

        public virtual async Task<T> GetByIdAsync(int id)
        {
            return await _dbContext.Set<T>().FindAsync(id);
        }

        public async Task<IReadOnlyList<T>> GetPagedReponseAsync(int pageNumber, int pageSize, string includeProperties = "")
        {
            var queryable = dbSet
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (string IncludeProperty in includeProperties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    queryable = queryable.Include(IncludeProperty);
                }
            }
            return await queryable.ToListAsync();
        }

        public async Task<T> AddAsync(T entity)
        {
            await dbSet.AddAsync(entity);
            _cacheService(cacheTech).Remove(cacheKey);
            return entity;
        }

        public Task UpdateAsync(T entity)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
            _cacheService(cacheTech).Remove(cacheKey);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(T entity)
        {
            dbSet.Remove(entity);
            _cacheService(cacheTech).Remove(cacheKey);
            return Task.CompletedTask;
        }

        public async Task<IReadOnlyList<T>> GetAllAsync(bool isCached = false)
        {
            if (!_cacheService(cacheTech).TryGet(cacheKey, out IReadOnlyList<T> cachedList))
            {
                cachedList = await _dbContext
                 .Set<T>()
                 .ToListAsync();
                _cacheService(cacheTech).Set(cacheKey, cachedList);
            }
            return cachedList;
        }

        public async Task<int> CountAsync()
        {
            return await dbSet.CountAsync();
        }

        public IQueryable<T> Query(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, string includeProperties = "")
        {
            IQueryable<T> Query = dbSet;

            if (filter != null)
            {
                Query = Query.Where(filter);
            }

            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (string IncludeProperty in includeProperties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    Query = Query.Include(IncludeProperty);
                }
            }

            if (orderBy != null)
            {
                return orderBy(Query);
            }
            return Query.AsQueryable();
        }
    }
}
