namespace webapi.domain;

public record Command(string Name) { }

public abstract class AbstractCommandUpdate<TCommand, TEntity>
{
    protected abstract TEntity Handler(TCommand command, TEntity entity);

}


public partial class Foo
{


    public class FooSave : AbstractCommandUpdate<Command, Foo>
    {
        protected override Foo Handler(Command command, Foo entity)  // ← override + Foo
        {
            /*entity.Name = command.Name;
            return entity;*/
            

            return new Foo()
            {
                Name = command.Name
            };
        }
    }

}