using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CoreArchV2.Data.GenericRepository
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        protected readonly DbContext _context;
        protected readonly DbSet<TEntity> _dbSet;

        public GenericRepository(DbContext context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
        }

        #region Async Methods

        public async Task<TEntity> InsertAsync(TEntity entity)
        {
            await _dbSet.AddAsync(entity).ConfigureAwait(false);
            return entity;
        }

        public async Task<IEnumerable<TEntity>> InsertRangeAsync(IEnumerable<TEntity> entities)
        {
            // EF Core 8 AddRangeAsync memory-efficient hale getirildi
            await _dbSet.AddRangeAsync(entities).ConfigureAwait(false);
            return entities;
        }

        public async Task<List<TEntity>> WhereAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbSet.AsNoTracking()
                               .Where(predicate)
                               .ToListAsync()
                               .ConfigureAwait(false);
        }

        public async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbSet.CountAsync(predicate).ConfigureAwait(false);
        }

        public async Task<List<TEntity>> GetAllAsync()
        {
            return await _dbSet.AsNoTracking().ToListAsync().ConfigureAwait(false);
        }

        public async Task<TEntity?> FindAsync(int id)
        {
            return await _dbSet.FindAsync(id).ConfigureAwait(false);
        }

        public async Task<TEntity?> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbSet.AsNoTracking()
                               .SingleOrDefaultAsync(predicate)
                               .ConfigureAwait(false);
        }

        public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> filter)
        {
            return await _dbSet.AnyAsync(filter).ConfigureAwait(false);
        }

        public async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter)
        {
            return await _dbSet.FirstOrDefaultAsync(filter).ConfigureAwait(false);
        }

        public async Task<TEntity?> FirstOrDefaultNoTrackingAsync(Expression<Func<TEntity, bool>> filter)
        {
            return await _dbSet.AsNoTracking()
                               .FirstOrDefaultAsync(filter)
                               .ConfigureAwait(false);
        }

        #endregion

        #region Sync Methods

        public IQueryable<TEntity> GetAll()
        {
            // Varsayılan NoTracking (sadece okuma senaryoları için hızlı)
            return _dbSet.AsNoTracking();
        }

        public IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
        {
            return _dbSet.Where(predicate).AsNoTracking();
        }

        public int Count(Expression<Func<TEntity, bool>> predicate)
        {
            return _dbSet.Count(predicate);
        }

        public TEntity? SingleOrDefault(Expression<Func<TEntity, bool>> filter)
        {
            return _dbSet.AsNoTracking().SingleOrDefault(filter);
        }

        public TEntity? FirstOrDefault(Expression<Func<TEntity, bool>> filter)
        {
            return _dbSet.AsNoTracking().FirstOrDefault(filter);
        }

        public TEntity? FirstOrDefaultNoTracking(Expression<Func<TEntity, bool>> filter)
        {
            return _dbSet.AsNoTracking().FirstOrDefault(filter);
        }

        public TEntity? Find(int id)
        {
            return _dbSet.Find(id);
        }

        public TEntity Insert(TEntity entity)
        {
            _dbSet.Add(entity);
            return entity;
        }

        public IEnumerable<TEntity> InsertRange(IEnumerable<TEntity> entities)
        {
            _dbSet.AddRange(entities);
            return entities;
        }

        public void Update(TEntity entity)
        {
            _dbSet.Update(entity);
        }

        public void UpdateRange(IEnumerable<TEntity> entities)
        {
            _dbSet.UpdateRange(entities);
        }

        public void Delete(TEntity entity)
        {
            _dbSet.Remove(entity);
        }

        public void DeleteRange(IEnumerable<TEntity> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        public bool Any(Expression<Func<TEntity, bool>> filter)
        {
            return _dbSet.Any(filter);
        }

        #endregion
    }
}
