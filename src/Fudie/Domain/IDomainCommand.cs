// Fudie/Domain/IDomainCommand.cs
namespace Fudie.Domain;

public interface IDomainCommand<TCommand, TEntity>
    where TCommand : class
    where TEntity : Entity
{
    Result<TEntity> Execute(TCommand command);
}