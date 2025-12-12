using Fudie.DependencyInjection;

namespace webapi.domain.slice;

public interface ICreateFoo
{
    public void Handle();
}
[Injectable]
public class CreateFoo(Foo.FooSave fooSave):ICreateFoo
{   
    public void Handle()
    {
        var foo = new Foo(Guid.NewGuid());
        var command = new Command("Pedro");
        _ = fooSave.Execute(foo, command);
    }
}