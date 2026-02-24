# Catálogo — Problema de agrupación por agregado

## Contexto

El sistema de permisos de Fudie se basa en el catálogo que cada microservicio expone en `GET /catalog`. La gateway lo consume al arrancar y lo sirve al frontend para que el usuario pueda configurar roles con control granular.

### Lo que tenemos hoy

El catálogo actual tiene dos niveles:

```
Servicio (ServiceId)
  └── Endpoint (ClassName + Description)
```

Ejemplo real del wireframe de roles:

```
Menús — menu-service
  ├── menu:read — Ver menús
  │     ├── GetMenu — Ver un menú
  │     └── GetMenus — Ver lista
  └── menu:write — Gestionar menús
        ├── CreateMenu
        ├── UpdateMenu
        └── ...
```

### El problema

Esto funciona cuando un servicio tiene un solo agregado. Pero `menu-service` tiene 3 agregados: **Menu**, **MenuItem** y **Allergen**. Con la estructura actual, los ~30 endpoints aparecen mezclados bajo un único bloque `menu-service`:

```
Menús — menu-service
  ├── CreateMenu
  ├── CreateMenuItem       ← ¿esto es de Menu o de MenuItem?
  ├── CreateAllergen       ← ¿y esto?
  ├── AddMenuItemPriceOption
  ├── UpdateAllergen
  └── ... (30+ endpoints mezclados)
```

El usuario que configura un rol no tiene forma de distinguir qué permisos corresponden a Menús, cuáles a Artículos del menú y cuáles a Alérgenos.

### Lo que necesitamos

Un nivel intermedio — el **agregado** — entre el servicio y el endpoint:

```
Menús — menu-service
  ├── Menús (icon: book-open)          ← agregado Menu
  │     ├── menu:read
  │     └── menu:write
  ├── Artículos (icon: utensils)       ← agregado MenuItem
  │     ├── menu-item:read
  │     └── menu-item:write
  └── Alérgenos (icon: alert-triangle) ← agregado Allergen
        ├── allergen:read
        └── allergen:write
```

## La convención de carpetas ya existe

La estructura de carpetas del proyecto ya tiene este nivel de agregado:

```
src/Menus/Features/Menus/Api/
  ├── MenuAggregate/
  │     ├── Commands/
  │     ├── Queries/
  │     └── MenuResponse.cs
  ├── MenuItemAggregate/
  │     ├── Commands/
  │     └── Queries/
  └── AllergenAggregate/
        ├── Commands/
        └── Queries/
```

El namespace de cada `IFeatureModule` refleja esta estructura:

```
Menus.Features.Menus.Api.MenuAggregate.Commands.CreateMenu
                         ^^^^^^^^^^^^^^
                         2º segmento desde el final
```

Podemos extraer `MenuAggregate` del namespace por convención. Pero el nombre técnico (`MenuAggregate`) no es suficiente para el frontend — necesitamos un **DisplayName** ("Artículos del menú") y un **Icon** ("utensils").

## Opciones evaluadas

### Opción 1: Atributo por clase (`[AggregateGroup]`)

```csharp
[AggregateGroup("menu", "Menús", "book-open")]
public class CreateMenu : IFeatureModule { ... }
```

**Pros:** Explícito, el compilador lo valida.
**Contras:** Repetitivo — hay que ponerlo en CADA `IFeatureModule` del agregado. Son 30+ clases en Menus. Difícil de mantener, propenso a inconsistencias.

**Descartada** — no escala como framework.

### Opción 2: Clase descriptor por agregado (`IAggregateDescription`)

Una interfaz en `Fudie.Features` y un archivo por carpeta de agregado:

```csharp
// En Fudie.Features
public interface IAggregateDescription
{
    string Id { get; }
    string DisplayName { get; }
    string? Icon { get; }
}
```

```csharp
// MenuAggregate/MenuAggregateDescription.cs
namespace Menus.Features.Menus.Api.MenuAggregate;

public class MenuAggregateDescription : IAggregateDescription
{
    public string Id => "menu";
    public string DisplayName => "Menús";
    public string? Icon => "book-open";
}
```

```csharp
// MenuItemAggregate/MenuItemAggregateDescription.cs
namespace Menus.Features.Menus.Api.MenuItemAggregate;

public class MenuItemAggregateDescription : IAggregateDescription
{
    public string Id => "menu-item";
    public string DisplayName => "Artículos del menú";
    public string? Icon => "utensils";
}
```

