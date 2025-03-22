using CoreArchV2.Data.GenericRepository;
using System.Data.Entity;

namespace CoreArchV2.Data.UnitOfWorkLicence
{
    public class UowLicence : IUowLicence, IDisposable
    {
        private readonly LicenceDbContext _context;
        private bool disposed;
        private DbContextTransaction transaction;

        public UowLicence(LicenceDbContext context)
        {
            _context = context;
        }

        public IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : class
        {
            return new GenericRepository<TEntity>(_context);
        }

        public int SaveChanges()
        {
            return _context.SaveChanges();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task<int> CommitAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public virtual void Dispose(bool disposing)
        {
            if (!disposed)
                if (disposing)
                    _context.Dispose();
            disposed = true;
        }
    }
}
