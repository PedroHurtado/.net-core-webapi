# Domain Specification: Plan

---

## 1. Enums

### BillingPeriod

```csharp
public enum BillingPeriod
{
    Monthly,
    Quarterly,
    Semester,
    Yearly
}
```

### FeatureType

```csharp
public enum FeatureType
{
    Boolean,
    Limit,
    Unlimited
}
```

---

## 2. Value Objects

### 2.1 Currency

#### Estructura

| Propiedad | Tipo |
|-----------|------|
| Code | string |
| Symbol | string |
| DecimalPlaces | int |

#### Validaciones

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| Code | NotEmpty | "Currency code is required" |
| Code | Length(3) | "Currency code must be exactly 3 characters" |
| Code | Uppercase | "Currency code must be uppercase" |
| Symbol | NotEmpty | "Currency symbol is required" |
| Symbol | Max(5) | "Currency symbol cannot exceed 5 characters" |
| DecimalPlaces | Between(0, 4) | "Decimal places must be between 0 and 4" |

#### Comando: Currency.Create

**Input**

| Campo | Tipo | Default |
|-------|------|---------|
| Code | string | |
| Symbol | string | |
| DecimalPlaces | int | 2 |

**Inyecta**: `IValidator<Currency>`

**Lógica**
```csharp
var currency = new Currency(command.Code, command.Symbol, command.DecimalPlaces);

return currencyValidator.ValidateOrThrow(currency);
```

**Estáticos**: `Currency.EUR`, `Currency.USD`, `Currency.GBP`, `Currency.FromCode(string)`

#### Tests Unitarios

✅ Currency válida
- Input: Code="EUR", Symbol="€", DecimalPlaces=2
- Resultado: Currency creada

✅ Currency con 0 decimales (JPY)
- Input: Code="JPY", Symbol="¥", DecimalPlaces=0
- Resultado: Currency creada

❌ Code vacío
- Input: Code=""
- Resultado: ValidationException "Currency code is required"

❌ Code con longitud incorrecta
- Input: Code="EU"
- Resultado: ValidationException "Currency code must be exactly 3 characters"

❌ Code en minúsculas
- Input: Code="eur"
- Resultado: ValidationException "Currency code must be uppercase"

❌ Symbol vacío
- Input: Symbol=""
- Resultado: ValidationException "Currency symbol is required"

❌ DecimalPlaces fuera de rango
- Input: DecimalPlaces=5
- Resultado: ValidationException "Decimal places must be between 0 and 4"

---

### 2.2 Money

#### Estructura

| Propiedad | Tipo |
|-----------|------|
| Amount | decimal |
| Currency | Currency |

#### Validaciones

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| Amount | >= 0 | "Amount cannot be negative" |
| Currency | NotNull | "Currency is required" |

#### Propiedades Calculadas

| Propiedad | Tipo | Fórmula |
|-----------|------|---------|
| IsZero | bool | `Amount == 0` |
| IsPositive | bool | `Amount > 0` |
| IsNegative | bool | `Amount < 0` |

#### Métodos

- `Add(Money other)` → Money
- `Subtract(Money other)` → Money
- `Multiply(decimal factor)` → Money

#### Comando: Money.Create

**Input**

| Campo | Tipo |
|-------|------|
| Amount | decimal |
| CurrencyCode | string |

**Inyecta**: `Currency.Create`, `IValidator<Money>`

**Lógica**
```csharp
var currency = Currency.FromCode(command.CurrencyCode);
var money = new Money(command.Amount, currency);

return moneyValidator.ValidateOrThrow(money);
```

#### Tests Unitarios

✅ Money válido
- Input: Amount=9.99, CurrencyCode="EUR"
- Resultado: Money creado

✅ Money con Amount=0
- Input: Amount=0, CurrencyCode="EUR"
- Resultado: Money creado (IsZero=true)

❌ Amount negativo
- Input: Amount=-5
- Resultado: ValidationException "Amount cannot be negative"

❌ CurrencyCode no soportado
- Input: CurrencyCode="XXX"
- Resultado: ArgumentException "Currency XXX not supported"

---

### 2.3 Feature

#### Estructura

| Propiedad | Tipo |
|-----------|------|
| Code | string |
| Name | string |
| Description | string? |
| Type | FeatureType |
| Limit | int? |
| Unit | string? |

#### Validaciones

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| Code | NotEmpty | "Feature code is required" |
| Code | Max(50) | "Feature code cannot exceed 50 characters" |
| Code | Uppercase | "Feature code must be uppercase" |
| Code | NoSpaces | "Feature code cannot contain spaces" |
| Name | NotEmpty | "Feature name is required" |
| Name | Max(100) | "Feature name cannot exceed 100 characters" |
| Description | Max(250) | "Feature description cannot exceed 250 characters" |
| Limit | NotNull when Type=Limit | "Limit is required when feature type is Limit" |
| Limit | > 0 when HasValue | "Limit must be greater than 0" |
| Limit | Null when Type=Boolean | "Limit is not allowed for Boolean feature type" |
| Limit | Null when Type=Unlimited | "Limit is not allowed for Unlimited feature type" |
| Unit | Max(50) | "Unit cannot exceed 50 characters" |

#### Propiedades Calculadas

| Propiedad | Tipo | Fórmula |
|-----------|------|---------|
| IsValid | bool | Según Type y Limit |
| DisplayValue | string | "100 reservas/mes", "Ilimitado", "Incluido" |

#### Comando: Feature.Create

**Input**

| Campo | Tipo |
|-------|------|
| Code | string |
| Name | string |
| Description | string? |
| Type | FeatureType |
| Limit | int? |
| Unit | string? |

**Inyecta**: `IValidator<Feature>`

**Lógica**
```csharp
var feature = new Feature(
    command.Code,
    command.Name,
    command.Description,
    command.Type,
    command.Limit,
    command.Unit);

return featureValidator.ValidateOrThrow(feature);
```

