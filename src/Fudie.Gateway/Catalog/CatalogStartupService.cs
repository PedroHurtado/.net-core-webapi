namespace Fudie.Gateway.Catalog;

public sealed class CatalogStartupService(
    IConfiguration configuration,
    IHttpClientFactory httpClientFactory,
    ICatalogRouteRegistry registry,
    IHostEnvironment environment,
    ILogger<CatalogStartupService> logger) : IHostedService
{
    private IDisposable? _changeTokenRegistration;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await LoadCatalogsWithRetry(cancellationToken);

        if (environment.IsDevelopment())
        {
            _changeTokenRegistration = ChangeToken.OnChange(
                configuration.GetReloadToken,
                () => _ = ReloadCatalogs());
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _changeTokenRegistration?.Dispose();
        return Task.CompletedTask;
    }

    private async Task LoadCatalogsWithRetry(CancellationToken cancellationToken)
    {
        const int maxRetries = 5;
        var delay = TimeSpan.FromSeconds(3);

        for (var attempt = 0; attempt <= maxRetries; attempt++)
        {
            var failed = await LoadCatalogs();
            if (failed == 0) return;

            if (attempt < maxRetries)
            {
                logger.LogInformation(
                    "Retrying {Count} failed clusters in {Delay}s (attempt {Attempt}/{Max})",
                    failed, delay.TotalSeconds, attempt + 1, maxRetries);
                await Task.Delay(delay, cancellationToken);
            }
        }
    }

    private async Task ReloadCatalogs()
    {
        try
        {
            logger.LogInformation("Configuration changed — reloading catalogs");
            registry.Clear();
            await LoadCatalogs();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error reloading catalogs after configuration change");
        }
    }

    private async Task<int> LoadCatalogs()
    {
        var internalSecret = configuration["Fudie:InternalSecret"];
        if (string.IsNullOrEmpty(internalSecret))
        {
            logger.LogWarning("Fudie:InternalSecret not configured — catalog pull skipped");
            return 0;
        }

        var clusters = configuration.GetSection("ReverseProxy:Clusters").GetChildren();
        var failed = 0;

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
                    failed++;
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
                failed++;
            }
        }

        return failed;
    }
}
