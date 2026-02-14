# Downstream Error Passthrough - Especificación para Claude Code

## Contexto

En la arquitectura de microservicios de Fudie, los servicios se comunican de forma síncrona usando **Refit**. Cuando un servicio downstream (B) devuelve un error con `ProblemDetails`, el servicio caller (A) debe reenviar esa respuesta **tal cual** al cliente, sin transformación ni lógica adicional.

Esto permite que los handlers sean **100% happy path**: si el servicio downstream falla, el flujo se interrumpe y el `ProblemDetails` original se devuelve directamente.

## Proyecto destino

Este código es **transversal** y va en el framework **Fudie**, en el namespace `Fudie.Http`. Debe convivir con el `GlobalExceptionHandler` existente sin interferir.

---

## Componentes a implementar

### 1. `DownstreamErrorPassthroughException`

**Namespace:** `Fudie.Http`

Excepción señal (no representa un error del servicio A). Transporta el status code, body y content-type de la respuesta del servicio downstream.

```csharp
public sealed class DownstreamErrorPassthroughException : Exception
{
    public int StatusCode { get; }
    public byte[] Body { get; }
    public string ContentType { get; }

    public DownstreamErrorPassthroughException(int statusCode, byte[] body, string contentType)
    {
        StatusCode = statusCode;
        Body = body;
        ContentType = contentType;
    }
}
```

### 2. `DownstreamPassthroughHandler` (DelegatingHandler)

**Namespace:** `Fudie.Http`

Se registra como `Transient` (obligatorio para `IHttpClientFactory`).

Intercepta las respuestas HTTP del servicio downstream. Si el status code **no es 2xx**, lee el body y lanza `DownstreamErrorPassthroughException` con los datos originales.

```csharp
public class DownstreamPassthroughHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
            return response;

        var body = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var contentType = response.Content.Headers.ContentType?.ToString()
                          ?? "application/problem+json";

        throw new DownstreamErrorPassthroughException(
            (int)response.StatusCode, body, contentType);
    }
}
```

### 3. `DownstreamPassthroughMiddleware`

**Namespace:** `Fudie.Http`

Middleware ASP.NET Core que captura `DownstreamErrorPassthroughException` y escribe la respuesta directamente en el `HttpContext`. Sin logging de error (el log ya se escribió en el servicio origen).

```csharp
public class DownstreamPassthroughMiddleware
{
    private readonly RequestDelegate _next;

    public DownstreamPassthroughMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DownstreamErrorPassthroughException ex)
        {
            context.Response.StatusCode = ex.StatusCode;
            context.Response.ContentType = ex.ContentType;
            await context.Response.Body.WriteAsync(ex.Body);
        }
    }
}
```

### 4. Registro en DI y Pipeline

**En el registro de servicios (DI):**

```csharp
services.AddTransient<DownstreamPassthroughHandler>();
```

**En cada cliente Refit que apunte a un servicio interno:**

```csharp
services.AddRefitClient<IRestaurantServiceClient>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(config["Services:Restaurant"]))
    .AddHttpMessageHandler<DownstreamPassthroughHandler>();
```

**En el pipeline de middleware (Program.cs):**

```csharp
// IMPORTANTE: DownstreamPassthroughMiddleware ANTES de UseExceptionHandler
app.UseMiddleware<DownstreamPassthroughMiddleware>();
app.UseExceptionHandler();  // Usa el GlobalExceptionHandler existente
```

**Orden crítico:** El `DownstreamPassthroughMiddleware` debe registrarse **antes** que `UseExceptionHandler()` para que intercepte `DownstreamErrorPassthroughException` antes de que el `GlobalExceptionHandler` la trate como un error genérico 500.

---

## Tests unitarios a implementar

### Tests para `DownstreamPassthroughHandler`

1. **Devuelve la respuesta si el status code es 2xx** (200, 201, 204)
2. **Lanza `DownstreamErrorPassthroughException` si el status code es 404**
3. **Lanza `DownstreamErrorPassthroughException` si el status code es 409**
4. **Lanza `DownstreamErrorPassthroughException` si el status code es 422**
5. **Lanza `DownstreamErrorPassthroughException` si el status code es 401**
6. **Lanza `DownstreamErrorPassthroughException` si el status code es 503**
7. **La excepción contiene el body original de la respuesta**
8. **La excepción contiene el content-type original**
9. **Si no hay content-type, usa "application/problem+json" por defecto**

### Tests para `DownstreamPassthroughMiddleware`

1. **Si no hay excepción, el request pasa al siguiente middleware normalmente**
2. **Si se lanza `DownstreamErrorPassthroughException`, escribe el status code correcto**
3. **Si se lanza `DownstreamErrorPassthroughException`, escribe el content-type correcto**
4. **Si se lanza `DownstreamErrorPassthroughException`, escribe el body original completo**
5. **Si se lanza otra excepción distinta, la propaga (no la captura)**

### Frameworks de test

Usar **xUnit** + **FluentAssertions** (o los que ya estéis usando en Fudie). Para el handler usar un `DelegatingHandler` fake como inner handler. Para el middleware usar `DefaultHttpContext`.

---

## Flujo completo

```
Cliente → Servicio A (handler happy path) → Refit → DownstreamPassthroughHandler → Servicio B
                                                              ↓
                                              B devuelve 409 + ProblemDetails
                                                              ↓
                                         Handler lanza DownstreamErrorPassthroughException
                                                              ↓
                                         DownstreamPassthroughMiddleware lo captura
                                                              ↓
                                         Escribe el ProblemDetails original en la respuesta
                                                              ↓
                                         Cliente recibe exactamente lo que devolvió B
```

## Notas importantes

- **Sin logging en el passthrough**: el error ya fue logueado en el servicio origen.
- **El `GlobalExceptionHandler` existente NO se modifica**: sigue gestionando errores locales (validación, not found propios, etc.).
- **`DownstreamPassthroughHandler` debe ser Transient**: obligatorio para `IHttpClientFactory`.
- **No usar `ExceptionFactory` de Refit**: no hace falta desactivar las excepciones de Refit porque el `DelegatingHandler` actúa antes de que Refit procese la respuesta.
