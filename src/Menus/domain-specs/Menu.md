# Domain Specification: Menu

---

## 1. Enums

### DepositType

```csharp
public enum DepositType
{
    PerPerson = 1,        // €X por persona
    PercentageOfBill = 2, // X% del total estimado
    FixedAmount = 3       // €X importe fijo
}
```

### PortionType (Shared)

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

### 2.1 DepositPolicy

#### Estructura

| Propiedad | Tipo |
|-----------|------|
| DepositType | DepositType |
| Amount | decimal |
| Percentage | decimal? |
| MinimumBillForDeposit | decimal? |
| MinimumGuestsForDeposit | int? |

#### Invariantes (Validator)

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| Amount | > 0 | "Amount must be greater than zero" |
| Percentage | NotNull when DepositType = PercentageOfBill | "Percentage must be specified for PercentageOfBill type" |
| Percentage | Null when DepositType != PercentageOfBill | "Percentage only applies to PercentageOfBill type" |
| Percentage | Between(1, 100) when HasValue | "Percentage must be between 1 and 100" |
| MinimumGuestsForDeposit | >= 1 when HasValue | "MinimumGuestsForDeposit must be at least 1" |
| MinimumBillForDeposit | >= 0 when HasValue | "MinimumBillForDeposit cannot be negative" |

#### Métodos

- `IsApplicable(int guestCount, decimal estimatedBill)` → bool
- `CalculateDeposit(int guestCount, decimal estimatedBill)` → decimal

#### Comando: DepositPolicy.Create

**Input**

| Campo | Tipo | Default |
|-------|------|---------|
| DepositType | DepositType | |
| Amount | decimal | |
| Percentage | decimal? | null |
| MinimumBillForDeposit | decimal? | null |
| MinimumGuestsForDeposit | int? | null |

**Inyecta**: `IValidator<DepositPolicy>`

**Lógica**
```csharp
var depositPolicy = new DepositPolicy(
    command.DepositType,
    command.Amount,
    command.Percentage,
    command.MinimumBillForDeposit,
    command.MinimumGuestsForDeposit);

return depositPolicyValidator.ValidateOrThrow(depositPolicy);
```

#### Tests Unitarios

✅ DepositPolicy tipo PerPerson válido
- Input: DepositType=PerPerson, Amount=15, MinimumGuestsForDeposit=6
- Resultado: DepositPolicy creado

✅ DepositPolicy tipo PercentageOfBill válido
- Input: DepositType=PercentageOfBill, Amount=0, Percentage=20, MinimumBillForDeposit=100
- Resultado: DepositPolicy creado

✅ DepositPolicy tipo FixedAmount válido
- Input: DepositType=FixedAmount, Amount=50
- Resultado: DepositPolicy creado

❌ Amount cero
- Input: Amount=0
- Resultado: ValidationException "Amount must be greater than zero"

❌ Amount negativo
- Input: Amount=-10
- Resultado: ValidationException "Amount must be greater than zero"

❌ PercentageOfBill sin Percentage
- Input: DepositType=PercentageOfBill, Percentage=null
- Resultado: ValidationException "Percentage must be specified for PercentageOfBill type"

❌ Percentage en tipo no PercentageOfBill
- Input: DepositType=PerPerson, Percentage=20
- Resultado: ValidationException "Percentage only applies to PercentageOfBill type"

❌ Percentage fuera de rango
- Input: DepositType=PercentageOfBill, Percentage=150
- Resultado: ValidationException "Percentage must be between 1 and 100"

❌ MinimumGuestsForDeposit menor que 1
- Input: MinimumGuestsForDeposit=0
- Resultado: ValidationException "MinimumGuestsForDeposit must be at least 1"

❌ MinimumBillForDeposit negativo
- Input: MinimumBillForDeposit=-50
- Resultado: ValidationException "MinimumBillForDeposit cannot be negative"

---

### 2.2 PriceOption (Shared)

#### Estructura

| Propiedad | Tipo |
|-----------|------|
| PortionType | PortionType |
| Price | decimal? |
| IsActive | bool |

#### Invariantes (Validator)

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| Price | NotNull when PortionType != MarketPrice | "Price is required for fixed portion types" |
| Price | >= 0 when HasValue | "Price cannot be negative" |

#### Propiedades Calculadas

| Propiedad | Tipo | Fórmula |
|-----------|------|---------|
| RequiresMarketPrice | bool | `PortionType == MarketPrice && !Price.HasValue` |
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
var option = new PriceOption(
    command.PortionType,
    command.Price,
    command.IsActive);

return priceOptionValidator.ValidateOrThrow(option);
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

### 2.3 CategoryItem (Shared)

#### Estructura

| Propiedad | Tipo |
|-----------|------|
| MenuItem | MenuItem (referencia) |
| DisplayOrder | int |
| PriceOverrides | IReadOnlyCollection&lt;PriceOption&gt; |

#### Invariantes (Validator)

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| MenuItem | NotNull | "MenuItem is required" |
| DisplayOrder | >= 0 | "DisplayOrder must be greater than or equal to 0" |

#### Comportamiento

- **Igualdad**: Dos `CategoryItem` son iguales si referencian el mismo `MenuItem.Id`
- **PriceOverrides vacío**: Se usan los `PriceOptions` del `MenuItem` referenciado
- **PriceOverrides con valores**: Reemplazan completamente los `PriceOptions` del `MenuItem` para este contexto

#### Comando: CategoryItem.Create

**Input**

| Campo | Tipo | Default |
|-------|------|---------|
| MenuItem | MenuItem | |
| DisplayOrder | int | 0 |
| PriceOverrides | HashSet&lt;PriceOption&gt;? | null |

