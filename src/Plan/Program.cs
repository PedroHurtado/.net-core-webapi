var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});


// Add services to the container.
builder.Services.AddEndpointsApiExplorer();

// Register PlanDbContext with Firestore provider
builder.Services.AddDbContext<PlanDbContext>((sp, options) =>
{
    options.UseFirestore(sp);
    options.LogTo(Console.WriteLine, LogLevel.Information, DbContextLoggerOptions.None);
}).AddInterfacesFor<PlanDbContext>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), ServiceLifetime.Singleton);
builder.Services.AddInjectables();

var app = builder.Build();

app.UseExceptionHandler();

var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".yaml"] = "application/x-yaml";

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "OpenApi")),
    RequestPath = "/plans/openapi",
    ContentTypeProvider = provider,
    OnPrepareResponse = ctx => ctx.Context.Response.Headers.CacheControl = "no-cache, no-store"
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI(c =>
    {
        c.RoutePrefix = "plans/swagger";
        c.SwaggerEndpoint("/plans/openapi/plan-api.yaml", "Plan API");
        c.UseRequestInterceptor("(req) => { req.credentials = 'include'; return req; }");
    });

    app.MapGet("/", () => Results.Redirect("/plans/swagger")).ExcludeFromDescription();
}

app.MapFeatures();
app.MapCatalog();

app.UseHttpsRedirection();

app.Run();

public partial class Program { }
