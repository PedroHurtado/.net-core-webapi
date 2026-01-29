# Domain Specification: MenuItem

---

## 1. Enums

### PortionType

```csharp
public enum PortionType
{
    Small = 1,        // Porción pequeña (tapa), ~25% de ración
    Half = 2,         // Media ración, ~50% de ración
    Full = 3,         // Ración completa
    MarketPrice = 4   // Precio según mercado
}
```

---

## 2. Value Objects

### 2.1 PriceOption

#### Estructura

| Propiedad | Tipo |
|-----------|------|
| Id | Guid |
| PortionType | PortionType |
| Price | decimal? |
| IsActive | bool |

#### Validaciones

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| Id | NotEmpty | "Price option id is required" |
| PortionType | IsEnum | "Invalid portion type" |
| Price | >= 0 when HasValue | "Price cannot be negative" |
| Price | NotNull when PortionType != MarketPrice | "Price is required for fixed portion types" |

#### Propiedades Calculadas

| Propiedad | Tipo | Fórmula |
|-----------|------|---------|
| RequiresMarketPrice | bool | `PortionType == PortionType.MarketPrice && !Price.HasValue` |
| DisplayPrice | string | `RequiresMarketPrice ? "S/M" : Price.Value.ToString("C")` |

#### Comando: PriceOption.Create

**Input**

| Campo | Tipo | Default |
|-------|------|---------|
| PortionType | PortionType | |
| Price | decimal? | |
| IsActive | bool | true |

**Inyecta**: `IValidator<PriceOption>`

**Lógica**
```csharp
var priceOption = new PriceOption(Guid.NewGuid())
{
    PortionType = command.PortionType,
    Price = command.Price,
    IsActive = command.IsActive
};

return priceOptionValidator.ValidateOrThrow(priceOption);
```

#### Tests Unitarios

✅ PriceOption con precio fijo válido
- Input: PortionType=Full, Price=14.00, IsActive=true
- Resultado: PriceOption creado, DisplayPrice="14,00 €"

✅ PriceOption MarketPrice sin precio
- Input: PortionType=MarketPrice, Price=null
- Resultado: PriceOption creado, RequiresMarketPrice=true, DisplayPrice="S/M"

✅ PriceOption MarketPrice con precio actualizado
- Input: PortionType=MarketPrice, Price=22.00
- Resultado: PriceOption creado, RequiresMarketPrice=false, DisplayPrice="22,00 €"

❌ Precio negativo
- Input: Price=-5.00
- Resultado: ValidationException "Price cannot be negative"

❌ Precio fijo sin valor
- Input: PortionType=Full, Price=null
- Resultado: ValidationException "Price is required for fixed portion types"

---

### 2.2 ItemDepositOverride

#### Estructura

| Propiedad | Tipo |
|-----------|------|
| DepositAmount | decimal |
| MinimumQuantityForDeposit | int? |

#### Validaciones

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| DepositAmount | > 0 | "Deposit amount must be greater than zero" |
| DepositAmount | <= 10000 | "Deposit amount cannot exceed 10000" |
| MinimumQuantityForDeposit | >= 1 when HasValue | "Minimum quantity must be at least 1" |
| MinimumQuantityForDeposit | <= 100 when HasValue | "Minimum quantity cannot exceed 100" |

#### Propiedades Calculadas

| Propiedad | Tipo | Fórmula |
|-----------|------|---------|
| AppliesToAllQuantities | bool | `!MinimumQuantityForDeposit.HasValue` |

#### Métodos

- `IsApplicable(int quantity)` → bool: `AppliesToAllQuantities || quantity >= MinimumQuantityForDeposit.Value`

#### Comando: ItemDepositOverride.Create

**Input**

| Campo | Tipo | Default |
|-------|------|---------|
| DepositAmount | decimal | |
| MinimumQuantityForDeposit | int? | null |

**Inyecta**: `IValidator<ItemDepositOverride>`

**Lógica**
```csharp
var depositOverride = new ItemDepositOverride(
    command.DepositAmount,
    command.MinimumQuantityForDeposit);

return depositOverrideValidator.ValidateOrThrow(depositOverride);
```

#### Tests Unitarios

✅ Override sin cantidad mínima
- Input: DepositAmount=30.00, MinimumQuantityForDeposit=null
- Resultado: ItemDepositOverride creado, AppliesToAllQuantities=true

✅ Override con cantidad mínima
- Input: DepositAmount=30.00, MinimumQuantityForDeposit=4
- Resultado: ItemDepositOverride creado, IsApplicable(5)=true, IsApplicable(2)=false

❌ Importe cero
- Input: DepositAmount=0
- Resultado: ValidationException "Deposit amount must be greater than zero"

❌ Importe negativo
- Input: DepositAmount=-10.00
- Resultado: ValidationException "Deposit amount must be greater than zero"

❌ Cantidad mínima cero
- Input: MinimumQuantityForDeposit=0
- Resultado: ValidationException "Minimum quantity must be at least 1"

---

### 2.3 NutritionalInfo

#### Estructura

| Propiedad | Tipo |
|-----------|------|
| Calories | int |
| Protein | decimal |
| Carbohydrates | decimal |
| Fat | decimal |
| Fiber | decimal? |
| Sugar | decimal? |
| Salt | decimal? |
| ServingSize | int |

#### Validaciones

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| Calories | >= 0 | "Calories cannot be negative" |
| Calories | <= 10000 | "Calories cannot exceed 10000 kcal" |
| Protein | >= 0 | "Protein cannot be negative" |
| Protein | <= 1000 | "Protein cannot exceed 1000g" |
| Carbohydrates | >= 0 | "Carbohydrates cannot be negative" |
| Carbohydrates | <= 1000 | "Carbohydrates cannot exceed 1000g" |
| Fat | >= 0 | "Fat cannot be negative" |
| Fat | <= 1000 | "Fat cannot exceed 1000g" |
| Fiber | >= 0 when HasValue | "Fiber cannot be negative" |
| Fiber | <= 1000 when HasValue | "Fiber cannot exceed 1000g" |
| Sugar | >= 0 when HasValue | "Sugar cannot be negative" |
| Sugar | <= 1000 when HasValue | "Sugar cannot exceed 1000g" |
| Salt | >= 0 when HasValue | "Salt cannot be negative" |
| Salt | <= 100 when HasValue | "Salt cannot exceed 100g" |
| ServingSize | > 0 | "Serving size must be greater than zero" |
| ServingSize | <= 10000 | "Serving size cannot exceed 10000g" |

