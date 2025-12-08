// Fudie/Domain/ICreateCommand.cs
namespace Fudie.Domain;

/// <summary>
/// Comando de dominio para crear una nueva entidad.
/// </summary>
/// <typeparam name="TCommand">Tipo del comando con los datos de creación.</typeparam>
/// <typeparam name="TEntity">Tipo de la entidad a crear.</typeparam>
public interface ICreateCommand<TCommand, TEntity>
    where TCommand : class
    where TEntity : Entity
{
    /// <summary>
    /// Ejecuta la creación de la entidad.
    /// </summary>
    /// <param name="command">Datos para crear la entidad.</param>
    /// <returns>Result con la entidad creada o errores de validación.</returns>
    Result<TEntity> Execute(TCommand command);
}