// Fudie/Domain/IModifyCommand.cs
namespace Fudie.Domain;

/// <summary>
/// Comando de dominio para modificar una entidad existente.
/// </summary>
/// <typeparam name="TCommand">Tipo del comando con los datos de modificación.</typeparam>
/// <typeparam name="TEntity">Tipo de la entidad a modificar.</typeparam>
public interface IModifyCommand<TCommand, TEntity>
    where TCommand : class
    where TEntity : Entity
{
    /// <summary>
    /// Ejecuta la modificación de la entidad.
    /// </summary>
    /// <param name="entity">Entidad existente a modificar.</param>
    /// <param name="command">Datos para modificar la entidad.</param>
    /// <returns>Result con la entidad modificada o errores de validación.</returns>
    Result<TEntity> Execute(TEntity entity, TCommand command);
}