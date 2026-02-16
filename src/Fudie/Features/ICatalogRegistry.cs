using Microsoft.AspNetCore.Http;

namespace Fudie.Features;

public interface ICatalogRegistry
{
    void Register(string className, Endpoint endpoint);
    IReadOnlyList<CatalogEntry> GetAll();
    IReadOnlyList<CatalogEntry> GetTenant();
}
