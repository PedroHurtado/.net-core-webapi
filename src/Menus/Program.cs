using System.Reflection;
using System.Text.Json.Serialization;
using Menus.Infrastructure;
using Fudie.Firestore.EntityFrameworkCore.Infrastructure;
using Fudie.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.FileProviders;

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

// Register MenusDbContext with Firestore provider
builder.Services.AddDbContext<MenusDbContext>((sp, options) =>
{
    options.UseFirestore(sp);
    options.LogTo(Console.WriteLine, LogLevel.Information, DbContextLoggerOptions.None);
}).AddInterfacesFor<MenusDbContext>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

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
    RequestPath = "/menus/openapi",
    ContentTypeProvider = provider,
    OnPrepareResponse = ctx => ctx.Context.Response.Headers.CacheControl = "no-cache, no-store"
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI(c =>
    {
        c.RoutePrefix = "menus/swagger";
        c.SwaggerEndpoint("/menus/openapi/allergen-api.yaml", "Allergen API");
        c.SwaggerEndpoint("/menus/openapi/menuitem-api.yaml", "MenuItem API");
        c.SwaggerEndpoint("/menus/openapi/menu-api.yaml", "Menu");
        c.UseRequestInterceptor("(req) => { req.credentials = 'include'; return req; }");
    });

    app.MapGet("/", () => Results.Redirect("/menus/swagger")).AllowAnonymous();
}

app.UseFudieAuthorization();
app.MapFeatures();
app.MapCatalog();

app.UseHttpsRedirection();

app.Run();

public partial class Program { }
