using Microsoft.EntityFrameworkCore;

using Plant01.Upper.Domain.Aggregation;
using Plant01.Upper.Domain.Repository;

namespace Plant01.Upper.Infrastructure.Repository;

public class BagRepository : AppRepository<Bag>, IBagRepository
{
    public BagRepository(IDbContextFactory<AppDbContext> factory) : base(factory)
    {
    }
}
