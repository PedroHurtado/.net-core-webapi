TypeDescriptor.AddAttributes(typeof(DateOnly), new TypeConverterAttribute(typeof(DateOnlyTypeConverter)));

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.Converters.Add(new TimeOnlyJsonConverter());
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped(typeof(Guid), sp =>
    sp.GetRequiredService<IFudieUser>().TenantId
    ?? throw new InvalidOperationException("TenantId is not available in the current request"));

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();

// Register SchedulersDbContext with Firestore provider
builder.Services.AddDbContext<SchedulersDbContext>((sp, options) =>
{
    options.UseFirestore(sp);
    options.LogTo(Console.WriteLine, LogLevel.Information, DbContextLoggerOptions.None);
}).AddInterfacesFor<SchedulersDbContext>();

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
    RequestPath = "/schedules/openapi",
    ContentTypeProvider = provider,
    OnPrepareResponse = ctx => ctx.Context.Response.Headers.CacheControl = "no-cache, no-store"
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI(c =>
    {
        c.RoutePrefix = "schedules/swagger";
        c.SwaggerEndpoint("/schedules/openapi/schedule-api.yaml", "Schedule API");
        c.SwaggerEndpoint("/schedules/openapi/service-schedule-api.yaml", "Service Schedule API");
        c.UseRequestInterceptor("(req) => { req.credentials = 'include'; return req; }");
    });

    app.MapGet("/", () => Results.Redirect("/schedules/swagger")).AllowAnonymous();
}

app.UseFudieAuthorization();
app.MapFeatures();
app.MapCatalog();

app.UseHttpsRedirection();

app.Run();

public partial class Program { }