#### Tests Unitarios

✅ Feature tipo Limit válido
- Input: Code="RESERVATIONS_MONTHLY", Name="Reservas", Type=Limit, Limit=100, Unit="reservas/mes"
- Resultado: Feature creado, DisplayValue="100 reservas/mes"

✅ Feature tipo Boolean válido
- Input: Code="PRIORITY_SUPPORT", Name="Soporte prioritario", Type=Boolean
- Resultado: Feature creado, DisplayValue="Incluido"

✅ Feature tipo Unlimited válido
- Input: Code="RESERVATIONS_MONTHLY", Name="Reservas", Type=Unlimited
- Resultado: Feature creado, DisplayValue="Ilimitado"

❌ Code vacío
- Input: Code=""
- Resultado: ValidationException "Feature code is required"

❌ Code con espacios
- Input: Code="RESERVATIONS MONTHLY"
- Resultado: ValidationException "Feature code cannot contain spaces"

❌ Code en minúsculas
- Input: Code="reservations_monthly"
- Resultado: ValidationException "Feature code must be uppercase"

❌ Name vacío
- Input: Name=""
- Resultado: ValidationException "Feature name is required"

❌ Type=Limit sin Limit
- Input: Type=Limit, Limit=null
- Resultado: ValidationException "Limit is required when feature type is Limit"

❌ Type=Limit con Limit=0
- Input: Type=Limit, Limit=0
- Resultado: ValidationException "Limit must be greater than 0"

❌ Type=Limit con Limit negativo
- Input: Type=Limit, Limit=-10
- Resultado: ValidationException "Limit must be greater than 0"

❌ Type=Boolean con Limit
- Input: Type=Boolean, Limit=100
- Resultado: ValidationException "Limit is not allowed for Boolean feature type"

❌ Type=Unlimited con Limit
- Input: Type=Unlimited, Limit=100
- Resultado: ValidationException "Limit is not allowed for Unlimited feature type"

---

### 2.4 PaymentProviderConfig

#### Estructura

| Propiedad | Tipo |
|-----------|------|
| Provider | string |
| ExternalProductId | string |
| ExternalPriceId | string |
| IsActive | bool |

#### Validaciones

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| Provider | NotEmpty | "Provider is required" |
| Provider | Max(50) | "Provider cannot exceed 50 characters" |
| ExternalProductId | NotEmpty | "External product ID is required" |
| ExternalProductId | Max(100) | "External product ID cannot exceed 100 characters" |
| ExternalPriceId | NotEmpty | "External price ID is required" |
| ExternalPriceId | Max(100) | "External price ID cannot exceed 100 characters" |

#### Comando: PaymentProviderConfig.Create

**Input**

| Campo | Tipo | Default |
|-------|------|---------|
| Provider | string | |
| ExternalProductId | string | |
| ExternalPriceId | string | |
| IsActive | bool | true |

**Inyecta**: `IValidator<PaymentProviderConfig>`

**Lógica**
```csharp
var config = new PaymentProviderConfig(
    command.Provider,
    command.ExternalProductId,
    command.ExternalPriceId,
    command.IsActive);

return configValidator.ValidateOrThrow(config);
```

#### Tests Unitarios

✅ Config válida activa
- Input: Provider="Stripe", ExternalProductId="prod_xxx", ExternalPriceId="price_xxx"
- Resultado: Config creada con IsActive=true

✅ Config válida inactiva
- Input: Provider="Stripe", ExternalProductId="prod_xxx", ExternalPriceId="price_xxx", IsActive=false
- Resultado: Config creada con IsActive=false

❌ Provider vacío
- Input: Provider=""
- Resultado: ValidationException "Provider is required"

❌ ExternalProductId vacío
- Input: ExternalProductId=""
- Resultado: ValidationException "External product ID is required"

❌ ExternalPriceId vacío
- Input: ExternalPriceId=""
- Resultado: ValidationException "External price ID is required"

---

### 2.5 PricingTier

#### Estructura

| Propiedad | Tipo |
|-----------|------|
| BillingPeriod | BillingPeriod |
| Price | Money |
| IsActive | bool |
| ProviderConfigurations | IReadOnlyCollection\<PaymentProviderConfig\> |

#### Propiedades Calculadas

| Propiedad | Tipo | Fórmula |
|-----------|------|---------|
| HasActiveProvider | bool | `ProviderConfigurations.Any(p => p.IsActive)` |

#### Validaciones

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| BillingPeriod | IsEnum | |
| Price | NotNull | "Price is required" |

#### Comando: PricingTier.Create

**Input**

| Campo | Tipo | Default |
|-------|------|---------|
| BillingPeriod | BillingPeriod | |
| Amount | decimal | |
| CurrencyCode | string | |
| IsActive | bool | false |

**Inyecta**: `Money.Create`, `IValidator<PricingTier>`

**Lógica**
```csharp
var money = moneyCreate.Execute(new CreateMoneyCommand(command.Amount, command.CurrencyCode));

var tier = new PricingTier(
    command.BillingPeriod,
    money,
    command.IsActive,
    Array.Empty<PaymentProviderConfig>().AsReadOnly());

return pricingTierValidator.ValidateOrThrow(tier);
```

#### Transform: PricingTier.Activate

Sin comando. Devuelve `current with { IsActive = true }`.

#### Transform: PricingTier.Deactivate

Sin comando. Devuelve `current with { IsActive = false }`.

#### Transform: PricingTier.UpdatePrice

**Input**

| Campo | Tipo |
|-------|------|
| Amount | decimal |
| CurrencyCode | string |

**Inyecta**: `Money.Create`, `IValidator<PricingTier>`

**Lógica**: Crea nuevo Money → `current with { Price = newMoney }` → ValidateOrThrow.

