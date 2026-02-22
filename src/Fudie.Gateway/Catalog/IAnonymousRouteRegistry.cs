namespace Fudie.Gateway.Catalog;

public interface IAnonymousRouteRegistry
{
    bool IsAnonymous(string method, string path);
    void Update(string clusterId, IReadOnlyList<AnonymousRoute> routes);
}
