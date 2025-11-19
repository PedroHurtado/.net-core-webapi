using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using webapi.common;
using webapi.common.dependencyinjection;
using webapi.common.infrastructure;
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
}); 

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseInMemoryDatabase("PizzaDb");

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
}).AddInterfacesFor<ApplicationDbContext>();


builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddInjectables();

var app = builder.Build();

app.UseExceptionHandler();

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

public partial class Program { }