#### Métodos

- `GetNutritionForPortion(decimal portionPercentage)` → NutritionalInfo

#### Comando: NutritionalInfo.Create

**Input**

| Campo | Tipo | Default |
|-------|------|---------|
| Calories | int | |
| Protein | decimal | |
| Carbohydrates | decimal | |
| Fat | decimal | |
| Fiber | decimal? | null |
| Sugar | decimal? | null |
| Salt | decimal? | null |
| ServingSize | int | |

**Inyecta**: `IValidator<NutritionalInfo>`

**Lógica**
```csharp
var nutritionalInfo = new NutritionalInfo(
    command.Calories,
    command.Protein,
    command.Carbohydrates,
    command.Fat,
    command.Fiber,
    command.Sugar,
    command.Salt,
    command.ServingSize);

return nutritionalInfoValidator.ValidateOrThrow(nutritionalInfo);
```

#### Tests Unitarios

✅ Info nutricional completa
- Input: Calories=600, Protein=45, Carbohydrates=2, Fat=45, Fiber=0, Sugar=0, Salt=3.2, ServingSize=200
- Resultado: NutritionalInfo creado

✅ Info nutricional básica (sin opcionales)
- Input: Calories=180, Protein=8, Carbohydrates=15, Fat=9, ServingSize=300
- Resultado: NutritionalInfo creado con Fiber=null, Sugar=null, Salt=null

✅ Calcular para porción
- Input: NutritionalInfo con Calories=600, Protein=45
- Acción: GetNutritionForPortion(0.25m)
- Resultado: Calories=150, Protein=11.25

❌ Calorías negativas
- Input: Calories=-100
- Resultado: ValidationException "Calories cannot be negative"

❌ ServingSize cero
- Input: ServingSize=0
- Resultado: ValidationException "Serving size must be greater than zero"

---



## 3. Aggregate: MenuItem

### Estructura

```
MenuItem (Aggregate Root)
├─ Id: Guid
├─ TenantId: Guid
├─ Name: string
├─ Description: string?
├─ ImageUrl: string?
├─ DisplayOrder: int
├─ IsActive: bool
├─ IsAvailable: bool
├─ IsHighRiskItem: bool
├─ RequiresAdvanceOrder: bool
├─ MinimumAdvanceOrderQuantity: int?
├─ IsAlwaysAvailable: bool
├─ AllergenNotes: string?
├─ DepositOverride: ItemDepositOverride?
├─ NutritionalInfo: NutritionalInfo?
├─ PriceOptions: IReadOnlyCollection<PriceOption>
├─ AvailableDays: IReadOnlyCollection<DayOfWeek>
└─ Allergens: IReadOnlyCollection<Allergen>
```

#### Propiedades

| Propiedad | Tipo | Modificador |
|-----------|------|-------------|
| Id | Guid | init |
| TenantId | Guid | protected set |
| Name | string | protected set |
| Description | string? | protected set |
| ImageUrl | string? | protected set |
| DisplayOrder | int | protected set |
| IsActive | bool | protected set |
| IsAvailable | bool | protected set |
| IsHighRiskItem | bool | protected set |
| RequiresAdvanceOrder | bool | protected set |
| MinimumAdvanceOrderQuantity | int? | protected set |
| IsAlwaysAvailable | bool | protected set |
| AllergenNotes | string? | protected set |
| DepositOverride | ItemDepositOverride? | protected set |
| NutritionalInfo | NutritionalInfo? | protected set |

#### Colecciones

```csharp
protected HashSet<PriceOption> _priceOptions = [];
public IReadOnlyCollection<PriceOption> PriceOptions => _priceOptions.ToList().AsReadOnly();

protected HashSet<DayOfWeek> _availableDays = [];
public IReadOnlyCollection<DayOfWeek> AvailableDays => _availableDays.ToList().AsReadOnly();

protected HashSet<Allergen> _allergens = [];
public IReadOnlyCollection<Allergen> Allergens => _allergens.ToList().AsReadOnly();
```

#### Propiedades Calculadas

| Propiedad | Tipo | Fórmula |
|-----------|------|---------|
| IsAvailableToday | bool | `IsAlwaysAvailable \|\| _availableDays.Contains(DateTime.Today.DayOfWeek)` |
| CanBeOrdered | bool | `IsActive && IsAvailableToday && IsAvailable` |
| HasDepositOverride | bool | `DepositOverride != null` |
| HasActivePriceOption | bool | `_priceOptions.Any(p => p.IsActive)` |

### Validaciones

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| Id | NotEmpty | "Id is required" |
| TenantId | NotEmpty | "Tenant id is required" |
| Name | NotEmpty | "Name is required" |
| Name | Max(100) | "Name cannot exceed 100 characters" |
| Description | Max(1000) | "Description cannot exceed 1000 characters" |
| ImageUrl | Max(500) | "Image URL cannot exceed 500 characters" |
| ImageUrl | ValidUrl when NotEmpty | "Image URL must be a valid URL" |
| AllergenNotes | Max(500) | "Allergen notes cannot exceed 500 characters" |
| MinimumAdvanceOrderQuantity | >= 1 when HasValue | "Minimum advance order quantity must be at least 1" |
| MinimumAdvanceOrderQuantity | <= 100 when HasValue | "Minimum advance order quantity cannot exceed 100" |
| RequiresAdvanceOrder | IsHighRiskItem must be true | "Only high-risk items can require advance order" |
| MinimumAdvanceOrderQuantity | RequiresAdvanceOrder must be true when HasValue | "Minimum quantity requires advance order to be enabled" |
| AvailableDays | NotEmpty when !IsAlwaysAvailable | "Available days are required when item is not always available" |
| PriceOptions | AtLeastOne | "Item must have at least one price option" |
| PriceOptions | UniquePortion | "Duplicate portion types are not allowed" |

---

## 4. Response

