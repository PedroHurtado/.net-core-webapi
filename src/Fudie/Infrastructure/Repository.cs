using Microsoft.EntityFrameworkCore.ChangeTracking;
using Fudie.Domain;

namespace Fudie.Infrastructure;

/// <summary>
/// Defines a contract for retrieving entities by their identifier.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
/// <typeparam name="ID">The identifier type.</typeparam>
public interface IGet<T, ID>
{
    /// <summary>
    /// Retrieves an entity by its identifier.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the entity.</returns>
    Task<T> Get(ID id);
}

/// <summary>
/// Defines a contract for adding entities to the repository.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public interface IAdd<T>
{
    /// <summary>
    /// Adds an entity to the repository.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    void Add(T entity);
}

/// <summary>
/// Defines a contract for updating entities.
/// Inherits from <see cref="IGet{T, ID}"/> to retrieve entities before updating.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
/// <typeparam name="ID">The identifier type.</typeparam>
public interface IUpdate<T, ID> : IGet<T, ID>
{
}

/// <summary>
/// Defines a contract for removing entities from the repository.
/// Inherits from <see cref="IGet{T, ID}"/> to retrieve entities before removing.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
/// <typeparam name="ID">The identifier type.</typeparam>
public interface IRemove<T, ID> : IGet<T, ID>
{
    /// <summary>
    /// Removes an entity from the repository.
    /// </summary>
    /// <param name="entity">The entity to remove.</param>
    void Remove(T entity);
}

/// <summary>
/// Defines a contract for querying entities.
/// </summary>
public interface IQuery
{
    /// <summary>
    /// Creates a queryable collection of entities with no tracking.
    /// </summary>
    /// <typeparam name="T">The entity type that inherits from <see cref="Entity"/>.</typeparam>
    /// <returns>An <see cref="IQueryable{T}"/> for the entity type.</returns>
    IQueryable<T> Query<T>() where T : Entity;
}

/// <summary>
/// Defines a contract for retrieving entities with validation.
/// Used primarily for foreign key validation and ensuring entity existence.
/// </summary>
public interface IEntityLookup
{
    /// <summary>
    /// Retrieves an entity by its identifier with optional tracking and navigation properties.
    /// This method is typically used for foreign key validation to ensure referenced entities exist.
    /// </summary>
    /// <typeparam name="T">The entity type that inherits from <see cref="Entity"/>.</typeparam>
    /// <typeparam name="ID">The identifier type.</typeparam>
    /// <param name="id">The entity identifier.</param>
    /// <param name="tracking">Indicates whether the entity should be tracked by the context. Default is true.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <param name="includeProperties">Navigation property names to eagerly load.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the entity.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the entity with the specified identifier is not found.</exception>
    Task<T> GetRequiredAsync<T, ID>(
        ID id,
        bool tracking = true,
        CancellationToken cancellationToken = default,
        params string[] includeProperties) where T : Entity;
}

/// <summary>
/// Defines a contract for managing database transactions and persisting changes.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Saves all changes made in this context to the database synchronously.
    /// </summary>
    /// <returns>The number of state entries written to the database.</returns>
    int SaveChanges();

    /// <summary>
    /// Saves all changes made in this context to the database asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous save operation. The task result contains the number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}



/// <summary>
/// Defines a contract for accessing entity entries in the context to track changes.
/// </summary>
public interface IChangeTracker
{
    /// <summary>
    /// Gets an <see cref="EntityEntry{TEntity}"/> for the given entity, providing access to change tracking information and operations.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="entity">The entity instance.</param>
    /// <returns>An <see cref="EntityEntry{TEntity}"/> for the specified entity.</returns>
    EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
}

