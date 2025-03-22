using CoreArchV2.Data.GenericRepository;

namespace CoreArchV2.Data.UnitOfWorkLicence
{
    public interface IUowLicence : IDisposable
    {
        IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : class;
        int SaveChanges();
        Task<int> CommitAsync();
    }
}
