using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using webapi.common;
using webapi.common.dependencyinjection;
using webapi.common.openapi;
using webapi.infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds(type =>
    {
        if (type == typeof(CustomProblemDetails))
            return "CustomProblemDetails";

        if (type.DeclaringType != null)
        {
            return $"{type.DeclaringType.Name}.{type.Name}";
        }
        return type.FullName?.Replace("+", ".").Replace(".", "") ?? type.Name;
    });

    options.MapType<decimal>(() => new OpenApiSchema
    {
        Type = "number",
        Format = "decimal"
    });

    options.MapType<decimal?>(() => new OpenApiSchema
    {
        Type = "number",
        Format = "decimal",
        Nullable = true
    });

    options.MapType<ProblemDetails>(() => new OpenApiSchema
    {
        Type = "object",
        Properties = new Dictionary<string, OpenApiSchema>
        {
            ["type"] = new OpenApiSchema { Type = "string", Example = new Microsoft.OpenApi.Any.OpenApiString("about:blank") },
            ["title"] = new OpenApiSchema { Type = "string" },
            ["status"] = new OpenApiSchema { Type = "integer", Format = "int32" },
            ["detail"] = new OpenApiSchema { Type = "string" },
            ["instance"] = new OpenApiSchema { Type = "string" },
            ["extensions"] = new OpenApiSchema
            {
                Type = "object",
                AdditionalPropertiesAllowed = true,
                Example = new Microsoft.OpenApi.Any.OpenApiObject
                {
                    ["traceId"] = new Microsoft.OpenApi.Any.OpenApiString("00-abc123-def456-01"),
                    ["timestamp"] = new Microsoft.OpenApi.Any.OpenApiString("2025-10-30T10:30:00Z")
                }
            }
        },
        AdditionalPropertiesAllowed = false
    });


builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseInMemoryDatabase("PizzaDb");

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddInjectables();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapGet("/", () => Results.Redirect("/swagger/index.html")).ExcludeFromDescription();
}

app.MapFeatures();

app.UseHttpsRedirection();



app.Run();

