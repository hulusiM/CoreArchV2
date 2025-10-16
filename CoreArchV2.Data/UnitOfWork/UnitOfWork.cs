using CoreArchV2.Data.GenericRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CoreArchV2.Data.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly CoreArchDbContext _context;
        private readonly Dictionary<Type, object> _repositories = new();
        private IDbContextTransaction? _currentTransaction;
        private bool _disposed;

        public UnitOfWork(CoreArchDbContext context)
        {
            _context = context;
        }

        public IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : class
        {
            if (_repositories.ContainsKey(typeof(TEntity)))
                return (IGenericRepository<TEntity>)_repositories[typeof(TEntity)];

            var repo = new GenericRepository<TEntity>(_context);
            _repositories.Add(typeof(TEntity), repo);
            return repo;
        }

        public int SaveChanges()
        {
            return _context.SaveChanges();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        #region Transaction Management

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            _currentTransaction ??= await _context.Database.BeginTransactionAsync().ConfigureAwait(false);
            return _currentTransaction;
        }

        public async Task CommitTransactionAsync()
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.CommitAsync().ConfigureAwait(false);
                await _currentTransaction.DisposeAsync().ConfigureAwait(false);
                _currentTransaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync().ConfigureAwait(false);
                await _currentTransaction.DisposeAsync().ConfigureAwait(false);
                _currentTransaction = null;
            }
        }

        #endregion

        #region Dispose Pattern (.NET 8)

        public async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                if (_currentTransaction != null)
                    await _currentTransaction.DisposeAsync().ConfigureAwait(false);

                await _context.DisposeAsync().ConfigureAwait(false);
                _disposed = true;
            }

            GC.SuppressFinalize(this);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _context.Dispose();
                _disposed = true;
            }

            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
