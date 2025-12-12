namespace webapi.domain.slice;

public class CreateFoo(Foo.FooSave fooSave)
{   
    public void Handle()
    {
        var foo = new Foo(Guid.NewGuid());
        var command = new Command("Pedro");
        _ = fooSave.Execute(foo, command);
    }
}