#### Tests Unitarios

✅ PricingTier válido
- Input: BillingPeriod=Monthly, Amount=9.99, CurrencyCode="EUR", IsActive=false
- Resultado: PricingTier creado con ProviderConfigurations vacío

✅ PricingTier activo
- Input: IsActive=true
- Resultado: PricingTier creado con IsActive=true

❌ Amount negativo
- Input: Amount=-5
- Resultado: ValidationException "Amount cannot be negative"

✅ Activate → IsActive=true
✅ Deactivate → IsActive=false
✅ UpdatePrice → nuevo Money preservando IsActive y ProviderConfigurations

---

## 3. Aggregate: Plan

### Estructura

```
Plan (Aggregate Root)
├─ Id: Guid
├─ Name: string
├─ Description: string
├─ IsActive: bool
├─ Features: IReadOnlyCollection<Feature>
└─ PricingTiers: IReadOnlyCollection<PricingTier>
    ├─ BillingPeriod: BillingPeriod
    ├─ Price: Money
    ├─ IsActive: bool
    └─ ProviderConfigurations: IReadOnlyCollection<PaymentProviderConfig>
```

#### Propiedades

| Propiedad | Tipo | Modificador |
|-----------|------|-------------|
| Id | Guid | init |
| Name | string | protected set |
| Description | string | protected set |
| IsActive | bool | protected set |

#### Colecciones

```csharp
protected HashSet<Feature> _features = [];
public IReadOnlyCollection<Feature> Features => _features.ToList().AsReadOnly();

protected HashSet<PricingTier> _pricingTiers = [];
public IReadOnlyCollection<PricingTier> PricingTiers => _pricingTiers.ToList().AsReadOnly();
```

#### Propiedades Calculadas

| Propiedad | Tipo | Fórmula |
|-----------|------|---------|
| HasActivePricingTierWithProvider | bool | `_pricingTiers.Any(t => t.IsActive && t.HasActiveProvider)` |

### Validaciones

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| Id | NotEmpty | "Id is required" |
| Name | NotEmpty | "Name is required" |
| Name | Max(100) | "Name cannot exceed 100 characters" |
| Description | NotEmpty | "Description is required" |
| Description | Max(500) | "Description cannot exceed 500 characters" |

---

## 4. Response

```csharp
public record PlanResponse(
    Guid Id,
    string Name,
    string Description,
    bool IsActive,
    bool HasActivePricingTierWithProvider,
    IReadOnlyCollection<FeatureResponse> Features,
    IReadOnlyCollection<PricingTierResponse> PricingTiers
);

public record PricingTierResponse(
    BillingPeriod BillingPeriod,
    MoneyResponse Price,
    bool IsActive,
    bool HasActiveProvider,
    IReadOnlyCollection<ProviderConfigResponse> ProviderConfigurations
);

public record MoneyResponse(
    decimal Amount,
    CurrencyResponse Currency
);

public record CurrencyResponse(
    string Code,
    string Symbol,
    int DecimalPlaces
);

public record FeatureResponse(
    string Code,
    string Name,
    string? Description,
    FeatureType Type,
    int? Limit,
    string? Unit,
    string DisplayValue
);

public record ProviderConfigResponse(
    string Provider,
    string ExternalProductId,
    string ExternalPriceId,
    bool IsActive
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

## 6. Comandos y Slices (Orden de Implementación)

---

### 6.1 Plan.Create

#### Event Storming
```
🟡[Admin] → 🔵(CreatePlan) → 🟤[[Plan]] → 🟠<PlanCreated>
```

#### Dominio

**Input**

| Campo | Tipo |
|-------|------|
| Name | string |
| Description | string |

**Inyecta**
- `IValidator<Plan>`

**Guards**

Ninguno.

**Lógica**
```csharp
var plan = new Plan(Guid.NewGuid())
{
    Name = command.Name,
    Description = command.Description,
    IsActive = false
};

return planValidator.ValidateOrThrow(plan);
```

**Tests Unitarios Dominio**

✅ Crear plan con datos válidos
- Input: Name="Plan Básico", Description="Ideal para empezar"
- Resultado: Plan creado con IsActive=false, Features vacío, PricingTiers vacío

❌ Name vacío
- Input: Name=""
- Resultado: ValidationException "Name is required"

❌ Description vacía
- Input: Description=""
- Resultado: ValidationException "Description is required"

#### Slice: POST /plans

**Request**
```csharp
public record CreatePlanRequest(
    string Name,
    string Description
);
```

**Response**: 201 Created → `PlanResponse`

**Tests Unitarios Servicio**

✅ Llama a Plan.Create con los parámetros correctos
- Verifica que se invoca planCreate.Execute con el command correcto

✅ Añade el plan al repositorio
- Verifica que repository.Add es llamado con el plan creado

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

✅ Retorna Response mapeado correctamente
- Verifica que el Response contiene los datos del plan

**Tests Integración**

✅ 201 Created → PlanResponse con IsActive=false, PricingTiers vacío

❌ 422 → Validación fallida

---

### 6.2 GetPlan

#### Event Storming
```
🟡[Admin] → 🔵(GetPlan) → 🟤[[Plan]] → 📊 PlanResponse
```

#### Slice: GET /plans/{id}

**Response**: 200 OK → `PlanResponse`

**Tests Unitarios Servicio**

✅ Obtiene el plan del repositorio con el id correcto
- Verifica que repository.Get es llamado con el id

✅ Retorna Response mapeado correctamente
- Verifica que el Response contiene los datos del plan

**Tests Integración**

✅ 200 OK → PlanResponse

❌ 404 → No encontrado

---

### 6.3 ListPlans

#### Event Storming
```
🟡[Admin] → 🔵(ListPlans) → 🟤[[Plan]] → 📊 PlanResponse[]
```

#### Slice: GET /plans

**QueryParams**: `?isActive=true` (opcional)

**Response**: 200 OK → `PlanResponse[]`

**Tests Unitarios Servicio**

✅ Retorna lista de planes mapeados correctamente
- Verifica que el Response contiene los datos de los planes

✅ Filtra por isActive cuando se proporciona
- Verifica que solo retorna planes con el estado indicado

**Tests Integración**

✅ 200 OK → Array de PlanResponse

✅ 200 OK → Array vacío si no hay planes

---

### 6.4 Plan.Update

#### Event Storming
```
🟡[Admin] → 🔵(UpdatePlan) → 🟤[[Plan]] → 🟠<PlanUpdated>
```

#### Dominio

**Input**

| Campo | Tipo |
|-------|------|
| Name | string |
| Description | string |

**Inyecta**
- `IValidator<Plan>`

**Guards**

Ninguno.

**Lógica**
```csharp
plan.Name = command.Name;
plan.Description = command.Description;

