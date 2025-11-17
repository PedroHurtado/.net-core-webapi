using Microsoft.EntityFrameworkCore.ChangeTracking;
using webapi.common.domain;

namespace webapi.common.infrastructure;

public interface IGet<T, ID>
{
    Task<T> Get(ID id);
}

public interface IAdd<T>
{
    void Add(T entity);
}

public interface IUpdate<T,ID>:IGet<T,ID>{
    void Update(T entity);
}

public interface IRemove<T, ID> : IGet<T, ID>
{
    void Remove(T entity);
}

public interface IQuery
{
    IQueryable<T> Query<T>() where T:Entity;
}

public interface IGetOrThrowAsync
{
    Task<T> GetOrThrowAsync<T, ID>(ID id,
       bool tracking = true, CancellationToken cancellationToken = default) where T : Entity;
}

public interface IUnitOfWork
{
    
    int SaveChanges();
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
public interface IRepository
{
    EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
}
