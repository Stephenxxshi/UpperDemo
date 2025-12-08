using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Plant01.Upper.Domain.Repository;

public interface IRepository<TEntity> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(object id);
    Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        int? skip = null, int? take = null);


    Task<List<TEntity>> WhereAsync(Expression<Func<TEntity, bool>> predicate);
    Task<ObservableCollection<TEntity>> GetAllAsObservableAsync();
    Task<ObservableCollection<TEntity>> WhereAsObservableAsync(Expression<Func<TEntity, bool>> predicate);

    Task<List<TEntity>> GetPagedAsync(int pageIndex, int pageSize);
    Task<List<TEntity>> GetPagedAsync<TKey>(int pageIndex, int pageSize, Expression<Func<TEntity, TKey>> orderBy, bool descending = false);
    Task<(List<TEntity> items, int maxPageCount)> GetPagedAsync(Expression<Func<TEntity, bool>>? predicate = null,Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,int? skip = null, int? take = null);

    Task AddAsync(TEntity entity);
    Task UpdateAsync(TEntity entity);
    Task DeleteAsync(TEntity entity);
}
