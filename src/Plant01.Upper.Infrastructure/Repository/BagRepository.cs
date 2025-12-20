using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

using Plant01.Upper.Domain.Aggregation;
using Plant01.Upper.Domain.Repository;

namespace Plant01.Upper.Infrastructure.Repository;

public class BagRepository : AppRepository<Bag>, IBagRepository
{
    public BagRepository(IDbContextFactory<AppDbContext> factory) : base(factory)
    {
    }

    public BagRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Bag?> GetByCodeAsync(string code)
    {
        if (_isSharedContext)
        {
            return await _sharedContext!.Set<Bag>().FirstOrDefaultAsync(x => x.BagCode == code);
        }

        using var context = await _contextFactory!.CreateDbContextAsync();
        return await context.Set<Bag>().FirstOrDefaultAsync(x => x.BagCode == code);
    }
}
