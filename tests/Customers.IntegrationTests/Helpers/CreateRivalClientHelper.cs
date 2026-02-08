namespace Customers.IntegrationTests.Helpers;

public static class CreateRivalClientHelper
{
    public static HttpClient CreateRivalClient(this WebApplicationFactory<Program> factory)
    {
        var rivalTenantId = Guid.NewGuid();

        var rivalFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(typeof(Guid), _ => rivalTenantId);
            });
        });

        return rivalFactory.CreateClient();
    }
}
