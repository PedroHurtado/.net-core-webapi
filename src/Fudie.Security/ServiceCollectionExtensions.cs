namespace Fudie.Security;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFudieSecurity(
        this IServiceCollection services,
        Action<FudieSecurityOptions> configureOptions)
    {
        services.Configure(configureOptions);

        services.AddRefitClient<IJwksApi>()
            .ConfigureHttpClient((sp, client) =>
            {
                var opts = sp.GetRequiredService<IOptions<FudieSecurityOptions>>().Value;
                var baseUrl = opts.JwksUrl;

                var uri = new Uri(baseUrl);
                client.BaseAddress = new Uri($"{uri.Scheme}://{uri.Authority}");
            });

        services.AddSingleton<IJwtValidator, JwtValidator>();

        return services;
    }
}
