var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Tenant ID (temporal - hardcoded for development)
var tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
builder.Services.AddScoped(typeof(Guid), _ => tenantId);

// User ID (temporal - hardcoded for development)
var userId = Guid.Parse("22222222-2222-2222-2222-222222222222");
builder.Services.AddScoped(_ => new CurrentUserId(userId));

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

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSingleton<IGoogleOAuthSettings, DevGoogleOAuthSettings>();
    builder.Services.AddSingleton<IJwtKeyProvider, DevJwtKeyProvider>();
}

var app = builder.Build();

app.UseExceptionHandler();

var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".yaml"] = "application/x-yaml";

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "OpenApi")),
    RequestPath = "/auth/openapi",
    ContentTypeProvider = provider,
    OnPrepareResponse = ctx => ctx.Context.Response.Headers.CacheControl = "no-cache, no-store"
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI(c =>
    {
        c.RoutePrefix = "auth/swagger";
        c.SwaggerEndpoint("/auth/openapi/auth-api.yaml", "Auth API");
        c.SwaggerEndpoint("/auth/openapi/session-api.yaml", "Session API");
        c.SwaggerEndpoint("/auth/openapi/tenant-roles-api.yaml", "Roles API");
        c.SwaggerEndpoint("/auth/openapi/memberships-api.yaml", "MemberShip API");
        c.SwaggerEndpoint("/auth/openapi/external-apps-api.yaml", "External Apps API");
        c.UseRequestInterceptor("(req) => { req.credentials = 'include'; return req; }");
    });

    app.MapGet("/", () => Results.Redirect("/auth/swagger")).ExcludeFromDescription();
    app.MapGet("/auth", () => Results.Redirect("/auth/swagger")).ExcludeFromDescription();

    app.MapGet("/auth/dev", () => Results.Content(DevLoginPage.Html, "text/html"))
        .AllowAnonymous()
        .ExcludeFromDescription();
}

app.MapFeatures();

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
