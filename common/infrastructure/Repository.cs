using webapi.common.domain;

namespace webapi.common.infrastructure;

public interface IGet<T, ID>
{
    T Get(ID id);
}

public interface IAdd<T>
{
    T Add();
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