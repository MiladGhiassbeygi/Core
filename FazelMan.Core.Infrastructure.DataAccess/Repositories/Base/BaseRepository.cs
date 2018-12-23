﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FazelMan.Core.Common.ApiResult;
using FazelMan.Core.Common.Dto.Api;
using FazelMan.Core.Domain.Abstract.Repositories.Base;
using FazelMan.Core.Domain.Entity.Base;
using FazelMan.Core.Infrastructure.DataAccess.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace FazelMan.Core.Infrastructure.DataAccess.Repositories.Base
{
    public abstract class BaseRepository<T, Type> : IBaseRepository<T, Type> where T : BaseEntity<Type>, new()
    {
        private readonly DbSet<T> _dbSet;
        private readonly IUnitOfWork _uow;

        protected BaseRepository(IUnitOfWork uow)
        {
            _uow = uow;
            _dbSet = _uow.Set<T>();
        }

        public virtual async Task<T> InsertAsync(T entity, bool isSave = true)
        {
            await _dbSet.AddAsync(entity);
            if (isSave) await _uow.SaveChangesAsync();
            return entity;
        }

        public virtual async Task InsertRangeAsync(List<T> entity, bool isSave = true)
        {
            await _dbSet.AddRangeAsync(entity);
            if (isSave) await _uow.SaveChangesAsync();
        }

        public virtual async Task<Type> DeleteAsync(Type id, bool isSave = true)
        {
            var entity = await _dbSet.FindAsync(id);
            entity.IsRemoved = true;
            if (isSave) _uow.SaveChanges();
            return entity.Id;
        }

        public virtual async Task DeleteRangeAsync(List<T> list, bool isSave = true)
        {
            foreach (var item in list)
            {
                var entity = await _dbSet.FindAsync(item.Id);
                entity.IsRemoved = true;
                _uow.SaveChanges();
            }
        }

        public virtual async Task<ApiResultList<T>> GetListAsync(PaginationDto pagination, bool isSortedByPriority = true, bool isSortedByCreateDate = false)
        {
            var query = _dbSet
                .AsNoTracking()
                .OrderByDescending(x => x.Id);

            if (isSortedByPriority)
                query = query.ThenByDescending(x => x.Priority.Value);

            if (isSortedByCreateDate)
                query = query.ThenByDescending(x => x.CreatedDate);

            var result = await query
            .Skip((pagination.PageIndex - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

            return new ApiResultList<T>
            {
                Result = result,
                FilteredCount = result.Count,
                TotalCount = query.Count()
            };
        }

        public virtual async Task<ApiResultList<T>> GetListAsync(bool isSortedByPriority = true, bool isSortedByCreateDate = false)
        {
            var query = _dbSet
                .OrderByDescending(x => x.Priority.HasValue)
                .AsNoTracking()
                .OrderByDescending(c => c.Id);

            if (isSortedByPriority)
                query = query.ThenByDescending(x => x.Priority.Value);

            if (isSortedByCreateDate)
                query = query.ThenByDescending(x => x.CreatedDate);

            var result = await query.ToListAsync();

            return new ApiResultList<T>
            {
                Result = result,
                FilteredCount = result.Count,
                TotalCount = query.Count()
            };
        }

        public virtual async Task<Type> UpdateAsync(T entity, bool isSave = true)
        {
            var model = await FindAsync(entity.Id);
            if (model == null)
            {
                return default(Type); //equal null
            }
            _uow.Entry(model).CurrentValues.SetValues(entity);
            if (isSave) await _uow.SaveChangesAsync();

            return entity.Id;
        }

        public virtual async Task<Type> UpdateRangeAsync(List<T> items, bool isSave = true)
        {
            _dbSet.UpdateRange(items);
            if (isSave) await _uow.SaveChangesAsync();
            return default(Type);
        }

        public virtual async Task<T> FindAsync(Type id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual T Find(Type id)
        {
            return _dbSet.Find(id);
        }

        public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> expression)
        {
            return await _dbSet.AnyAsync(expression);
        }

        public virtual IQueryable<T> GetDbSet(Expression<Func<T, bool>> expression)
        {
            IQueryable<T> localEntities = _dbSet.AsQueryable();
            if (expression != null)
            {
                localEntities = localEntities.Where(expression);
            }
            return localEntities;
        }

        public virtual DbSet<T> GetDbSet()
        {
            return _dbSet;
        }
    }
}
