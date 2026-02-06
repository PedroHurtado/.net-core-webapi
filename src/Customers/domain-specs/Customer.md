# Domain Specification: Customer

---

## 1. Enums

*No hay enums en este agregado. Todos los clasificadores (tipo de establecimiento, cocinas, amenidades, opciones dietéticas) son strings libres gestionados por el administrador del customere.*

---

## 2. Value Objects

### 2.1 GeoPoint

#### Estructura (Positional Record)

```csharp
public partial record GeoPoint(
    decimal Latitude,
    decimal Longitude
);
```

#### Invariantes (Validator)

> Estas reglas se implementan en `GeoPointValidator : AbstractValidator<GeoPoint>`

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| Latitude | Between(-90, 90) | "Latitude must be between -90 and 90" |
| Longitude | Between(-180, 180) | "Longitude must be between -180 and 180" |

#### Comando: GeoPoint.Create

**Input**

| Campo | Tipo |
|-------|------|
| Latitude | decimal |
| Longitude | decimal |

**Inyecta**: `IValidator<GeoPoint>`

**Lógica**
```csharp
var geoPoint = new GeoPoint(command.Latitude, command.Longitude);

return geoPointValidator.ValidateOrThrow(geoPoint);
```

#### Tests Unitarios

✅ GeoPoint válido
- Input: Latitude=38.0389, Longitude=-1.4917
- Resultado: GeoPoint creado

✅ GeoPoint en límites (polo norte)
- Input: Latitude=90, Longitude=0
- Resultado: GeoPoint creado

✅ GeoPoint en límites (antimeridiano)
- Input: Latitude=0, Longitude=-180
- Resultado: GeoPoint creado

❌ Latitude fuera de rango (superior)
- Input: Latitude=91
- Resultado: ValidationException "Latitude must be between -90 and 90"

❌ Latitude fuera de rango (inferior)
- Input: Latitude=-91
- Resultado: ValidationException "Latitude must be between -90 and 90"

❌ Longitude fuera de rango (superior)
- Input: Longitude=181
- Resultado: ValidationException "Longitude must be between -180 and 180"

❌ Longitude fuera de rango (inferior)
- Input: Longitude=-181
- Resultado: ValidationException "Longitude must be between -180 and 180"

---

### 2.2 Address

#### Estructura (Positional Record)

```csharp
public partial record Address(
    string Street,
    string City,
    string PostalCode,
    string Region,
    string Country,
    GeoPoint Location
);
```

#### Invariantes (Validator)

> Estas reglas se implementan en `AddressValidator : AbstractValidator<Address>`

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| Street | NotEmpty | "Street is required" |
| Street | Max(200) | "Street cannot exceed 200 characters" |
| City | NotEmpty | "City is required" |
| City | Max(100) | "City cannot exceed 100 characters" |
| PostalCode | NotEmpty | "Postal code is required" |
| PostalCode | Max(20) | "Postal code cannot exceed 20 characters" |
| Region | NotEmpty | "Region is required" |
| Region | Max(100) | "Region cannot exceed 100 characters" |
| Country | NotEmpty | "Country is required" |
| Country | Max(100) | "Country cannot exceed 100 characters" |
| Location | NotNull | "Location is required" |

#### Propiedades Calculadas

> Solo métodos query (devuelven datos derivados, sin efectos secundarios). **NO incluir métodos command.**

| Propiedad | Tipo | Fórmula |
|-----------|------|---------|
| FullAddress | string | `$"{Street}, {PostalCode} {City}, {Region}, {Country}"` |

#### Comando: Address.Create

**Input**

| Campo | Tipo |
|-------|------|
| Street | string |
| City | string |
| PostalCode | string |
| Region | string |
| Country | string |
| Latitude | decimal |
| Longitude | decimal |

**Inyecta**: `GeoPoint.Create`, `IValidator<Address>`

**Lógica**
```csharp
var location = geoPointCreate.Execute(new CreateGeoPointCommand(
    command.Latitude,
    command.Longitude));

var address = new Address(
    command.Street,
    command.City,
    command.PostalCode,
    command.Region,
    command.Country,
    location);

return addressValidator.ValidateOrThrow(address);
```

#### Tests Unitarios

✅ Address válida
- Input: Street="Ctra. Murcia, 23", City="La Puebla de Mula", PostalCode="30193", Region="Murcia", Country="España", Latitude=38.0389, Longitude=-1.4917
- Resultado: Address creada, FullAddress="Ctra. Murcia, 23, 30193 La Puebla de Mula, Murcia, España", Location.Latitude=38.0389

❌ Street vacío
- Input: Street=""
- Resultado: ValidationException "Street is required"

❌ City vacío
- Input: City=""
- Resultado: ValidationException "City is required"

❌ PostalCode vacío
- Input: PostalCode=""
- Resultado: ValidationException "Postal code is required"

❌ Latitude fuera de rango
- Input: Latitude=91
- Resultado: ValidationException "Latitude must be between -90 and 90"

❌ Longitude fuera de rango
- Input: Longitude=-181
- Resultado: ValidationException "Longitude must be between -180 and 180"

---

### 2.3 ContactInfo

#### Estructura (Positional Record)

```csharp
public partial record ContactInfo(
    string Phone,
    string? Email,
    string? WebsiteUrl
);
```

#### Invariantes (Validator)

> Estas reglas se implementan en `ContactInfoValidator : AbstractValidator<ContactInfo>`

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| Phone | NotEmpty | "Phone is required" |
| Phone | Max(20) | "Phone cannot exceed 20 characters" |
| Email | Max(150) | "Email cannot exceed 150 characters" |
| Email | ValidEmail when NotEmpty | "Email must be a valid email address" |
| WebsiteUrl | Max(500) | "Website URL cannot exceed 500 characters" |
| WebsiteUrl | ValidUrl when NotEmpty | "Website URL must be a valid URL" |

#### Comando: ContactInfo.Create

**Input**

| Campo | Tipo |
|-------|------|
| Phone | string |
| Email | string? |
| WebsiteUrl | string? |

**Inyecta**: `IValidator<ContactInfo>`

**Lógica**
```csharp
var contactInfo = new ContactInfo(
    command.Phone,
    command.Email,
    command.WebsiteUrl);

return contactInfoValidator.ValidateOrThrow(contactInfo);
```

#### Tests Unitarios

✅ ContactInfo completo
- Input: Phone="639079481", Email="juanjo@example.com", WebsiteUrl="https://facebook.com/elbardeljuanjo"
- Resultado: ContactInfo creado

✅ ContactInfo solo teléfono
- Input: Phone="639079481", Email=null, WebsiteUrl=null
- Resultado: ContactInfo creado

❌ Phone vacío
- Input: Phone=""
- Resultado: ValidationException "Phone is required"

❌ Email inválido
- Input: Email="not-an-email"
- Resultado: ValidationException "Email must be a valid email address"

❌ WebsiteUrl inválido
- Input: WebsiteUrl="not-a-url"
- Resultado: ValidationException "Website URL must be a valid URL"

---

### 2.4 BillingInfo

#### Estructura (Positional Record)

```csharp
public partial record BillingInfo(
    string BusinessName,
    string TaxId,
    Address BillingAddress
);
```

#### Invariantes (Validator)

> Estas reglas se implementan en `BillingInfoValidator : AbstractValidator<BillingInfo>`

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| BusinessName | NotEmpty | "Business name is required" |
| BusinessName | Max(200) | "Business name cannot exceed 200 characters" |
| TaxId | NotEmpty | "Tax ID is required" |
| TaxId | Max(50) | "Tax ID cannot exceed 50 characters" |
| BillingAddress | NotNull | "Billing address is required" |

#### Comando: BillingInfo.Create

**Input**

| Campo | Tipo |
|-------|------|
| BusinessName | string |
| TaxId | string |
| BillingAddress | CreateAddressCommand |

**Inyecta**: `Address.Create`, `IValidator<BillingInfo>`

**Lógica**
```csharp
var billingAddress = addressCreate.Execute(command.BillingAddress);

var billingInfo = new BillingInfo(
    command.BusinessName,
    command.TaxId,
    billingAddress);

return billingInfoValidator.ValidateOrThrow(billingInfo);
```

#### Tests Unitarios

✅ BillingInfo válido
- Input: BusinessName="Bar Juanjo SL", TaxId="B12345678", BillingAddress=válida
- Resultado: BillingInfo creado

✅ BillingInfo con dirección fiscal diferente a dirección física
- Input: BusinessName="Bar Juanjo SL", TaxId="B12345678", BillingAddress={Street="C/ Gran Vía, 1", City="Murcia"...}
- Resultado: BillingInfo creado con dirección diferente

❌ BusinessName vacío
- Input: BusinessName=""
- Resultado: ValidationException "Business name is required"

❌ TaxId vacío
- Input: TaxId=""
- Resultado: ValidationException "Tax ID is required"

❌ BillingAddress inválida
- Input: BillingAddress con Street=""
- Resultado: ValidationException "Street is required"

---

### 2.5 PriceRange

#### Estructura (Positional Record)

```csharp
public partial record PriceRange(
    decimal MinPrice,
    decimal MaxPrice
);
```