return planValidator.ValidateOrThrow(plan);
```

**Tests Unitarios Dominio**

✅ Actualizar plan existente
- Precondición: Plan existe
- Input: Name="Plan Actualizado", Description="Nueva descripción"
- Resultado: Plan actualizado

❌ Name vacío
- Input: Name=""
- Resultado: ValidationException "Name is required"

#### Slice: PUT /plans/{id}

**Request**
```csharp
public record UpdatePlanRequest(
    string Name,
    string Description
);
```

**Response**: 204 No Content

**Tests Unitarios Servicio**

✅ Obtiene el plan del repositorio
- Verifica que repository.Get es llamado con el id correcto

✅ Llama a Plan.Update con los parámetros correctos
- Verifica que se invoca planUpdate.Execute con el command correcto

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

**Tests Integración**

✅ 204 No Content

✅ Persistencia: PUT → GET → verificar datos actualizados

❌ 404 → Plan no encontrado

❌ 422 → Validación fallida

---

### 6.5 Plan.AddFeature

#### Event Storming
```
🟡[Admin] → 🔵(AddFeature) → 🟤[[Plan]] → 🟠<FeatureAdded>
                                  │
                        🟣{CodeÚnico}
```

#### Dominio

**Input**

| Campo | Tipo |
|-------|------|
| Code | string |
| Name | string |
| Description | string? |
| Type | FeatureType |
| Limit | int? |
| Unit | string? |

**Inyecta**
- `Feature.Create`
- `IValidator<Plan>`

**Guards**

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Code ya existe | 409 | ConflictGuard | "Feature with code '{Code}' already exists" |

**Lógica**
```csharp
ConflictGuard.ThrowIf(
    plan.Features.Any(f => f.Code == command.Code),
    $"Feature with code '{command.Code}' already exists");

var feature = featureCreate.Execute(new CreateFeatureCommand(
    command.Code,
    command.Name,
    command.Description,
    command.Type,
    command.Limit,
    command.Unit));

plan._features.Add(feature);

return planValidator.ValidateOrThrow(plan);
```

**Tests Unitarios Dominio**

✅ Añadir Feature tipo Limit
✅ Añadir Feature tipo Boolean
✅ Añadir Feature tipo Unlimited
❌ Code duplicado → ConflictException
❌ Validación de Feature falla

#### Slice: POST /plans/{id}/features

**Request**
```csharp
public record AddFeatureRequest(
    string Code,
    string Name,
    string? Description,
    FeatureType Type,
    int? Limit,
    string? Unit
);
```

**Response**: 201 Created → `PlanResponse`

**Tests Integración**

✅ 201 Created → PlanResponse con Feature añadido
✅ Persistencia: POST → GET → verificar Feature añadido
❌ 404 → Plan no encontrado
❌ 409 → Code duplicado
❌ 422 → Validación fallida

---

### 6.6 Plan.UpdateFeature

#### Event Storming
```
🟡[Admin] → 🔵(UpdateFeature) → 🟤[[Plan]] → 🟠<FeatureUpdated>
                                    │
                          🟣{FeatureExiste}
```

#### Dominio

**Input**

| Campo | Tipo |
|-------|------|
| Name | string |
| Description | string? |
| Type | FeatureType |
| Limit | int? |
| Unit | string? |

*Code viene en la ruta*

**Inyecta**: `Feature.Create`, `IValidator<Plan>`

**Guards**

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Feature no existe | 404 | NotFoundGuard | "Feature with code '{Code}' not found" |

#### Slice: PUT /plans/{id}/features/{code}

**Response**: 204 No Content

**Tests Integración**

✅ 204 No Content
✅ Persistencia: PUT → GET → verificar Feature actualizado
❌ 404 → Plan o Feature no encontrado
❌ 422 → Validación fallida

---

### 6.7 Plan.RemoveFeature

#### Event Storming
```
🟡[Admin] → 🔵(RemoveFeature) → 🟤[[Plan]] → 🟠<FeatureRemoved>
                                    │
                          🟣{FeatureExiste}
                          🟣{NoEsElÚltimo}
```

#### Dominio

*Code viene en la ruta*

**Inyecta**: `IValidator<Plan>`

**Guards**

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Feature no existe | 404 | NotFoundGuard | "Feature with code '{Code}' not found" |
| Es el último y Plan activo | 422 | ValidationGuard | "Cannot remove last feature from active plan" |

#### Slice: DELETE /plans/{id}/features/{code}

**Response**: 204 No Content

**Tests Integración**

✅ 204 No Content
✅ Persistencia: DELETE → GET → verificar Feature eliminado
❌ 404 → Plan o Feature no encontrado
❌ 422 → Es el último en plan activo

---

### 6.8 Plan.AddPricingTier

#### Event Storming
```
🟡[Admin] → 🔵(AddPricingTier) → 🟤[[Plan]] → 🟠<PricingTierAdded>
                                       │
                             🟣{BillingPeriodÚnico}
