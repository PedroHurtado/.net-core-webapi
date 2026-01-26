using System.Reflection;
using System.Text.Json.Serialization;
using Customer.Infrastructure;
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

// Register CustomerDbContext with Firestore provider
builder.Services.AddDbContext<CustomerDbContext>((sp, options) =>
{
    options.UseFirestore(sp);
    options.LogTo(Console.WriteLine, LogLevel.Information, DbContextLoggerOptions.None);
}).AddInterfacesFor<CustomerDbContext>();

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
    RequestPath = "/openapi",
    ContentTypeProvider = provider
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI(c =>
    {
        c.RoutePrefix = "swagger";
        c.SwaggerEndpoint("/openapi/menuitem-api.yaml", "MenuItem API");
    });

    app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
}

app.MapFeatures();

app.UseHttpsRedirection();

app.Run();

public partial class Program { }
