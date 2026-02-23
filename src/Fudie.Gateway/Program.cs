using Microsoft.AspNetCore.RateLimiting;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Auth middleware
builder.Services.Configure<AuthOptions>(
    builder.Configuration.GetSection("Gateway:Auth"));

var idp = builder.Configuration["Gateway:Auth:Idp"] ?? "auth-cluster";
var idpAddress = builder.Configuration[$"ReverseProxy:Clusters:{idp}:Destinations:destination1:Address"]
    ?? throw new InvalidOperationException($"IDP cluster '{idp}' not found in YARP configuration");

builder.Services
    .AddRefitClient<IAuthService>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(idpAddress));

// Anonymous route catalog
builder.Services.AddSingleton<IAnonymousRouteRegistry, AnonymousRouteRegistry>();
builder.Services.AddHostedService<CatalogStartupService>();

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("default", opt =>
    {
        opt.Window = TimeSpan.FromSeconds(10);
        opt.PermitLimit = 100;
    });
});

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.SetIsOriginAllowed(origin =>
                new Uri(origin).Host == "localhost")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        });
    });
}

var app = builder.Build();

app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.UseCors();
    var portalPath = Path.Combine(app.Environment.ContentRootPath, "DevPortal.html");
    app.MapGet("/", () => Results.Content(File.ReadAllText(portalPath), "text/html"));
}

app.MapReverseProxy(proxyPipeline =>
{
    proxyPipeline.UseMiddleware<AuthMiddleware>();
});

app.Run();
