using Microsoft.EntityFrameworkCore;

namespace Plant01.Upper.Infrastructure.Repository;

public class AppRepository<TEntity> : EfRepository<TEntity, AppDbContext> where TEntity : class
{
    // Constructor for Standalone usage (injected by DI usually)
    public AppRepository(IDbContextFactory<AppDbContext> factory) : base(factory)
    {
    }

    // Constructor for UnitOfWork usage (created manually by UnitOfWork)
    public AppRepository(AppDbContext context) : base(context)
    {
    }
}