```

#### Dominio

**Input**

| Campo | Tipo | Default |
|-------|------|---------|
| BillingPeriod | BillingPeriod | |
| Amount | decimal | |
| CurrencyCode | string | |
| IsActive | bool | false |

**Inyecta**
- `PricingTier.Create`
- `IValidator<Plan>`

**Guards**

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| BillingPeriod ya existe | 409 | ConflictGuard | "Pricing tier with billing period '{BillingPeriod}' already exists in the plan" |

**Lógica**
```csharp
ConflictGuard.ThrowIf(
    plan.PricingTiers.Any(t => t.BillingPeriod == command.BillingPeriod),
    $"Pricing tier with billing period '{command.BillingPeriod}' already exists in the plan");

var tier = pricingTierCreate.Execute(new CreatePricingTierCommand(
    command.BillingPeriod,
    command.Amount,
    command.CurrencyCode,
    command.IsActive));

plan._pricingTiers.Add(tier);

return planValidator.ValidateOrThrow(plan);
```

**Tests Unitarios Dominio**

✅ Añadir PricingTier Monthly
- Input: BillingPeriod=Monthly, Amount=9.99, CurrencyCode="EUR"
- Resultado: Tier añadido con IsActive=false, ProviderConfigurations vacío

✅ Añadir múltiples períodos (Monthly + Yearly)
- Resultado: Plan con 2 PricingTiers

❌ BillingPeriod duplicado
- Precondición: Plan ya tiene Monthly
- Input: BillingPeriod=Monthly
- Resultado: ConflictException

❌ Amount negativo
- Input: Amount=-5
- Resultado: ValidationException

#### Slice: POST /plans/{id}/pricing-tiers

**Request**
```csharp
public record Request(
    BillingPeriod BillingPeriod,
    decimal Amount,
    string CurrencyCode,
    bool IsActive = false
);
```

**Response**: 201 Created → `PlanResponse`

**Tests Integración**

✅ 201 Created → PlanResponse con PricingTier añadido
✅ Persistencia: POST → GET → verificar PricingTier
✅ Múltiples BillingPeriods
❌ 404 → Plan no encontrado
❌ 409 → BillingPeriod duplicado
❌ 422 → Validación fallida (amount negativo)

---

### 6.9 Plan.UpdatePricingTier

#### Event Storming
```
🟡[Admin] → 🔵(UpdatePricingTier) → 🟤[[Plan]] → 🟠<PricingTierUpdated>
                                          │
                                🟣{TierExiste}
```

#### Dominio

**Input**

| Campo | Tipo |
|-------|------|
| BillingPeriod | BillingPeriod |
| Amount | decimal |
| CurrencyCode | string |

**Inyecta**: `PricingTier.UpdatePrice`, `IValidator<Plan>`

**Guards**

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Tier no existe | 404 | NotFoundGuard | "Pricing tier with billing period '{BillingPeriod}' not found" |

**Lógica**: Busca tier → PricingTier.UpdatePrice transform → remove+add → ValidateOrThrow.

Preserva `IsActive` y `ProviderConfigurations`.

**Tests Unitarios Dominio**

✅ Actualizar precio de tier existente
✅ Preserva IsActive y ProviderConfigurations
❌ Tier no existe → NotFoundException

#### Slice: PUT /plans/{id}/pricing-tiers/{billingPeriod}

**Request**
```csharp
public record Request(
    decimal Amount,
    string CurrencyCode
);
```

**Response**: 200 OK → `PlanResponse`

**Tests Integración**

✅ 200 OK → PlanResponse con precio actualizado
✅ Persistencia
✅ Preserva IsActive
❌ 404 → Plan o tier no encontrado

---

### 6.10 Plan.RemovePricingTier

#### Event Storming
```
🟡[Admin] → 🔵(RemovePricingTier) → 🟤[[Plan]] → 🟠<PricingTierRemoved>
                                          │
                                🟣{TierExiste}
                                🟣{NoEsElÚltimoActivoConProvider}
```

#### Dominio

**Input**

| Campo | Tipo |
|-------|------|
| BillingPeriod | BillingPeriod |

**Inyecta**: `IValidator<Plan>`

**Guards**

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Tier no existe | 404 | NotFoundGuard | "Pricing tier with billing period '{BillingPeriod}' not found" |
| Plan activo y es el último tier activo con provider activo | 422 | ValidationGuard | "Cannot remove the last active pricing tier with an active provider from an active plan" |

**Tests Unitarios Dominio**

✅ Eliminar tier (plan tiene varios)
✅ Eliminar último tier (plan inactivo)
❌ Tier no existe → NotFoundException
❌ Último tier activo con provider en plan activo → ValidationException

#### Slice: DELETE /plans/{id}/pricing-tiers/{billingPeriod}

**Response**: 204 No Content

**Tests Integración**

✅ 204 No Content
✅ Persistencia
❌ 404 → Plan o tier no encontrado
❌ 422 → Último tier activo de plan activo

---

### 6.11 Plan.ActivatePricingTier

#### Event Storming
```
🟡[Admin] → 🔵(ActivatePricingTier) → 🟤[[Plan]] → 🟠<PricingTierActivated>
                                            │
                                  🟣{TierExiste}
                                  🟣{NoYaActivo}
