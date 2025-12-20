using Microsoft.EntityFrameworkCore;

using Plant01.Upper.Domain.Repository;

using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Plant01.Upper.Infrastructure.Repository;

public class EfRepository<TEntity, TContext> : IRepository<TEntity> 
    where TEntity : class 
    where TContext : DbContext
{
    protected readonly IDbContextFactory<TContext>? _contextFactory;
    protected readonly TContext? _sharedContext;
    protected readonly bool _isSharedContext;

    public EfRepository(IDbContextFactory<TContext> contextFactory)
    {
        _contextFactory = contextFactory;
        _isSharedContext = false;
    }

    public EfRepository(TContext context)
    {
        _sharedContext = context;
        _isSharedContext = true;
    }

    /// <summary>
    /// Helper to execute read operations with correct context handling (Tracking vs NoTracking)
    /// </summary>
    private async Task<TResult> ExecuteReadAsync<TResult>(Func<IQueryable<TEntity>, Task<TResult>> operation)
    {
        if (_isSharedContext)
        {
            return await operation(_sharedContext!.Set<TEntity>());
        }
        
        using var context = await _contextFactory!.CreateDbContextAsync();
        return await operation(context.Set<TEntity>().AsNoTracking());
    }

    /// <summary>
    /// Helper to execute write operations with correct context handling (Auto-Save vs Manual Save)
    /// </summary>
    private async Task ExecuteWriteAsync(Func<DbSet<TEntity>, Task> operation)
    {
        if (_isSharedContext)
        {
            await operation(_sharedContext!.Set<TEntity>());
        }
        else
        {
            using var context = await _contextFactory!.CreateDbContextAsync();
            await operation(context.Set<TEntity>());
            await context.SaveChangesAsync();
        }
    }

    public async Task<TEntity?> GetByIdAsync(object id)
    {
        if (_isSharedContext)
        {
            return await _sharedContext!.Set<TEntity>().FindAsync(id);
        }
        
        using var context = await _contextFactory!.CreateDbContextAsync();
        return await context.Set<TEntity>().FindAsync(id);
    }

    public async Task<List<TEntity>> GetAllAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        int? skip = null, int? take = null)
    {
        return await ExecuteReadAsync(async query => 
        {
            if (predicate != null) query = query.Where(predicate);
            if (orderBy != null) query = orderBy(query);
            if (skip.HasValue) query = query.Skip(skip.Value);
            if (take.HasValue) query = query.Take(take.Value);
            return await query.ToListAsync();
        });
    }

    public async Task<ObservableCollection<TEntity>> GetAllAsObservableAsync()
    {
        var list = await GetAllAsync();
        return new ObservableCollection<TEntity>(list);
    }

    public async Task<List<TEntity>> WhereAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await ExecuteReadAsync(async query => await query.Where(predicate).ToListAsync());
    }

    public async Task<ObservableCollection<TEntity>> WhereAsObservableAsync(Expression<Func<TEntity, bool>> predicate)
    {
        var list = await WhereAsync(predicate);
        return new ObservableCollection<TEntity>(list);
    }

    public async Task<List<TEntity>> GetPagedAsync(int pageIndex, int pageSize)
    {
        return await ExecuteReadAsync(async query => 
            await query.Skip(pageIndex * pageSize).Take(pageSize).ToListAsync());
    }

    public async Task<List<TEntity>> GetPagedAsync<TKey>(int pageIndex, int pageSize, Expression<Func<TEntity, TKey>> orderBy, bool descending = false)
    {
        return await ExecuteReadAsync(async query => 
        {
            query = descending ? query.OrderByDescending(orderBy) : query.OrderBy(orderBy);
            return await query.Skip(pageIndex * pageSize).Take(pageSize).ToListAsync();
        });
    }

    public async Task<(List<TEntity> items, int maxPageCount)> GetPagedAsync(
        Expression<Func<TEntity, bool>>? predicate = null, 
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null, 
        int? skip = null, int? take = null)
    {
        return await ExecuteReadAsync(async query => 
        {
            if (predicate != null) query = query.Where(predicate);
            
            var count = await query.CountAsync();
            
            if (orderBy != null) query = orderBy(query);
            if (skip.HasValue) query = query.Skip(skip.Value);
            if (take.HasValue) query = query.Take(take.Value);
            
            var items = await query.ToListAsync();
            return (items, count);
        });
    }

    public async Task AddAsync(TEntity entity)
    {
        await ExecuteWriteAsync(async dbSet => await dbSet.AddAsync(entity));
    }

    public async Task UpdateAsync(TEntity entity)
    {
        await ExecuteWriteAsync(dbSet => 
        {
            dbSet.Update(entity);
            return Task.CompletedTask;
        });
    }

    public async Task DeleteAsync(TEntity entity)
    {
        await ExecuteWriteAsync(dbSet => 
        {
            dbSet.Remove(entity);
            return Task.CompletedTask;
        });
    }
}
