namespace webapi.domain;

using Fudie.DependencyInjection;
using Fudie.Domain;
public record Command(string Name) { }

public partial class Foo
{

    [Injectable(ServiceLifetime.Singleton)]
    public class FooSave : AbstractModifyCommand<Command, Foo>
    {
        public override Foo Execute(Foo entity, Command command)
        {
           entity.Name = command.Name;
           return entity;
        }
    }

}