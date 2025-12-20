using Microsoft.EntityFrameworkCore;

using Plant01.Upper.Domain.Aggregation;
using Plant01.Upper.Domain.Repository;

namespace Plant01.Upper.Infrastructure.Repository;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private readonly Dictionary<Type, object> _repositories = new();

    public UnitOfWork(IDbContextFactory<AppDbContext> factory)
    {
        _context = factory.CreateDbContext();
    }

    public IBagRepository BagRepository
    {
        get
        {
            if (_repositories.ContainsKey(typeof(Bag)))
            {
                return (IBagRepository)_repositories[typeof(Bag)];
            }

            var repo = new BagRepository(_context);
            _repositories.Add(typeof(Bag), repo);
            return repo;
        }
    }

    public IRepository<TEntity> Repository<TEntity>() where TEntity : class
    {
        if (_repositories.ContainsKey(typeof(TEntity)))
        {
            return (IRepository<TEntity>)_repositories[typeof(TEntity)];
        }

        if (typeof(TEntity) == typeof(Bag))
        {
            return (IRepository<TEntity>)BagRepository;
        }

        // Create AppRepository with the shared context
        var repository = new AppRepository<TEntity>(_context);
        _repositories.Add(typeof(TEntity), repository);
        return repository;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