#### Invariantes (Validator)

> Estas reglas se implementan en `PriceRangeValidator : AbstractValidator<PriceRange>`

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| MinPrice | >= 0 | "Minimum price cannot be negative" |
| MaxPrice | >= 0 | "Maximum price cannot be negative" |
| MaxPrice | >= MinPrice | "Maximum price must be greater than or equal to minimum price" |

#### Comando: PriceRange.Create

**Input**

| Campo | Tipo |
|-------|------|
| MinPrice | decimal |
| MaxPrice | decimal |

**Inyecta**: `IValidator<PriceRange>`

**Lógica**
```csharp
var priceRange = new PriceRange(
    command.MinPrice,
    command.MaxPrice);

return priceRangeValidator.ValidateOrThrow(priceRange);
```

#### Tests Unitarios

✅ PriceRange válido
- Input: MinPrice=10, MaxPrice=20
- Resultado: PriceRange creado

✅ PriceRange con mismo valor
- Input: MinPrice=15, MaxPrice=15
- Resultado: PriceRange creado

❌ MinPrice negativo
- Input: MinPrice=-5
- Resultado: ValidationException "Minimum price cannot be negative"

❌ MaxPrice menor que MinPrice
- Input: MinPrice=20, MaxPrice=10
- Resultado: ValidationException "Maximum price must be greater than or equal to minimum price"

---

### 2.6 CultureCode

#### Estructura (Positional Record)

```csharp
public partial record CultureCode(
    string Code
);
```

#### Invariantes (Validator)

> Estas reglas se implementan en `CultureCodeValidator : AbstractValidator<CultureCode>`

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| Code | NotEmpty | "Culture code is required" |
| Code | Matches(`^[a-z]{2}-[A-Z]{2}$`) | "Culture code must follow format 'xx-XX' (e.g. es-ES)" |

#### Comando: CultureCode.Create

**Input**

| Campo | Tipo |
|-------|------|
| Code | string |

**Inyecta**: `IValidator<CultureCode>`

**Lógica**
```csharp
var cultureCode = new CultureCode(command.Code);

return cultureCodeValidator.ValidateOrThrow(cultureCode);
```

#### Tests Unitarios

✅ CultureCode válido
- Input: Code="es-ES"
- Resultado: CultureCode creado

✅ CultureCode inglés
- Input: Code="en-GB"
- Resultado: CultureCode creado

❌ Code vacío
- Input: Code=""
- Resultado: ValidationException "Culture code is required"

❌ Code formato incorrecto
- Input: Code="español"
- Resultado: ValidationException "Culture code must follow format 'xx-XX' (e.g. es-ES)"

❌ Code formato incorrecto (solo idioma)
- Input: Code="es"
- Resultado: ValidationException "Culture code must follow format 'xx-XX' (e.g. es-ES)"

---

### 2.7 SocialLink

#### Estructura (Positional Record)

```csharp
public partial record SocialLink(
    string Platform,
    string Url
);
```

#### Invariantes (Validator)

> Estas reglas se implementan en `SocialLinkValidator : AbstractValidator<SocialLink>`

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| Platform | NotEmpty | "Platform is required" |
| Platform | Max(50) | "Platform cannot exceed 50 characters" |
| Url | NotEmpty | "URL is required" |
| Url | Max(500) | "URL cannot exceed 500 characters" |
| Url | ValidUrl | "URL must be a valid URL" |

#### Comando: SocialLink.Create

**Input**

| Campo | Tipo |
|-------|------|
| Platform | string |
| Url | string |

**Inyecta**: `IValidator<SocialLink>`

**Lógica**
```csharp
var socialLink = new SocialLink(
    command.Platform,
    command.Url);

return socialLinkValidator.ValidateOrThrow(socialLink);
```

#### Tests Unitarios

✅ SocialLink válido
- Input: Platform="Facebook", Url="https://facebook.com/elbardeljuanjo"
- Resultado: SocialLink creado

✅ SocialLink Instagram
- Input: Platform="Instagram", Url="https://instagram.com/elbardeljuanjo"
- Resultado: SocialLink creado

✅ SocialLink TripAdvisor
- Input: Platform="TripAdvisor", Url="https://tripadvisor.es/Customer_Review-..."
- Resultado: SocialLink creado

❌ Platform vacío
- Input: Platform=""
- Resultado: ValidationException "Platform is required"

❌ Url vacío
- Input: Url=""
- Resultado: ValidationException "URL is required"

❌ Url inválida
- Input: Url="not-a-url"
- Resultado: ValidationException "URL must be a valid URL"

---

### 2.8 CustomerImage

#### Estructura (Positional Record)

```csharp
public partial record CustomerImage(
    Guid Id,
    string Url,
    string? AltText,
    int DisplayOrder,
    bool IsCover
);
```

#### Invariantes (Validator)

> Estas reglas se implementan en `CustomerImageValidator : AbstractValidator<CustomerImage>`

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| Id | NotEmpty | "Image id is required" |
| Url | NotEmpty | "Image URL is required" |
| Url | Max(500) | "Image URL cannot exceed 500 characters" |
| Url | ValidUrl | "Image URL must be a valid URL" |
| AltText | Max(200) | "Alt text cannot exceed 200 characters" |
| DisplayOrder | >= 0 | "Display order cannot be negative" |

#### Comando: CustomerImage.Create

**Input**

| Campo | Tipo | Default |
|-------|------|---------|
| Url | string | |
| AltText | string? | null |
| DisplayOrder | int | 0 |
| IsCover | bool | false |

**Inyecta**: `IValidator<CustomerImage>`

**Lógica**
```csharp
var image = new CustomerImage(Guid.NewGuid())
{
    Url = command.Url,
    AltText = command.AltText,
    DisplayOrder = command.DisplayOrder,
    IsCover = command.IsCover
};

return imageValidator.ValidateOrThrow(image);
```

#### Tests Unitarios

✅ Imagen válida con todos los campos
- Input: Url="https://cdn.fudie.com/images/fachada.jpg", AltText="Fachada del customere", DisplayOrder=1, IsCover=true
- Resultado: CustomerImage creada

✅ Imagen válida solo con URL
- Input: Url="https://cdn.fudie.com/images/interior.jpg"
- Resultado: CustomerImage creada con AltText=null, DisplayOrder=0, IsCover=false

❌ Url vacía
- Input: Url=""
- Resultado: ValidationException "Image URL is required"

❌ Url inválida
- Input: Url="not-a-url"
- Resultado: ValidationException "Image URL must be a valid URL"

❌ AltText excede máximo
- Input: AltText=string(201)
- Resultado: ValidationException "Alt text cannot exceed 200 characters"

❌ DisplayOrder negativo
- Input: DisplayOrder=-1
- Resultado: ValidationException "Display order cannot be negative"

---

## 3. Aggregate: Customer

### Estructura

```
Customer (Aggregate Root)
├─ Id: Guid
├─ Name: string
├─ Slug: string
├─ Description: string?
├─ LogoUrl: string?
├─ EstablishmentType: string
├─ DefaultCulture: string
├─ TimeZoneId: string
├─ IsActive: bool
├─ Address: Address
├─ ContactInfo: ContactInfo
├─ BillingInfo: BillingInfo
├─ PriceRange: PriceRange?
├─ Images: IReadOnlyCollection<CustomerImage>
├─ CuisineTypes: IReadOnlyCollection<string>
├─ ServiceAmenities: IReadOnlyCollection<string>
├─ DietaryOptions: IReadOnlyCollection<string>
├─ SupportedCultures: IReadOnlyCollection<CultureCode>
└─ SocialLinks: IReadOnlyCollection<SocialLink>
```

#### Propiedades

| Propiedad | Tipo | Modificador |
|-----------|------|-------------|
| Id | Guid | init |
| Name | string | protected set |
| Slug | string | protected set |
| Description | string? | protected set |
| LogoUrl | string? | protected set |
| EstablishmentType | string | protected set |
| DefaultCulture | string | protected set |
| TimeZoneId | string | protected set |
| IsActive | bool | protected set |
| Address | Address | protected set |
| ContactInfo | ContactInfo | protected set |
| BillingInfo | BillingInfo | protected set |
| PriceRange | PriceRange? | protected set |

#### Colecciones

```csharp
protected HashSet<CustomerImage> _images = [];
public IReadOnlyCollection<CustomerImage> Images => _images.ToList().AsReadOnly();

protected HashSet<string> _cuisineTypes = [];
public IReadOnlyCollection<string> CuisineTypes => _cuisineTypes.ToList().AsReadOnly();

protected HashSet<string> _serviceAmenities = [];
public IReadOnlyCollection<string> ServiceAmenities => _serviceAmenities.ToList().AsReadOnly();

protected HashSet<string> _dietaryOptions = [];
public IReadOnlyCollection<string> DietaryOptions => _dietaryOptions.ToList().AsReadOnly();

protected HashSet<CultureCode> _supportedCultures = [];
public IReadOnlyCollection<CultureCode> SupportedCultures => _supportedCultures.ToList().AsReadOnly();

protected HashSet<SocialLink> _socialLinks = [];
public IReadOnlyCollection<SocialLink> SocialLinks => _socialLinks.ToList().AsReadOnly();
```