```

#### Dominio

**Input**

| Campo | Tipo |
|-------|------|
| BillingPeriod | BillingPeriod |

**Inyecta**: `PricingTier.Activate`, `IValidator<Plan>`

**Guards**

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Tier no existe | 404 | NotFoundGuard | "Pricing tier with billing period '{BillingPeriod}' not found" |
| Ya está activo | 409 | ConflictGuard | "Pricing tier with billing period '{BillingPeriod}' is already active" |

**Lógica**: Busca tier → PricingTier.Activate transform → remove+add → ValidateOrThrow.

**Tests Unitarios Dominio**

✅ Activar tier inactivo → IsActive=true
❌ Tier no existe → NotFoundException
❌ Ya activo → ConflictException

#### Slice: POST /plans/{id}/pricing-tiers/{billingPeriod}/activate

**Response**: 200 OK → `PlanResponse`

**Tests Integración**

✅ 200 OK → PlanResponse con tier activado
✅ Persistencia
❌ 404 → Plan o tier no encontrado
❌ 409 → Ya activo

---

### 6.12 Plan.DeactivatePricingTier

#### Event Storming
```
🟡[Admin] → 🔵(DeactivatePricingTier) → 🟤[[Plan]] → 🟠<PricingTierDeactivated>
                                              │
                                    🟣{TierExiste}
                                    🟣{NoEsElÚltimoActivoConProvider}
```

#### Dominio

**Input**

| Campo | Tipo |
|-------|------|
| BillingPeriod | BillingPeriod |

**Inyecta**: `PricingTier.Deactivate`, `IValidator<Plan>`

**Guards**

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Tier no existe | 404 | NotFoundGuard | "Pricing tier with billing period '{BillingPeriod}' not found" |
| Plan activo y es el último tier activo con provider activo | 422 | ValidationGuard | "Cannot deactivate the last active pricing tier with an active provider from an active plan" |

**Lógica**: Busca tier → PricingTier.Deactivate transform → remove+add → ValidateOrThrow.

**Tests Unitarios Dominio**

✅ Desactivar tier activo → IsActive=false
✅ Desactivar con otros tiers activos (plan activo OK)
❌ Tier no existe → NotFoundException
❌ Último tier activo con provider en plan activo → ValidationException

#### Slice: POST /plans/{id}/pricing-tiers/{billingPeriod}/deactivate

**Response**: 200 OK → `PlanResponse`

**Tests Integración**

✅ 200 OK → PlanResponse con tier desactivado
✅ Persistencia
❌ 404 → Plan o tier no encontrado
❌ 422 → Último tier activo de plan activo

---

### 6.13 Plan.AddPricingTierProviderConfiguration

#### Event Storming
```
🟡[Admin] → 🔵(AddProviderConfig) → 🟤[[Plan]] → 🟠<ProviderConfigAdded>
                                          │
                                🟣{TierExiste}
                                🟣{ProviderÚnicoEnTier}
```

#### Dominio

**Input**

| Campo | Tipo | Default |
|-------|------|---------|
| BillingPeriod | BillingPeriod | |
| Provider | string | |
| ExternalProductId | string | |
| ExternalPriceId | string | |
| IsActive | bool | true |

**Inyecta**: `PaymentProviderConfig.Create`, `IValidator<Plan>`

**Guards**

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Tier no existe | 404 | NotFoundGuard | "Pricing tier with billing period '{BillingPeriod}' not found" |
| Provider duplicado en tier | 409 | ConflictGuard | "Configuration for '{Provider}' already exists in pricing tier '{BillingPeriod}'" |

**Lógica**: Busca tier → ConflictGuard → providerConfigCreate.Execute → rebuild tier con `with { ProviderConfigurations = updatedConfigs }` → remove+add → ValidateOrThrow.

**Tests Unitarios Dominio**

✅ Añadir primera config (Stripe)
✅ Añadir segunda config (Paddle)
❌ Tier no existe → NotFoundException
❌ Provider duplicado → ConflictException

#### Slice: POST /plans/{id}/pricing-tiers/{billingPeriod}/provider-configurations

**Request**
```csharp
public record Request(
    string Provider,
    string ExternalProductId,
    string ExternalPriceId,
    bool IsActive = true
);
```

**Response**: 201 Created → `PlanResponse`

**Tests Integración**

✅ 201 Created → PlanResponse con Config añadida
✅ Persistencia
✅ Múltiples providers en un tier
❌ 404 → Plan o tier no encontrado
❌ 409 → Provider duplicado

---

### 6.14 Plan.UpdatePricingTierProviderConfiguration

#### Event Storming
```
🟡[Admin] → 🔵(UpdateProviderConfig) → 🟤[[Plan]] → 🟠<ProviderConfigUpdated>
                                             │
                                   🟣{TierExiste}
                                   🟣{ConfigExiste}
```

#### Dominio

**Input**

| Campo | Tipo |
|-------|------|
| BillingPeriod | BillingPeriod |
| Provider | string |
| ExternalProductId | string |
| ExternalPriceId | string |

**Inyecta**: `PaymentProviderConfig.Create`, `IValidator<Plan>`

**Guards**

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Tier no existe | 404 | NotFoundGuard | "Pricing tier with billing period '{BillingPeriod}' not found" |
| Config no existe | 404 | NotFoundGuard | "Configuration for '{Provider}' not found in pricing tier '{BillingPeriod}'" |

**Lógica**: Busca tier → busca config → crea nueva config preservando IsActive → rebuild tier → remove+add → ValidateOrThrow.

**Tests Unitarios Dominio**

✅ Actualizar IDs de Stripe → IDs actualizados, IsActive preservado
❌ Tier no existe → NotFoundException
❌ Config no existe → NotFoundException

#### Slice: PUT /plans/{id}/pricing-tiers/{billingPeriod}/provider-configurations/{provider}

**Request**
```csharp
public record Request(
    string ExternalProductId,
    string ExternalPriceId
);
```

**Response**: 200 OK → `PlanResponse`

**Tests Integración**

✅ 200 OK → PlanResponse con Config actualizada
✅ Persistencia
✅ Preserva IsActive
❌ 404 → Plan, tier o config no encontrada

---

### 6.15 Plan.ActivatePricingTierProviderConfiguration

#### Event Storming
```
🟡[Admin] → 🔵(ActivateProviderConfig) → 🟤[[Plan]] → 🟠<ProviderConfigActivated>
                                               │
                                     🟣{TierExiste}
                                     🟣{ConfigExiste}
                                     🟣{NoYaActiva}