**Inyecta**: `IValidator<CategoryItem>`

**Lógica**
```csharp
var item = new CategoryItem(
    command.MenuItem,
    command.DisplayOrder,
    command.PriceOverrides);

return categoryItemValidator.ValidateOrThrow(item);
```

#### Tests Unitarios

✅ CategoryItem válido sin PriceOverrides
- Input: MenuItem=valid, DisplayOrder=1
- Resultado: CategoryItem creado, PriceOverrides vacío

✅ CategoryItem válido con PriceOverrides
- Input: MenuItem=valid, DisplayOrder=1, PriceOverrides=[{Full, 18.00}]
- Resultado: CategoryItem creado con override de precio

❌ MenuItem null
- Input: MenuItem=null
- Resultado: ValidationException "MenuItem is required"

❌ DisplayOrder negativo
- Input: DisplayOrder=-1
- Resultado: ValidationException "DisplayOrder must be greater than or equal to 0"

---

## 3. Entidades

### 3.1 MenuCategory (Entity)

#### Estructura

| Propiedad | Tipo | Modificador |
|-----------|------|-------------|
| Id | Guid | protected set |
| Name | string | protected set |
| Description | string? | protected set |
| DisplayOrder | int | protected set |
| IsActive | bool | protected set |

#### Colecciones

```csharp
protected HashSet<CategoryItem> _items = [];
public IReadOnlyCollection<CategoryItem> Items => _items.ToList().AsReadOnly();
```

#### Invariantes (Validator)

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| Id | NotEmpty | "Id is required" |
| Name | NotEmpty | "Name is required" |
| Name | MaxLength(100) | "Name cannot exceed 100 characters" |
| Description | MaxLength(500) | "Description cannot exceed 500 characters" |
| DisplayOrder | >= 0 | "DisplayOrder must be greater than or equal to 0" |

#### Comando: MenuCategory.Create

**Input**

| Campo | Tipo | Default |
|-------|------|---------|
| Name | string | |
| Description | string? | null |
| DisplayOrder | int | 0 |

**Inyecta**: `IValidator<MenuCategory>`

**Lógica**
```csharp
var category = new MenuCategory(Guid.NewGuid())
{
    Name = command.Name,
    Description = command.Description,
    DisplayOrder = command.DisplayOrder,
    IsActive = true
};

return categoryValidator.ValidateOrThrow(category);
```

#### Comando: MenuCategory.Update

**Input**

| Campo | Tipo |
|-------|------|
| Name | string |
| Description | string? |
| DisplayOrder | int |

**Inyecta**: `IValidator<MenuCategory>`

**Lógica**
```csharp
category.Name = command.Name;
category.Description = command.Description;
category.DisplayOrder = command.DisplayOrder;

return categoryValidator.ValidateOrThrow(category);
```

#### Comando: MenuCategory.AddItem

**Input**

| Campo | Tipo | Default |
|-------|------|---------|
| MenuItem | MenuItem | |
| DisplayOrder | int | 0 |
| PriceOverrides | HashSet&lt;PriceOption&gt;? | null |

**Inyecta**: `CategoryItem.Create`, `IValidator<MenuCategory>`

**Guards**

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| MenuItem ya existe | 409 | ConflictGuard | "This item already exists in the category" |

**Lógica**
```csharp
var itemExists = category._items.Any(i => i.MenuItem.Id == command.MenuItem.Id);

ConflictGuard.ThrowIf(itemExists, "This item already exists in the category");

var item = createCategoryItem.Execute(new CreateCategoryItemCommand(
    command.MenuItem,
    command.DisplayOrder,
    command.PriceOverrides));

category._items.Add(item);

return categoryValidator.ValidateOrThrow(category);
```

---

## 4. Aggregate: Menu

### Estructura

```
Menu (Aggregate Root)
├─ Id: Guid
├─ TenantId: Guid
├─ Name: string
├─ Description: string?
├─ IsActive: bool
├─ DisplayOrder: int
├─ EffectiveFrom: DateTime?
├─ EffectiveUntil: DateTime?
├─ DepositPolicy: DepositPolicy?
└─ Categories: IReadOnlyCollection<MenuCategory>
```

#### Propiedades

| Propiedad | Tipo | Modificador |
|-----------|------|-------------|
| Id | Guid | protected set |
| TenantId | Guid | protected set |
| Name | string | protected set |
| Description | string? | protected set |
| IsActive | bool | protected set |
| DisplayOrder | int | protected set |
| EffectiveFrom | DateTime? | protected set |
| EffectiveUntil | DateTime? | protected set |
| DepositPolicy | DepositPolicy? | protected set |

#### Colecciones

```csharp
protected HashSet<MenuCategory> _categories = [];
public IReadOnlyCollection<MenuCategory> Categories => _categories.ToList().AsReadOnly();
```

### Invariantes (Validator)

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| Id | NotEmpty | "Id is required" |
| TenantId | NotEmpty | "TenantId is required" |
| Name | NotEmpty | "Name is required" |
| Name | MaxLength(100) | "Name cannot exceed 100 characters" |
| Description | MaxLength(500) | "Description cannot exceed 500 characters" |
| DisplayOrder | >= 0 | "DisplayOrder must be greater than or equal to 0" |
| EffectiveFrom/Until | EffectiveFrom < EffectiveUntil (si ambos presentes) | "Start date must be earlier than end date" |

---

## 5. Response