#### Propiedades Calculadas

> Solo métodos query (devuelven datos derivados, sin efectos secundarios). **NO incluir métodos command.**

| Propiedad | Tipo | Fórmula |
|-----------|------|---------|
| HasPriceRange | bool | `PriceRange != null` |
| HasLogo | bool | `!string.IsNullOrEmpty(LogoUrl)` |
| HasImages | bool | `_images.Any()` |
| CoverImage | CustomerImage? | `_images.FirstOrDefault(i => i.IsCover) ?? _images.OrderBy(i => i.DisplayOrder).FirstOrDefault()` |
| IsProfileComplete | bool | `HasLogo && HasImages && Description != null && CuisineTypes.Any()` |

### Validaciones

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| Id | NotEmpty | "Id is required" |
| Name | NotEmpty | "Name is required" |
| Name | Max(150) | "Name cannot exceed 150 characters" |
| Slug | NotEmpty | "Slug is required" |
| Slug | Max(150) | "Slug cannot exceed 150 characters" |
| Slug | Matches(`^[a-z0-9]+(?:-[a-z0-9]+)*$`) | "Slug must contain only lowercase letters, numbers, and hyphens" |
| Description | Max(2000) | "Description cannot exceed 2000 characters" |
| LogoUrl | Max(500) | "Logo URL cannot exceed 500 characters" |
| LogoUrl | ValidUrl when NotEmpty | "Logo URL must be a valid URL" |
| EstablishmentType | NotEmpty | "Establishment type is required" |
| EstablishmentType | Max(100) | "Establishment type cannot exceed 100 characters" |
| DefaultCulture | NotEmpty | "Default culture is required" |
| DefaultCulture | Matches(`^[a-z]{2}-[A-Z]{2}$`) | "Default culture must follow format 'xx-XX' (e.g. es-ES)" |
| TimeZoneId | NotEmpty | "Time zone is required" |
| TimeZoneId | Max(100) | "Time zone cannot exceed 100 characters" |
| Address | NotNull | "Address is required" |
| ContactInfo | NotNull | "Contact info is required" |
| BillingInfo | NotNull | "Billing info is required" |

---

## 4. Response

```csharp
public record CustomerResponse(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string? LogoUrl,
    string EstablishmentType,
    string DefaultCulture,
    string TimeZoneId,
    bool IsActive,
    bool HasPriceRange,
    bool HasLogo,
    bool HasImages,
    bool IsProfileComplete,
    AddressResponse Address,
    ContactInfoResponse ContactInfo,
    BillingInfoResponse BillingInfo,
    PriceRangeResponse? PriceRange,
    CustomerImageResponse? CoverImage,
    IReadOnlyCollection<CustomerImageResponse> Images,
    IReadOnlyCollection<string> CuisineTypes,
    IReadOnlyCollection<string> ServiceAmenities,
    IReadOnlyCollection<string> DietaryOptions,
    IReadOnlyCollection<CultureCodeResponse> SupportedCultures,
    IReadOnlyCollection<SocialLinkResponse> SocialLinks
);

public record GeoPointResponse(
    decimal Latitude,
    decimal Longitude
);

public record AddressResponse(
    string Street,
    string City,
    string PostalCode,
    string Region,
    string Country,
    GeoPointResponse Location,
    string FullAddress
);

public record ContactInfoResponse(
    string Phone,
    string? Email,
    string? WebsiteUrl
);

public record BillingInfoResponse(
    string BusinessName,
    string TaxId,
    AddressResponse BillingAddress
);

public record PriceRangeResponse(
    decimal MinPrice,
    decimal MaxPrice
);

public record CultureCodeResponse(
    string Code
);

public record SocialLinkResponse(
    string Platform,
    string Url
);

public record CustomerImageResponse(
    Guid Id,
    string Url,
    string? AltText,
    int DisplayOrder,
    bool IsCover
);
```

---

## 5. Event Storming - Leyenda

| Color | Elemento | Símbolo | Descripción |
|-------|----------|---------|-------------|
| 🟠 Naranja | Domain Event | `<EventName>` | Algo que ocurrió (pasado) |
| 🔵 Azul | Command | `(CommandName)` | Intención/Acción (imperativo) |
| 🟡 Amarillo | Actor | `[ActorName]` | Usuario o sistema que inicia |
| 🟣 Púrpura | Policy | `{PolicyName}` | Regla de negocio/Política |
| 🟤 Marrón | Aggregate | `[[AggregateName]]` | Entidad raíz del agregado |
| 🔴 Rojo | Hot Spot | `⚠️` | Dudas o conflictos pendientes |
| 🟢 Verde | Read Model | `📊` | Vista/Proyección de datos |
| ⚪ Blanco | External System | `⚡` | Sistema externo |

---

## 6. Comandos

> ⚠️ **IMPORTANTE**: El orden de los comandos respeta las dependencias.
> - Customer.Create incluye Address, ContactInfo y BillingInfo obligatorios (vienen del flujo de pago)
> - Las Queries (Get, List) van después de Create
> - Los Set/Remove de VOs opcionales (PriceRange) van después de Update
> - Add/Remove de colecciones (Images, SocialLinks, SupportedCultures) van después
> - Activate/Deactivate van al final

> **Tests de dominio**: Usar `TestableCustomer` para preparar estado previo. Usar `DomainFixture` para resolver comandos y validators. **NO encadenar comandos** para crear estado.
>
> **Tests de slice**: Usar `TestableCustomer` para el estado que devuelve el repository mock. Usar `DomainFixture` para resolver el comando que la slice inyecta. Mock de `IRepository` e `IUnitOfWork`.

---

### 6.1 Customer.Create

#### Event Storming
```
🟡[System] → 🔵(CreateCustomer) → 🟤[[Customer]] → 🟠<CustomerCreated>
                                        │
                              ⚡{PaymentCompleted}
                              🟣{SlugÚnico}
```

#### Input

| Campo | Tipo |
|-------|------|
| Name | string |
| Slug | string |
| Description | string? |
| EstablishmentType | string |
| DefaultCulture | string |
| TimeZoneId | string |
| Address | CreateAddressCommand |
| ContactInfo | CreateContactInfoCommand |
| BillingInfo | CreateBillingInfoCommand |

#### Inyecta
- `Address.Create`
- `ContactInfo.Create`
- `BillingInfo.Create`
- `IValidator<Customer>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Slug ya existe | 409 | ConflictGuard | "Customer with slug '{Slug}' already exists" |

#### Lógica
```csharp
await ConflictGuard.ThrowIfAsync(
    await customerRepository.ExistsBySlugAsync(command.Slug),
    $"Customer with slug '{command.Slug}' already exists");

var address = addressCreate.Execute(command.Address);
var contactInfo = contactInfoCreate.Execute(command.ContactInfo);
var billingInfo = billingInfoCreate.Execute(command.BillingInfo);

var customer = new Customer(Guid.NewGuid())
{
    Name = command.Name,
    Slug = command.Slug,
    Description = command.Description,
    EstablishmentType = command.EstablishmentType,
    DefaultCulture = command.DefaultCulture,
    TimeZoneId = command.TimeZoneId,
    IsActive = false,
    Address = address,
    ContactInfo = contactInfo,
    BillingInfo = billingInfo,
    PriceRange = null,
    LogoUrl = null
};

return customerValidator.ValidateOrThrow(customer);
```

#### Slice: POST /customer
**Request**
```csharp
public record CreateCustomerRequest(
    string Name,
    string Slug,
    string? Description,
    string EstablishmentType,
    string DefaultCulture,
    string TimeZoneId,
    CreateAddressRequest Address,
    CreateContactInfoRequest ContactInfo,
    CreateBillingInfoRequest BillingInfo
);

public record CreateAddressRequest(
    string Street,
    string City,
    string PostalCode,
    string Region,
    string Country,
    decimal Latitude,
    decimal Longitude
);

public record CreateContactInfoRequest(
    string Phone,
    string? Email,
    string? WebsiteUrl
);

