using CoreArchV2.Data.GenericRepository;
using Microsoft.EntityFrameworkCore.Storage;

namespace CoreArchV2.Data.UnitOfWork
{
    public interface IUnitOfWork : IAsyncDisposable, IDisposable
    {
        IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : class;

        int SaveChanges();
        Task<int> SaveChangesAsync();

        Task<IDbContextTransaction> BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