```csharp
public record MenuResponse(
    Guid Id,
    Guid TenantId,
    string Name,
    string? Description,
    bool IsActive,
    int DisplayOrder,
    DateTime? EffectiveFrom,
    DateTime? EffectiveUntil,
    DepositPolicyResponse? DepositPolicy,
    IReadOnlyCollection<MenuCategoryResponse> Categories
);

public record DepositPolicyResponse(
    DepositType DepositType,
    decimal Amount,
    decimal? Percentage,
    decimal? MinimumBillForDeposit,
    int? MinimumGuestsForDeposit
);

public record MenuCategoryResponse(
    Guid Id,
    string Name,
    string? Description,
    int DisplayOrder,
    bool IsActive,
    IReadOnlyCollection<CategoryItemResponse> Items
);

public record CategoryItemResponse(
    MenuItemSummaryResponse MenuItem,
    int DisplayOrder,
    IReadOnlyCollection<PriceOptionResponse> PriceOverrides
);

public record MenuItemSummaryResponse(
    Guid Id,
    string Name,
    string? Description,
    string? ImageUrl,
    bool IsActive,
    bool IsAvailable
);

public record PriceOptionResponse(
    PortionType PortionType,
    decimal? Price,
    bool IsActive,
    bool RequiresMarketPrice,
    string DisplayPrice
);
```

---

## 6. Event Storming - Leyenda

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

## 7. Lógica de Cálculo de Fianzas

### Interacción entre DepositPolicy (Menu) e ItemDepositOverride (MenuItem)

La fianza de una reserva se calcula combinando la política del menú con los overrides individuales de los items:

**Regla Principal:**
1. **Items CON `DepositOverride` que APLICA**: Se excluyen del cálculo de `DepositPolicy` del menú y se suma directamente su `DepositAmount`
2. **Items SIN `DepositOverride` o que NO APLICA**: Se incluyen en el cálculo de `DepositPolicy` del menú

**Fórmula:**
```
FianzaTotal = DepositPolicy.Calculate(itemsSinOverrideAplicable, guestCount) + SUM(DepositosItemsConOverrideAplicable)
```

### Ejemplo Práctico

```
┌─────────────────────────────────────────────────────────────────────┐
│ CONFIGURACIÓN                                                       │
├─────────────────────────────────────────────────────────────────────┤
│ Menu: DepositPolicy = 10% del total                                 │
│ Reserva: 8 personas                                                 │
├─────────────────────────────────────────────────────────────────────┤
│ ITEMS PEDIDOS                                                       │
├───────────────────┬────────┬──────┬───────┬─────────────────────────┤
│ Item              │ Precio │ Cant │ Total │ Override                │
├───────────────────┼────────┼──────┼───────┼─────────────────────────┤
│ Pulpo             │ €22    │ x2   │ €44   │ €30 si 4+ porciones     │
│                   │        │      │       │ → NO aplica (solo 2)    │
├───────────────────┼────────┼──────┼───────┼─────────────────────────┤
│ Paella            │ €45    │ x1   │ €45   │ €25 (siempre)           │
│                   │        │      │       │ → APLICA €25            │
├───────────────────┼────────┼──────┼───────┼─────────────────────────┤
│ Croquetas         │ €8     │ x3   │ €24   │ Sin override            │
├───────────────────┼────────┼──────┼───────┼─────────────────────────┤
│ Jamón             │ €14    │ x2   │ €28   │ Sin override            │
└───────────────────┴────────┴──────┴───────┴─────────────────────────┘

┌─────────────────────────────────────────────────────────────────────┐
│ CÁLCULO                                                             │
├─────────────────────────────────────────────────────────────────────┤
│ 1. Items para DepositPolicy (SIN override que aplique):             │
│    Pulpo(€44) + Croquetas(€24) + Jamón(€28) = €96                   │
│                                                                     │
│ 2. DepositoMenu: 10% de €96 = €9.60                                 │
│                                                                     │
│ 3. DepositosOverride: €25 (Paella)                                  │
│                                                                     │
│ 4. FianzaTotal = €9.60 + €25 = €34.60                               │
└─────────────────────────────────────────────────────────────────────┘
```

### Casos Especiales

**Caso 1: Solo items sin override**
```
Items: Croquetas(€24) + Jamón(€28) = €52
DepositoMenu: 10% de €52 = €5.20
DepositosOverride: €0
FianzaTotal = €5.20
```

**Caso 2: Solo items con override aplicable**
```
Items: Paella(€45) con Override €25, Cochinillo(€60) con Override €40
DepositoMenu: 10% de €0 = €0 (no hay items sin override)
DepositosOverride: €25 + €40 = €65
FianzaTotal = €65
```

**Caso 3: Override no aplica por cantidad mínima**
```
Items: Pulpo(€44) con Override €30 si 4+ porciones (pidieron solo 2)
→ Override NO aplica, Pulpo entra en cálculo del menú
DepositoMenu: 10% de €44 = €4.40
DepositosOverride: €0
FianzaTotal = €4.40
```

**Caso 4: Menu sin DepositPolicy**
```
Menu: DepositPolicy = null
Items: Paella(€45) con Override €25, Croquetas(€24) sin override
DepositoMenu: €0 (no hay política)
DepositosOverride: €25
FianzaTotal = €25
```

---

## 8. Comandos

> ⚠️ **IMPORTANTE**: El orden de los comandos respeta las dependencias.
> - Las Queries (GetMenu, ListMenus) van después de Create porque son necesarias para verificar persistencia
> - Activate/Deactivate van al final porque dependen de tener Categories con Items

---

### 8.1 Menu.Create ✅

#### Event Storming
```
🟡[RestaurantOwner] → 🔵(CreateMenu) → 🟤[[Menu]] → 🟠<MenuCreated>
```

#### Input

| Campo | Tipo | Default |
|-------|------|---------|
| TenantId | Guid | |
| Name | string | |
| Description | string? | null |
| EffectiveFrom | DateTime? | null |
| EffectiveUntil | DateTime? | null |

