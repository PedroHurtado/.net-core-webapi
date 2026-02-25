namespace Fudie.Gateway.Catalog;

public sealed class CatalogRouteRegistry : ICatalogRouteRegistry
{
    private readonly object _lock = new();
    private readonly Dictionary<string, CatalogResponse> _catalogsByCluster = [];
    private volatile ImmutableList<RouteRule> _anonymousRules = [];
    private volatile ImmutableList<RouteRule> _internalRules = [];
    private volatile ImmutableList<RouteRule> _allRules = [];

    public bool IsAnonymous(string method, string path)
        => _anonymousRules.Any(r => r.Matches(method, path));

    public bool IsInternal(string method, string path)
        => _internalRules.Any(r => r.Matches(method, path));

    public bool IsKnownRoute(string method, string path)
        => _allRules.Any(r => r.Matches(method, path));

    public void Clear()
    {
        lock (_lock)
        {
            _catalogsByCluster.Clear();
            _anonymousRules = [];
            _internalRules = [];
            _allRules = [];
        }
    }

    public void Update(string clusterId, CatalogResponse catalog)
    {
        lock (_lock)
        {
            _catalogsByCluster[clusterId] = catalog;
            RebuildRules();
        }
    }

    public IReadOnlyList<CatalogResponse> GetAll()
        => [.. _catalogsByCluster.Values];

    private void RebuildRules()
    {
        var anonymous = new List<RouteRule>();
        var @internal = new List<RouteRule>();
        var all = new List<RouteRule>();

        foreach (var catalog in _catalogsByCluster.Values)
        {
            foreach (var entry in catalog.Entries)
            {
                all.Add(new RouteRule(entry.HttpVerb, entry.RoutePattern));

                if (entry.IsAnonymous)
                    anonymous.Add(new RouteRule(entry.HttpVerb, entry.RoutePattern));

                if (entry.IsInternal)
                    @internal.Add(new RouteRule(entry.HttpVerb, entry.RoutePattern));
            }
        }

        _anonymousRules = [.. anonymous];
        _internalRules = [.. @internal];
        _allRules = [.. all];
    }

    private sealed class RouteRule
    {
        private readonly string _method;
        private readonly Regex _pathPattern;

        public RouteRule(string method, string routePattern)
        {
            _method = method.ToUpperInvariant();
            _pathPattern = ConvertToRegex(routePattern);
        }

        public bool Matches(string method, string path)
            => string.Equals(_method, method, StringComparison.OrdinalIgnoreCase)
               && _pathPattern.IsMatch(path);

        private static Regex ConvertToRegex(string routePattern)
        {
            var segments = Regex.Split(routePattern, @"(\{[^}]+\})");

            var pattern = string.Concat(segments.Select(s =>
            {
                if (s.StartsWith('{') && s.EndsWith('}'))
                {
                    var param = s[1..^1];
                    return param.StartsWith("**") ? ".+" : "[^/]+";
                }
                return Regex.Escape(s);
            }));

            return new Regex($"^{pattern}$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }
    }
}
