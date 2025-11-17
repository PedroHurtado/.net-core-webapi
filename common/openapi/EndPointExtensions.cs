using webapi.common.openapi;

public static class EndpointExtensions
{
    // Versión CON tipo de respuesta
    public static RouteHandlerBuilder WithStandardOpenApi<TResponse>(
        this RouteHandlerBuilder builder,
        string name,
        string summary,
        string description,
        string tag,
        int successStatusCode = StatusCodes.Status200OK,
        params int[] additionalErrorCodes)
    {
        builder = builder
            .WithOpenApi()
            .WithName(name)
            .WithSummary(summary)
            .WithDescription(description)
            .WithTags(tag)
            .Produces<TResponse>(successStatusCode)
            .Produces<CustomProblemDetails>(StatusCodes.Status400BadRequest);

        foreach (var errorCode in additionalErrorCodes)
        {
            builder = builder.Produces<CustomProblemDetails>(errorCode);
        }

        return builder;
    }

    // Versión SIN tipo de respuesta (para NoContent, Accepted sin body, etc.)
    public static RouteHandlerBuilder WithStandardOpenApi(
        this RouteHandlerBuilder builder,
        string name,
        string summary,
        string description,
        string tag,
        int successStatusCode,
        params int[] additionalErrorCodes)
    {
        builder = builder
            .WithOpenApi()
            .WithName(name)
            .WithSummary(summary)
            .WithDescription(description)
            .WithTags(tag)
            .Produces(successStatusCode) // Sin tipo genérico
            .Produces<CustomProblemDetails>(StatusCodes.Status400BadRequest);

        foreach (var errorCode in additionalErrorCodes)
        {
            builder = builder.Produces<CustomProblemDetails>(errorCode);
        }

        return builder;
    }
}