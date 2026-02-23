namespace Fudie.Gateway.Catalog;

public sealed class CatalogStartupService(
    IConfiguration configuration,
    IHttpClientFactory httpClientFactory,
    IAnonymousRouteRegistry registry,
    ILogger<CatalogStartupService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var clusters = configuration.GetSection("ReverseProxy:Clusters").GetChildren();

        foreach (var cluster in clusters)
        {
            var clusterId = cluster.Key;
            var address = cluster["Destinations:destination1:Address"];
            if (address is null) continue;

            try
            {
                var client = httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(address);
                var catalogService = RestService.For<ICatalogService>(client);

                var response = await catalogService.GetAnonymousRoutes();

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogWarning(
                        "Failed to fetch anonymous routes from {ClusterId} at {Address}: {StatusCode}",
                        clusterId, address, response.StatusCode);
                    continue;
                }

                if (response.Content is { Count: > 0 })
                {
                    registry.Update(clusterId, response.Content);
                    logger.LogInformation(
                        "Loaded {Count} anonymous routes from {ClusterId}",
                        response.Content.Count, clusterId);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "Could not fetch anonymous routes from {ClusterId} at {Address}",
                    clusterId, address);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