public record CreateBillingInfoRequest(
    string BusinessName,
    string TaxId,
    CreateAddressRequest BillingAddress
);
```

**Response**: 201 Created → `CustomerResponse`

#### Tests Unitarios (Dominio)

✅ Crear customere con datos válidos
- Input: Name="El Bar del Juanjo", Slug="el-bar-del-juanjo", EstablishmentType="Bar", DefaultCulture="es-ES", TimeZoneId="Europe/Madrid", Address=válida, ContactInfo=válido, BillingInfo=válido
- Resultado: Customer creado con IsActive=false, PriceRange=null, Images vacío, colecciones vacías

❌ Name vacío
- Input: Name=""
- Resultado: ValidationException "Name is required"

❌ Slug vacío
- Input: Slug=""
- Resultado: ValidationException "Slug is required"

❌ Slug con formato incorrecto
- Input: Slug="El Bar del Juanjo"
- Resultado: ValidationException "Slug must contain only lowercase letters, numbers, and hyphens"

❌ Slug duplicado
- Precondición: Ya existe customere con Slug="el-bar-del-juanjo"
- Resultado: ConflictException "Customer with slug 'el-bar-del-juanjo' already exists"

❌ DefaultCulture formato incorrecto
- Input: DefaultCulture="español"
- Resultado: ValidationException "Default culture must follow format 'xx-XX' (e.g. es-ES)"

❌ TimeZoneId vacío
- Input: TimeZoneId=""
- Resultado: ValidationException "Time zone is required"

❌ Address inválida
- Input: Address con Street=""
- Resultado: ValidationException "Street is required"

❌ Address con coordenadas inválidas
- Input: Address con Latitude=91
- Resultado: ValidationException "Latitude must be between -90 and 90"

❌ BillingInfo inválido
- Input: BillingInfo con TaxId=""
- Resultado: ValidationException "Tax ID is required"

#### Tests Unitarios (Slice)

✅ Verifica slug único antes de crear
- Verifica que customerRepository.ExistsBySlugAsync es llamado con el slug

✅ Llama a Customer.Create con los parámetros correctos
- Verifica que se invoca customerCreate.Execute con el command correcto

✅ Añade el customer al repositorio
- Verifica que repository.Add es llamado con el customer creado

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

✅ Retorna Response mapeado correctamente
- Verifica que el Response contiene los datos del customer

#### Tests Integración

✅ 201 Created → CustomerResponse con IsActive=false

❌ 409 → Slug duplicado

❌ 422 → Validación fallida

---

### 6.2 Customer.Update

#### Event Storming
```
🟡[CustomerOwner] → 🔵(UpdateCustomer) → 🟤[[Customer]] → 🟠<CustomerUpdated>
                                                  │
                                        🟣{SlugÚnico}
```

#### Input

| Campo | Tipo |
|-------|------|
| Name | string |
| Slug | string |
| Description | string? |
| LogoUrl | string? |
| EstablishmentType | string |
| DefaultCulture | string |
| TimeZoneId | string |
| CuisineTypes | string[] |
| ServiceAmenities | string[] |
| DietaryOptions | string[] |

#### Inyecta
- `IValidator<Customer>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Slug ya existe (otro customer) | 409 | ConflictGuard | "Customer with slug '{Slug}' already exists" |

#### Lógica
```csharp
if (customer.Slug != command.Slug)
{
    await ConflictGuard.ThrowIfAsync(
        await customerRepository.ExistsBySlugAsync(command.Slug),
        $"Customer with slug '{command.Slug}' already exists");
}

customer.Name = command.Name;
customer.Slug = command.Slug;
customer.Description = command.Description;
customer.LogoUrl = command.LogoUrl;
customer.EstablishmentType = command.EstablishmentType;
customer.DefaultCulture = command.DefaultCulture;
customer.TimeZoneId = command.TimeZoneId;

customer._cuisineTypes.Clear();
foreach (var cuisine in command.CuisineTypes)
{
    customer._cuisineTypes.Add(cuisine);
}

customer._serviceAmenities.Clear();
foreach (var amenity in command.ServiceAmenities)
{
    customer._serviceAmenities.Add(amenity);
}

customer._dietaryOptions.Clear();
foreach (var option in command.DietaryOptions)
{
    customer._dietaryOptions.Add(option);
}

return customerValidator.ValidateOrThrow(customer);
```

#### Slice: PUT /customer

**Request**
```csharp
public record UpdateCustomerRequest(
    string Name,
    string Slug,
    string? Description,
    string? LogoUrl,
    string EstablishmentType,
    string DefaultCulture,
    string TimeZoneId,
    string[] CuisineTypes,
    string[] ServiceAmenities,
    string[] DietaryOptions
);
```

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

> Estado previo: `TestableCustomer` con propiedades iniciales.

✅ Actualizar customere existente
- Precondición: Customer existe
- Input: Name="El Bar del Juanjo y la María", Description="Nueva descripción", CuisineTypes=["Española", "Mediterránea", "Murciana"]
- Resultado: Customer actualizado

✅ Cambiar slug (único)
- Precondición: Customer con Slug="el-bar-del-juanjo"
- Input: Slug="bar-juanjo-la-maria"
- Resultado: Customer actualizado con nuevo slug

✅ Actualizar colecciones de strings
- Input: CuisineTypes=["Española", "Tapas"], ServiceAmenities=["Terraza", "Pet friendly"], DietaryOptions=["Celíacos", "Vegano"]
- Resultado: Colecciones reemplazadas completamente

❌ Name vacío
- Input: Name=""
- Resultado: ValidationException "Name is required"

❌ Slug duplicado (otro customer)
- Precondición: Otro customer tiene Slug="bar-juanjo-la-maria"
- Resultado: ConflictException "Customer with slug 'bar-juanjo-la-maria' already exists"

#### Tests Unitarios (Slice)

✅ Obtiene el customer del repositorio (tenantId del JWT)
- Verifica que repository.GetByIdAsync es llamado con el tenantId del JWT

✅ Verifica slug único si cambió
- Verifica que customerRepository.ExistsBySlugAsync es llamado solo si el slug cambió

✅ Llama a Customer.Update con los parámetros correctos
- Verifica que se invoca customerUpdate.Execute con el command correcto

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

#### Tests Integración

✅ 204 No Content

❌ 404 → Customer no encontrado

❌ 409 → Slug duplicado

❌ 422 → Validación fallida

---

### 6.3 Customer.UpdateAddress

#### Event Storming
```
🟡[CustomerOwner] → 🔵(UpdateAddress) → 🟤[[Customer]] → 🟠<AddressUpdated>
```

#### Input

| Campo | Tipo |
|-------|------|
| Street | string |
| City | string |
| PostalCode | string |
| Region | string |
| Country | string |
| Latitude | decimal |
| Longitude | decimal |

#### Inyecta
- `Address.Create`
- `IValidator<Customer>`

#### Guards
Ninguno.

#### Lógica
```csharp
var address = addressCreate.Execute(new CreateAddressCommand(
    command.Street,
    command.City,
    command.PostalCode,
    command.Region,
    command.Country,
    command.Latitude,
    command.Longitude));

customer.Address = address;

return customerValidator.ValidateOrThrow(customer);
```

#### Slice: PUT /customer/address

**Request**
```csharp
public record UpdateAddressRequest(
    string Street,
    string City,
    string PostalCode,
    string Region,
    string Country,
    decimal Latitude,
    decimal Longitude
);
```

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

> Estado previo: `TestableCustomer` con Address.

✅ Actualizar dirección
- Input: Street="Ctra. Murcia, 25", City="La Puebla de Mula", Latitude=38.0390, Longitude=-1.4918
- Resultado: Address actualizada con Location.Latitude=38.0390

❌ Street vacío
- Input: Street=""
- Resultado: ValidationException "Street is required"

❌ Coordenadas inválidas
- Input: Latitude=91
- Resultado: ValidationException "Latitude must be between -90 and 90"

#### Tests Unitarios (Slice)

✅ Obtiene el customer del repositorio (tenantId del JWT)
- Verifica que repository.GetByIdAsync es llamado con el tenantId del JWT

✅ Llama a Customer.UpdateAddress con los parámetros correctos
- Verifica que se invoca updateAddress.Execute con el command correcto

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

#### Tests Integración

✅ 204 No Content

❌ 404 → Customer no encontrado

❌ 422 → Validación fallida

---

### 6.4 Customer.UpdateContactInfo

#### Event Storming
```
🟡[CustomerOwner] → 🔵(UpdateContactInfo) → 🟤[[Customer]] → 🟠<ContactInfoUpdated>
```

#### Input

| Campo | Tipo |
|-------|------|
| Phone | string |
| Email | string? |
| WebsiteUrl | string? |

#### Inyecta
- `ContactInfo.Create`
- `IValidator<Customer>`

#### Guards
Ninguno.

#### Lógica
```csharp
var contactInfo = contactInfoCreate.Execute(new CreateContactInfoCommand(
    command.Phone,
    command.Email,
    command.WebsiteUrl));

customer.ContactInfo = contactInfo;

return customerValidator.ValidateOrThrow(customer);
```

#### Slice: PUT /customer/contact-info

**Request**
```csharp
public record UpdateContactInfoRequest(
    string Phone,
    string? Email,
    string? WebsiteUrl
);
```

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

> Estado previo: `TestableCustomer` con ContactInfo.

✅ Actualizar contacto completo
- Input: Phone="639079481", Email="juanjo@bar.com", WebsiteUrl="https://elbardeljuanjo.com"
- Resultado: ContactInfo actualizado

✅ Actualizar solo teléfono
- Input: Phone="639079482", Email=null, WebsiteUrl=null
- Resultado: ContactInfo actualizado

❌ Phone vacío
- Input: Phone=""
- Resultado: ValidationException "Phone is required"

#### Tests Unitarios (Slice)

✅ Obtiene el customer del repositorio (tenantId del JWT)
- Verifica que repository.GetByIdAsync es llamado con el tenantId del JWT

✅ Llama a Customer.UpdateContactInfo con los parámetros correctos
- Verifica que se invoca updateContactInfo.Execute con el command correcto

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

#### Tests Integración

✅ 204 No Content

❌ 404 → Customer no encontrado

❌ 422 → Validación fallida

---

### 6.5 Customer.UpdateBillingInfo