#### Inyecta
- `IValidator<Menu>`

#### Guards
Ninguno.

#### Lógica
```csharp
var menu = new Menu(Guid.NewGuid())
{
    TenantId = command.TenantId,
    Name = command.Name,
    Description = command.Description,
    EffectiveFrom = command.EffectiveFrom,
    EffectiveUntil = command.EffectiveUntil,
    DisplayOrder = 0,
    IsActive = false
};

return menuValidator.ValidateOrThrow(menu);
```

#### Slice: POST /menus

**Request**
```csharp
public record CreateMenuRequest(
    string Name,
    string? Description,
    DateTime? EffectiveFrom,
    DateTime? EffectiveUntil
);
```

**Response**: 201 Created → `MenuResponse`

#### Tests Unitarios (Dominio)

✅ Crear menú con datos válidos
- Input: TenantId=valid, Name="Carta Principal", Description="Nuestra carta"
- Resultado: Menu creado con IsActive=false, DisplayOrder=0, Categories vacío

✅ Crear menú con fechas de vigencia
- Input: Name="Menú Navidad", EffectiveFrom=2025-12-01, EffectiveUntil=2025-12-31
- Resultado: Menu creado con fechas configuradas

❌ Name vacío
- Input: Name=""
- Resultado: ValidationException "Name is required"

❌ TenantId vacío
- Input: TenantId=Guid.Empty
- Resultado: ValidationException "TenantId is required"

❌ EffectiveFrom posterior a EffectiveUntil
- Input: EffectiveFrom=2025-12-31, EffectiveUntil=2025-12-01
- Resultado: ValidationException "Start date must be earlier than end date"

#### Tests Integración

✅ 201 Created → MenuResponse

❌ 422 → Validación fallida

---

### 8.2 GetMenu

#### Event Storming
```
🟡[User] → 🔵(GetMenu) → 🟤[[Menu]] → 📊 MenuResponse
```

#### Slice: GET /menus/{id}

**Response**: 200 OK → `MenuResponse`

#### Tests Unitarios (Servicio)

✅ Obtiene el menu del repositorio con el id correcto
- Verifica que repository.GetByIdAsync es llamado con el id

✅ Retorna Response mapeado correctamente
- Verifica que el Response contiene los datos del menu

#### Tests Integración

✅ 200 OK → MenuResponse

❌ 404 → No encontrado

---

### 8.3 ListMenus

#### Event Storming
```
🟡[User] → 🔵(ListMenus) → 🟤[[Menu]] → 📊 MenuResponse[]
```

#### Slice: GET /menus?isActive=true

**QueryParams**: `?isActive=true` (opcional)

**Response**: 200 OK → `MenuResponse[]`

#### Tests Unitarios (Servicio)

✅ Retorna lista de menus mapeados correctamente
- Verifica que el Response contiene los datos de los menus

✅ Filtra por isActive cuando se proporciona
- Verifica que solo retorna menus con el estado indicado

#### Tests Integración

✅ 200 OK → Array de MenuResponse

✅ 200 OK → Array vacío si no hay menús

---

### 8.4 Menu.Update ✅

#### Event Storming
```
🟡[RestaurantOwner] → 🔵(UpdateMenu) → 🟤[[Menu]] → 🟠<MenuUpdated>
```

#### Input

| Campo | Tipo |
|-------|------|
| Name | string |
| Description | string? |
| EffectiveFrom | DateTime? |
| EffectiveUntil | DateTime? |
| DisplayOrder | int |

#### Inyecta
- `IValidator<Menu>`

#### Guards
Ninguno.

#### Lógica
```csharp
menu.Name = command.Name;
menu.Description = command.Description;
menu.EffectiveFrom = command.EffectiveFrom;
menu.EffectiveUntil = command.EffectiveUntil;
menu.DisplayOrder = command.DisplayOrder;

return menuValidator.ValidateOrThrow(menu);
```

#### Slice: PUT /menus/{id}

**Request**
```csharp
public record UpdateMenuRequest(
    string Name,
    string? Description,
    DateTime? EffectiveFrom,
    DateTime? EffectiveUntil,
    int DisplayOrder
);
```

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

✅ Actualizar menú existente
- Precondición: Menu existe
- Input: Name="Carta Actualizada", Description="Nueva descripción"
- Resultado: Menu actualizado

❌ Name vacío
- Input: Name=""
- Resultado: ValidationException "Name is required"

❌ DisplayOrder negativo
- Input: DisplayOrder=-1
- Resultado: ValidationException "DisplayOrder must be greater than or equal to 0"

#### Tests Integración

✅ 204 No Content

❌ 404 → Menu no encontrado

❌ 422 → Validación fallida

---

### 8.5 Menu.SetDepositPolicy ✅

#### Event Storming
```
🟡[RestaurantOwner] → 🔵(SetDepositPolicy) → 🟤[[Menu]] → 🟠<DepositPolicyConfigured>
```

#### Input

| Campo | Tipo | Default |
|-------|------|---------|
| DepositType | DepositType | |
| Amount | decimal | |
| Percentage | decimal? | null |
| MinimumBillForDeposit | decimal? | null |
| MinimumGuestsForDeposit | int? | null |

#### Inyecta
- `DepositPolicy.Create`
- `IValidator<Menu>`

#### Guards
Ninguno.

#### Lógica
```csharp
var depositPolicy = createDepositPolicy.Execute(new CreateDepositPolicyCommand(
    command.DepositType,
    command.Amount,
    command.Percentage,
    command.MinimumBillForDeposit,
    command.MinimumGuestsForDeposit));

menu.DepositPolicy = depositPolicy;

return menuValidator.ValidateOrThrow(menu);
```

