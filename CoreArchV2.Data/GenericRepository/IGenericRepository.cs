using CoreArchV2.Utilies.SessionOperations;
using System.Linq.Expressions;

namespace CoreArchV2.Data.GenericRepository
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        SessionContext _workContext { get; set; }

        #region Async Method

        Task<TEntity> InsertAsync(TEntity entity);
        Task<IEnumerable<TEntity>> InsertRangeAsync(IEnumerable<TEntity> entities); //Çoklu ekleme
        Task<IEnumerable<TEntity>> WhereAsync(Expression<Func<TEntity, bool>> predicate);
        Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate);
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<TEntity> FindAsync(int id);
        Task<TEntity> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> filter);

        #endregion Async Method


        #region Sync Method

        TEntity SingleOrDefault(Expression<Func<TEntity, bool>> filter);
        TEntity FirstOrDefault(Expression<Func<TEntity, bool>> filter);
        Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter);
        TEntity FirstOrDefaultNoTracking(Expression<Func<TEntity, bool>> filter);
        Task<TEntity> FirstOrDefaultNoTrackingAsync(Expression<Func<TEntity, bool>> filter);
        IQueryable<TEntity> GetAll();
        IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> predicate);
        int Count(Expression<Func<TEntity, bool>> predicate);
        TEntity Find(int id);
        TEntity FindForInsertUpdateDelete(int id);
        TEntity Insert(TEntity entity);
        IEnumerable<TEntity> InsertRange(IEnumerable<TEntity> entities);
        void Update(TEntity entity);
        void UpdateRange(IEnumerable<TEntity> entities);
        void Delete(TEntity entity);
        void DeleteRange(IEnumerable<TEntity> entities); //Çoklu silme
        bool Any(Expression<Func<TEntity, bool>> filter);

        #endregion Sync Method
    }
}