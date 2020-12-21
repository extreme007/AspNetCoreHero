﻿using AspNetCoreHero.Application.Enums.Services;
using AspNetCoreHero.Application.Interfaces.Repositories;
using AspNetCoreHero.Application.Interfaces.Shared;
using AspNetCoreHero.Infrastructure.Persistence.Contexts;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        //private DbSet<T> dbSet;

        public GenericRepositoryAsync(ApplicationContext dbContext, Func<CacheTech, ICacheService> cacheService)
        {
            _dbContext = dbContext;
            _cacheService = cacheService;
            //dbSet = _dbContext.Set<T>();
        }

        public async Task RefreshCache()
        {
            _cacheService(cacheTech).Remove(cacheKey);
            var cachedList = await _dbContext.Set<T>().ToListAsync();
            _cacheService(cacheTech).Set(cacheKey, cachedList);
        }

        public T Add(T entity)
        {
            _dbContext.Set<T>().Add(entity);
            //_cacheService(cacheTech).Remove(cacheKey);
            BackgroundJob.Enqueue(() => RefreshCache());
            return entity;
        }

        public async Task<T> AddAsync(T entity)
        {
            await _dbContext.Set<T>().AddAsync(entity);
            //_cacheService(cacheTech).Remove(cacheKey);
            BackgroundJob.Enqueue(() => RefreshCache());
            return entity;
        }

        public int Count()
        {
            return _dbContext.Set<T>().Count();
        }

        public async Task<int> CountAsync()
        {
            return await _dbContext.Set<T>().CountAsync();
        }

        public void Delete(T entity)
        {
            _dbContext.Set<T>().Remove(entity);
            //_cacheService(cacheTech).Remove(cacheKey);
            BackgroundJob.Enqueue(() => RefreshCache());
        }

        public Task DeleteAsync(T entity)
        {
            _dbContext.Set<T>().Remove(entity);
            //_cacheService(cacheTech).Remove(cacheKey);
            BackgroundJob.Enqueue(() => RefreshCache());
            return Task.CompletedTask;
        }

        public T Find(Expression<Func<T, bool>> match)
        {
            return _dbContext.Set<T>().SingleOrDefault(match);
        }

        public async Task<T> FindAsync(Expression<Func<T, bool>> match)
        {
            return await _dbContext.Set<T>().SingleOrDefaultAsync(match);
        }

        public ICollection<T> FindAll(Expression<Func<T, bool>> match)
        {
            return _dbContext.Set<T>().Where(match).ToList();
        }

        public async Task<ICollection<T>> FindAllAsync(Expression<Func<T, bool>> match)
        {
            return await _dbContext.Set<T>().Where(match).ToListAsync();
        }
      
        public IQueryable<T> FindBy(Expression<Func<T, bool>> predicate)
        {
            IQueryable<T> query = _dbContext.Set<T>().Where(predicate);
            return query;
        }

        public async Task<ICollection<T>> FindByAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbContext.Set<T>().Where(predicate).ToListAsync();
        }

        public ICollection<T> GetAll()
        {
            if (!_cacheService(cacheTech).TryGet(cacheKey, out ICollection<T> cachedList))
            {
                cachedList = _dbContext
                 .Set<T>()
                 .ToList();
                _cacheService(cacheTech).Set(cacheKey, cachedList);
            }
            return cachedList;
        }

        public async Task<ICollection<T>> GetAllAsync()
        {
            if (!_cacheService(cacheTech).TryGet(cacheKey, out ICollection<T> cachedList))
            {
                cachedList = await _dbContext
                 .Set<T>()
                 .ToListAsync();
                _cacheService(cacheTech).Set(cacheKey, cachedList);
            }
            return cachedList;
        }

        public async Task<ICollection<T>> GetAllIncludingAsync(params Expression<Func<T, object>>[] includeProperties)
        {
            if (!_cacheService(cacheTech).TryGet(cacheKey, out ICollection<T> cachedList))
            {
                IQueryable<T> queryable = _dbContext.Set<T>();
                foreach (Expression<Func<T, object>> includeProperty in includeProperties)
                {
                    queryable = queryable.Include<T, object>(includeProperty);
                }

                cachedList = await queryable.ToListAsync();
                 
                _cacheService(cacheTech).Set(cacheKey, cachedList);
            }         

            return cachedList;
        }

        public ICollection<T> GetAllIncluding(params Expression<Func<T, object>>[] includeProperties)
        {
            if (!_cacheService(cacheTech).TryGet(cacheKey, out ICollection<T> cachedList))
            {
                IQueryable<T> queryable = _dbContext.Set<T>();
                foreach (Expression<Func<T, object>> includeProperty in includeProperties)
                {
                    queryable = queryable.Include<T, object>(includeProperty);
                }

                cachedList =  queryable.ToList();

                _cacheService(cacheTech).Set(cacheKey, cachedList);
            }

            return cachedList;
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
                //_cacheService(cacheTech).Remove(cacheKey);
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
                //_cacheService(cacheTech).Remove(cacheKey);
                BackgroundJob.Enqueue(() => RefreshCache());
            }
            return exist;
        }

        public IQueryable<T> QueryInclude(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, string includeProperties = "")
        {
            IQueryable<T> Query = _dbContext.Set<T>();

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
            var queryable = _dbContext.Set<T>()
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
            var queryable = _dbContext.Set<T>()
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
            return _dbContext.Database.ExecuteSqlCommand(fullquery, parameters);
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
            return await _dbContext.Database.ExecuteSqlCommandAsync(fullquery, parameters);
        }
    }
}