#### Slice: PUT /menus/{id}/deposit-policy

**Request**
```csharp
public record SetDepositPolicyRequest(
    DepositType DepositType,
    decimal Amount,
    decimal? Percentage,
    decimal? MinimumBillForDeposit,
    int? MinimumGuestsForDeposit
);
```

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

✅ Configurar política PerPerson
- Input: DepositType=PerPerson, Amount=15, MinimumGuestsForDeposit=6
- Resultado: DepositPolicy configurado

✅ Configurar política PercentageOfBill
- Input: DepositType=PercentageOfBill, Amount=0, Percentage=20
- Resultado: DepositPolicy configurado

❌ Validación de DepositPolicy falla
- Input: DepositType=PercentageOfBill, Percentage=null
- Resultado: ValidationException "Percentage must be specified for PercentageOfBill type"

#### Tests Integración

✅ 204 No Content

❌ 404 → Menu no encontrado

❌ 422 → Validación fallida

---

### 8.6 Menu.RemoveDepositPolicy ✅

#### Event Storming
```
🟡[RestaurantOwner] → 🔵(RemoveDepositPolicy) → 🟤[[Menu]] → 🟠<DepositPolicyRemoved>
```

#### Input
Ninguno

#### Inyecta
- `IValidator<Menu>`

#### Guards
Ninguno.

#### Lógica
```csharp
menu.DepositPolicy = null;

return menuValidator.ValidateOrThrow(menu);
```

#### Slice: DELETE /menus/{id}/deposit-policy

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

✅ Eliminar política existente
- Precondición: Menu con DepositPolicy configurado
- Resultado: DepositPolicy=null

✅ Eliminar política inexistente (idempotente)
- Precondición: Menu sin DepositPolicy
- Resultado: Sin cambios

#### Tests Integración

✅ 204 No Content

❌ 404 → Menu no encontrado

---

### 8.7 Menu.AddCategory ✅

#### Event Storming
```
🟡[RestaurantOwner] → 🔵(AddCategory) → 🟤[[Menu]] → 🟠<CategoryAdded>
                                              │
                                    🟣{UniqueCategoryName}
```

#### Input

| Campo | Tipo | Default |
|-------|------|---------|
| Name | string | |
| Description | string? | null |
| DisplayOrder | int | 0 |

#### Inyecta
- `MenuCategory.Create`
- `IValidator<Menu>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Name ya existe (case-insensitive) | 409 | ConflictGuard | "A category with this name already exists" |

#### Lógica
```csharp
var duplicateName = menu._categories.Any(c => 
    c.Name.Equals(command.Name, StringComparison.OrdinalIgnoreCase));

ConflictGuard.ThrowIf(duplicateName, "A category with this name already exists");

var category = createCategory.Execute(new CreateCategoryCommand(
    command.Name,
    command.Description,
    command.DisplayOrder));

menu._categories.Add(category);

return menuValidator.ValidateOrThrow(menu);
```

#### Slice: POST /menus/{id}/categories

**Request**
```csharp
public record AddCategoryRequest(
    string Name,
    string? Description,
    int DisplayOrder = 0
);
```

**Response**: 201 Created → `MenuResponse`

#### Tests Unitarios (Dominio)

✅ Añadir categoría válida
- Precondición: Menu sin categoría "Entrantes"
- Input: Name="Entrantes", Description="Primeros platos"
- Resultado: Categoría añadida con IsActive=true

❌ Nombre duplicado
- Precondición: Menu ya tiene categoría "Entrantes"
- Input: Name="entrantes" (case-insensitive)
- Resultado: ConflictException "A category with this name already exists"

❌ Validación de categoría falla
- Input: Name=""
- Resultado: ValidationException "Name is required"

#### Tests Integración

✅ 201 Created → MenuResponse con categoría añadida

❌ 404 → Menu no encontrado

❌ 409 → Nombre duplicado

❌ 422 → Validación fallida

---

### 8.8 Menu.UpdateCategory ✅

#### Event Storming
```
🟡[RestaurantOwner] → 🔵(UpdateCategory) → 🟤[[Menu]] → 🟠<CategoryUpdated>
                                                 │
                                       🟣{CategoryExists}
                                       🟣{UniqueCategoryName}
```

#### Input

| Campo | Tipo |
|-------|------|
| CategoryId | Guid |
| Name | string |
| Description | string? |
| DisplayOrder | int |

#### Inyecta
- `MenuCategory.Update`
- `IValidator<Menu>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Categoría no existe | 404 | NotFoundGuard | "Category not found" |
| Nombre duplicado (otra categoría) | 409 | ConflictGuard | "A category with this name already exists" |

#### Lógica
```csharp
var category = menu._categories.FirstOrDefault(c => c.Id == command.CategoryId);

NotFoundGuard.ThrowIfNull(category, command.CategoryId);

var duplicateName = menu._categories.Any(c =>
    c.Id != command.CategoryId &&
    c.Name.Equals(command.Name, StringComparison.OrdinalIgnoreCase));

ConflictGuard.ThrowIf(duplicateName, "A category with this name already exists");

updateCategory.Execute(category!, new UpdateCategoryDetailsCommand(
    command.Name,
    command.Description,
    command.DisplayOrder));

return menuValidator.ValidateOrThrow(menu);
```

#### Slice: PUT /menus/{id}/categories/{categoryId}

**Request**
```csharp
public record UpdateCategoryRequest(
    string Name,
    string? Description,
    int DisplayOrder
);
```

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

✅ Actualizar categoría existente
- Precondición: Menu tiene categoría con Id=X
- Input: CategoryId=X, Name="Entrantes Fríos"
- Resultado: Categoría actualizada