```csharp
public record MenuItemResponse(
    Guid Id,
    Guid TenantId,
    string Name,
    string? Description,
    string? ImageUrl,
    int DisplayOrder,
    bool IsActive,
    bool IsAvailable,
    bool IsHighRiskItem,
    bool RequiresAdvanceOrder,
    int? MinimumAdvanceOrderQuantity,
    bool IsAlwaysAvailable,
    string? AllergenNotes,
    bool IsAvailableToday,
    bool CanBeOrdered,
    bool HasDepositOverride,
    ItemDepositOverrideResponse? DepositOverride,
    NutritionalInfoResponse? NutritionalInfo,
    IReadOnlyCollection<PriceOptionResponse> PriceOptions,
    IReadOnlyCollection<DayOfWeek> AvailableDays,
    IReadOnlyCollection<Allergen> Allergens
);

public record PriceOptionResponse(
    Guid Id,
    PortionType PortionType,
    decimal? Price,
    bool IsActive,
    bool RequiresMarketPrice,
    string DisplayPrice
);

public record ItemDepositOverrideResponse(
    decimal DepositAmount,
    int? MinimumQuantityForDeposit,
    bool AppliesToAllQuantities
);

public record NutritionalInfoResponse(
    int Calories,
    decimal Protein,
    decimal Carbohydrates,
    decimal Fat,
    decimal? Fiber,
    decimal? Sugar,
    decimal? Salt,
    int ServingSize
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

---

### 6.1 MenuItem.Create

#### Event Storming
```
🟡[RestaurantOwner] → 🔵(CreateMenuItem) → 🟤[[MenuItem]] → 🟠<MenuItemCreated>
```

#### Input

| Campo | Tipo |
|-------|------|
| TenantId | Guid |
| Name | string |
| Description | string? |
| ImageUrl | string? |
| DisplayOrder | int |
| IsHighRiskItem | bool |
| RequiresAdvanceOrder | bool |
| MinimumAdvanceOrderQuantity | int? |
| IsAlwaysAvailable | bool |
| AvailableDays | DayOfWeek[] |
| AllergenNotes | string? |
| PriceOptions | CreatePriceOptionCommand[] |

#### Inyecta
- `PriceOption.Create`
- `IValidator<MenuItem>`

#### Guards
Ninguno.

#### Lógica
```csharp
var priceOptions = command.PriceOptions
    .Select(po => priceOptionCreate.Execute(po))
    .ToList();

var menuItem = new MenuItem(Guid.NewGuid())
{
    TenantId = command.TenantId,
    Name = command.Name,
    Description = command.Description,
    ImageUrl = command.ImageUrl,
    DisplayOrder = command.DisplayOrder,
    IsActive = false,
    IsAvailable = true,
    IsHighRiskItem = command.IsHighRiskItem,
    RequiresAdvanceOrder = command.RequiresAdvanceOrder,
    MinimumAdvanceOrderQuantity = command.MinimumAdvanceOrderQuantity,
    IsAlwaysAvailable = command.IsAlwaysAvailable,
    AllergenNotes = command.AllergenNotes,
    DepositOverride = null,
    NutritionalInfo = null
};

foreach (var priceOption in priceOptions)
{
    menuItem._priceOptions.Add(priceOption);
}

if (!command.IsAlwaysAvailable)
{
    foreach (var day in command.AvailableDays)
    {
        menuItem._availableDays.Add(day);
    }
}

return menuItemValidator.ValidateOrThrow(menuItem);
```

#### Slice: POST /menu-items

**Request**
```csharp
public record CreateMenuItemRequest(
    string Name,
    string? Description,
    string? ImageUrl,
    int DisplayOrder,
    bool IsHighRiskItem,
    bool RequiresAdvanceOrder,
    int? MinimumAdvanceOrderQuantity,
    bool IsAlwaysAvailable,
    DayOfWeek[] AvailableDays,
    string? AllergenNotes,
    CreatePriceOptionRequest[] PriceOptions
);

public record CreatePriceOptionRequest(
    PortionType PortionType,
    decimal? Price,
    bool IsActive = true
);
```

**Response**: 201 Created → `MenuItemResponse`

#### Tests Unitarios (Dominio)

✅ Crear MenuItem con datos válidos
- Input: Name="Pulpo al Horno", IsAlwaysAvailable=true, PriceOptions=[{Full, 22.00}]
- Resultado: MenuItem creado con IsActive=false, IsAvailable=true

✅ Crear MenuItem con múltiples opciones de precio
- Input: Name="Jamón Ibérico", PriceOptions=[{Small, 3.50}, {Half, 7.00}, {Full, 14.00}]
- Resultado: MenuItem creado con 3 PriceOptions

✅ Crear MenuItem con disponibilidad por días
- Input: IsAlwaysAvailable=false, AvailableDays=[Friday, Saturday]
- Resultado: MenuItem con AvailableDays=[Friday, Saturday]

✅ Crear MenuItem de alto riesgo con pedido anticipado
- Input: IsHighRiskItem=true, RequiresAdvanceOrder=true, MinimumAdvanceOrderQuantity=4
- Resultado: MenuItem configurado correctamente

❌ Name vacío
- Input: Name=""
- Resultado: ValidationException "Name is required"

❌ Sin opciones de precio
- Input: PriceOptions=[]
- Resultado: ValidationException "Item must have at least one price option"

❌ PortionType duplicado
- Input: PriceOptions=[{Full, 10.00}, {Full, 12.00}]
- Resultado: ValidationException "Duplicate portion types are not allowed"

❌ RequiresAdvanceOrder sin IsHighRiskItem
- Input: IsHighRiskItem=false, RequiresAdvanceOrder=true
- Resultado: ValidationException "Only high-risk items can require advance order"

❌ No siempre disponible sin días
- Input: IsAlwaysAvailable=false, AvailableDays=[]
- Resultado: ValidationException "Available days are required when item is not always available"

#### Tests Unitarios (Servicio)

✅ Llama a MenuItem.Create con los parámetros correctos
- Verifica que se invoca menuItemCreate.Execute con el command correcto

✅ Añade el menuItem al repositorio
- Verifica que repository.Add es llamado con el menuItem creado

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

✅ Retorna Response mapeado correctamente
- Verifica que el Response contiene los datos del menuItem

#### Tests Integración

✅ 201 Created → MenuItemResponse

❌ 422 → Validación fallida

---

### 6.2 MenuItem.Update

#### Event Storming
```
🟡[RestaurantOwner] → 🔵(UpdateMenuItem) → 🟤[[MenuItem]] → 🟠<MenuItemUpdated>
```

#### Input

| Campo | Tipo |
|-------|------|
| Name | string |
| Description | string? |
| ImageUrl | string? |
| DisplayOrder | int |
| IsHighRiskItem | bool |
| RequiresAdvanceOrder | bool |
| MinimumAdvanceOrderQuantity | int? |
| IsAlwaysAvailable | bool |
| AvailableDays | DayOfWeek[] |
| AllergenNotes | string? |

#### Inyecta
- `IValidator<MenuItem>`

#### Guards
Ninguno.

#### Lógica
```csharp
menuItem.Name = command.Name;
menuItem.Description = command.Description;
menuItem.ImageUrl = command.ImageUrl;
menuItem.DisplayOrder = command.DisplayOrder;
menuItem.IsHighRiskItem = command.IsHighRiskItem;
menuItem.RequiresAdvanceOrder = command.RequiresAdvanceOrder;
menuItem.MinimumAdvanceOrderQuantity = command.MinimumAdvanceOrderQuantity;
menuItem.IsAlwaysAvailable = command.IsAlwaysAvailable;
menuItem.AllergenNotes = command.AllergenNotes;

