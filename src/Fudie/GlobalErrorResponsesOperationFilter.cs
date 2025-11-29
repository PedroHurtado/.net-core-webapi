using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Fudie.OpenApi;

namespace Fudie;

public class GlobalErrorResponsesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Agregar respuesta 400 si no existe
        if (!operation.Responses.ContainsKey("400"))
        {
            operation.Responses.Add("400", new OpenApiResponse
            {
                Description = "Bad Request",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/problem+json"] = new OpenApiMediaType
                    {
                        Schema = context.SchemaGenerator.GenerateSchema(
                            typeof(CustomProblemDetails), 
                            context.SchemaRepository)
                    }
                }
            });
        }

        if (!operation.Responses.ContainsKey("422"))
        {
            operation.Responses.Add("422", new OpenApiResponse
            {
                Description = "Unprocesed entity",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/problem+json"] = new OpenApiMediaType
                    {
                        Schema = context.SchemaGenerator.GenerateSchema(
                            typeof(CustomProblemDetails), 
                            context.SchemaRepository)
                    }
                }
            });
        }

        // Agregar respuesta 404 si no existe (excepto para POST)
        if (!operation.Responses.ContainsKey("404") && 
            context.ApiDescription.HttpMethod?.ToUpper() != "POST")
        {
            operation.Responses.Add("404", new OpenApiResponse
            {
                Description = "Not Found",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/problem+json"] = new OpenApiMediaType
                    {
                        Schema = context.SchemaGenerator.GenerateSchema(
                            typeof(CustomProblemDetails), 
                            context.SchemaRepository)
                    }
                }
            });
        }

        // Agregar respuesta 500 si no existe
        if (!operation.Responses.ContainsKey("500"))
        {
            operation.Responses.Add("500", new OpenApiResponse
            {
                Description = "Internal Server Error",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/problem+json"] = new OpenApiMediaType
                    {
                        Schema = context.SchemaGenerator.GenerateSchema(
                            typeof(CustomProblemDetails), 
                            context.SchemaRepository)
                    }
                }
            });
        }
    }
}