✅ Renombrar con mismo nombre (sin cambio)
- Precondición: Categoría "Entrantes"
- Input: Name="Entrantes"
- Resultado: Sin error

❌ Categoría no existe
- Input: CategoryId=inexistente
- Resultado: NotFoundException

❌ Nombre duplicado con otra categoría
- Precondición: Menu tiene "Entrantes" y "Postres"
- Input: CategoryId de "Postres", Name="Entrantes"
- Resultado: ConflictException "A category with this name already exists"

#### Tests Integración

✅ 204 No Content

❌ 404 → Menu o Categoría no encontrada

❌ 409 → Nombre duplicado

❌ 422 → Validación fallida

---

### 8.9 Menu.RemoveCategory ✅

#### Event Storming
```
🟡[RestaurantOwner] → 🔵(RemoveCategory) → 🟤[[Menu]] → 🟠<CategoryRemoved>
                                                 │
                                       🟣{CategoryExists}
                                       🟣{CategoryEmpty}
```

#### Input

| Campo | Tipo |
|-------|------|
| CategoryId | Guid |

#### Inyecta
- `IValidator<Menu>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Categoría no existe | 404 | NotFoundGuard | "Category not found" |
| Categoría tiene items | 422 | ValidationGuard | "Cannot remove a category that contains items" |

#### Lógica
```csharp
var category = menu._categories.FirstOrDefault(c => c.Id == command.CategoryId);

NotFoundGuard.ThrowIfNull(category, command.CategoryId);

ValidationGuard.ThrowIf(
    category!.Items.Count != 0,
    "Cannot remove a category that contains items",
    "CategoryId");

menu._categories.Remove(category);

return menuValidator.ValidateOrThrow(menu);
```

#### Slice: DELETE /menus/{id}/categories/{categoryId}

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

✅ Eliminar categoría vacía
- Precondición: Categoría sin items
- Resultado: Categoría eliminada

❌ Categoría no existe
- Resultado: NotFoundException

❌ Categoría con items
- Precondición: Categoría tiene items
- Resultado: ValidationException "Cannot remove a category that contains items"

#### Tests Integración

✅ 204 No Content

❌ 404 → Menu o Categoría no encontrada

❌ 422 → Categoría tiene items

---

### 8.10 Menu.AddItemToCategory

#### Event Storming
```
🟡[RestaurantOwner] → 🔵(AddItemToCategory) → 🟤[[Menu]] → 🟠<ItemAddedToCategory>
                                                    │
                                          🟣{CategoryExists}
                                          🟣{ItemNotDuplicate}
```

#### Input

| Campo | Tipo | Default |
|-------|------|---------|
| CategoryId | Guid | |
| MenuItemId | Guid | |
| DisplayOrder | int | 0 |
| PriceOverrides | PriceOptionData[]? | null |

#### Inyecta
- `MenuCategory.AddItem`
- `IValidator<Menu>`
- `IMenuItemRepository` (para obtener el MenuItem)

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Categoría no existe | 404 | NotFoundGuard | "Category not found" |
| MenuItem no existe | 404 | NotFoundGuard | "MenuItem not found" |
| MenuItem ya existe en categoría | 409 | ConflictGuard | "This item already exists in the category" |

#### Lógica
```csharp
var category = menu._categories.FirstOrDefault(c => c.Id == command.CategoryId);
NotFoundGuard.ThrowIfNull(category, command.CategoryId);

var menuItem = await menuItemRepository.GetByIdAsync(command.MenuItemId);
NotFoundGuard.ThrowIfNull(menuItem, command.MenuItemId);

var priceOverrides = command.PriceOverrides?
    .Select(p => createPriceOption.Execute(new CreatePriceOptionCommand(p.PortionType, p.Price, p.IsActive)))
    .ToHashSet();

addItem.Execute(category!, new AddItemCommand(
    menuItem!,
    command.DisplayOrder,
    priceOverrides));

return menuValidator.ValidateOrThrow(menu);
```

#### Slice: POST /menus/{id}/categories/{categoryId}/items

**Request**
```csharp
public record AddItemToCategoryRequest(
    Guid MenuItemId,
    int DisplayOrder = 0,
    PriceOptionData[]? PriceOverrides = null
);

public record PriceOptionData(
    PortionType PortionType,
    decimal? Price,
    bool IsActive = true
);
```

**Response**: 201 Created → `MenuResponse`

#### Tests Unitarios (Dominio)

✅ Añadir item a categoría sin overrides
- Precondición: Categoría existe, MenuItem existe
- Input: CategoryId=X, MenuItemId=Y
- Resultado: CategoryItem añadido, PriceOverrides vacío

✅ Añadir item con PriceOverrides
- Input: CategoryId=X, MenuItemId=Y, PriceOverrides=[{Full, 18.00}]
- Resultado: CategoryItem añadido con override de precio

❌ Categoría no existe
- Resultado: NotFoundException "Category not found"

❌ MenuItem no existe
- Resultado: NotFoundException "MenuItem not found"

❌ MenuItem duplicado
- Precondición: MenuItem ya está en la categoría
- Resultado: ConflictException "This item already exists in the category"

#### Tests Integración

✅ 201 Created → MenuResponse con item añadido

❌ 404 → Menu, Categoría o MenuItem no encontrado

❌ 409 → Item duplicado

❌ 422 → Validación fallida

---

### 8.11 Menu.UpdateCategoryItem

#### Event Storming
```
🟡[RestaurantOwner] → 🔵(UpdateCategoryItem) → 🟤[[Menu]] → 🟠<CategoryItemUpdated>
                                                     │
                                           🟣{CategoryExists}
                                           🟣{ItemExists}