menuItem._availableDays.Clear();
if (!command.IsAlwaysAvailable)
{
    foreach (var day in command.AvailableDays)
    {
        menuItem._availableDays.Add(day);
    }
}

return menuItemValidator.ValidateOrThrow(menuItem);
```

#### Slice: PUT /menu-items/{id}

**Request**
```csharp
public record UpdateMenuItemRequest(
    string Name,
    string? Description,
    string? ImageUrl,
    int DisplayOrder,
    bool IsHighRiskItem,
    bool RequiresAdvanceOrder,
    int? MinimumAdvanceOrderQuantity,
    bool IsAlwaysAvailable,
    DayOfWeek[] AvailableDays,
    string? AllergenNotes
);
```

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

✅ Actualizar MenuItem existente
- Precondición: MenuItem existe
- Input: Name="Pulpo a la Gallega", Description="Nueva descripción"
- Resultado: MenuItem actualizado

✅ Cambiar de siempre disponible a días específicos
- Precondición: MenuItem con IsAlwaysAvailable=true
- Input: IsAlwaysAvailable=false, AvailableDays=[Saturday]
- Resultado: MenuItem con AvailableDays=[Saturday]

✅ Activar alto riesgo y pedido anticipado
- Precondición: MenuItem con IsHighRiskItem=false
- Input: IsHighRiskItem=true, RequiresAdvanceOrder=true, MinimumAdvanceOrderQuantity=4
- Resultado: MenuItem configurado correctamente

✅ Desactivar pedido anticipado antes de quitar alto riesgo
- Precondición: MenuItem con IsHighRiskItem=true, RequiresAdvanceOrder=true
- Input: IsHighRiskItem=true, RequiresAdvanceOrder=false
- Resultado: MenuItem con RequiresAdvanceOrder=false

❌ Name vacío
- Input: Name=""
- Resultado: ValidationException "Name is required"

❌ Quitar alto riesgo con pedido anticipado activo
- Precondición: MenuItem con RequiresAdvanceOrder=true
- Input: IsHighRiskItem=false, RequiresAdvanceOrder=true
- Resultado: ValidationException "Only high-risk items can require advance order"

❌ No siempre disponible sin días
- Input: IsAlwaysAvailable=false, AvailableDays=[]
- Resultado: ValidationException "Available days are required when item is not always available"

#### Tests Unitarios (Servicio)

✅ Obtiene el menuItem del repositorio
- Verifica que repository.GetByIdAsync es llamado con el id correcto

✅ Llama a MenuItem.Update con los parámetros correctos
- Verifica que se invoca menuItemUpdate.Execute con el command correcto

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

#### Tests Integración

✅ 204 No Content

❌ 404 → MenuItem no encontrado

❌ 422 → Validación fallida

---

### 6.3 MenuItem.Activate

#### Event Storming
```
🟡[RestaurantOwner] → 🔵(ActivateMenuItem) → 🟤[[MenuItem]] → 🟠<MenuItemActivated>
                                                    │
                                          🟣{TieneOpcionPrecioActiva}
```

#### Input
Ninguno

#### Inyecta
- `IValidator<MenuItem>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Ya está activo | 409 | ConflictGuard | "Menu item is already active" |
| No tiene PriceOption activa | 422 | ValidationGuard | "Menu item must have at least one active price option" |

#### Lógica
```csharp
ConflictGuard.ThrowIf(menuItem.IsActive, "Menu item is already active");
ValidationGuard.ThrowIf(!menuItem.HasActivePriceOption, "Menu item must have at least one active price option", nameof(menuItem.PriceOptions));

menuItem.IsActive = true;

return menuItemValidator.ValidateOrThrow(menuItem);
```

#### Slice: POST /menu-items/{id}/activate

**Response**: 200 OK → `MenuItemResponse`

#### Tests Unitarios (Dominio)

✅ Activar MenuItem con opciones de precio activas
- Precondición: MenuItem con PriceOption activa, IsActive=false
- Resultado: MenuItem con IsActive=true

❌ MenuItem ya activo
- Precondición: MenuItem con IsActive=true
- Resultado: ConflictException "Menu item is already active"

❌ MenuItem sin PriceOption activa
- Precondición: MenuItem con todas las PriceOptions inactivas
- Resultado: ValidationException "Menu item must have at least one active price option"

#### Tests Unitarios (Servicio)

✅ Obtiene el menuItem del repositorio
- Verifica que repository.GetByIdAsync es llamado con el id correcto

✅ Llama a MenuItem.Activate
- Verifica que se invoca menuItemActivate.Execute

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

✅ Retorna Response mapeado correctamente
- Verifica que el Response contiene IsActive=true

#### Tests Integración

✅ 200 OK → MenuItemResponse con IsActive=true

❌ 404 → MenuItem no encontrado

❌ 409 → Ya estaba activo

❌ 422 → Falta PriceOption activa

---

### 6.4 MenuItem.Deactivate

#### Event Storming
```
🟡[RestaurantOwner] → 🔵(DeactivateMenuItem) → 🟤[[MenuItem]] → 🟠<MenuItemDeactivated>
```