La estructura de carpetas quedaría:

```
src/Menus/Features/Menus/Api/
  ├── MenuAggregate/
  │     ├── MenuAggregateDescription.cs  ← metadata
  │     ├── MenuResponse.cs
  │     ├── Commands/
  │     └── Queries/
  ├── MenuItemAggregate/
  │     ├── MenuItemAggregateDescription.cs  ← metadata
  │     ├── MenuItemResponse.cs
  │     ├── Commands/
  │     └── Queries/
  └── AllergenAggregate/
        ├── AllergenAggregateDescription.cs  ← metadata
        ├── Commands/
        └── Queries/
```

**Descubrimiento automático en `MapFeatures()`:**

`RouteExtension` escanea los assemblies buscando todas las clases que implementen `IAggregateDescription`, las indexa por namespace, y al registrar cada `IFeatureModule` busca la `IAggregateDescription` cuyo namespace coincida con el segmento de agregado del feature.

```csharp
// En MapFeatures(), antes del foreach
var aggregateDescriptions = assemblies
    .SelectMany(a => a.GetTypes())
    .Where(t => t.IsClass && !t.IsAbstract && t.IsAssignableTo(typeof(IAggregateDescription)))
    .Select(t => (IAggregateDescription)Activator.CreateInstance(t)!)
    .ToDictionary(d => d.GetType().Namespace!, d => d);

// Al registrar cada feature
var featureNamespace = feature.GetType().Namespace!;
var aggregateNamespace = featureNamespace[..featureNamespace.LastIndexOf('.')]; // strip Commands/Queries
var description = aggregateDescriptions.GetValueOrDefault(aggregateNamespace);
```

**Validación en startup:**
- Si un `IFeatureModule` no tiene un `IAggregateDescription` asociado en su namespace de agregado → excepción.
- Si existe una `IAggregateDescription` huérfana (sin ningún `IFeatureModule` en su namespace) → warning.

**Pros:**
- Un solo archivo por agregado, vive junto al código.
- Autodescubierto por el framework — sin configuración manual en appsettings.
- Type-safe: el compilador valida que la clase compile.
- El desarrollador crea la carpeta del agregado, pone su descriptor, y listo.
- Fácil de validar en startup: escanear que todos los features tengan su descriptor.

**Contras:**
- Es un archivo que el desarrollador puede olvidar al crear un agregado nuevo.
- Si obligamos con excepción en startup, cualquier descuido rompe el servicio.
- Si hacemos fallback silencioso, acumulamos inconsistencias.
- Un archivo más por agregado (aunque es mínimo: 7 líneas).

### Opción 3: Configuración en `appsettings.json` del servicio

```json
{
  "Fudie": {
    "ServiceId": "menu-service",
    "ServiceName": "Menús",
    "Aggregates": {
      "Menu": { "DisplayName": "Menús", "Icon": "book-open" },
      "MenuItem": { "DisplayName": "Artículos", "Icon": "utensils" },
      "Allergen": { "DisplayName": "Alérgenos", "Icon": "alert-triangle" }
    }
  }
}
```

El `CatalogRegistry` extrae el nombre del agregado del namespace y lo cruza con la config.

**Pros:** Centralizado por servicio, sin tocar clases.
**Contras:**
- Si obligamos con excepción al no encontrar la key → rompe startup por un despiste (como el error de Sessions).
- La metadata de presentación (DisplayName, Icon) queda distribuida en N appsettings de N microservicios.
- El desarrollador puede equivocarse en la key (typo en "MenueItem" vs "MenuItem").

### Opción 4: Convención en el servicio + metadata de presentación en la gateway

**Separar responsabilidades:**

- **Cada microservicio** solo expone el ID técnico del agregado, extraído por convención del namespace. Si la convención no se cumple (no existe segmento `{X}Aggregate`), excepción en startup → esto habría cazado el error de Sessions de inmediato.

- **La gateway** mantiene un diccionario centralizado de DisplayName + Icon para todos los agregados de todos los servicios. Es un solo sitio, no N.

