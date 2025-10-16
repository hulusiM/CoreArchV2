using System.Linq.Expressions;

namespace CoreArchV2.Data.GenericRepository
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        #region Async Methods

        Task<TEntity> InsertAsync(TEntity entity);
        Task<IEnumerable<TEntity>> InsertRangeAsync(IEnumerable<TEntity> entities);
        Task<List<TEntity>> WhereAsync(Expression<Func<TEntity, bool>> predicate);
        Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate);
        Task<List<TEntity>> GetAllAsync();
        Task<TEntity?> FindAsync(int id);
        Task<TEntity?> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> filter);
        Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter);
        Task<TEntity?> FirstOrDefaultNoTrackingAsync(Expression<Func<TEntity, bool>> filter);

        #endregion

        #region Sync Methods

        IQueryable<TEntity> GetAll();
        IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> predicate);
        int Count(Expression<Func<TEntity, bool>> predicate);
        TEntity? SingleOrDefault(Expression<Func<TEntity, bool>> filter);
        TEntity? FirstOrDefault(Expression<Func<TEntity, bool>> filter);
        TEntity? FirstOrDefaultNoTracking(Expression<Func<TEntity, bool>> filter);
        TEntity? Find(int id);
        TEntity Insert(TEntity entity);
        IEnumerable<TEntity> InsertRange(IEnumerable<TEntity> entities);
        void Update(TEntity entity);
        void UpdateRange(IEnumerable<TEntity> entities);
        void Delete(TEntity entity);
        void DeleteRange(IEnumerable<TEntity> entities);
        bool Any(Expression<Func<TEntity, bool>> filter);

        #endregion
    }
}