#### Input
Ninguno

#### Inyecta
- `IValidator<MenuItem>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Ya está inactivo | 409 | ConflictGuard | "Menu item is already inactive" |

#### Lógica
```csharp
ConflictGuard.ThrowIf(!menuItem.IsActive, "Menu item is already inactive");

menuItem.IsActive = false;

return menuItemValidator.ValidateOrThrow(menuItem);
```

#### Slice: POST /menu-items/{id}/deactivate

**Response**: 200 OK → `MenuItemResponse`

#### Tests Unitarios (Dominio)

✅ Desactivar MenuItem activo
- Precondición: MenuItem con IsActive=true
- Resultado: MenuItem con IsActive=false

❌ MenuItem ya inactivo
- Precondición: MenuItem con IsActive=false
- Resultado: ConflictException "Menu item is already inactive"

#### Tests Unitarios (Servicio)

✅ Obtiene el menuItem del repositorio
- Verifica que repository.GetByIdAsync es llamado con el id correcto

✅ Llama a MenuItem.Deactivate
- Verifica que se invoca menuItemDeactivate.Execute

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

✅ Retorna Response mapeado correctamente
- Verifica que el Response contiene IsActive=false

#### Tests Integración

✅ 200 OK → MenuItemResponse con IsActive=false

❌ 404 → MenuItem no encontrado

❌ 409 → Ya estaba inactivo

---

### 6.5 MenuItem.MarkAsAvailable

#### Event Storming
```
🟡[Waiter] → 🔵(MarkAsAvailable) → 🟤[[MenuItem]] → 🟠<MenuItemMarkedAvailable>
```

#### Input
Ninguno

#### Inyecta
- `IValidator<MenuItem>`

#### Guards
Ninguno.

#### Lógica
```csharp
menuItem.IsAvailable = true;

return menuItemValidator.ValidateOrThrow(menuItem);
```

#### Slice: POST /menu-items/{id}/mark-available

**Response**: 200 OK → `MenuItemResponse`

#### Tests Unitarios (Dominio)

✅ Marcar como disponible
- Precondición: MenuItem con IsAvailable=false
- Resultado: MenuItem con IsAvailable=true

✅ Marcar como disponible (idempotente)
- Precondición: MenuItem con IsAvailable=true
- Resultado: MenuItem sin cambios

#### Tests Integración

✅ 200 OK → MenuItemResponse con IsAvailable=true

❌ 404 → MenuItem no encontrado

---

### 6.6 MenuItem.MarkAsUnavailable

#### Event Storming
```
🟡[Waiter] → 🔵(MarkAsUnavailable) → 🟤[[MenuItem]] → 🟠<MenuItemMarkedUnavailable>
```

#### Input
Ninguno

#### Inyecta
- `IValidator<MenuItem>`

#### Guards
Ninguno.

#### Lógica
```csharp
menuItem.IsAvailable = false;

return menuItemValidator.ValidateOrThrow(menuItem);
```

#### Slice: POST /menu-items/{id}/mark-unavailable

**Response**: 200 OK → `MenuItemResponse`

#### Tests Unitarios (Dominio)

✅ Marcar como no disponible (agotado)
- Precondición: MenuItem con IsAvailable=true
- Resultado: MenuItem con IsAvailable=false, CanBeOrdered=false

✅ Marcar como no disponible (idempotente)
- Precondición: MenuItem con IsAvailable=false
- Resultado: MenuItem sin cambios

#### Tests Integración

✅ 200 OK → MenuItemResponse con IsAvailable=false

❌ 404 → MenuItem no encontrado

---

### 6.7 MenuItem.SetDepositOverride

#### Event Storming
```
🟡[RestaurantOwner] → 🔵(SetDepositOverride) → 🟤[[MenuItem]] → 🟠<DepositOverrideConfigured>
```

#### Input

| Campo | Tipo |
|-------|------|
| DepositAmount | decimal |
| MinimumQuantityForDeposit | int? |

#### Inyecta
- `ItemDepositOverride.Create`
- `IValidator<MenuItem>`

#### Guards
Ninguno.

#### Lógica
```csharp
var depositOverride = itemDepositOverrideCreate.Execute(new CreateItemDepositOverrideCommand(
    command.DepositAmount,
    command.MinimumQuantityForDeposit));

menuItem.DepositOverride = depositOverride;

return menuItemValidator.ValidateOrThrow(menuItem);
```

#### Slice: PUT /menu-items/{id}/deposit-override

**Request**
```csharp
public record SetDepositOverrideRequest(
    decimal DepositAmount,
    int? MinimumQuantityForDeposit
);
```

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

✅ Configurar override sin cantidad mínima
- Input: DepositAmount=30.00, MinimumQuantityForDeposit=null
- Resultado: DepositOverride configurado, HasDepositOverride=true

✅ Configurar override con cantidad mínima
- Input: DepositAmount=30.00, MinimumQuantityForDeposit=4
- Resultado: DepositOverride.IsApplicable(5)=true

❌ Importe cero
- Input: DepositAmount=0
- Resultado: ValidationException "Deposit amount must be greater than zero"

#### Tests Integración

✅ 204 No Content

❌ 404 → MenuItem no encontrado

❌ 422 → Validación fallida

---

### 6.8 MenuItem.RemoveDepositOverride

#### Event Storming
```
🟡[RestaurantOwner] → 🔵(RemoveDepositOverride) → 🟤[[MenuItem]] → 🟠<DepositOverrideRemoved>
```

#### Input
Ninguno

#### Inyecta
- `IValidator<MenuItem>`

#### Guards
Ninguno.

#### Lógica
```csharp
menuItem.DepositOverride = null;