```

#### Dominio

**Input**

| Campo | Tipo |
|-------|------|
| BillingPeriod | BillingPeriod |
| Provider | string |

**Inyecta**: `PaymentProviderConfig.Create`, `IValidator<Plan>`

**Guards**

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Tier no existe | 404 | NotFoundGuard | "Pricing tier with billing period '{BillingPeriod}' not found" |
| Config no existe | 404 | NotFoundGuard | "Configuration for '{Provider}' not found in pricing tier '{BillingPeriod}'" |
| Ya activa | 409 | ConflictGuard | "Configuration for '{Provider}' is already active in pricing tier '{BillingPeriod}'" |

**Lógica**: Busca tier → busca config → crea nueva config con IsActive=true → rebuild tier → remove+add → ValidateOrThrow.

**Tests Unitarios Dominio**

✅ Activar config inactiva → IsActive=true
❌ Tier no existe → NotFoundException
❌ Config no existe → NotFoundException
❌ Ya activa → ConflictException

#### Slice: POST /plans/{id}/pricing-tiers/{billingPeriod}/provider-configurations/{provider}/activate

**Response**: 200 OK → `PlanResponse`

**Tests Integración**

✅ 200 OK → PlanResponse con config activada
✅ Persistencia
❌ 404 → Plan, tier o config no encontrada
❌ 409 → Ya activa

---

### 6.16 Plan.DeactivatePricingTierProviderConfiguration

#### Event Storming
```
🟡[Admin] → 🔵(DeactivateProviderConfig) → 🟤[[Plan]] → 🟠<ProviderConfigDeactivated>
                                                 │
                                       🟣{TierExiste}
                                       🟣{ConfigExiste}
                                       🟣{NoYaInactiva}
                                       🟣{NoEsElÚltimoProviderActivoDelÚltimoTierActivo}
```

#### Dominio

**Input**

| Campo | Tipo |
|-------|------|
| BillingPeriod | BillingPeriod |
| Provider | string |

**Inyecta**: `PaymentProviderConfig.Create`, `IValidator<Plan>`

**Guards**

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Tier no existe | 404 | NotFoundGuard | "Pricing tier with billing period '{BillingPeriod}' not found" |
| Config no existe | 404 | NotFoundGuard | "Configuration for '{Provider}' not found in pricing tier '{BillingPeriod}'" |
| Ya inactiva | 409 | ConflictGuard | "Configuration for '{Provider}' is already inactive in pricing tier '{BillingPeriod}'" |
| Plan activo + último provider activo del último tier activo | 422 | ValidationGuard | "Cannot deactivate the last active provider configuration from the last active pricing tier of an active plan" |

**Lógica**: Busca tier → busca config → guards → crea nueva config con IsActive=false → rebuild tier → remove+add → ValidateOrThrow.

**Tests Unitarios Dominio**

✅ Desactivar config (hay otras activas) → IsActive=false
✅ Desactivar en plan inactivo (OK)
❌ Tier no existe → NotFoundException
❌ Config no existe → NotFoundException
❌ Ya inactiva → ConflictException
❌ Último provider activo del último tier activo de plan activo → ValidationException

#### Slice: POST /plans/{id}/pricing-tiers/{billingPeriod}/provider-configurations/{provider}/deactivate

**Response**: 200 OK → `PlanResponse`

**Tests Integración**

✅ 200 OK → PlanResponse con config desactivada
✅ Persistencia
❌ 404 → Plan, tier o config no encontrada
❌ 409 → Ya inactiva
❌ 422 → Último provider activo de plan activo

---

### 6.17 Plan.Activate

#### Event Storming
```
🟡[Admin] → 🔵(ActivatePlan) → 🟤[[Plan]] → 🟠<PlanActivated>
                                    │
                          🟣{TieneFeatures}
                          🟣{TienePricingTierActivoConProvider}
```

#### Dominio

**Input**

Ninguno

**Inyecta**
- `IValidator<Plan>`

**Guards**

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Ya está activo | 409 | ConflictGuard | "Plan is already active" |
| No tiene Features | 422 | ValidationGuard | "Plan must have at least one feature" |
| No tiene PricingTier activo con provider activo | 422 | ValidationGuard | "Plan must have at least one active pricing tier with an active provider configuration" |

**Lógica**
```csharp
ConflictGuard.ThrowIf(plan.IsActive, "Plan is already active");
ValidationGuard.ThrowIf(!plan.Features.Any(), "Plan must have at least one feature", nameof(plan.Features));
ValidationGuard.ThrowIf(!plan.HasActivePricingTierWithProvider, "Plan must have at least one active pricing tier with an active provider configuration", nameof(plan.PricingTiers));

plan.IsActive = true;

return planValidator.ValidateOrThrow(plan);
```

**Tests Unitarios Dominio**

✅ Activar plan completo
- Precondición: Plan con Features y PricingTier activo con ProviderConfig activa, IsActive=false
- Resultado: Plan con IsActive=true

❌ Plan ya activo → ConflictException

❌ Plan sin Features → ValidationException

❌ Plan sin PricingTier activo con provider → ValidationException

#### Slice: POST /plans/{id}/activate

**Response**: 200 OK → `PlanResponse`

**Tests Integración**

✅ 200 OK → PlanResponse con IsActive=true
✅ Persistencia: POST → GET → verificar IsActive=true
❌ 404 → Plan no encontrado
❌ 409 → Ya estaba activo
❌ 422 → Falta Feature o PricingTier con provider

---

### 6.18 Plan.Deactivate

#### Event Storming
```
🟡[Admin] → 🔵(DeactivatePlan) → 🟤[[Plan]] → 🟠<PlanDeactivated>
```

#### Dominio

**Input**

Ninguno

**Inyecta**
- `IValidator<Plan>`

**Guards**

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Ya está inactivo | 409 | ConflictGuard | "Plan is already inactive" |

**Lógica**
```csharp
ConflictGuard.ThrowIf(!plan.IsActive, "Plan is already inactive");