#### Event Storming
```
🟡[CustomerOwner] → 🔵(UpdateBillingInfo) → 🟤[[Customer]] → 🟠<BillingInfoUpdated>
```

#### Input

| Campo | Tipo |
|-------|------|
| BusinessName | string |
| TaxId | string |
| BillingAddress | CreateAddressCommand |

#### Inyecta
- `BillingInfo.Create`
- `IValidator<Customer>`

#### Guards
Ninguno.

#### Lógica
```csharp
var billingInfo = billingInfoCreate.Execute(new CreateBillingInfoCommand(
    command.BusinessName,
    command.TaxId,
    command.BillingAddress));

customer.BillingInfo = billingInfo;

return customerValidator.ValidateOrThrow(customer);
```

#### Slice: PUT /customer/billing-info

**Request**
```csharp
public record UpdateBillingInfoRequest(
    string BusinessName,
    string TaxId,
    CreateAddressRequest BillingAddress
);
```

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

> Estado previo: `TestableCustomer` con BillingInfo.

✅ Actualizar datos fiscales
- Input: BusinessName="Juanjo y María SL", TaxId="B87654321", BillingAddress=válida
- Resultado: BillingInfo actualizado

✅ Cambiar dirección fiscal manteniendo datos fiscales
- Input: BusinessName="Bar Juanjo SL", TaxId="B12345678", BillingAddress={Street="C/ Nueva, 5", City="Murcia"...}
- Resultado: BillingInfo actualizado con nueva dirección

❌ BusinessName vacío
- Input: BusinessName=""
- Resultado: ValidationException "Business name is required"

❌ TaxId vacío
- Input: TaxId=""
- Resultado: ValidationException "Tax ID is required"

#### Tests Unitarios (Slice)

✅ Obtiene el customer del repositorio (tenantId del JWT)
- Verifica que repository.GetByIdAsync es llamado con el tenantId del JWT

✅ Llama a Customer.UpdateBillingInfo con los parámetros correctos
- Verifica que se invoca updateBillingInfo.Execute con el command correcto

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

#### Tests Integración

✅ 204 No Content

❌ 404 → Customer no encontrado

❌ 422 → Validación fallida

---

### 6.6 Customer.SetPriceRange

#### Event Storming
```
🟡[CustomerOwner] → 🔵(SetPriceRange) → 🟤[[Customer]] → 🟠<PriceRangeConfigured>
```

#### Input

| Campo | Tipo |
|-------|------|
| MinPrice | decimal |
| MaxPrice | decimal |

#### Inyecta
- `PriceRange.Create`
- `IValidator<Customer>`

#### Guards
Ninguno.

#### Lógica
```csharp
var priceRange = priceRangeCreate.Execute(new CreatePriceRangeCommand(
    command.MinPrice,
    command.MaxPrice));

customer.PriceRange = priceRange;

return customerValidator.ValidateOrThrow(customer);
```

#### Slice: PUT /customer/price-range

**Request**
```csharp
public record SetPriceRangeRequest(
    decimal MinPrice,
    decimal MaxPrice
);
```

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

> Estado previo: `TestableCustomer`.

✅ Configurar rango de precios
- Input: MinPrice=10, MaxPrice=30
- Resultado: PriceRange configurado, HasPriceRange=true

❌ MinPrice negativo
- Input: MinPrice=-5
- Resultado: ValidationException "Minimum price cannot be negative"

❌ MaxPrice menor que MinPrice
- Input: MinPrice=30, MaxPrice=10
- Resultado: ValidationException "Maximum price must be greater than or equal to minimum price"

#### Tests Integración

✅ 204 No Content

❌ 404 → Customer no encontrado

❌ 422 → Validación fallida

---

### 6.7 Customer.RemovePriceRange

#### Event Storming
```
🟡[CustomerOwner] → 🔵(RemovePriceRange) → 🟤[[Customer]] → 🟠<PriceRangeRemoved>
```

#### Input
Ninguno

#### Inyecta
- `IValidator<Customer>`

#### Guards
Ninguno.

#### Lógica
```csharp
customer.PriceRange = null;

return customerValidator.ValidateOrThrow(customer);
```

#### Slice: DELETE /customer/price-range

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

> Estado previo: `TestableCustomer` con/sin PriceRange.

✅ Eliminar rango de precios existente
- Precondición: Customer con PriceRange configurado
- Resultado: PriceRange=null, HasPriceRange=false

✅ Eliminar rango de precios inexistente (idempotente)
- Precondición: Customer sin PriceRange
- Resultado: Sin cambios

#### Tests Integración

✅ 204 No Content

❌ 404 → Customer no encontrado

---

### 6.8 Customer.AddImage

#### Event Storming
```
🟡[CustomerOwner] → 🔵(AddImage) → 🟤[[Customer]] → 🟠<ImageAdded>
                                           │
                                 🟣{UrlÚnica}
                                 🟣{SoloCoverÚnico}
```

#### Input

| Campo | Tipo | Default |
|-------|------|---------|
| Url | string | |
| AltText | string? | null |
| DisplayOrder | int | 0 |
| IsCover | bool | false |

#### Inyecta
- `CustomerImage.Create`
- `IValidator<Customer>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Url ya existe | 409 | ConflictGuard | "Image with this URL already exists" |

#### Lógica
```csharp
ConflictGuard.ThrowIf(
    customer.Images.Any(i => i.Url == command.Url),
    "Image with this URL already exists");

// Si la nueva imagen es cover, quitar cover de las existentes
if (command.IsCover)
{
    var currentCover = customer._images.FirstOrDefault(i => i.IsCover);
    if (currentCover != null)
    {
        var demoted = customerImageCreate.Execute(new CreateCustomerImageCommand(
            currentCover.Url,
            currentCover.AltText,
            currentCover.DisplayOrder,
            false));
        // Preservar el Id original
        demoted = demoted with { Id = currentCover.Id };
        customer._images.Remove(currentCover);
        customer._images.Add(demoted);
    }
}

var image = customerImageCreate.Execute(new CreateCustomerImageCommand(
    command.Url,
    command.AltText,
    command.DisplayOrder,
    command.IsCover));

customer._images.Add(image);

return customerValidator.ValidateOrThrow(customer);
```

#### Slice: POST /customer/images

**Request**
```csharp
public record AddImageRequest(
    string Url,
    string? AltText,
    int DisplayOrder = 0,
    bool IsCover = false
);
```

**Response**: 201 Created → `CustomerResponse`

#### Tests Unitarios (Dominio)

> Estado previo: `TestableCustomer` con/sin Images.

✅ Añadir primera imagen
- Precondición: Customer sin imágenes
- Input: Url="https://cdn.fudie.com/images/fachada.jpg", IsCover=true
- Resultado: Imagen añadida, HasImages=true, CoverImage=la imagen añadida

✅ Añadir imagen adicional sin cover
- Precondición: Customer con 1 imagen cover
- Input: Url="https://cdn.fudie.com/images/interior.jpg", IsCover=false
- Resultado: 2 imágenes, cover no cambia

✅ Añadir imagen como cover (desplaza cover anterior)
- Precondición: Customer con imagen A como cover
- Input: Url="https://cdn.fudie.com/images/nueva-fachada.jpg", IsCover=true
- Resultado: Nueva imagen es cover, imagen A ya no es cover

❌ URL duplicada
- Precondición: Customer ya tiene imagen con misma URL
- Resultado: ConflictException "Image with this URL already exists"

❌ URL inválida
- Input: Url="not-a-url"
- Resultado: ValidationException "Image URL must be a valid URL"

#### Tests Unitarios (Slice)

✅ Obtiene el customer del repositorio (tenantId del JWT)
- Verifica que repository.GetByIdAsync es llamado con el tenantId del JWT

✅ Llama a Customer.AddImage con los parámetros correctos
- Verifica que se invoca addImage.Execute con el command correcto

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

✅ Retorna Response mapeado correctamente
- Verifica que el Response contiene la imagen añadida

#### Tests Integración

✅ 201 Created → CustomerResponse con imagen añadida

❌ 404 → Customer no encontrado

❌ 409 → URL duplicada

❌ 422 → Validación fallida

---

### 6.9 Customer.UpdateImage

#### Event Storming
```
🟡[CustomerOwner] → 🔵(UpdateImage) → 🟤[[Customer]] → 🟠<ImageUpdated>
                                              │
                                    🟣{ImageExiste}
                                    🟣{SoloCoverÚnico}
```

#### Input

| Campo | Tipo |
|-------|------|
| AltText | string? |
| DisplayOrder | int |
| IsCover | bool |

*ImageId viene en la ruta*

#### Inyecta
- `CustomerImage.Create`
- `IValidator<Customer>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Imagen no existe | 404 | NotFoundGuard | "Image not found" |

