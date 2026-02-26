namespace Customers.Features.Customers.Api.CustomerAggregate;

public class CustomerAggregateDescription : IAggregateDescription
{
    public string Id => "customer";
    public string DisplayName => "Customers";
    public string? Icon => "users";
    public string ReadDescription => "View customers";
    public string WriteDescription => "Manage customers";
}
