namespace Fudie.Domain;

/// <summary>
/// Comando de dominio abstracto para crear una nueva entidad.
/// </summary>
/// <typeparam name="TCommand">Tipo del comando con los datos de creación.</typeparam>
/// <typeparam name="TEntity">Tipo de la entidad a crear.</typeparam>
public abstract class AbstractCreateCommand<TCommand, TEntity>
    where TCommand : class
    where TEntity : IEntity
{
    /// <summary>
    /// Ejecuta la creación de la entidad.
    /// </summary>
    /// <param name="command">Datos para crear la entidad.</param>
    /// <returns>La entidad creada.</returns>
    public abstract TEntity Execute(TCommand command);
}