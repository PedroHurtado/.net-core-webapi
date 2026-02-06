var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Tenant ID (temporal - hardcoded for development)
var tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
builder.Services.AddScoped(typeof(Guid), _ => tenantId);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();

// Register CustomersDbContext with Firestore provider
builder.Services.AddDbContext<CustomersDbContext>((sp, options) =>
{
    options.UseFirestore(sp);
    options.LogTo(Console.WriteLine, LogLevel.Information, DbContextLoggerOptions.None);
}).AddInterfacesFor<CustomersDbContext>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddSingleton(TimeProvider.System);

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
    RequestPath = "/openapi",
    ContentTypeProvider = provider
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI(c =>
    {
        c.RoutePrefix = "swagger";
        c.SwaggerEndpoint("/openapi/customer-api.yaml", "Customer API");
    });

    app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
}

app.MapFeatures();

app.UseHttpsRedirection();

app.Run();

public partial class Program { }