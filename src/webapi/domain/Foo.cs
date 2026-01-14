namespace webapi.domain;

using Fudie.Domain;
public partial class Foo(Guid id) : Entity<Guid>(id)
{
    public string Name {get;protected set;} = string.Empty;
}