# Fudie Gateway — Auth Middleware

## Objetivo

Crear un middleware en `Fudie.Gateway` que intercepte todas las peticiones antes de YARP y resuelva la autenticación contra el servicio Auth (IDP).

---

## Flujos de autenticación

### 1. Cookie de sesión

- El cliente envía una cookie (nombre configurable, ej: `fudie_session`)
- El middleware llama a `POST /auth/resolve` reenviando la cookie
- El servicio Auth valida la sesión, la refresca y devuelve un JWT efímero en el header `X-Auth-Token`
- El middleware inyecta el JWT como `Authorization: Bearer {token}` en la request downstream
- La cookie actualizada (`Set-Cookie`) se devuelve al cliente, NO se reenvía al backend

### 2. API Key

- El cliente envía `Authorization: Bearer XXXXX` (API key)
- El middleware llama a `POST /auth/resolve-api-key` con el valor en el header `X-Api-Key`
- El servicio Auth valida la API key y devuelve un JWT efímero en `X-Auth-Token`
- El middleware inyecta el JWT como `Authorization: Bearer {token}` en la request downstream

### 3. AllowAnonymous

- Al arrancar, la gateway consulta `/catalog` de cada microservicio registrado en YARP
- Se construye un mapa de rutas anónimas
- Si la ruta está en el catálogo como anónima, pasa directo sin tocar Auth

### 4. Sin credenciales

- Si no hay cookie ni API key y la ruta no es anónima → `401 Unauthorized`

---

## Configuración

Todo configurable desde `appsettings.json`, nada hardcodeado:

- Nombre de la cookie de sesión
- URL de `/auth/resolve`
- URL de `/auth/resolve-api-key`
---

## Catálogo de endpoints anónimos — Pub/Sub

### Flujo

1. Cada microservicio al arrancar publica su catálogo en un topic de Pub/Sub (ej: `catalog-updates`)
2. La gateway es suscriptora del topic y recibe los catálogos en tiempo real
3. Al recibir un mensaje, actualiza el mapa de rutas anónimas con swap atómico (`Interlocked.Exchange`)
4. Cero polling, cero timer, cero endpoint manual

### Infraestructura

- **Desarrollo**: Pub/Sub emulator en Docker (`dipjyotimetia/pubsub-emulator`)
- **Producción**: Google Cloud Pub/Sub

### Ventajas

- Cada servicio es dueño de su catálogo y lo anuncia al arrancar
- La gateway no necesita conocer las URLs de los servicios para el catálogo
- Si un servicio se reinicia o se despliega una versión nueva, publica automáticamente → la gateway se actualiza sin intervención
- Escala sin problemas: N instancias de gateway reciben todas el mensaje

---

## Decisiones técnicas

### IHttpClientFactory en el middleware

Los middlewares en ASP.NET Core son singletons. Para evitar problemas de DNS stale, `IHttpClientFactory` se inyecta en el método `InvokeAsync` (method injection), no en el constructor:

```csharp
public async Task InvokeAsync(HttpContext context, IHttpClientFactory httpClientFactory)
{
    using var client = httpClientFactory.CreateClient();
    // ...
}
```

En el constructor solo queda lo que es singleton: `RequestDelegate` e `IConfiguration`.

### Resiliencia en llamadas a Auth

Si `/auth/resolve` o `/auth/resolve-api-key` falla (timeout, 500, servicio caído) → `502 Bad Gateway`. Sin retry automático; el cliente puede reintentar. Circuit breaker se añade cuando haya necesidad real, no complejidad prematura.

### Concurrencia del catálogo anónimo

`Interlocked.Exchange` con un `ImmutableHashSet<string>` nuevo en cada refresh. Las requests en curso siguen leyendo el set viejo, el swap es atómico. Cero bloqueo, cero locks.

### Matching de rutas anónimas

`RouteExtension.MapFeatures()` registra cada endpoint en `ICatalogRegistry` con el class name y el objeto `Endpoint` de ASP.NET Core. El `Endpoint` tiene metadata, incluyendo `IAllowAnonymousMetadata` cuando el endpoint tiene `.AllowAnonymous()`. El catálogo ya tiene toda la información para exponer qué rutas son anónimas: route pattern + HTTP verb extraídos de la metadata del endpoint.

### Propagación de headers al cliente

De la respuesta de Auth solo se extraen `X-Auth-Token` y `Set-Cookie`. El resto se descarta. No se reenvía la respuesta completa de Auth al cliente.

---

## Archivos de referencia

- `src/Fudie.Gateway/Program.cs` — Gateway actual con YARP, rate limiting y CORS
- `src/Auth/Features/Sessions/Api/Commands/ResolveAuth.cs` — Endpoint cookie → JWT
- `src/Auth/Features/Sessions/Api/Commands/ResolveApiKey.cs` — Endpoint API key → JWT
- `src/Fudie/Features/CatalogEndpointExtensions.cs` — Catálogo de endpoints por servicio
- `src/Fudie/Features/RouteExtension.cs` — Registro de endpoints y metadata en ICatalogRegistry
- `docker-compose.yml` — La gateway depende de todos los servicios
