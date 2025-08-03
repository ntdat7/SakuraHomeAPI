using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using SakuraHomeAPI.Data;
using SakuraHomeAPI.Models.Base;
using SakuraHomeAPI.Repositories.Interfaces;

namespace SakuraHomeAPI.Repositories.Implementations
{
    /// <summary>
    /// Generic repository implementation using Entity Framework Core
    /// Provides comprehensive CRUD operations with performance optimizations
    /// </summary>
    /// <typeparam name="T">Entity type that implements IEntity</typeparam>
    public class BaseRepository<T> : IRepository<T> where T : class, IEntity
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public BaseRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<T>();
        }

        #region Async Read Operations

        public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
        }

        public virtual async Task<T?> GetByIdAsync(int id, params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = _dbSet;
            
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }
            
            return await query.FirstOrDefaultAsync(e => e.Id == id);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _dbSet;
            
            // Apply soft delete filter if entity supports it
            if (typeof(ISoftDelete).IsAssignableFrom(typeof(T)))
            {
                query = query.Where(e => !((ISoftDelete)e).IsDeleted);
            }
            
            return await query.ToListAsync(cancellationToken);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = _dbSet;
            
            // Apply includes
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }
            
            // Apply soft delete filter if entity supports it
            if (typeof(ISoftDelete).IsAssignableFrom(typeof(T)))
            {
                query = query.Where(e => !((ISoftDelete)e).IsDeleted);
            }
            
            return await query.ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> GetAsync(
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = _dbSet;

            // Apply soft delete filter if entity supports it
            if (typeof(ISoftDelete).IsAssignableFrom(typeof(T)))
            {
                query = query.Where(e => !((ISoftDelete)e).IsDeleted);
            }

            // Apply filter
            if (filter != null)
            {
                query = query.Where(filter);
            }

            // Apply includes
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }

            // Apply ordering
            if (orderBy != null)
            {
                query = orderBy(query);
            }

            return await query.ToListAsync();
        }

        public virtual async Task<T?> GetFirstAsync(
            Expression<Func<T, bool>> filter,
            params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = _dbSet;

            // Apply soft delete filter if entity supports it
            if (typeof(ISoftDelete).IsAssignableFrom(typeof(T)))
            {
                query = query.Where(e => !((ISoftDelete)e).IsDeleted);
            }

            // Apply includes
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }

            return await query.FirstOrDefaultAsync(filter);
        }

        public virtual async Task<T?> GetSingleAsync(
            Expression<Func<T, bool>> filter,
            params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = _dbSet;

            // Apply soft delete filter if entity supports it
            if (typeof(ISoftDelete).IsAssignableFrom(typeof(T)))
            {
                query = query.Where(e => !((ISoftDelete)e).IsDeleted);
            }

            // Apply includes
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }

            return await query.SingleOrDefaultAsync(filter);
        }

        #endregion

        #region Pagination & Counting

        public virtual async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = _dbSet;

            // Apply soft delete filter if entity supports it
            if (typeof(ISoftDelete).IsAssignableFrom(typeof(T)))
            {
                query = query.Where(e => !((ISoftDelete)e).IsDeleted);
            }

            // Apply filter
            if (filter != null)
            {
                query = query.Where(filter);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply includes
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }

            // Apply ordering
            if (orderBy != null)
            {
                query = orderBy(query);
            }

            // Apply pagination
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? filter = null, CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _dbSet;

            // Apply soft delete filter if entity supports it
            if (typeof(ISoftDelete).IsAssignableFrom(typeof(T)))
            {
                query = query.Where(e => !((ISoftDelete)e).IsDeleted);
            }

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return await query.CountAsync(cancellationToken);
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _dbSet;

            // Apply soft delete filter if entity supports it
            if (typeof(ISoftDelete).IsAssignableFrom(typeof(T)))
            {
                query = query.Where(e => !((ISoftDelete)e).IsDeleted);
            }

            return await query.AnyAsync(filter, cancellationToken);
        }

        #endregion

        #region Write Operations

        public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            // Set audit fields if entity supports it
            if (entity is IAuditable auditableEntity)
            {
                auditableEntity.CreatedAt = DateTime.UtcNow;
                auditableEntity.UpdatedAt = DateTime.UtcNow;
            }

            await _dbSet.AddAsync(entity, cancellationToken);
            return entity;
        }

        public virtual async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            var entityList = entities.ToList();
            var now = DateTime.UtcNow;

            // Set audit fields for all entities
            foreach (var entity in entityList.OfType<IAuditable>())
            {
                entity.CreatedAt = now;
                entity.UpdatedAt = now;
            }

            await _dbSet.AddRangeAsync(entityList, cancellationToken);
        }

        public virtual Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            // Set audit fields if entity supports it
            if (entity is IAuditable auditableEntity)
            {
                auditableEntity.UpdatedAt = DateTime.UtcNow;
            }

            _dbSet.Update(entity);
            return Task.FromResult(entity);
        }

        public virtual Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            var entityList = entities.ToList();
            var now = DateTime.UtcNow;

            // Set audit fields for all entities
            foreach (var entity in entityList.OfType<IAuditable>())
            {
                entity.UpdatedAt = now;
            }

            _dbSet.UpdateRange(entityList);
            return Task.CompletedTask;
        }

        public virtual async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await GetByIdAsync(id, cancellationToken);
            if (entity == null)
                return false;

            return await DeleteAsync(entity, cancellationToken);
        }

        public virtual Task<bool> DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
                return Task.FromResult(false);

            _dbSet.Remove(entity);
            return Task.FromResult(true);
        }

        public virtual Task<int> DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            if (entities == null)
                return Task.FromResult(0);

            var entityList = entities.ToList();
            _dbSet.RemoveRange(entityList);
            return Task.FromResult(entityList.Count);
        }

        public virtual async Task<int> DeleteAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default)
        {
            var entities = await _dbSet.Where(filter).ToListAsync(cancellationToken);
            _dbSet.RemoveRange(entities);
            return entities.Count;
        }

        #endregion

        #region Soft Delete Support

        public virtual async Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await GetByIdAsync(id, cancellationToken);
            if (entity == null || !(entity is ISoftDelete softDeleteEntity))
                return false;

            softDeleteEntity.IsDeleted = true;
            softDeleteEntity.DeletedAt = DateTime.UtcNow;

            if (entity is IAuditable auditableEntity)
            {
                auditableEntity.UpdatedAt = DateTime.UtcNow;
            }

            await UpdateAsync(entity, cancellationToken);
            return true;
        }

        public virtual async Task<bool> RestoreAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbSet.IgnoreQueryFilters()
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
                
            if (entity == null || !(entity is ISoftDelete softDeleteEntity))
                return false;

            softDeleteEntity.IsDeleted = false;
            softDeleteEntity.DeletedAt = null;

            if (entity is IAuditable auditableEntity)
            {
                auditableEntity.UpdatedAt = DateTime.UtcNow;
            }

            await UpdateAsync(entity, cancellationToken);
            return true;
        }

        public virtual async Task<IEnumerable<T>> GetAllWithDeletedAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.IgnoreQueryFilters().ToListAsync(cancellationToken);
        }

        #endregion

        #region Advanced Queries

        public virtual async Task<IEnumerable<T>> FromSqlRawAsync(string sql, params object[] parameters)
        {
            return await _dbSet.FromSqlRaw(sql, parameters).ToListAsync();
        }

        public virtual IQueryable<T> GetQueryable()
        {
            IQueryable<T> query = _dbSet;
            
            // Apply soft delete filter if entity supports it
            if (typeof(ISoftDelete).IsAssignableFrom(typeof(T)))
            {
                query = query.Where(e => !((ISoftDelete)e).IsDeleted);
            }
            
            return query;
        }

        public virtual IQueryable<T> GetQueryable(params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = _dbSet;

            // Apply soft delete filter if entity supports it
            if (typeof(ISoftDelete).IsAssignableFrom(typeof(T)))
            {
                query = query.Where(e => !((ISoftDelete)e).IsDeleted);
            }

            // Apply includes
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }

            return query;
        }

        #endregion

        #region Save Operations

        public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        #endregion
    }
}