return menuItemValidator.ValidateOrThrow(menuItem);
```

#### Slice: DELETE /menu-items/{id}/deposit-override

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

✅ Eliminar override existente
- Precondición: MenuItem con DepositOverride configurado
- Resultado: DepositOverride=null, HasDepositOverride=false

✅ Eliminar override inexistente (idempotente)
- Precondición: MenuItem sin DepositOverride
- Resultado: Sin cambios

#### Tests Integración

✅ 204 No Content

❌ 404 → MenuItem no encontrado

---

### 6.9 MenuItem.SetNutritionalInfo

#### Event Storming
```
🟡[RestaurantOwner] → 🔵(SetNutritionalInfo) → 🟤[[MenuItem]] → 🟠<NutritionalInfoConfigured>
```

#### Input

| Campo | Tipo |
|-------|------|
| Calories | int |
| Protein | decimal |
| Carbohydrates | decimal |
| Fat | decimal |
| Fiber | decimal? |
| Sugar | decimal? |
| Salt | decimal? |
| ServingSize | int |

#### Inyecta
- `NutritionalInfo.Create`
- `IValidator<MenuItem>`

#### Guards
Ninguno.

#### Lógica
```csharp
var nutritionalInfo = nutritionalInfoCreate.Execute(new CreateNutritionalInfoCommand(
    command.Calories,
    command.Protein,
    command.Carbohydrates,
    command.Fat,
    command.Fiber,
    command.Sugar,
    command.Salt,
    command.ServingSize));

menuItem.NutritionalInfo = nutritionalInfo;

return menuItemValidator.ValidateOrThrow(menuItem);
```

#### Slice: PUT /menu-items/{id}/nutritional-info

**Request**
```csharp
public record SetNutritionalInfoRequest(
    int Calories,
    decimal Protein,
    decimal Carbohydrates,
    decimal Fat,
    decimal? Fiber,
    decimal? Sugar,
    decimal? Salt,
    int ServingSize
);
```

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

✅ Configurar info nutricional completa
- Input: Calories=600, Protein=45, Carbohydrates=2, Fat=45, ServingSize=200
- Resultado: NutritionalInfo configurado

❌ Calorías negativas
- Input: Calories=-100
- Resultado: ValidationException "Calories cannot be negative"

#### Tests Integración

✅ 204 No Content

❌ 404 → MenuItem no encontrado

❌ 422 → Validación fallida

---

### 6.10 MenuItem.RemoveNutritionalInfo

#### Event Storming
```
🟡[RestaurantOwner] → 🔵(RemoveNutritionalInfo) → 🟤[[MenuItem]] → 🟠<NutritionalInfoRemoved>
```

#### Input
Ninguno

#### Inyecta
- `IValidator<MenuItem>`

#### Guards
Ninguno.

#### Lógica
```csharp
menuItem.NutritionalInfo = null;

return menuItemValidator.ValidateOrThrow(menuItem);
```

#### Slice: DELETE /menu-items/{id}/nutritional-info

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

✅ Eliminar info nutricional existente
- Precondición: MenuItem con NutritionalInfo configurado
- Resultado: NutritionalInfo=null

#### Tests Integración

✅ 204 No Content

❌ 404 → MenuItem no encontrado

---

### 6.11 MenuItem.AddPriceOption

#### Event Storming
```
🟡[RestaurantOwner] → 🔵(AddPriceOption) → 🟤[[MenuItem]] → 🟠<PriceOptionAdded>
                                                  │
                                        🟣{PortionTypeÚnico}
```

#### Input

| Campo | Tipo |
|-------|------|
| PortionType | PortionType |
| Price | decimal? |
| IsActive | bool |

#### Inyecta
- `PriceOption.Create`
- `IValidator<MenuItem>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| PortionType ya existe | 409 | ConflictGuard | "Price option with portion type '{PortionType}' already exists" |

#### Lógica
```csharp
ConflictGuard.ThrowIf(
    menuItem.PriceOptions.Any(p => p.PortionType == command.PortionType),
    $"Price option with portion type '{command.PortionType}' already exists");

var priceOption = priceOptionCreate.Execute(new CreatePriceOptionCommand(
    command.PortionType,
    command.Price,
    command.IsActive));

menuItem._priceOptions.Add(priceOption);

return menuItemValidator.ValidateOrThrow(menuItem);
```

#### Slice: POST /menu-items/{id}/price-options

**Request**
```csharp
public record AddPriceOptionRequest(
    PortionType PortionType,
    decimal? Price,
    bool IsActive = true
);
```

**Response**: 201 Created → `MenuItemResponse`

#### Tests Unitarios (Dominio)

✅ Añadir nueva opción de precio
- Precondición: MenuItem sin PriceOption de tipo Half
- Input: PortionType=Half, Price=7.00
- Resultado: PriceOption añadida

✅ Añadir opción MarketPrice sin precio
- Input: PortionType=MarketPrice, Price=null
- Resultado: PriceOption añadida con RequiresMarketPrice=true

❌ PortionType duplicado
- Precondición: MenuItem ya tiene PriceOption de tipo Full
- Input: PortionType=Full, Price=15.00
- Resultado: ConflictException "Price option with portion type 'Full' already exists"

#### Tests Integración

✅ 201 Created → MenuItemResponse con PriceOption añadida

❌ 404 → MenuItem no encontrado

❌ 409 → PortionType duplicado

❌ 422 → Validación fallida

---

### 6.12 MenuItem.UpdatePriceOption

#### Event Storming
```
🟡[RestaurantOwner] → 🔵(UpdatePriceOption) → 🟤[[MenuItem]] → 🟠<PriceOptionUpdated>
                                                    │
                                          🟣{PriceOptionExiste}
```

#### Input

| Campo | Tipo |
|-------|------|
| Price | decimal? |
| IsActive | bool |

*PortionType viene en la ruta*

#### Inyecta
- `PriceOption.Create`
- `IValidator<MenuItem>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| PriceOption no existe | 404 | NotFoundGuard | "Price option with portion type '{PortionType}' not found" |
| Desactivar última activa | 422 | ValidationGuard | "Cannot deactivate last active price option" |

#### Lógica
```csharp
var existing = menuItem.PriceOptions.FirstOrDefault(p => p.PortionType == portionType);
NotFoundGuard.ThrowIfNull(existing, $"Price option with portion type '{portionType}' not found");

if (!command.IsActive && menuItem.PriceOptions.Count(p => p.IsActive) <= 1 && existing.IsActive)
{
    ValidationGuard.Throw("Cannot deactivate last active price option", nameof(menuItem.PriceOptions));
}

var updated = priceOptionCreate.Execute(new CreatePriceOptionCommand(
    portionType,
    command.Price,
    command.IsActive));

menuItem._priceOptions.Remove(existing);
menuItem._priceOptions.Add(updated);

