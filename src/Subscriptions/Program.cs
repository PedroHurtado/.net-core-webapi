var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped(typeof(Guid), sp =>
    sp.GetRequiredService<IFudieUser>().TenantId
    ?? throw new InvalidOperationException("TenantId is not available in the current request"));

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();

// Register SubscriptionsDbContext with Firestore provider
builder.Services.AddDbContext<SubscriptionsDbContext>((sp, options) =>
{
    options.UseFirestore(sp);
    options.LogTo(Console.WriteLine, LogLevel.Information, DbContextLoggerOptions.None);
}).AddInterfacesFor<SubscriptionsDbContext>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), ServiceLifetime.Singleton);
builder.Services.AddFudieSecurity(opts =>
    builder.Configuration.GetSection(FudieSecurityOptions.SectionName).Bind(opts));

builder.Services.AddInjectables();

var app = builder.Build();

app.UseExceptionHandler();

var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".yaml"] = "application/x-yaml";

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "OpenApi")),
    RequestPath = "/subscriptions/openapi",
    ContentTypeProvider = provider,
    OnPrepareResponse = ctx => ctx.Context.Response.Headers.CacheControl = "no-cache, no-store"
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI(c =>
    {
        c.RoutePrefix = "subscriptions/swagger";
        c.SwaggerEndpoint("/subscriptions/openapi/subscription-api.yaml", "Subscription API");
        c.SwaggerEndpoint("/subscriptions/openapi/billinghistory.-api.yaml", "Billinghistory API");
        c.UseRequestInterceptor("(req) => { req.credentials = 'include'; return req; }");
    });

    app.MapGet("/", () => Results.Redirect("/subscriptions/swagger")).AllowAnonymous();
}

app.UseFudieAuthorization();
app.MapFeatures();
app.MapCatalog();

app.UseHttpsRedirection();

app.Run();

public partial class Program { }
