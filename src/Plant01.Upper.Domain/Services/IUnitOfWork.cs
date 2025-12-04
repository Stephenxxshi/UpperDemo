using Plant01.Upper.Domain.Services;

namespace Plant01.Upper.Domain.Services;

public interface IUnitOfWork : IDisposable
{
    IRepository<TEntity> Repository<TEntity>() where TEntity : class;
    Task<int> SaveChangesAsync();
}