return menuItemValidator.ValidateOrThrow(menuItem);
```

#### Slice: PUT /menu-items/{id}/price-options/{portionType}

**Request**
```csharp
public record UpdatePriceOptionRequest(
    decimal? Price,
    bool IsActive
);
```

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

✅ Actualizar precio
- Precondición: MenuItem tiene PriceOption de tipo Full con Price=14.00
- Input: Price=16.00, IsActive=true
- Resultado: PriceOption actualizada con Price=16.00

✅ Actualizar precio de mercado
- Precondición: MenuItem tiene PriceOption de tipo MarketPrice con Price=null
- Input: Price=22.00, IsActive=true
- Resultado: PriceOption actualizada con Price=22.00, RequiresMarketPrice=false

✅ Desactivar opción (hay otras activas)
- Precondición: MenuItem con 3 PriceOptions activas
- Input: IsActive=false
- Resultado: PriceOption desactivada

❌ PriceOption no existe
- Precondición: MenuItem no tiene PriceOption de tipo Half
- Resultado: NotFoundException "Price option with portion type 'Half' not found"

❌ Desactivar última activa
- Precondición: MenuItem con 1 PriceOption activa
- Input: IsActive=false
- Resultado: ValidationException "Cannot deactivate last active price option"

#### Tests Integración

✅ 204 No Content

❌ 404 → MenuItem o PriceOption no encontrada

❌ 422 → Validación fallida

---

### 6.13 MenuItem.RemovePriceOption

#### Event Storming
```
🟡[RestaurantOwner] → 🔵(RemovePriceOption) → 🟤[[MenuItem]] → 🟠<PriceOptionRemoved>
                                                    │
                                          🟣{PriceOptionExiste}
                                          🟣{NoEsLaÚltima}
```

#### Input
*PortionType viene en la ruta*

#### Inyecta
- `IValidator<MenuItem>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| PriceOption no existe | 404 | NotFoundGuard | "Price option with portion type '{PortionType}' not found" |
| Es la última | 422 | ValidationGuard | "Cannot remove last price option" |

#### Lógica
```csharp
var existing = menuItem.PriceOptions.FirstOrDefault(p => p.PortionType == portionType);
NotFoundGuard.ThrowIfNull(existing, $"Price option with portion type '{portionType}' not found");

ValidationGuard.ThrowIf(
    menuItem.PriceOptions.Count <= 1,
    "Cannot remove last price option",
    nameof(menuItem.PriceOptions));

menuItem._priceOptions.Remove(existing);

return menuItemValidator.ValidateOrThrow(menuItem);
```

#### Slice: DELETE /menu-items/{id}/price-options/{portionType}

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

✅ Eliminar PriceOption (hay varias)
- Precondición: MenuItem con 3 PriceOptions
- Resultado: PriceOption eliminada, quedan 2

❌ PriceOption no existe
- Resultado: NotFoundException "Price option with portion type 'Half' not found"

❌ Última PriceOption
- Precondición: MenuItem con 1 PriceOption
- Resultado: ValidationException "Cannot remove last price option"

#### Tests Integración

✅ 204 No Content

❌ 404 → MenuItem o PriceOption no encontrada

❌ 422 → Es la última

---

### 6.14 MenuItem.AddAllergen

#### Event Storming
```
🟡[RestaurantOwner] → 🔵(AddAllergen) → 🟤[[MenuItem]] → 🟠<AllergenAdded>
                                               │
                                     🟣{AllergenÚnico}
```

#### Input

| Campo | Tipo |
|-------|------|
| AllergenId | Guid |

#### Inyecta
- `AllergenReference.Create`
- `IValidator<MenuItem>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Allergen ya existe | 409 | ConflictGuard | "Allergen already exists in this item" |

#### Lógica
```csharp
 ConflictGuard.ThrowIf(
                menuItem.Allergens.Any(a => a.Id == command.Allergen.Id),
                "Allergen already exists in this item");

            menuItem._allergens.Add(command.Allergen);

            return menuItemValidator.ValidateOrThrow(menuItem);
```

#### Slice: POST /menu-items/{id}/allergens

**Request**
```csharp
public record AddAllergenRequest(
    Guid AllergenId
);
```

**Response**: 201 Created → `MenuItemResponse`

#### Tests Unitarios (Dominio)

✅ Añadir alérgeno válido
- Precondición: MenuItem sin el alérgeno GLUTEN
- Input: AllergenId=gluten-guid
- Resultado: AllergenReference añadida

✅ Añadir múltiples alérgenos
- Input: AllergenId=lacteos-guid (después de añadir gluten)
- Resultado: MenuItem con 2 alérgenos

❌ Alérgeno duplicado
- Precondición: MenuItem ya tiene el alérgeno GLUTEN
- Input: AllergenId=gluten-guid
- Resultado: ConflictException "Allergen already exists in this item"

#### Tests Integración

✅ 201 Created → MenuItemResponse con Allergen añadido

❌ 404 → MenuItem no encontrado

❌ 409 → Alérgeno duplicado

---

### 6.15 MenuItem.RemoveAllergen

#### Event Storming
```
🟡[RestaurantOwner] → 🔵(RemoveAllergen) → 🟤[[MenuItem]] → 🟠<AllergenRemoved>
                                                  │
                                        🟣{AllergenExiste}
```

#### Input
*AllergenId viene en la ruta*

#### Inyecta
- `IValidator<MenuItem>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Allergen no existe | 404 | NotFoundGuard | "Allergen not found in this item" |

#### Lógica
```csharp
var existing = menuItem.Allergens.FirstOrDefault(a => a.AllergenId == allergenId);
NotFoundGuard.ThrowIfNull(existing, "Allergen not found in this item");

menuItem._allergens.Remove(existing);

return menuItemValidator.ValidateOrThrow(menuItem);
```

#### Slice: DELETE /menu-items/{id}/allergens/{allergenId}

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

✅ Eliminar alérgeno existente
- Precondición: MenuItem con alérgeno GLUTEN
- Input: AllergenId=gluten-guid
- Resultado: AllergenReference eliminada

❌ Alérgeno no existe
- Precondición: MenuItem sin alérgeno SULFITOS
- Input: AllergenId=sulfitos-guid
- Resultado: NotFoundException "Allergen not found in this item"

#### Tests Integración

✅ 204 No Content