```
Microservicio (convención de carpetas → ID técnico)
    ↓
GET /catalog devuelve: { aggregate: "menu", className: "CreateMenu", ... }
    ↓
Gateway (diccionario centralizado → DisplayName + Icon)
    ↓
Frontend recibe: { aggregate: "menu", displayName: "Menús", icon: "book-open", ... }
```

**Pros:**
- La convención se valida en startup con excepción (fail-fast).
- La metadata de presentación está en UN solo sitio (la gateway), no distribuida.
- Los microservicios no necesitan saber nada de UI.
- Fácil de mantener: al añadir un agregado, el servicio falla si no sigue la convención, y la gateway es el único sitio donde añadir el display name.

**Contras:**
- Si se añade un agregado nuevo y no se actualiza la gateway, el frontend no tendrá display name (requiere fallback o validación).
- La gateway se convierte en el punto central de metadata de presentación.

## Preguntas abiertas

1. **¿Qué convención usar para extraer el ID del agregado?**
   - Del namespace: 2º segmento desde el final (`MenuAggregate` → strip suffix → `Menu`)
   - Del namespace: 3er segmento desde el inicio (`Menus`) — el plural de la feature
   - ¿Se usa el singular (`Menu`) o el plural (`Menus`) como ID?

2. **¿Qué pasa con el scope automático?**
   - Hoy los GETs van a `{x}:read` y los POST/PUT/PATCH/DELETE a `{x}:write`
   - ¿Qué es `{x}`? ¿El nombre del agregado (`menu`, `menu-item`) o un `CustomGroup` vía `RequireGroup()`?
   - Si es el nombre del agregado, ¿se deriva automáticamente del namespace?

3. **¿Dónde vive el mapeo DisplayName + Icon?**
   - En cada microservicio (appsettings.json)
   - En la gateway (diccionario centralizado)
   - Combinación: ID técnico en el servicio, presentación en la gateway

4. **¿Qué hacer cuando falla la convención?**
   - Excepción en startup (fail-fast, lo habría cazado en Sessions)
   - Warning en logs + fallback al nombre técnico
   - ¿Solo en desarrollo o también en producción?

5. **¿Se necesita el aggregate como campo en `CatalogEntry`?**
   - Hoy no existe — habría que añadirlo
   - Impacta en: `CatalogEntry`, `CatalogRegistry.Register()`, `RouteExtension.MapFeatures()`, el endpoint `/catalog`, y el consumo desde la gateway

## Propuesta favorita: Opción 3 (appsettings.json) con validación

De las opciones evaluadas, la que mejor equilibra simplicidad y mantenibilidad es la **Opción 3** con una mejora: validación cruzada en startup.

### Cómo funcionaría

Cada microservicio declara sus agregados en `appsettings.json`:

```json
{
  "Fudie": {
    "ServiceId": "menu-service",
    "ServiceName": "Menús",
    "Aggregates": {
      "Menu": { "DisplayName": "Menús", "Icon": "book-open" },
      "MenuItem": { "DisplayName": "Artículos del menú", "Icon": "utensils" },
      "Allergen": { "DisplayName": "Alérgenos", "Icon": "alert-triangle" }
    }
  }
}
```

Otro ejemplo con un servicio de un solo agregado:

```json
{
  "Fudie": {
    "ServiceId": "customer-service",
    "ServiceName": "Clientes",
    "Aggregates": {
      "Customer": { "DisplayName": "Clientes", "Icon": "users" }
    }
  }
}
```

### Flujo en `MapFeatures()`

`RouteExtension.MapFeatures()` ya tiene acceso a `builder.ServiceProvider`:

```csharp
var catalog = builder.ServiceProvider.GetRequiredService<ICatalogRegistry>();
var configuration = builder.ServiceProvider.GetRequiredService<IConfiguration>();
var aggregates = configuration.GetSection("Fudie:Aggregates");
```

Al registrar cada `IFeatureModule`:

1. Extraer el nombre del agregado del namespace (2º segmento desde el final, strip suffix `Aggregate`)
2. Buscar la key en `Fudie:Aggregates`
3. Si no existe → **excepción en startup** con mensaje claro:
   ```
   Aggregate 'Session' not found in Fudie:Aggregates configuration.
   Namespace: Auth.Features.Sessions.Api.Commands.ResolveAuth
   Expected namespace pattern: {Project}.Features.{Feature}.Api.{Aggregate}Aggregate.{Commands|Queries}.{ClassName}
   Add the aggregate to appsettings.json or fix the namespace convention.
   ```
