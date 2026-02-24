using Microsoft.AspNetCore.Http;

namespace Fudie.Features;

public interface ICatalogRegistry
{
    void Register(string className, Endpoint endpoint, IAggregateDescription aggregate);
    Endpoint? FindEndpoint(string displayName);
    string? FindClassName(Endpoint endpoint);
    int EndpointMapCount { get; }
    IReadOnlyList<CatalogEntry> GetAll();
    IReadOnlyList<CatalogEntry> GetTenant();
}
