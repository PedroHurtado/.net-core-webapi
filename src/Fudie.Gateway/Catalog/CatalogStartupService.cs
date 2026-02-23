namespace Fudie.Gateway.Catalog;

public sealed class CatalogStartupService(
    IConfiguration configuration,
    IHttpClientFactory httpClientFactory,
    ICatalogRouteRegistry registry,
    ILogger<CatalogStartupService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var internalSecret = configuration["Fudie:InternalSecret"];
        if (string.IsNullOrEmpty(internalSecret))
        {
            logger.LogWarning("Fudie:InternalSecret not configured — catalog pull skipped");
            return;
        }

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
                client.DefaultRequestHeaders.Add("X-Internal-Key", internalSecret);

                var catalogService = RestService.For<ICatalogService>(client);
                var response = await catalogService.GetCatalog();

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogWarning(
                        "Failed to fetch catalog from {ClusterId} at {Address}: {StatusCode}",
                        clusterId, address, response.StatusCode);
                    continue;
                }

                if (response.Content is not null)
                {
                    registry.Update(clusterId, response.Content);
                    logger.LogInformation(
                        "Loaded {Count} catalog entries from {ClusterId} ({ServiceId})",
                        response.Content.Entries.Count, clusterId, response.Content.ServiceId);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "Could not fetch catalog from {ClusterId} at {Address}",
                    clusterId, address);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
