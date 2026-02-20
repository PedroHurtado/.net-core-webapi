using Microsoft.AspNetCore.Http;

namespace Fudie.Features;

public interface ICatalogRegistry
{
    void Register(string className, Endpoint endpoint);
    string? FindClassName(Endpoint endpoint);
    int EndpointMapCount { get; }
    IReadOnlyList<CatalogEntry> GetAll();
    IReadOnlyList<CatalogEntry> GetTenant();
}