4. Si existe → pasar `AggregateId`, `DisplayName` e `Icon` al `CatalogRegistry`

### Validación cruzada (doble red de seguridad)

La excepción en startup cubre DOS errores simultáneamente:

| Error | Qué lo detecta |
|-------|----------------|
| Namespace sin `{X}Aggregate` (como Sessions antes del fix) | No encuentra el segmento → excepción |
| Agregado en namespace pero sin entry en appsettings | No encuentra la key → excepción |
| Typo en la key del appsettings | El namespace no matchea → excepción |

Esto habría cazado el error de Sessions de inmediato.

### Cambio en `CatalogEntry`

```csharp
// Antes
public record CatalogEntry(
    string ClassName, string HttpVerb, string RoutePattern,
    bool IsAnonymous, bool IsAuthenticated, bool IsInternal,
    bool IsPlatform, bool IsExcluded, string? CustomGroup, string? Description);

// Después
public record CatalogEntry(
    string ClassName, string HttpVerb, string RoutePattern,
    bool IsAnonymous, bool IsAuthenticated, bool IsInternal,
    bool IsPlatform, bool IsExcluded, string? CustomGroup, string? Description,
    string AggregateId, string AggregateDisplayName, string? AggregateIcon);
```

### Respuesta del endpoint `/catalog` con agregados

```json
{
  "serviceId": "menu-service",
  "serviceName": "Menús",
  "entries": [
    {
      "className": "CreateMenu",
      "httpVerb": "POST",
      "routePattern": "/menus",
      "aggregateId": "Menu",
      "aggregateDisplayName": "Menús",
      "aggregateIcon": "book-open",
      "description": "Crear un menú",
      "isAnonymous": false,
      "isInternal": false
    },
    {
      "className": "CreateMenuItem",
      "httpVerb": "POST",
      "routePattern": "/menu-items",
      "aggregateId": "MenuItem",
      "aggregateDisplayName": "Artículos del menú",
      "aggregateIcon": "utensils",
      "description": "Crear un artículo",
      "isAnonymous": false,
      "isInternal": false
    }
  ]
}
```

### Por qué esta opción y no las otras

| Criterio | Atributo por clase | Clase descriptor | appsettings.json | Gateway centralizada |
|----------|-------------------|-----------------|------------------|---------------------|
| Archivos a tocar por agregado | Todos los IFeatureModule | 1 clase nueva | 0 (solo config) | 0 (solo config gateway) |
| Riesgo de olvido | Alto (N clases) | Medio (1 clase) | Bajo (1 sección) | Bajo (1 diccionario) |
| Validación en startup | Compilador | Posible | Posible | No (es otro servicio) |
| Metadata cerca del código | Sí | Sí | Sí (mismo repo) | No (otro repo) |
| Escala como framework | No | Regular | Sí | Sí pero acoplado |

La opción appsettings.json gana porque:
- **Cero archivos nuevos** por agregado — solo una sección de configuración
- **Validación cruzada** namespace ↔ config en startup
- **La metadata vive en el mismo repo** del microservicio, cerca del código
- **Patrón familiar** — los desarrolladores ya conocen appsettings.json
- **El endpoint `/catalog` devuelve todo lo necesario** — la gateway no necesita un diccionario propio

### Riesgo principal y mitigación

**Riesgo:** La metadata de presentación queda distribuida en N microservicios.

**Mitigación:** Cada servicio es owner de su propia metadata. Si un equipo añade un agregado, es responsable de añadir la entrada en su propio appsettings. La excepción en startup garantiza que no se olvide.

## Impacto en el framework Fudie

Este cambio afecta al core del framework:
- `Fudie.Features`: `CatalogEntry`, `CatalogRegistry`, `RouteExtension`, `ICatalogRegistry`
- Todos los microservicios que usen `MapFeatures()`
- La gateway que consume `/catalog`
- El frontend que renderiza el UI de roles

No es un cambio que se pueda hacer incrementalmente sin pensarlo bien. La decisión de dónde vive la metadata (servicio vs gateway) y cómo se valida la convención tiene implicaciones a largo plazo para cualquier equipo que adopte Fudie.
