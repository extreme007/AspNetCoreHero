﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCoreHero.Application.Interfaces.Repositories
{
    public interface IGenericRepositoryAsync<T> where T : class
    {
        IQueryable<T> QueryInclude(Expression<Func<T, bool>> filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        string includeProperties = "");
        Task<ICollection<T>> GetPagedResponseAsync(int pageNumber, int pageSize, string includeProperties = "");

        T Add(T entity);
        Task<T> AddAsync(T entity);

        T Update(T entity, object key);
        Task<T> UpdateAsync(T entity, object key);

        void Delete(T entity);
        Task DeleteAsync(T entity);

        int Count();
        Task<int> CountAsync();

        T Find(Expression<Func<T, bool>> match);
        Task<T> FindAsync(Expression<Func<T, bool>> match);

        ICollection<T> FindAll(Expression<Func<T, bool>> match);
        Task<ICollection<T>> FindAllAsync(Expression<Func<T, bool>> match);
      
        IQueryable<T> FindBy(Expression<Func<T, bool>> predicate);
        Task<ICollection<T>> FindByAsync(Expression<Func<T, bool>> predicate);

        T GetById(int id);
        Task<T> GetByIdAsync(int id);

        ICollection<T> GetAll();
        Task<ICollection<T>> GetAllAsync();

        IQueryable<T> GetAllIncluding(params Expression<Func<T, object>>[] includeProperties);

        //store procedure
        int ExecWithStoreProcedure(string query, params object[] parameters);
        Task<int> ExecWithStoreProcedureAsync(string query, params object[] parameters);
        Task<int> ExecuteWithStoreProcedureAsync(string query, params object[] parameters);
        int ExecuteWithStoreProcedure(string query, params object[] parameters);
    }
}