❌ 404 → MenuItem o Allergen no encontrado

---

## 7. Queries

### GetMenuItem

**Slice**: GET /menu-items/{id}

**Response**: 200 OK → `MenuItemResponse`

#### Tests Integración

✅ 200 OK → MenuItemResponse

❌ 404 → No encontrado

---

### ListMenuItems

**Slice**: GET /menu-items?isActive=true&isAvailable=true

**Response**: 200 OK → `MenuItemResponse[]`

#### Tests Integración

✅ 200 OK → Array de MenuItemResponse

✅ 200 OK → Array vacío si no hay menuItems

---

## 8. Resumen de Endpoints

| Método | Ruta | Comando/Query | Response |
|--------|------|---------------|----------|
| POST | /menu-items | MenuItem.Create | 201 → `MenuItemResponse` |
| GET | /menu-items | ListMenuItems | 200 → `MenuItemResponse[]` |
| GET | /menu-items/{id} | GetMenuItem | 200 → `MenuItemResponse` |
| PUT | /menu-items/{id} | MenuItem.Update | 204 |
| POST | /menu-items/{id}/activate | MenuItem.Activate | 200 → `MenuItemResponse` |
| POST | /menu-items/{id}/deactivate | MenuItem.Deactivate | 200 → `MenuItemResponse` |
| POST | /menu-items/{id}/mark-available | MenuItem.MarkAsAvailable | 200 → `MenuItemResponse` |
| POST | /menu-items/{id}/mark-unavailable | MenuItem.MarkAsUnavailable | 200 → `MenuItemResponse` |
| PUT | /menu-items/{id}/deposit-override | MenuItem.SetDepositOverride | 204 |
| DELETE | /menu-items/{id}/deposit-override | MenuItem.RemoveDepositOverride | 204 |
| PUT | /menu-items/{id}/nutritional-info | MenuItem.SetNutritionalInfo | 204 |
| DELETE | /menu-items/{id}/nutritional-info | MenuItem.RemoveNutritionalInfo | 204 |
| POST | /menu-items/{id}/price-options | MenuItem.AddPriceOption | 201 → `MenuItemResponse` |
| PUT | /menu-items/{id}/price-options/{portionType} | MenuItem.UpdatePriceOption | 204 |
| DELETE | /menu-items/{id}/price-options/{portionType} | MenuItem.RemovePriceOption | 204 |
| POST | /menu-items/{id}/allergens | MenuItem.AddAllergen | 201 → `MenuItemResponse` |
| DELETE | /menu-items/{id}/allergens/{allergenId} | MenuItem.RemoveAllergen | 204 |

---

## 9. Persistencia (Firestore)

### Colección

`/menu-items/{menuItemId}`

### Configuración DbContext

```csharp
modelBuilder.Entity<MenuItem>(entity =>
{
    // QueryFilter: multi-tenancy by TenantId
    entity.HasQueryFilter(m => m.TenantId == tenantId);

    // Ignore: propiedades computed (no backing fields)
    entity.Ignore(m => m.IsAvailableToday);
    entity.Ignore(m => m.CanBeOrdered);
    entity.Ignore(m => m.HasDepositOverride);
    entity.Ignore(m => m.HasActivePriceOption);

    // ComplexType: DepositOverride
    entity.ComplexProperty(m => m.DepositOverride, deposit =>
    {
        deposit.Ignore(d => d.AppliesToAllQuantities);
    });

    // ComplexType: NutritionalInfo
    entity.ComplexProperty(m => m.NutritionalInfo);

    // ArrayOf: PriceOptions (usa backing field _priceOptions)
    entity.ArrayOf(m => m.PriceOptions, option =>
    {
        option.Ignore(p => p.RequiresMarketPrice);
        option.Ignore(p => p.DisplayPrice);
    });

    // ArrayOf: AvailableDays (usa backing field _availableDays)
    entity.ArrayOf(m => m.AvailableDays);

    // ArrayOf Reference: Allergens (references to Allergen aggregate)
    entity.ArrayOf(m => m.Allergens).AsReferences();
});
```

### Documento Ejemplo

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "tenantId": "tenant-001-guid",
  "name": "Pulpo al Horno",
  "description": "Pulpo gallego con pimentón y aceite de oliva",
  "imageUrl": "https://cdn.fudie.com/images/pulpo.jpg",
  "displayOrder": 1,
  "isActive": true,
  "isAvailable": true,
  "isHighRiskItem": true,
  "requiresAdvanceOrder": true,
  "minimumAdvanceOrderQuantity": 4,
  "isAlwaysAvailable": false,
  "allergenNotes": "Puede contener trazas de crustáceos",
  "depositOverride": {
    "depositAmount": 30.00,
    "minimumQuantityForDeposit": 4
  },
  "nutritionalInfo": {
    "calories": 450,
    "protein": 38.5,
    "carbohydrates": 8.2,
    "fat": 28.0,
    "fiber": 2.5,
    "sugar": 1.0,
    "salt": 2.8,
    "servingSize": 350
  },
  "priceOptions": [
    {
      "id": "price-001-guid",
      "portionType": 4,
      "price": 22.00,
      "isActive": true
    }
  ],
  "availableDays": [5, 6],
  "allergens": [
    { "allergenId": "allergen-moluscos-guid" },
    { "allergenId": "allergen-sulfitos-guid" }
  ]
}
```

---

## 10. Hot Spots ⚠️

| # | Pregunta | Estado |
|---|----------|--------|
| 1 | ¿Cómo se vincula MenuItem a Menu/MenuCategory? | Decidido: Mediante agregados separados (AlaCarteMenu, DailyMenu) que referencian MenuItem |
| 2 | ¿Los alérgenos deben validarse contra el catálogo del sistema? | Pendiente: Depende de si se implementa validación cross-aggregate |
| 3 | ¿Qué pasa si un alérgeno se desactiva en el catálogo? | Decidido: MenuItem mantiene la referencia, frontend muestra advertencia |
| 4 | ¿MenuItem puede existir sin estar en ningún menú? | Decidido: Sí, es un agregado independiente |
| 5 | ¿Cómo se manejan los precios en diferentes monedas? | Pendiente: Por ahora asume moneda del tenant |

---

**Fecha**: 2025-01-26
**Autor**: Equipo Fudie