plan.IsActive = false;

return planValidator.ValidateOrThrow(plan);
```

**Tests Unitarios Dominio**

✅ Desactivar plan activo → IsActive=false

❌ Plan ya inactivo → ConflictException

#### Slice: POST /plans/{id}/deactivate

**Response**: 200 OK → `PlanResponse`

**Tests Integración**

✅ 200 OK → PlanResponse con IsActive=false
✅ Persistencia: POST → GET → verificar IsActive=false
❌ 404 → Plan no encontrado
❌ 409 → Ya estaba inactivo

---

## 7. Resumen de Endpoints (Orden de Implementación)

| # | Método | Ruta | Comando/Query | Response |
|---|--------|------|---------------|----------|
| 1 | POST | /plans | Plan.Create | 201 → `PlanResponse` |
| 2 | GET | /plans/{id} | GetPlan | 200 → `PlanResponse` |
| 3 | GET | /plans | ListPlans | 200 → `PlanResponse[]` |
| 4 | PUT | /plans/{id} | Plan.Update | 204 |
| 5 | POST | /plans/{id}/features | Plan.AddFeature | 201 → `PlanResponse` |
| 6 | PUT | /plans/{id}/features/{code} | Plan.UpdateFeature | 204 |
| 7 | DELETE | /plans/{id}/features/{code} | Plan.RemoveFeature | 204 |
| 8 | POST | /plans/{id}/pricing-tiers | Plan.AddPricingTier | 201 → `PlanResponse` |
| 9 | PUT | /plans/{id}/pricing-tiers/{billingPeriod} | Plan.UpdatePricingTier | 200 → `PlanResponse` |
| 10 | DELETE | /plans/{id}/pricing-tiers/{billingPeriod} | Plan.RemovePricingTier | 204 |
| 11 | POST | /plans/{id}/pricing-tiers/{billingPeriod}/activate | Plan.ActivatePricingTier | 200 → `PlanResponse` |
| 12 | POST | /plans/{id}/pricing-tiers/{billingPeriod}/deactivate | Plan.DeactivatePricingTier | 200 → `PlanResponse` |
| 13 | POST | .../provider-configurations | Plan.AddPricingTierProviderConfiguration | 201 → `PlanResponse` |
| 14 | PUT | .../provider-configurations/{provider} | Plan.UpdatePricingTierProviderConfiguration | 200 → `PlanResponse` |
| 15 | POST | .../provider-configurations/{provider}/activate | Plan.ActivatePricingTierProviderConfiguration | 200 → `PlanResponse` |
| 16 | POST | .../provider-configurations/{provider}/deactivate | Plan.DeactivatePricingTierProviderConfiguration | 200 → `PlanResponse` |
| 17 | POST | /plans/{id}/activate | Plan.Activate | 200 → `PlanResponse` |
| 18 | POST | /plans/{id}/deactivate | Plan.Deactivate | 200 → `PlanResponse` |

---

## 8. Persistencia (Firestore)

### Colección

`/plans/{planId}`

### Configuración DbContext

```csharp
modelBuilder.Entity<PlanAgg>(entity =>
{
    // Ignore: propiedades computed
    entity.Ignore(p => p.HasActivePricingTierWithProvider);

    // ArrayOf: Features
    entity.ArrayOf(p => p.Features, feature =>
    {
        feature.Ignore(f => f.IsValid);
        feature.Ignore(f => f.DisplayValue);
    });

    // ArrayOf: PricingTiers (con nested maps)
    entity.ArrayOf(p => p.PricingTiers, tier =>
    {
        tier.Ignore(t => t.HasActiveProvider);
        // Price y Currency se mapean por convención (nested maps)
        // ProviderConfigurations se mapea por convención (nested ArrayOf)
    });
});
```

### Documento Ejemplo

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Plan Básico",
  "description": "Ideal para empezar",
  "isActive": true,
  "features": [
    {
      "code": "RESERVATIONS_MONTHLY",
      "name": "Reservas mensuales",
      "description": null,
      "type": "Limit",
      "limit": 100,
      "unit": "reservas/mes"
    }
  ],
  "pricingTiers": [
    {
      "billingPeriod": "Monthly",
      "price": {
        "amount": 9.99,
        "currency": {
          "code": "EUR",
          "symbol": "€",
          "decimalPlaces": 2
        }
      },
      "isActive": true,
      "providerConfigurations": [
        {
          "provider": "Stripe",
          "externalProductId": "prod_xxx",
          "externalPriceId": "price_xxx",
          "isActive": true
        }
      ]
    },
    {
      "billingPeriod": "Yearly",
      "price": {
        "amount": 99.99,
        "currency": {
          "code": "EUR",
          "symbol": "€",
          "decimalPlaces": 2
        }
      },
      "isActive": true,
      "providerConfigurations": [
        {
          "provider": "Stripe",
          "externalProductId": "prod_yyy",
          "externalPriceId": "price_yyy",
          "isActive": true
        }
      ]
    }
  ]
}
```

---

## 9. Hot Spots ⚠️

| # | Pregunta | Estado |
|---|----------|--------|
| 1 | ¿Customers existentes se ven afectados cuando cambian Features? | Pendiente |
| 2 | ¿Name de Plan debe ser único en sistema? | Pendiente |
| 3 | ¿Se puede eliminar un Plan o solo desactivar? | Solo desactivar |

---

**Fecha**: 2025-01-28
**Última actualización**: 2026-02-13
**Autor**: Equipo Fudie
