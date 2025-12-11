namespace Fudie.Domain;

/// <summary>
/// Comando de dominio abstracto para modificar una entidad existente sin datos adicionales.
/// </summary>
/// <typeparam name="TEntity">Tipo de la entidad a modificar.</typeparam>
public abstract class AbstractModifyCommand<TEntity>
    where TEntity : Entity
{
    /// <summary>
    /// Ejecuta la modificación de la entidad.
    /// </summary>
    /// <param name="entity">Entidad existente a modificar.</param>
    /// <returns>La entidad modificada.</returns>
    public abstract TEntity Execute(TEntity entity);

    /// <summary>
    /// Ejecuta la modificación de forma asíncrona obteniendo la entidad del Task.
    /// </summary>
    /// <param name="entityTask">Task que resuelve la entidad a modificar.</param>
    /// <returns>La entidad modificada.</returns>
    public async Task<TEntity> ExecuteAsync(Task<TEntity> entityTask)
    {
        var entity = await entityTask;
        return Execute(entity);
    }
}

/// <summary>
/// Comando de dominio abstracto para modificar una entidad existente con datos.
/// </summary>
/// <typeparam name="TCommand">Tipo del comando con los datos de modificación.</typeparam>
/// <typeparam name="TEntity">Tipo de la entidad a modificar.</typeparam>
public abstract class AbstractModifyCommand<TCommand, TEntity>
    where TCommand : class
    where TEntity : Entity
{
    /// <summary>
    /// Ejecuta la modificación de la entidad.
    /// </summary>
    /// <param name="entity">Entidad existente a modificar.</param>
    /// <param name="command">Datos para modificar la entidad.</param>
    /// <returns>La entidad modificada.</returns>
    public abstract TEntity Execute(TEntity entity, TCommand command);

    /// <summary>
    /// Ejecuta la modificación de forma asíncrona obteniendo la entidad del Task.
    /// </summary>
    /// <param name="entityTask">Task que resuelve la entidad a modificar.</param>
    /// <param name="command">Datos para modificar la entidad.</param>
    /// <returns>La entidad modificada.</returns>
    public async Task<TEntity> ExecuteAsync(Task<TEntity> entityTask, TCommand command)
    {
        var entity = await entityTask;
        return Execute(entity, command);
    }
}
