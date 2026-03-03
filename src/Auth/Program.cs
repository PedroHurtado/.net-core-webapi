var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped(typeof(Guid), sp =>
    sp.GetRequiredService<IFudieUser>().TenantId ?? Guid.Empty);
builder.Services.AddScoped(sp =>
    new CurrentUserId(sp.GetRequiredService<IFudieUser>().UserId ?? Guid.Empty));

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();

// Register AuthDbContext with Firestore provider
builder.Services.AddDbContext<AuthDbContext>((sp, options) =>
{
    options.UseFirestore(sp);
    options.LogTo(Console.WriteLine, LogLevel.Information, DbContextLoggerOptions.None);
}).AddInterfacesFor<AuthDbContext>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), ServiceLifetime.Singleton);
builder.Services.AddFudieLocalJwtValidation();

builder.Services.AddInjectables();

builder.Services
    .AddRefitClient<IGoogleOAuthApi>(new RefitSettings
    {
        ContentSerializer = new SystemTextJsonContentSerializer(new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        })
    })
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://oauth2.googleapis.com"));

builder.Services
    .AddRefitClient<IGoogleCertsApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://www.googleapis.com"));

builder.Services
    .AddRefitClient<ICustomerApi>()
    .ConfigureHttpClient((sp, c) =>
    {
        var config = sp.GetRequiredService<IConfiguration>();
        c.BaseAddress = new Uri(config["Fudie:Services:Customers"]!);
    })
    .AddHttpMessageHandler<InternalAuthHandler>();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSingleton<IGoogleOAuthSettings, DevGoogleOAuthSettings>();
}

var app = builder.Build();

app.UseExceptionHandler();

app.UseFudieOpenApi();

if (app.Environment.IsDevelopment())
{
    app.MapGet("/auth/dev", () => Results.Content(DevLoginPage.Html, "text/html"))
        .AllowAnonymous();
}

app.MapFeatures(builder => builder.UseFudieAuthorization());

app.UseHttpsRedirection();

app.Run();

public partial class Program { }

public static class DevLoginPage
{
    public const string Html = """
        <!DOCTYPE html>
        <html>
        <body>
            <form method="POST" action="/auth/login/google">
                <button type="submit">Entrar con Google</button>
            </form>
        </body>
        </html>
        """;
}