```

#### Input

| Campo | Tipo |
|-------|------|
| CategoryId | Guid |
| MenuItemId | Guid |
| DisplayOrder | int |
| PriceOverrides | PriceOptionData[]? |

#### Inyecta
- `CategoryItem.Create`
- `PriceOption.Create`
- `IValidator<Menu>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Categoría no existe | 404 | NotFoundGuard | "Category not found" |
| Item no existe en categoría | 404 | NotFoundGuard | "Item not found in category" |

#### Lógica
```csharp
var category = menu._categories.FirstOrDefault(c => c.Id == command.CategoryId);
NotFoundGuard.ThrowIfNull(category, command.CategoryId);

var existingItem = category!._items.FirstOrDefault(i => i.MenuItem.Id == command.MenuItemId);
NotFoundGuard.ThrowIfNull(existingItem, "Item not found in category");

var priceOverrides = command.PriceOverrides?
    .Select(p => createPriceOption.Execute(new CreatePriceOptionCommand(p.PortionType, p.Price, p.IsActive)))
    .ToHashSet();

var updatedItem = createCategoryItem.Execute(new CreateCategoryItemCommand(
    existingItem!.MenuItem,
    command.DisplayOrder,
    priceOverrides));

category._items.Remove(existingItem);
category._items.Add(updatedItem);

return menuValidator.ValidateOrThrow(menu);
```

#### Slice: PUT /menus/{id}/categories/{categoryId}/items/{menuItemId}

**Request**
```csharp
public record UpdateCategoryItemRequest(
    int DisplayOrder,
    PriceOptionData[]? PriceOverrides
);
```

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

✅ Actualizar DisplayOrder
- Precondición: Item existe con DisplayOrder=0
- Input: DisplayOrder=5
- Resultado: Item con DisplayOrder=5

✅ Configurar PriceOverrides
- Precondición: Item sin overrides
- Input: PriceOverrides=[{Full, 18.00}]
- Resultado: Item con override de precio

✅ Eliminar PriceOverrides
- Precondición: Item con overrides
- Input: PriceOverrides=null
- Resultado: Item sin overrides (usa precios del MenuItem)

❌ Categoría no existe
- Resultado: NotFoundException "Category not found"

❌ Item no existe en categoría
- Resultado: NotFoundException "Item not found in category"

#### Tests Integración

✅ 204 No Content

❌ 404 → Menu, Categoría o Item no encontrado

❌ 422 → Validación fallida

---

### 8.12 Menu.RemoveItemFromCategory

#### Event Storming
```
🟡[RestaurantOwner] → 🔵(RemoveItemFromCategory) → 🟤[[Menu]] → 🟠<ItemRemovedFromCategory>
                                                        │
                                              🟣{CategoryExists}
                                              🟣{ItemExists}
```

#### Input

| Campo | Tipo |
|-------|------|
| CategoryId | Guid |
| MenuItemId | Guid |

#### Inyecta
- `IValidator<Menu>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Categoría no existe | 404 | NotFoundGuard | "Category not found" |
| Item no existe en categoría | 404 | NotFoundGuard | "Item not found in category" |

#### Lógica
```csharp
var category = menu._categories.FirstOrDefault(c => c.Id == command.CategoryId);
NotFoundGuard.ThrowIfNull(category, command.CategoryId);

var item = category!._items.FirstOrDefault(i => i.MenuItem.Id == command.MenuItemId);
NotFoundGuard.ThrowIfNull(item, "Item not found in category");

category._items.Remove(item!);

return menuValidator.ValidateOrThrow(menu);
```

#### Slice: DELETE /menus/{id}/categories/{categoryId}/items/{menuItemId}

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

✅ Eliminar item de categoría
- Precondición: Item existe en categoría
- Resultado: Item eliminado

❌ Categoría no existe
- Resultado: NotFoundException "Category not found"

❌ Item no existe en categoría
- Resultado: NotFoundException "Item not found in category"

#### Tests Integración

✅ 204 No Content

❌ 404 → Menu, Categoría o Item no encontrado

---

### 8.13 Menu.Activate

#### Event Storming
```
🟡[RestaurantOwner] → 🔵(ActivateMenu) → 🟤[[Menu]] → 🟠<MenuActivated>
                                               │
                                     🟣{TieneCategorias}
                                     🟣{TieneItemsEnCategorias}
```

#### Input
Ninguno

#### Inyecta
- `IValidator<Menu>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Ya está activo | 409 | ConflictGuard | "Menu is already active" |
| No tiene categorías | 422 | ValidationGuard | "Menu must have at least one category" |
| Ninguna categoría tiene items | 422 | ValidationGuard | "Menu must have at least one category with items" |

#### Lógica
```csharp
ConflictGuard.ThrowIf(menu.IsActive, "Menu is already active");

ValidationGuard.ThrowIf(
    !menu.Categories.Any(),
    "Menu must have at least one category",
    nameof(menu.Categories));

ValidationGuard.ThrowIf(
    !menu.Categories.Any(c => c.Items.Any()),
    "Menu must have at least one category with items",
    nameof(menu.Categories));

menu.IsActive = true;

return menuValidator.ValidateOrThrow(menu);
```

#### Slice: POST /menus/{id}/activate

**Response**: 200 OK → `MenuResponse`

#### Tests Unitarios (Dominio)

✅ Activar menú con categorías e items
- Precondición: Menu con IsActive=false, tiene categoría con items
- Resultado: Menu con IsActive=true

❌ Menú ya activo
- Precondición: Menu con IsActive=true
- Resultado: ConflictException "Menu is already active"

