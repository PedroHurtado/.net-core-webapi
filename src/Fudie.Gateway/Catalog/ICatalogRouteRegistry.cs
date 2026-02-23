namespace Fudie.Gateway.Catalog;

public interface ICatalogRouteRegistry
{
    bool IsAnonymous(string method, string path);
    bool IsInternal(string method, string path);
    void Clear();
    void Update(string clusterId, CatalogResponse catalog);
    IReadOnlyList<CatalogResponse> GetAll();
}
