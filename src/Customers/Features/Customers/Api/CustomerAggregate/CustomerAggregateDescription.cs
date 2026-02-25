namespace Customers.Features.Customers.Api.CustomerAggregate;

public class CustomerAggregateDescription : IAggregateDescription
{
    public string Id => "customer";
    public string DisplayName => "Clientes";
    public string? Icon => "users";
}
