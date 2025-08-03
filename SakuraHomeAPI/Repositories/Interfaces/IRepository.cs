using System.Linq.Expressions;
using SakuraHomeAPI.Models.Base;

namespace SakuraHomeAPI.Repositories.Interfaces
{
    /// <summary>
    /// Generic repository interface providing common CRUD operations
    /// Supports both sync and async operations with flexible querying
    /// </summary>
    /// <typeparam name="T">Entity type that implements IEntity</typeparam>
    public interface IRepository<T> where T : class, IEntity
    {
        #region Async Read Operations
        
        /// <summary>
        /// Get entity by ID asynchronously
        /// </summary>
        Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Get entity by ID with related entities included
        /// </summary>
        Task<T?> GetByIdAsync(int id, params Expression<Func<T, object>>[] includeProperties);
        
        /// <summary>
        /// Get all entities asynchronously
        /// </summary>
        Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Get all entities with includes
        /// </summary>
        Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includeProperties);
        
        /// <summary>
        /// Get entities with filtering, sorting, and includes
        /// </summary>
        Task<IEnumerable<T>> GetAsync(
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            params Expression<Func<T, object>>[] includeProperties);
        
        /// <summary>
        /// Get first entity matching the filter
        /// </summary>
        Task<T?> GetFirstAsync(
            Expression<Func<T, bool>> filter,
            params Expression<Func<T, object>>[] includeProperties);
        
        /// <summary>
        /// Get single entity matching the filter (throws if more than one)
        /// </summary>
        Task<T?> GetSingleAsync(
            Expression<Func<T, bool>> filter,
            params Expression<Func<T, object>>[] includeProperties);
        
        #endregion

        #region Pagination & Counting
        
        /// <summary>
        /// Get paged results with total count
        /// </summary>
        Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            params Expression<Func<T, object>>[] includeProperties);
        
        /// <summary>
        /// Count entities matching filter
        /// </summary>
        Task<int> CountAsync(Expression<Func<T, bool>>? filter = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Check if any entity matches the filter
        /// </summary>
        Task<bool> ExistsAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default);
        
        #endregion

        #region Write Operations
        
        /// <summary>
        /// Add new entity
        /// </summary>
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Add multiple entities
        /// </summary>
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Update existing entity
        /// </summary>
        Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Update multiple entities
        /// </summary>
        Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Delete entity by ID
        /// </summary>
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Delete entity
        /// </summary>
        Task<bool> DeleteAsync(T entity, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Delete multiple entities
        /// </summary>
        Task<int> DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Delete entities matching filter
        /// </summary>
        Task<int> DeleteAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default);
        
        #endregion

        #region Soft Delete Support
        
        /// <summary>
        /// Soft delete entity (if implements ISoftDelete)
        /// </summary>
        Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Restore soft deleted entity
        /// </summary>
        Task<bool> RestoreAsync(int id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Get all entities including soft deleted ones
        /// </summary>
        Task<IEnumerable<T>> GetAllWithDeletedAsync(CancellationToken cancellationToken = default);
        
        #endregion

        #region Advanced Queries
        
        /// <summary>
        /// Execute raw SQL query
        /// </summary>
        Task<IEnumerable<T>> FromSqlRawAsync(string sql, params object[] parameters);
        
        /// <summary>
        /// Get queryable for complex operations
        /// </summary>
        IQueryable<T> GetQueryable();
        
        /// <summary>
        /// Get queryable with includes
        /// </summary>
        IQueryable<T> GetQueryable(params Expression<Func<T, object>>[] includeProperties);
        
        #endregion

        #region Save Operations
        
        /// <summary>
        /// Save all changes to database
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        
        #endregion
    }
}