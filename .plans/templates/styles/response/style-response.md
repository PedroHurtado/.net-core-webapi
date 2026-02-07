# Estilo: Response

## Ubicación

- Archivo: `{Aggregate}Response.cs`
- Namespace: `{Microservice}.Features.{Feature}.Api.{Aggregate}Aggregate`
- Carpeta: `src/{Microservice}/Features/{Feature}/Api/{Aggregate}Aggregate/`

---

## Estructura General

Un único archivo contiene todos los records de respuesta del agregado: el record raíz, los value objects y las entidades hijas. Cada record incluye un método estático `Map` que transforma dominio → API.

```csharp
namespace {Microservice}.Features.{Feature}.Api.{Aggregate}Aggregate;

public record {Aggregate}Response(
    Guid Id,
    string Name,
    ...)
{
    public static {Aggregate}Response Map({Aggregate} entity) => new(
        Id: entity.Id,
        Name: entity.Name,
        ...);
}

public record {ValueObject}Response(
    string Property1,
    string Property2)
{
    public static {ValueObject}Response Map({ValueObject} vo) => new(
        Property1: vo.Property1,
        Property2: vo.Property2);
}
```

---

## Método Map

Cada record define un `public static {Record} Map({DomainType} source)` con named arguments.

### Propiedades directas

Mapeo 1:1 del dominio al response, incluyendo propiedades calculadas:

```csharp
public static {Aggregate}Response Map({Aggregate} entity) => new(
    Id: entity.Id,
    Name: entity.Name,
    IsActive: entity.IsActive,
    HasImages: entity.HasImages,           // propiedad calculada
    IsProfileComplete: entity.IsProfileComplete); // propiedad calculada
```

### Value objects anidados (obligatorios)

Delegar al `Map` del sub-response:

```csharp
    Address: AddressResponse.Map(entity.Address),
    ContactInfo: ContactInfoResponse.Map(entity.ContactInfo));
```

### Value objects anidados (nullable)

Ternario con `is not null`:

```csharp
    PriceRange: entity.PriceRange is not null
        ? PriceRangeResponse.Map(entity.PriceRange)
        : null,
    CoverImage: entity.CoverImage is not null
        ? {Child}Response.Map(entity.CoverImage)
        : null);
```

### Colecciones de objetos

`.Select({Record}.Map).ToList().AsReadOnly()`:

```csharp
    Images: entity.Images
        .Select(CustomerImageResponse.Map)
        .ToList()
        .AsReadOnly(),
    SocialLinks: entity.SocialLinks
        .Select(SocialLinkResponse.Map)
        .ToList()
        .AsReadOnly());
```

### Colecciones de strings

Pasar directamente (el tipo `IReadOnlyCollection<string>` coincide):

```csharp
    CuisineTypes: entity.CuisineTypes,
    ServiceAmenities: entity.ServiceAmenities);
```

### Diccionarios

`.ToDictionary(...).AsReadOnly()`:

```csharp
    WeeklyHours: entity.WeeklyHours
        .ToDictionary(
            kvp => kvp.Key,
            kvp => DayScheduleResponse.Map(kvp.Value))
        .AsReadOnly());
```

---

## Orden de records en el archivo

1. `{Aggregate}Response` (raíz)
2. Value objects simples (ej: `GeoPointResponse`)
3. Value objects compuestos que dependen de los simples (ej: `AddressResponse` que usa `GeoPointResponse`)
4. Otros value objects (ej: `ContactInfoResponse`, `BillingInfoResponse`, `PriceRangeResponse`)
5. Entidades hijas (ej: `CustomerImageResponse`, `SocialLinkResponse`)

---

## Reglas

- **No `using`** → Van en `GlobalUsings.cs`
- **No XML docs**
- **Un solo archivo** con todos los records del agregado
- Cada record tiene su propio `Map` estático
- Usar **named arguments** en el constructor del `new()`
- Las **propiedades calculadas** del dominio se incluyen directamente (ej: `HasPriceRange`, `IsProfileComplete`, `FullAddress`)
- **No incluir TenantId** en las respuestas de API
- Las slices usan `{Aggregate}Response.Map(entity)` → no crear ni duplicar el Response en cada slice