#### Lógica
```csharp
var existing = customer.Images.FirstOrDefault(i => i.Id == imageId);
NotFoundGuard.ThrowIfNull(existing, "Image not found");

// Si esta imagen se marca como cover, quitar cover de las demás
if (command.IsCover && !existing.IsCover)
{
    var currentCover = customer._images.FirstOrDefault(i => i.IsCover);
    if (currentCover != null)
    {
        var demoted = customerImageCreate.Execute(new CreateCustomerImageCommand(
            currentCover.Url,
            currentCover.AltText,
            currentCover.DisplayOrder,
            false));
        demoted = demoted with { Id = currentCover.Id };
        customer._images.Remove(currentCover);
        customer._images.Add(demoted);
    }
}

var updated = customerImageCreate.Execute(new CreateCustomerImageCommand(
    existing.Url,
    command.AltText,
    command.DisplayOrder,
    command.IsCover));
updated = updated with { Id = existing.Id };

customer._images.Remove(existing);
customer._images.Add(updated);

return customerValidator.ValidateOrThrow(customer);
```

#### Slice: PUT /customer/images/{imageId}

**Request**
```csharp
public record UpdateImageRequest(
    string? AltText,
    int DisplayOrder,
    bool IsCover
);
```

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

> Estado previo: `TestableCustomer` con Images.

✅ Actualizar alt text y orden
- Precondición: Customer tiene imagen con Id=X
- Input: AltText="Fachada renovada", DisplayOrder=2, IsCover=false
- Resultado: Imagen actualizada, URL se mantiene

✅ Promover imagen a cover
- Precondición: Customer con imagen A (cover) e imagen B
- Input: ImageId de B, IsCover=true
- Resultado: B es cover, A ya no es cover

❌ Imagen no existe
- Precondición: Customer sin imagen con Id=X
- Resultado: NotFoundException "Image not found"

#### Tests Unitarios (Slice)

✅ Obtiene el customer del repositorio (tenantId del JWT)
- Verifica que repository.GetByIdAsync es llamado con el tenantId del JWT

✅ Llama a Customer.UpdateImage con los parámetros correctos
- Verifica que se invoca updateImage.Execute con el imageId y command correctos

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

#### Tests Integración

✅ 204 No Content

❌ 404 → Customer o Imagen no encontrada

❌ 422 → Validación fallida

---

### 6.10 Customer.RemoveImage

#### Event Storming
```
🟡[CustomerOwner] → 🔵(RemoveImage) → 🟤[[Customer]] → 🟠<ImageRemoved>
                                              │
                                    🟣{ImageExiste}
```

#### Input
*ImageId viene en la ruta*

#### Inyecta
- `IValidator<Customer>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Imagen no existe | 404 | NotFoundGuard | "Image not found" |

#### Lógica
```csharp
var existing = customer.Images.FirstOrDefault(i => i.Id == imageId);
NotFoundGuard.ThrowIfNull(existing, "Image not found");

customer._images.Remove(existing);

return customerValidator.ValidateOrThrow(customer);
```

#### Slice: DELETE /customer/images/{imageId}

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

> Estado previo: `TestableCustomer` con Images.

✅ Eliminar imagen (hay varias)
- Precondición: Customer con 3 imágenes
- Resultado: Imagen eliminada, quedan 2

✅ Eliminar última imagen
- Precondición: Customer con 1 imagen
- Resultado: Imagen eliminada, HasImages=false

✅ Eliminar imagen cover (siguiente asume cover por DisplayOrder)
- Precondición: Customer con imagen A (cover) e imagen B
- Acción: Eliminar imagen A
- Resultado: CoverImage retorna imagen B (por DisplayOrder)

❌ Imagen no existe
- Resultado: NotFoundException "Image not found"

#### Tests Unitarios (Slice)

✅ Obtiene el customer del repositorio (tenantId del JWT)
- Verifica que repository.GetByIdAsync es llamado con el tenantId del JWT

✅ Llama a Customer.RemoveImage con el imageId correcto
- Verifica que se invoca removeImage.Execute con el imageId

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

#### Tests Integración

✅ 204 No Content

❌ 404 → Customer o Imagen no encontrada

---

### 6.11 Customer.AddSocialLink

#### Event Storming
```
🟡[CustomerOwner] → 🔵(AddSocialLink) → 🟤[[Customer]] → 🟠<SocialLinkAdded>
                                               │
                                     🟣{PlatformÚnica}
```

#### Input

| Campo | Tipo |
|-------|------|
| Platform | string |
| Url | string |

#### Inyecta
- `SocialLink.Create`
- `IValidator<Customer>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Platform ya existe | 409 | ConflictGuard | "Social link for '{Platform}' already exists" |

#### Lógica
```csharp
ConflictGuard.ThrowIf(
    customer.SocialLinks.Any(s => s.Platform.Equals(command.Platform, StringComparison.OrdinalIgnoreCase)),
    $"Social link for '{command.Platform}' already exists");

var socialLink = socialLinkCreate.Execute(new CreateSocialLinkCommand(
    command.Platform,
    command.Url));

customer._socialLinks.Add(socialLink);

return customerValidator.ValidateOrThrow(customer);
```

#### Slice: POST /customer/social-links

**Request**
```csharp
public record AddSocialLinkRequest(
    string Platform,
    string Url
);
```

**Response**: 201 Created → `CustomerResponse`

#### Tests Unitarios (Dominio)

> Estado previo: `TestableCustomer` con/sin SocialLinks.

✅ Añadir enlace Facebook
- Precondición: Customer sin SocialLink de Facebook
- Input: Platform="Facebook", Url="https://facebook.com/elbardeljuanjo"
- Resultado: SocialLink añadido

✅ Añadir enlace Instagram
- Input: Platform="Instagram", Url="https://instagram.com/elbardeljuanjo"
- Resultado: SocialLink añadido

❌ Platform duplicada
- Precondición: Customer ya tiene SocialLink de Facebook
- Input: Platform="Facebook"
- Resultado: ConflictException "Social link for 'Facebook' already exists"

❌ Platform duplicada (case-insensitive)
- Precondición: Customer ya tiene SocialLink de "Facebook"
- Input: Platform="facebook"
- Resultado: ConflictException "Social link for 'facebook' already exists"

#### Tests Unitarios (Slice)

✅ Obtiene el customer del repositorio (tenantId del JWT)
- Verifica que repository.GetByIdAsync es llamado con el tenantId del JWT

✅ Llama a Customer.AddSocialLink con los parámetros correctos
- Verifica que se invoca addSocialLink.Execute con el command correcto

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

✅ Retorna Response mapeado correctamente
- Verifica que el Response contiene el SocialLink añadido

#### Tests Integración

✅ 201 Created → CustomerResponse con SocialLink añadido

❌ 404 → Customer no encontrado

❌ 409 → Platform duplicada

❌ 422 → Validación fallida

---

### 6.12 Customer.UpdateSocialLink

#### Event Storming
```
🟡[CustomerOwner] → 🔵(UpdateSocialLink) → 🟤[[Customer]] → 🟠<SocialLinkUpdated>
                                                  │
                                        🟣{SocialLinkExiste}
```

#### Input

| Campo | Tipo |
|-------|------|
| Url | string |

*Platform viene en la ruta*

#### Inyecta
- `SocialLink.Create`
- `IValidator<Customer>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| SocialLink no existe | 404 | NotFoundGuard | "Social link for '{Platform}' not found" |

#### Lógica
```csharp
var existing = customer.SocialLinks.FirstOrDefault(s => s.Platform.Equals(platform, StringComparison.OrdinalIgnoreCase));
NotFoundGuard.ThrowIfNull(existing, $"Social link for '{platform}' not found");

var updated = socialLinkCreate.Execute(new CreateSocialLinkCommand(
    existing.Platform,
    command.Url));

customer._socialLinks.Remove(existing);
customer._socialLinks.Add(updated);

return customerValidator.ValidateOrThrow(customer);
```

#### Slice: PUT /customer/social-links/{platform}

**Request**
```csharp
public record UpdateSocialLinkRequest(
    string Url
);
```

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

> Estado previo: `TestableCustomer` con SocialLinks.

✅ Actualizar URL de Facebook
- Precondición: Customer con SocialLink de Facebook
- Input: Url="https://facebook.com/bardeljuanjo-nuevo"
- Resultado: SocialLink actualizado, Platform se mantiene

❌ SocialLink no existe
- Precondición: Customer sin SocialLink de TripAdvisor
- Resultado: NotFoundException "Social link for 'TripAdvisor' not found"

#### Tests Unitarios (Slice)

✅ Obtiene el customer del repositorio (tenantId del JWT)
- Verifica que repository.GetByIdAsync es llamado con el tenantId del JWT

✅ Llama a Customer.UpdateSocialLink con los parámetros correctos
- Verifica que se invoca updateSocialLink.Execute con el platform y command correctos

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

#### Tests Integración

✅ 204 No Content

❌ 404 → Customer o SocialLink no encontrado

❌ 422 → Validación fallida

---

### 6.13 Customer.RemoveSocialLink

#### Event Storming
```
🟡[CustomerOwner] → 🔵(RemoveSocialLink) → 🟤[[Customer]] → 🟠<SocialLinkRemoved>
                                                  │
                                        🟣{SocialLinkExiste}
```

#### Input
*Platform viene en la ruta*

#### Inyecta
- `IValidator<Customer>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| SocialLink no existe | 404 | NotFoundGuard | "Social link for '{Platform}' not found" |

