# Gateway — Evolución del Catálogo

## Estado actual

La gateway ya tiene implementado:
- `AuthMiddleware` con flujos cookie, API key, anonymous y 401
- `AnonymousRouteRegistry` con `ImmutableHashSet` + swap atómico
- `CatalogStartupService` que pull `/catalog/anonymous` de cada cluster al arrancar
- `IAuthService` con Refit para `ResolveAuth` y `ResolveApiKey`
- Config en `appsettings.json` sección `Gateway:Auth` con `CookieName`, `Idp`, `AuthTokenHeader`, `ApiKeyHeader`

## Cambios pendientes

### 1. Catálogo completo en vez de solo rutas anónimas

El registry debe almacenar el catálogo COMPLETO de cada servicio, no solo las rutas anónimas. Cada entrada necesita los flags: `IsAnonymous`, `IsInternal`, `IsExcluded`, `IsPlatform`, etc.

**Motivo:** el middleware necesita distinguir tres tipos de ruta:
- **IsInternal** → `403 Forbidden` siempre (servicio-a-servicio, nunca por gateway)
- **IsAnonymous** → pasa sin auth
- **Normal** → flujo cookie/API key

### 2. Rutas internas que la gateway debe bloquear (403)

| Ruta | Servicio | Motivo |
|------|----------|--------|
| `POST /subscriptions` | subscriptions-service | Lo llama un webhook de Stripe, no un cliente |
| `POST /customer` | customer-service | Lo llama el servicio de subscriptions internamente |
| `GET /catalog` | todos los servicios | Solo lo consume la gateway en startup |

Estas rutas están marcadas con `RequireInternal()` en los servicios y autenticadas con `Fudie:InternalSecret`.

### 3. La gateway expone `/catalog` unificado para frontend

En vez de que el frontend llame a N servicios para obtener el catálogo, la gateway lo sirve directamente desde memoria:

- `GET /catalog` en la gateway (fuera del pipeline YARP, como el DevPortal)
- Cero latencia extra — datos ya en memoria
- Frontend hace UNA llamada, obtiene catálogo agregado de todos los servicios
- Esto permite al frontend configurar roles sin conocer los servicios individuales

**Consecuencia:** las rutas `*-catalog-route` de YARP (`auth-catalog-route`, `menus-catalog-route`, etc.) se pueden eliminar del `appsettings.json`. El catálogo ya no se consulta servicio por servicio a través de YARP.

### 4. Modelo de datos del catálogo

El `CatalogEntry` actual en `Fudie.Features` tiene:
```
CatalogEntry(DisplayName, ClassName, HttpVerb, IsPlatform, IsInternal, IsExcluded, CustomGroup)
```

Donde `IsExcluded = true` cuando el endpoint tiene `AllowAnonymousAttribute` o `ExcludeFromDescriptionAttribute`.

Para la gateway necesitamos que el `/catalog` de cada servicio devuelva esta información completa, incluyendo el route pattern (ej: `/auth/resolve`) y el HTTP method — datos que el `CatalogRegistry` del servicio ya tiene pero el endpoint actual no expone.

**Formato esperado del nuevo endpoint de catálogo en cada servicio:**
```json
[
  {
    "className": "ResolveAuth",
    "displayName": "ResolveAuth",
    "httpVerb": "POST",
    "routePattern": "/auth/resolve",
    "isPlatform": false,
    "isInternal": false,
    "isAnonymous": true,
    "customGroup": null
  }
]
```

### 5. Archivos a modificar en la gateway

- `Catalog/AnonymousRoute.cs` → renombrar/reemplazar por un DTO más completo con todos los flags
- `Catalog/IAnonymousRouteRegistry.cs` → renombrar a `ICatalogRegistry` (o similar) con métodos `IsAnonymous`, `IsInternal`, `GetAll`, `GetTenant`
- `Catalog/AnonymousRouteRegistry.cs` → implementación con catálogo completo + sets derivados para lookups rápidos
- `Catalog/ICatalogService.cs` → cambiar response type al DTO completo
- `Catalog/CatalogStartupService.cs` → adaptar al nuevo response
- `Auth/AuthMiddleware.cs` → añadir check de `IsInternal` → 403 antes de anonymous/cookie/apikey
- `Program.cs` → añadir `app.MapGet("/catalog", ...)` fuera del pipeline YARP
- `appsettings.json` → eliminar las rutas `*-catalog-route` de YARP

### 6. Flujo completo del middleware (orden actualizado)

```
Request entrante (pipeline YARP)
  │
  ├─ IsInternal? → 403 Forbidden
  ├─ IsAnonymous? → pasa sin auth → next()
  ├─ Tiene cookie? → POST /auth/resolve → JWT downstream → next()
  ├─ Tiene Bearer (API key)? → POST /auth/resolve-api-key → JWT downstream → next()
  └─ Sin credenciales → 401 Unauthorized
```

### 7. Servicio-side (fuera de la gateway)

Cada microservicio necesita:
- Modificar o crear un endpoint que devuelva el catálogo completo con route patterns y flags
- El endpoint actual `GET /catalog` devuelve `Dictionary<string, List<string>>` (grupos de permisos), que es lo que frontend necesita para roles
- Posiblemente se necesite un endpoint diferente para el consumo de la gateway (con route patterns) vs el consumo de frontend (con grupos de permisos), o unificar ambos formatos

### 8. Pub/Sub para actualizaciones en runtime

- Al arrancar: la gateway hace pull de `/catalog` de cada cluster (ya implementado en `CatalogStartupService`)
- En runtime: cuando un servicio se reinicia o despliega nueva versión, publica su catálogo por Pub/Sub
- La gateway es suscriptora y actualiza el registry con swap atómico
- Infraestructura Pub/Sub ya existe (`Fudie.PubSub` / `Fudie.PubSub.Gcp`)
