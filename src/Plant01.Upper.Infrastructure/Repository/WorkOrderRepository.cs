using Microsoft.EntityFrameworkCore;

using Plant01.Upper.Domain.Entities;
using Plant01.Upper.Infrastructure.Repository;

namespace Plant01.Upper.Domain.Repository;

public class WorkOrderRepository : AppRepository<WorkOrder>, IWorkOrderRepository
{
    public WorkOrderRepository(IDbContextFactory<AppDbContext> factory) : base(factory)
    {
    }
}
