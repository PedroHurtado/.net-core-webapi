# Catálogo y Autorización — Diseño

## JWT Claims

| Claim | Significado |
|-------|------------|
| `sub` | Usuario autenticado |
| `tid` | Tenant del usuario |

---

## Autenticación interna (servicio-a-servicio)

Las llamadas internas **no usan un claim especial en el JWT**. Se autentican con:

- `X-Internal-Key: {Fudie:InternalSecret}` → shared secret, autentica que es un servicio autorizado
- `Authorization: Bearer {JWT}` → **opcional**, si hay usuario en el contexto. Solo para trazabilidad, no para autorización

`.RequireInternal()` es un nivel **aislado**. No se combina con ninguna otra extensión. El micro valida `X-Internal-Key` y punto.

---

## Extension Methods — Niveles de autenticación

| Extensión | Requiere | Ejemplo |
|-----------|----------|---------|
| `.AllowAnonymous()` | Nada | `POST /auth/login` |
| `.RequireAuthenticated()` | JWT con `sub` | `POST /subscription/checkout` |
| Sin extensión (defecto) | JWT con `sub` + `tid` | `GET /menus`, `POST /reservations` |
| `.RequirePlatform()` | JWT con `sub` + `tid` == PlatformTenantId | Escritura en catálogo maestro |
| `.RequireInternal()` | `X-Internal-Key` | `POST /customers`, `POST /auth/onboard` |

### Combinaciones válidas

`.RequireInternal()` **nunca se combina** con nada. Es internal y punto.

`.RequireGroup(string group)` no es un nivel de autenticación. Es una capa de permisos que se aplica sobre endpoints que ya tienen `sub` + `tid`:

- Sin extensión + `.RequireGroup("menus:write")` → `sub` + `tid` + check de grupo
- `.RequirePlatform()` + `.RequireGroup("catalog:write")` → `sub` + `tid` == PlatformTenantId + check de grupo

Los administradores de cada tenant configuran qué roles tienen acceso a qué grupos.

---

## Responsabilidades

### Gateway (simple, tres reglas)

1. `IsInternal` → **404 Not Found** (no dar pistas de que existe)
2. `IsAnonymous` → pasa sin auth
3. Todo lo demás → resuelve cookie/API key → JWT downstream

La gateway NO valida claims, NO sabe qué necesita cada endpoint. Solo resuelve la autenticación y bloquea lo que no debe entrar.

**Desarrollo**: En `ASPNETCORE_ENVIRONMENT=Development` la gateway **no bloquea** los endpoints internos. Se dejan pasar para poder probarlos desde Swagger/OpenAPI con el header `X-Internal-Key`.

### Cada microservicio (`FudieAuthorizationMiddleware`)

1. Valida JWT (firma con JWKS + expiración)
2. Lee la metadata del endpoint (los `Requirement` de los extension methods)
3. Enforce según los claims del JWT

Cada servicio es dueño de su autorización. Si la request llegó al micro, el micro decide si pasa o no.

---

## Catálogo

### Problema actual

El `CatalogEntry` actual expone classNames de C# y grupos tipo `serviceId:read`. Esto es inútil para un admin de restaurante que necesita configurar permisos de su equipo.

```
customers:read → ["GetCustomers", "GetCustomerById"]
menus:write → ["CreateMenu", "UpdateMenu", "DeleteMenu"]
```

Un restaurante necesita ver:

```
Clientes → Ver clientes, Editar clientes
Menús → Crear menú, Editar menú, Eliminar menú
```

### Configuración del servicio

Cada microservicio declara en `appsettings.json`:

```json
"Fudie": {
    "ServiceId": "menus",
    "ServiceName": "Menu management"
}
```

- `ServiceId` → identificador técnico (key de i18n, agrupaciones)
- `ServiceName` → nombre legible en inglés (fallback de traducción)

### Descriptions en los endpoints

Cada endpoint declara su description en inglés usando `.WithDescriptionCatalog()` — extension method propio de Fudie, no depende de APIs de Microsoft:

```csharp
app.MapPost("/menus", Handler)
    .WithDescriptionCatalog("Create menu");
```

El dev solo se preocupa de poner la description en inglés al crear el endpoint. No hay archivos separados que mantener.

### i18n — Estrategia de traducciones

El backend **no traduce**. Sirve todo en inglés como fallback. La traducción es responsabilidad del frontend.

**Flujo:**

1. El catálogo de cada micro devuelve `ServiceId`, `ServiceName` y `description` por endpoint — todo en inglés
2. La gateway agrega los catálogos de todos los servicios
3. Para generar traducciones: se consume `GET /catalog` (interno), se pasa a una IA, se generan los archivos de i18n (`es.json`, `ca.json`, etc.)
4. Los archivos se suben a storage/CDN
5. El frontend (Angular i18n) carga las traducciones del CDN, usa `className` como key, y si no encuentra traducción muestra el fallback en inglés del catálogo

**Ventajas:**
- El dev solo escribe `.WithDescriptionCatalog("Create menu")` — cero fricción
- Añadir un endpoint nuevo = aparece automáticamente en el catálogo con su description en inglés
- Traducir = un paso automatizable con IA sobre el catálogo agregado
- Escala a N idiomas sin tocar código backend
- Mismo mecanismo sirve para traducir planes, alérgenos y cualquier dato de Firestore

### CatalogEntry — Datos necesarios

**Técnicos** (para gateway y middleware):
- ClassName
- HttpVerb
- RoutePattern
- IsAnonymous, IsInternal, IsPlatform, IsExcluded
- CustomGroup

**Presentación** (para frontend de permisos):
- Description (en inglés, fallback)

### Contrato `/catalog` de cada micro

Cada microservicio expone `/catalog` con:
- `ServiceId` + `ServiceName`
- Catálogo bruto completo (todas las entradas con todos los flags + description)
- Sin agrupaciones, sin filtros, sin lógica

### Gateway `/catalog`

La gateway es el único punto que el frontend consume:
- Agrega los catálogos de todos los servicios
- Aplica la lógica de agrupaciones
- Filtra según `tid` (plataforma ve todo, tenant normal ve lo suyo)

---

## Pendiente de definir

- [ ] Contrato JSON exacto del `/catalog` bruto de cada micro
- [ ] Contrato JSON exacto del `/catalog` agregado de la gateway
- [ ] `RequireAuthenticated()` — crear el extension method y el `AuthenticatedRequirement`
- [ ] Lógica de `FudieAuthorizationMiddleware` para evaluar cada nivel
- [ ] Eliminar `GenerateTokenInternal()` de `IInternalTokenService` y su implementación — ya no se necesita con `X-Internal-Key`. Archivos afectados:
  - `src/Auth/Infrastructure/Jwt/IInternalTokenService.cs`
  - `src/Auth/Infrastructure/Jwt/InternalTokenService.cs`
  - `src/Auth/Infrastructure/Customers/InternalAuthHandler.cs`
  - `tests/Auth.UnitTests/Infrastructure/Jwt/InternalTokenServiceTests.cs`
  - `tests/Auth.UnitTests/Infrastructure/Jwt/IInternalTokenServiceContractTests.cs`
  - `tests/Auth.UnitTests/Infrastructure/Customers/InternalAuthHandlerTests.cs`