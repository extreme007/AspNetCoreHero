using AspNetCoreHero.Application.Enums;
using AspNetCoreHero.Application.Interfaces.Repositories;
using AspNetCoreHero.Application.Interfaces.Shared;
using AspNetCoreHero.Infrastructure.Persistence.Contexts;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCoreHero.Infrastructure.Persistence.Repositories
{
    public class GenericRepositoryAsync<T> : IGenericRepositoryAsync<T> where T : class
    {
        private readonly string cacheKey = $"{typeof(T)}";
        private readonly ApplicationContext _dbContext;
        private readonly IDistributedCache _distributedCache;

        public GenericRepositoryAsync(ApplicationContext dbContext, IDistributedCache distributedCache)
        {
            _dbContext = dbContext;
            _distributedCache = distributedCache;
        }

        public IQueryable<T> Entities => _dbContext.Set<T>();

        public async Task RefreshCache()
        {
            await _distributedCache.RemoveAsync(cacheKey);
            var cachedList = await Entities.ToListAsync();
            var encodedResult = JsonConvert.SerializeObject(cachedList);
            await _distributedCache.SetStringAsync(cacheKey, encodedResult);
        }

        public T Add(T entity)
        {
            _dbContext.Set<T>().Add(entity);
            BackgroundJob.Enqueue(() => RefreshCache());
            return entity;
        }

        public async Task<T> AddAsync(T entity)
        {
            await _dbContext.Set<T>().AddAsync(entity);
            BackgroundJob.Enqueue(() => RefreshCache());
            return entity;
        }

        public int Count()
        {
            return Entities.Count();
        }

        public async Task<int> CountAsync()
        {
            return await Entities.CountAsync();
        }


        public Task DeleteAsync(T entity)
        {
            _dbContext.Set<T>().Remove(entity);
            BackgroundJob.Enqueue(() => RefreshCache());
            return Task.CompletedTask;
        }

        public T Find(Expression<Func<T, bool>> match)
        {
            return Entities.SingleOrDefault(match);
        }

        public async Task<T> FindAsync(Expression<Func<T, bool>> match)
        {
            return await Entities.SingleOrDefaultAsync(match);
        }

        public ICollection<T> FindAll(Expression<Func<T, bool>> match)
        {
            return Entities.Where(match).ToList();
        }

        public async Task<ICollection<T>> FindAllAsync(Expression<Func<T, bool>> match)
        {
            return await Entities.Where(match).ToListAsync();
        }
      
        public IQueryable<T> FindBy(Expression<Func<T, bool>> predicate)
        {
            IQueryable<T> query = Entities.Where(predicate);
            return query;
        }

        public async Task<ICollection<T>> FindByAsync(Expression<Func<T, bool>> predicate)
        {
            return await Entities.Where(predicate).ToListAsync();
        }

        public ICollection<T> GetAll()
        {
            var encodedResult = _distributedCache.GetString(cacheKey);
            if (string.IsNullOrEmpty(encodedResult))
            {
                var cachedList = Entities
                 .ToList();
                 _distributedCache.SetString(cacheKey, JsonConvert.SerializeObject(cachedList));
                return cachedList;
            }
            return JsonConvert.DeserializeObject<ICollection<T>>(encodedResult);
        }

        public async Task<ICollection<T>> GetAllAsync()
        {
            var encodedResult = await _distributedCache.GetStringAsync(cacheKey);
            if (string.IsNullOrEmpty(encodedResult))
            {
                var cachedList = await Entities.ToListAsync();
                await _distributedCache.SetStringAsync(cacheKey,JsonConvert.SerializeObject(cachedList));
                return cachedList;
            }
            return JsonConvert.DeserializeObject<ICollection<T>>(encodedResult);
        }

        public async Task<ICollection<T>> GetAllIncludingAsync(params Expression<Func<T, object>>[] includeProperties)
        {
            var encodedResult = await _distributedCache.GetStringAsync(cacheKey);
            if (string.IsNullOrEmpty(encodedResult))
            {
                IQueryable<T> queryable = Entities;
                foreach (Expression<Func<T, object>> includeProperty in includeProperties)
                {
                    queryable = queryable.Include<T, object>(includeProperty);
                }

               var cachedList = await queryable.ToListAsync();

                await _distributedCache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(cachedList));
                return cachedList;
            }

            return JsonConvert.DeserializeObject<ICollection<T>>(encodedResult);
        }

        public ICollection<T> GetAllIncluding(params Expression<Func<T, object>>[] includeProperties)
        {
            var encodedResult = _distributedCache.GetString(cacheKey);
            if (string.IsNullOrEmpty(encodedResult))
            {
                IQueryable<T> queryable = Entities;
                foreach (Expression<Func<T, object>> includeProperty in includeProperties)
                {
                    queryable = queryable.Include<T, object>(includeProperty);
                }

                var cachedList =  queryable.ToList();
                 _distributedCache.SetString(cacheKey, JsonConvert.SerializeObject(cachedList));
                return cachedList;
            }

            return JsonConvert.DeserializeObject<ICollection<T>>(encodedResult);
        }

        public T GetById(int id)
        {
            return _dbContext.Set<T>().Find(id);
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await _dbContext.Set<T>().FindAsync(id);
        }

        public T Update(T entity, object key)
        {
            T exist = _dbContext.Set<T>().Find(key);
            if (exist != null)
            {
                _dbContext.Entry(exist).CurrentValues.SetValues(entity);
                _dbContext.Entry(entity).State = EntityState.Modified;
                BackgroundJob.Enqueue(() => RefreshCache());
            }
            return exist;
        }
            
        public async Task<T> UpdateAsync(T entity, object key)
        {
            T exist = await _dbContext.Set<T>().FindAsync(key);
            if (exist != null)
            {                
                _dbContext.Entry(exist).CurrentValues.SetValues(entity);
                _dbContext.Entry(entity).State = EntityState.Modified;
                BackgroundJob.Enqueue(() => RefreshCache());
            }
            return exist;
        }

        public Task UpdateAsync(T entity)
        {
            _dbContext.Entry(entity).CurrentValues.SetValues(entity);
            BackgroundJob.Enqueue(() => RefreshCache());
            return Task.CompletedTask;
        }

        public IQueryable<T> QueryInclude(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, string includeProperties = "")
        {
            IQueryable<T> Query = Entities;

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

        public async Task<ICollection<T>> GetPagedResponseAsync(int pageNumber, int pageSize, string includeProperties = "")
        {
            var queryable = Entities
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

        public ICollection<T> GetPagedResponse(int pageNumber, int pageSize, string includeProperties = "")
        {
            var queryable = Entities
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

            return  queryable.ToList();
        }


        public async Task<ICollection<T>> ExecWithStoreProcedureAsync(string query, params object[] parameters)
        {
            string pamameterString = string.Empty;
            if (!query.Contains("@p"))
            {
                if (parameters != null)
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        pamameterString += " @p" + i.ToString() + ",";
                    }
                }
                if (!string.IsNullOrEmpty(pamameterString))
                {
                    pamameterString = pamameterString.Substring(0, pamameterString.Length - 1);
                }
            }
            string fullquery = query + pamameterString;
            return await _dbContext.Set<T>().FromSqlRaw(fullquery, parameters).ToListAsync();
        }

        public ICollection<T> ExecWithStoreProcedure(string query, params object[] parameters)
        {
            string pamameterString = string.Empty;
            if (!query.Contains("@p"))
            {
                if (parameters != null)
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        pamameterString += " @p" + i.ToString() + ",";
                    }
                }
                if (!string.IsNullOrEmpty(pamameterString))
                {
                    pamameterString = pamameterString.Substring(0, pamameterString.Length - 1);
                }
            }
            string fullquery = query + pamameterString;
            return _dbContext.Set<T>().FromSqlRaw(fullquery, parameters).ToList();
        }

        public int ExecWithStoreProcedureCommand(string query, params object[] parameters)
        {
            string pamameterString = string.Empty;
            if (!query.Contains("@p"))
            {
                if (parameters != null)
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        pamameterString += " @p" + i.ToString() + ",";
                    }
                }
                if (!string.IsNullOrEmpty(pamameterString))
                {
                    pamameterString = pamameterString.Substring(0, pamameterString.Length - 1);
                }
            }
            string fullquery = query + pamameterString;
            return _dbContext.Database.ExecuteSqlRaw(fullquery, parameters);
        }

        public async Task<int> ExecWithStoreProcedureCommandAsync(string query, params object[] parameters)
        {
            string pamameterString = string.Empty;
            if (!query.Contains("@p"))
            {
                if (parameters != null)
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        pamameterString += " @p" + i.ToString() + ",";
                    }
                }
                if (!string.IsNullOrEmpty(pamameterString))
                {
                    pamameterString = pamameterString.Substring(0, pamameterString.Length - 1);
                }
            }
            string fullquery = query + pamameterString;
            return await _dbContext.Database.ExecuteSqlRawAsync(fullquery, parameters);
        }      
    }
}
