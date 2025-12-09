namespace Fudie.Domain;

public interface ICreateCommand<TCommand, TEntity>
    where TCommand : class
    where TEntity : Entity
{
    TEntity Execute(TCommand command);
}