#### Lógica
```csharp
var existing = customer.SocialLinks.FirstOrDefault(s => s.Platform.Equals(platform, StringComparison.OrdinalIgnoreCase));
NotFoundGuard.ThrowIfNull(existing, $"Social link for '{platform}' not found");

customer._socialLinks.Remove(existing);

return customerValidator.ValidateOrThrow(customer);
```

#### Slice: DELETE /customer/social-links/{platform}

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

> Estado previo: `TestableCustomer` con SocialLinks.

✅ Eliminar SocialLink existente
- Precondición: Customer con SocialLink de Facebook
- Resultado: SocialLink eliminado

❌ SocialLink no existe
- Resultado: NotFoundException "Social link for 'TripAdvisor' not found"

#### Tests Unitarios (Slice)

✅ Obtiene el customer del repositorio (tenantId del JWT)
- Verifica que repository.GetByIdAsync es llamado con el tenantId del JWT

✅ Llama a Customer.RemoveSocialLink con el platform correcto
- Verifica que se invoca removeSocialLink.Execute con el platform

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

#### Tests Integración

✅ 204 No Content

❌ 404 → Customer o SocialLink no encontrado

---

### 6.14 Customer.AddSupportedCulture

#### Event Storming
```
🟡[CustomerOwner] → 🔵(AddSupportedCulture) → 🟤[[Customer]] → 🟠<SupportedCultureAdded>
                                                      │
                                            🟣{CultureÚnica}
```

#### Input

| Campo | Tipo |
|-------|------|
| Code | string |

#### Inyecta
- `CultureCode.Create`
- `IValidator<Customer>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Culture ya existe | 409 | ConflictGuard | "Culture '{Code}' is already supported" |
| Culture es la DefaultCulture | 409 | ConflictGuard | "Culture '{Code}' is already the default culture" |

#### Lógica
```csharp
ConflictGuard.ThrowIf(
    customer.DefaultCulture == command.Code,
    $"Culture '{command.Code}' is already the default culture");

ConflictGuard.ThrowIf(
    customer.SupportedCultures.Any(c => c.Code == command.Code),
    $"Culture '{command.Code}' is already supported");

var cultureCode = cultureCodeCreate.Execute(new CreateCultureCodeCommand(command.Code));

customer._supportedCultures.Add(cultureCode);

return customerValidator.ValidateOrThrow(customer);
```

#### Slice: POST /customer/supported-cultures

**Request**
```csharp
public record AddSupportedCultureRequest(
    string Code
);
```

**Response**: 201 Created → `CustomerResponse`

#### Tests Unitarios (Dominio)

> Estado previo: `TestableCustomer` con DefaultCulture.

✅ Añadir cultura soportada
- Precondición: Customer con DefaultCulture="es-ES", sin culturas adicionales
- Input: Code="en-GB"
- Resultado: CultureCode añadido

✅ Añadir múltiples culturas
- Input: Code="fr-FR" (después de añadir en-GB)
- Resultado: Customer con 2 culturas soportadas

❌ Culture duplicada
- Precondición: Customer ya soporta "en-GB"
- Input: Code="en-GB"
- Resultado: ConflictException "Culture 'en-GB' is already supported"

❌ Culture es la default
- Precondición: Customer con DefaultCulture="es-ES"
- Input: Code="es-ES"
- Resultado: ConflictException "Culture 'es-ES' is already the default culture"

❌ Formato incorrecto
- Input: Code="english"
- Resultado: ValidationException "Culture code must follow format 'xx-XX' (e.g. es-ES)"

#### Tests Unitarios (Slice)

✅ Obtiene el customer del repositorio (tenantId del JWT)
- Verifica que repository.GetByIdAsync es llamado con el tenantId del JWT

✅ Llama a Customer.AddSupportedCulture con los parámetros correctos
- Verifica que se invoca addSupportedCulture.Execute con el command correcto

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

✅ Retorna Response mapeado correctamente
- Verifica que el Response contiene la CultureCode añadida

#### Tests Integración

✅ 201 Created → CustomerResponse con CultureCode añadida

❌ 404 → Customer no encontrado

❌ 409 → Culture duplicada o es default

❌ 422 → Validación fallida

---

### 6.15 Customer.RemoveSupportedCulture

#### Event Storming
```
🟡[CustomerOwner] → 🔵(RemoveSupportedCulture) → 🟤[[Customer]] → 🟠<SupportedCultureRemoved>
                                                        │
                                              🟣{CultureExiste}
```

#### Input
*Code viene en la ruta*

#### Inyecta
- `IValidator<Customer>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Culture no existe | 404 | NotFoundGuard | "Culture '{Code}' not found" |

#### Lógica
```csharp
var existing = customer.SupportedCultures.FirstOrDefault(c => c.Code == code);
NotFoundGuard.ThrowIfNull(existing, $"Culture '{code}' not found");

customer._supportedCultures.Remove(existing);

return customerValidator.ValidateOrThrow(customer);
```

#### Slice: DELETE /customer/supported-cultures/{code}

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

> Estado previo: `TestableCustomer` con SupportedCultures.

✅ Eliminar cultura soportada
- Precondición: Customer soporta "en-GB"
- Input: Code="en-GB"
- Resultado: CultureCode eliminada

❌ Culture no existe
- Precondición: Customer no soporta "fr-FR"
- Resultado: NotFoundException "Culture 'fr-FR' not found"

#### Tests Unitarios (Slice)

✅ Obtiene el customer del repositorio (tenantId del JWT)
- Verifica que repository.GetByIdAsync es llamado con el tenantId del JWT

✅ Llama a Customer.RemoveSupportedCulture con el code correcto
- Verifica que se invoca removeSupportedCulture.Execute con el code

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

#### Tests Integración

✅ 204 No Content

❌ 404 → Customer o Culture no encontrada

---

### 6.16 Customer.Activate

#### Event Storming
```
🟡[System] → 🔵(ActivateCustomer) → 🟤[[Customer]] → 🟠<CustomerActivated>
                                           │
                                 🟣{PerfilCompleto}
```

#### Input
Ninguno

#### Inyecta
- `IValidator<Customer>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Ya está activo | 409 | ConflictGuard | "Customer is already active" |
| Perfil incompleto | 422 | ValidationGuard | "Customer profile must be complete before activation" |

#### Lógica
```csharp
ConflictGuard.ThrowIf(customer.IsActive, "Customer is already active");
ValidationGuard.ThrowIf(!customer.IsProfileComplete, "Customer profile must be complete before activation", nameof(customer.IsProfileComplete));

customer.IsActive = true;

return customerValidator.ValidateOrThrow(customer);
```

#### Slice: POST /customer/activate

**Response**: 200 OK → `CustomerResponse`

#### Tests Unitarios (Dominio)

> Estado previo: `TestableCustomer` con perfil completo/incompleto.

✅ Activar customer con perfil completo
- Precondición: Customer con Logo, Images (al menos 1), Description y CuisineTypes, IsActive=false
- Resultado: Customer con IsActive=true

❌ Customer ya activo
- Precondición: Customer con IsActive=true
- Resultado: ConflictException "Customer is already active"

❌ Perfil incompleto (sin logo)
- Precondición: Customer sin LogoUrl
- Resultado: ValidationException "Customer profile must be complete before activation"

❌ Perfil incompleto (sin imágenes)
- Precondición: Customer sin Images
- Resultado: ValidationException "Customer profile must be complete before activation"

❌ Perfil incompleto (sin cuisine types)
- Precondición: Customer sin CuisineTypes
- Resultado: ValidationException "Customer profile must be complete before activation"

#### Tests Unitarios (Slice)

✅ Obtiene el customer del repositorio (tenantId del JWT)
- Verifica que repository.GetByIdAsync es llamado con el tenantId del JWT

✅ Llama a Customer.Activate
- Verifica que se invoca customerActivate.Execute

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

✅ Retorna Response mapeado correctamente
- Verifica que el Response contiene IsActive=true

#### Tests Integración

✅ 200 OK → CustomerResponse con IsActive=true

❌ 404 → Customer no encontrado

❌ 409 → Ya estaba activo

❌ 422 → Perfil incompleto

---

### 6.17 Customer.Deactivate

#### Event Storming
```
🟡[System] → 🔵(DeactivateCustomer) → 🟤[[Customer]] → 🟠<CustomerDeactivated>
```

#### Input
Ninguno

#### Inyecta
- `IValidator<Customer>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Ya está inactivo | 409 | ConflictGuard | "Customer is already inactive" |

#### Lógica
```csharp
ConflictGuard.ThrowIf(!customer.IsActive, "Customer is already inactive");

customer.IsActive = false;

return customerValidator.ValidateOrThrow(customer);
```

#### Slice: POST /customer/deactivate

**Response**: 200 OK → `CustomerResponse`

#### Tests Unitarios (Dominio)

> Estado previo: `TestableCustomer` con IsActive=true/false.

✅ Desactivar customer activo
- Precondición: Customer con IsActive=true
- Resultado: Customer con IsActive=false

❌ Customer ya inactivo
- Precondición: Customer con IsActive=false
- Resultado: ConflictException "Customer is already inactive"

#### Tests Unitarios (Slice)

✅ Obtiene el customer del repositorio (tenantId del JWT)
- Verifica que repository.GetByIdAsync es llamado con el tenantId del JWT

