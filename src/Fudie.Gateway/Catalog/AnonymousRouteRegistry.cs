namespace Fudie.Gateway.Catalog;

public sealed class AnonymousRouteRegistry : IAnonymousRouteRegistry
{
    private readonly object _lock = new();
    private readonly Dictionary<string, ImmutableHashSet<string>> _routesByCluster = [];
    private volatile ImmutableHashSet<string> _allRoutes = [];

    public bool IsAnonymous(string method, string path)
        => _allRoutes.Contains(BuildKey(method, path));

    public void Update(string clusterId, IReadOnlyList<AnonymousRoute> routes)
    {
        lock (_lock)
        {
            _routesByCluster[clusterId] = routes
                .Select(r => BuildKey(r.Method, r.Path))
                .ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);

            _allRoutes = _routesByCluster.Values
                .SelectMany(s => s)
                .ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);
        }
    }

    private static string BuildKey(string method, string path)
        => $"{method.ToUpperInvariant()}:{path.ToLowerInvariant()}";
}