❌ Menú sin categorías
- Precondición: Menu sin categorías
- Resultado: ValidationException "Menu must have at least one category"

❌ Menú con categorías vacías
- Precondición: Menu con categorías pero sin items en ninguna
- Resultado: ValidationException "Menu must have at least one category with items"

#### Tests Integración

✅ 200 OK → MenuResponse con IsActive=true

❌ 404 → Menu no encontrado

❌ 409 → Ya estaba activo

❌ 422 → Falta categoría o items

---

### 8.14 Menu.Deactivate

#### Event Storming
```
🟡[RestaurantOwner] → 🔵(DeactivateMenu) → 🟤[[Menu]] → 🟠<MenuDeactivated>
```

#### Input
Ninguno

#### Inyecta
- `IValidator<Menu>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Ya está inactivo | 409 | ConflictGuard | "Menu is already inactive" |

#### Lógica
```csharp
ConflictGuard.ThrowIf(!menu.IsActive, "Menu is already inactive");

menu.IsActive = false;

return menuValidator.ValidateOrThrow(menu);
```

#### Slice: POST /menus/{id}/deactivate

**Response**: 200 OK → `MenuResponse`

#### Tests Unitarios (Dominio)

✅ Desactivar menú activo
- Precondición: Menu con IsActive=true
- Resultado: Menu con IsActive=false

❌ Menú ya inactivo
- Precondición: Menu con IsActive=false
- Resultado: ConflictException "Menu is already inactive"

#### Tests Integración

✅ 200 OK → MenuResponse con IsActive=false

❌ 404 → Menu no encontrado

❌ 409 → Ya estaba inactivo

---

## 9. Resumen de Endpoints (Orden de Implementación)

| # | Método | Ruta | Comando/Query | Response | Estado |
|---|--------|------|---------------|----------|--------|
| 1 | POST | /menus | Menu.Create | 201 → `MenuResponse` | ✅ |
| 2 | GET | /menus/{id} | GetMenu | 200 → `MenuResponse` | |
| 3 | GET | /menus | ListMenus | 200 → `MenuResponse[]` | |
| 4 | PUT | /menus/{id} | Menu.Update | 204 | ✅ |
| 5 | PUT | /menus/{id}/deposit-policy | Menu.SetDepositPolicy | 204 | ✅ |
| 6 | DELETE | /menus/{id}/deposit-policy | Menu.RemoveDepositPolicy | 204 | ✅ |
| 7 | POST | /menus/{id}/categories | Menu.AddCategory | 201 → `MenuResponse` | ✅ |
| 8 | PUT | /menus/{id}/categories/{categoryId} | Menu.UpdateCategory | 204 | ✅ |
| 9 | DELETE | /menus/{id}/categories/{categoryId} | Menu.RemoveCategory | 204 | ✅ |
| 10 | POST | /menus/{id}/categories/{categoryId}/items | Menu.AddItemToCategory | 201 → `MenuResponse` | |
| 11 | PUT | /menus/{id}/categories/{categoryId}/items/{menuItemId} | Menu.UpdateCategoryItem | 204 | |
| 12 | DELETE | /menus/{id}/categories/{categoryId}/items/{menuItemId} | Menu.RemoveItemFromCategory | 204 | |
| 13 | POST | /menus/{id}/activate | Menu.Activate | 200 → `MenuResponse` | |
| 14 | POST | /menus/{id}/deactivate | Menu.Deactivate | 200 → `MenuResponse` | |

---

## 10. Persistencia (Firestore)

### Colección

`/menus/{menuId}`

### Configuración DbContext

```csharp
modelBuilder.Entity<Menu>(entity =>
{
    // QueryFilter: multi-tenancy by TenantId
    entity.HasQueryFilter(m => m.TenantId == tenantId);

    // ComplexType: DepositPolicy
    entity.ComplexProperty(m => m.DepositPolicy);

    // ArrayOf: Categories (usa backing field _categories)
    entity.ArrayOf(m => m.Categories, category =>
    {
        // ArrayOf: Items dentro de Category (usa backing field _items)
        category.ArrayOf(c => c.Items, item =>
        {
            // Reference: MenuItem
            item.Property(i => i.MenuItem).AsReference();

            // Ignore: propiedades calculadas de PriceOption
            item.ArrayOf(i => i.PriceOverrides, po =>
            {
                po.Ignore(p => p.RequiresMarketPrice);
                po.Ignore(p => p.DisplayPrice);
            });
        });
    });
});
```

### Documento Ejemplo

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "tenantId": "tenant-001-guid",
  "name": "Carta Principal",
  "description": "Nuestra carta de especialidades",
  "isActive": true,
  "displayOrder": 1,
  "effectiveFrom": null,
  "effectiveUntil": null,
  "depositPolicy": {
    "depositType": 1,
    "amount": 15.00,
    "percentage": null,
    "minimumBillForDeposit": null,
    "minimumGuestsForDeposit": 6
  },
  "categories": [
    {
      "id": "category-001-guid",
      "name": "Pescados y Mariscos",
      "description": "Productos frescos del día",
      "displayOrder": 1,
      "isActive": true,
      "items": [
        {
          "menuItem": {
            "__ref__": "/menu-items/menuitem-001-guid"
          },
          "displayOrder": 1,
          "priceOverrides": [
            {
              "portionType": 4,
              "price": 25.00,
              "isActive": true
            }
          ]
        }
      ]
    }
  ]
}
```

---

## 11. Hot Spots ⚠️

| # | Pregunta | Estado |
|---|----------|--------|
| 1 | ¿Se debe validar que los PortionTypes en PriceOverrides sean únicos? | Pendiente |

---

**Fecha**: 2025-01-30
**Autor**: Equipo Fudie