✅ Llama a Customer.Deactivate
- Verifica que se invoca customerDeactivate.Execute

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

✅ Retorna Response mapeado correctamente
- Verifica que el Response contiene IsActive=false

#### Tests Integración

✅ 200 OK → CustomerResponse con IsActive=false

❌ 404 → Customer no encontrado

❌ 409 → Ya estaba inactivo

---

## 7. Queries

### GetCustomer

**Slice**: GET /customer

> El tenantId se obtiene del JWT. Devuelve el customer del tenant autenticado.

**Response**: 200 OK → `CustomerResponse`

#### Tests Unitarios (Slice)

✅ Obtiene el customer del repositorio (tenantId del JWT)
- Verifica que repository.GetByIdAsync es llamado con el tenantId del JWT

✅ Retorna Response mapeado correctamente
- Verifica que el Response contiene los datos del customer

#### Tests Integración

✅ 200 OK → CustomerResponse

❌ 404 → No encontrado

---

## 8. Resumen de Endpoints

> **Convención de seguridad**: Todos los endpoints usan `/customer` (singular) sin `{id}` en la URL. El tenantId se obtiene exclusivamente del JWT interno. Los sub-recursos mantienen sus identificadores propios en la ruta ({imageId}, {platform}, {code}).

| # | Método | Ruta | Comando/Query | Response |
|---|--------|------|---------------|----------|
| 1 | POST | /customer | Customer.Create | 201 → `CustomerResponse` |
| 2 | GET | /customer | GetCustomer | 200 → `CustomerResponse` |
| 3 | PUT | /customer | Customer.Update | 204 |
| 4 | PUT | /customer/address | Customer.UpdateAddress | 204 |
| 5 | PUT | /customer/contact-info | Customer.UpdateContactInfo | 204 |
| 6 | PUT | /customer/billing-info | Customer.UpdateBillingInfo | 204 |
| 7 | PUT | /customer/price-range | Customer.SetPriceRange | 204 |
| 8 | DELETE | /customer/price-range | Customer.RemovePriceRange | 204 |
| 9 | POST | /customer/images | Customer.AddImage | 201 → `CustomerResponse` |
| 10 | PUT | /customer/images/{imageId} | Customer.UpdateImage | 204 |
| 11 | DELETE | /customer/images/{imageId} | Customer.RemoveImage | 204 |
| 12 | POST | /customer/social-links | Customer.AddSocialLink | 201 → `CustomerResponse` |
| 13 | PUT | /customer/social-links/{platform} | Customer.UpdateSocialLink | 204 |
| 14 | DELETE | /customer/social-links/{platform} | Customer.RemoveSocialLink | 204 |
| 15 | POST | /customer/supported-cultures | Customer.AddSupportedCulture | 201 → `CustomerResponse` |
| 16 | DELETE | /customer/supported-cultures/{code} | Customer.RemoveSupportedCulture | 204 |
| 17 | POST | /customer/activate | Customer.Activate | 200 → `CustomerResponse` |
| 18 | POST | /customer/deactivate | Customer.Deactivate | 200 → `CustomerResponse` |

---

## 9. Persistencia (Firestore)

### Colección

`/customers/{customerId}`

### Configuración DbContext

```csharp
modelBuilder.Entity<CustomerAgg>(entity =>
{
    // Ignore: propiedades computed (no backing fields)
    entity.Ignore(r => r.HasPriceRange);
    entity.Ignore(r => r.HasLogo);
    entity.Ignore(r => r.HasImages);
    entity.Ignore(r => r.CoverImage);
    entity.Ignore(r => r.IsProfileComplete);

    // ComplexType: Address (con GeoPoint anidado)
    entity.ComplexProperty(r => r.Address, address =>
    {
        address.Ignore(a => a.FullAddress);

        // GeoPoint: se almacena como GeoPoint nativo de Firestore
        address.ComplexProperty(a => a.Location);
    });

    // ComplexType: ContactInfo
    entity.ComplexProperty(r => r.ContactInfo);

    // ComplexType: BillingInfo (con Address anidado que contiene GeoPoint)
    entity.ComplexProperty(r => r.BillingInfo, billing =>
    {
        billing.ComplexProperty(b => b.BillingAddress, billingAddress =>
        {
            billingAddress.Ignore(a => a.FullAddress);
            billingAddress.ComplexProperty(a => a.Location);
        });
    });

    // ComplexType: PriceRange (nullable)
    entity.ComplexProperty(r => r.PriceRange);

    // ArrayOf: Images (usa backing field _images)
    entity.ArrayOf(r => r.Images, image =>
    {
        // No computed properties que ignorar
    });

    // ArrayOf: SupportedCultures (usa backing field _supportedCultures)
    entity.ArrayOf(r => r.SupportedCultures);

    // ArrayOf: SocialLinks (usa backing field _socialLinks)
    entity.ArrayOf(r => r.SocialLinks);

    // ArrayOf: string collections (usa backing fields)
    entity.ArrayOf(r => r.CuisineTypes);
    entity.ArrayOf(r => r.ServiceAmenities);
    entity.ArrayOf(r => r.DietaryOptions);
});
```

### Documento Ejemplo

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "El Bar del Juanjo",
  "slug": "el-bar-del-juanjo",
  "description": "Bar escondido en un pequeño rincón de la cuenca del Mula donde encontramos tapas típicas y elaboraciones con toques creativos.",
  "logoUrl": "https://cdn.fudie.com/customers/el-bar-del-juanjo/logo.jpg",
  "establishmentType": "Bar",
  "defaultCulture": "es-ES",
  "timeZoneId": "Europe/Madrid",
  "isActive": true,
  "address": {
    "street": "Ctra. Murcia, 23",
    "city": "La Puebla de Mula",
    "postalCode": "30193",
    "region": "Murcia",
    "country": "España",
    "location": {
      "latitude": 38.0389,
      "longitude": -1.4917
    }
  },
  "contactInfo": {
    "phone": "639079481",
    "email": null,
    "websiteUrl": "https://facebook.com/elbardeljuanjo"
  },
  "billingInfo": {
    "businessName": "Bar Juanjo SL",
    "taxId": "B12345678",
    "billingAddress": {
      "street": "Ctra. Murcia, 23",
      "city": "La Puebla de Mula",
      "postalCode": "30193",
      "region": "Murcia",
      "country": "España",
      "location": {
        "latitude": 38.0389,
        "longitude": -1.4917
      }
    }
  },
  "priceRange": {
    "minPrice": 10,
    "maxPrice": 30
  },
  "images": [
    {
      "id": "img-001-guid",
      "url": "https://cdn.fudie.com/customers/el-bar-del-juanjo/fachada.jpg",
      "altText": "Fachada del Bar del Juanjo",
      "displayOrder": 0,
      "isCover": true
    },
    {
      "id": "img-002-guid",
      "url": "https://cdn.fudie.com/customers/el-bar-del-juanjo/interior.jpg",
      "altText": "Interior del customere",
      "displayOrder": 1,
      "isCover": false
    },
    {
      "id": "img-003-guid",
      "url": "https://cdn.fudie.com/customers/el-bar-del-juanjo/terraza.jpg",
      "altText": "Terraza exterior",
      "displayOrder": 2,
      "isCover": false
    }
  ],
  "cuisineTypes": ["Española", "Mediterránea", "Murciana"],
  "serviceAmenities": ["Terraza", "Acepta mascotas", "Accesible", "Niños bienvenidos", "Tronas", "Cambiador bebés"],
  "dietaryOptions": ["Opciones para celíacos", "Opciones veganas"],
  "supportedCultures": [
    { "code": "en-GB" }
  ],
  "socialLinks": [
    { "platform": "Facebook", "url": "https://facebook.com/elbardeljuanjo" },
    { "platform": "TripAdvisor", "url": "https://tripadvisor.es/Customer_Review-g1087583-d8864861" },
    { "platform": "Google", "url": "https://maps.google.com/?cid=..." }
  ]
}
```

---

## 10. Hot Spots ⚠️

| # | Pregunta | Estado |
|---|----------|--------|
| 1 | ¿El Slug se genera automáticamente desde el Name o lo define el admin? | Pendiente |
| 2 | ¿Cómo se gestiona el flujo de creación desde el pago? ¿Es el payment provider quien dispara Customer.Create? | Pendiente: Depende de la integración con Stripe |
| 3 | ¿Se necesita validar que TimeZoneId sea un IANA timezone válido? | Pendiente |
| 4 | ¿Awards/Reconocimientos (Solete Repsol, Michelin) se añaden en V2? | Pendiente: Decidido aparcar para V2 |
| 5 | ¿Las traducciones de menús se disparan automáticamente al añadir una SupportedCulture? | Pendiente: Depende de la integración con el servicio de traducción IA |
| 6 | ¿Customer necesita referencia al Plan/Subscription activo? | Decidido: Se aborda en otra etapa |
| 7 | ¿Se puede eliminar un Customer o solo desactivar? | Pendiente |
| 8 | ¿Se necesita límite máximo de imágenes por customere? | Pendiente |

---

**Fecha**: 2026-02-06
**Autor**: Equipo Fudie
