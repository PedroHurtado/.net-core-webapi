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

## 3. Aggregate: Plan

### Estructura

```
Plan (Aggregate Root)
├─ Id: Guid
├─ Name: string
├─ Description: string
├─ Price: Money
├─ BillingPeriod: BillingPeriod
├─ IsActive: bool
├─ Features: IReadOnlyCollection<Feature>
└─ ProviderConfigurations: IReadOnlyCollection<PaymentProviderConfig>
```

#### Propiedades

| Propiedad | Tipo | Modificador |
|-----------|------|-------------|
| Id | Guid | init |
| Name | string | protected set |
| Description | string | protected set |
| Price | Money | protected set |
| BillingPeriod | BillingPeriod | protected set |
| IsActive | bool | protected set |

#### Colecciones

```csharp
protected HashSet<Feature> _features = [];
public IReadOnlyCollection<Feature> Features => _features.ToList().AsReadOnly();

protected HashSet<PaymentProviderConfig> _providerConfigurations = [];
public IReadOnlyCollection<PaymentProviderConfig> ProviderConfigurations => _providerConfigurations.ToList().AsReadOnly();
```

#### Propiedades Calculadas

| Propiedad | Tipo | Fórmula |
|-----------|------|---------|
| HasActiveProvider | bool | `_providerConfigurations.Any(p => p.IsActive)` |

### Validaciones

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| Id | NotEmpty | "Id is required" |
| Name | NotEmpty | "Name is required" |
| Name | Max(100) | "Name cannot exceed 100 characters" |
| Description | NotEmpty | "Description is required" |
| Description | Max(500) | "Description cannot exceed 500 characters" |
| Price | NotNull | "Price is required" |
| BillingPeriod | IsEnum | |

---

## 4. Response

```csharp
public record PlanResponse(
    Guid Id,
    string Name,
    string Description,
    MoneyResponse Price,
    BillingPeriod BillingPeriod,
    bool IsActive,
    bool HasActiveProvider,
    IReadOnlyCollection<FeatureResponse> Features,
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
| Amount | decimal |
| CurrencyCode | string |
| BillingPeriod | BillingPeriod |

**Inyecta**
- `Money.Create`
- `IValidator<Plan>`

**Guards**

Ninguno.

**Lógica**
```csharp
var price = moneyCreate.Execute(new CreateMoneyCommand(command.Amount, command.CurrencyCode));

var plan = new Plan(Guid.NewGuid())
{
    Name = command.Name,
    Description = command.Description,
    Price = price,
    BillingPeriod = command.BillingPeriod,
    IsActive = false
};

return planValidator.ValidateOrThrow(plan);
```

**Tests Unitarios Dominio**

✅ Crear plan con datos válidos
- Input: Name="Plan Básico", Description="Ideal para empezar", Amount=9.99, CurrencyCode="EUR", BillingPeriod=Monthly
- Resultado: Plan creado con IsActive=false, Features vacío, ProviderConfigurations vacío

❌ Name vacío
- Input: Name=""
- Resultado: ValidationException "Name is required"

❌ Description vacía
- Input: Description=""
- Resultado: ValidationException "Description is required"

❌ Amount negativo
- Input: Amount=-5
- Resultado: ValidationException "Amount cannot be negative"

❌ CurrencyCode inválido
- Input: CurrencyCode="XXX"
- Resultado: ArgumentException "Currency XXX not supported"

#### Slice: POST /plans

**Request**
```csharp
public record CreatePlanRequest(
    string Name,
    string Description,
    decimal Amount,
    string CurrencyCode,
    BillingPeriod BillingPeriod
);
```

**Response**: 201 Created → `PlanResponse`

**Tests Unitarios Servicio**

✅ Llama a Money.Create con los parámetros correctos
- Verifica que se invoca moneyCreate.Execute con Amount y CurrencyCode

✅ Llama a Plan.Create con los parámetros correctos
- Verifica que se invoca planCreate.Execute con el command correcto

✅ Añade el plan al repositorio
- Verifica que repository.Add es llamado con el plan creado

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

✅ Retorna Response mapeado correctamente
- Verifica que el Response contiene los datos del plan

**Tests Integración**

✅ 201 Created → PlanResponse con IsActive=false

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
| Amount | decimal |
| CurrencyCode | string |
| BillingPeriod | BillingPeriod |

**Inyecta**
- `Money.Create`
- `IValidator<Plan>`

**Guards**

Ninguno.

**Lógica**
```csharp
var price = moneyCreate.Execute(new CreateMoneyCommand(command.Amount, command.CurrencyCode));

plan.Name = command.Name;
plan.Description = command.Description;
plan.Price = price;
plan.BillingPeriod = command.BillingPeriod;

return planValidator.ValidateOrThrow(plan);
```

**Tests Unitarios Dominio**

✅ Actualizar plan existente
- Precondición: Plan existe
- Input: Name="Plan Actualizado", Description="Nueva descripción", Amount=12.99, CurrencyCode="EUR", BillingPeriod=Monthly
- Resultado: Plan actualizado

❌ Name vacío
- Input: Name=""
- Resultado: ValidationException "Name is required"

#### Slice: PUT /plans/{id}

**Request**
```csharp
public record UpdatePlanRequest(
    string Name,
    string Description,
    decimal Amount,
    string CurrencyCode,
    BillingPeriod BillingPeriod
);
```

**Response**: 204 No Content

**Tests Unitarios Servicio**

✅ Obtiene el plan del repositorio
- Verifica que repository.Get es llamado con el id correcto

✅ Llama a Money.Create con los parámetros correctos
- Verifica que se invoca moneyCreate.Execute con Amount y CurrencyCode

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
- Precondición: Plan sin Feature RESERVATIONS_MONTHLY
- Input: Code="RESERVATIONS_MONTHLY", Name="Reservas", Type=Limit, Limit=100
- Resultado: Feature añadido al Plan

✅ Añadir Feature tipo Boolean
- Input: Code="PRIORITY_SUPPORT", Name="Soporte", Type=Boolean
- Resultado: Feature añadido al Plan

✅ Añadir Feature tipo Unlimited
- Input: Code="RESERVATIONS_MONTHLY", Name="Reservas", Type=Unlimited
- Resultado: Feature añadido al Plan

❌ Code duplicado
- Precondición: Plan ya tiene Feature RESERVATIONS_MONTHLY
- Input: Code="RESERVATIONS_MONTHLY"
- Resultado: ConflictException "Feature with code 'RESERVATIONS_MONTHLY' already exists"

❌ Validación de Feature falla
- Input: Type=Limit, Limit=null
- Resultado: ValidationException "Limit is required when feature type is Limit"

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

**Tests Unitarios Servicio**

✅ Obtiene el plan del repositorio
- Verifica que repository.Get es llamado con el id correcto

✅ Llama a Plan.AddFeature con los parámetros correctos
- Verifica que se invoca addFeature.Execute con el command correcto

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

✅ Retorna Response mapeado correctamente
- Verifica que el Response contiene el Feature añadido

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

**Inyecta**
- `Feature.Create`
- `IValidator<Plan>`

**Guards**

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Feature no existe | 404 | NotFoundGuard | "Feature with code '{Code}' not found" |

**Lógica**
```csharp
var existing = plan.Features.FirstOrDefault(f => f.Code == code);
NotFoundGuard.ThrowIfNull(existing, $"Feature with code '{code}' not found");

var updated = featureCreate.Execute(new CreateFeatureCommand(
    code,
    command.Name,
    command.Description,
    command.Type,
    command.Limit,
    command.Unit));

plan._features.Remove(existing);
plan._features.Add(updated);

return planValidator.ValidateOrThrow(plan);
```

**Tests Unitarios Dominio**

✅ Actualizar Feature existente
- Precondición: Plan tiene Feature RESERVATIONS_MONTHLY con Limit=100
- Input: Name="Reservas", Type=Limit, Limit=200
- Resultado: Feature actualizado con Limit=200

✅ Cambiar de Limit a Unlimited
- Precondición: Plan tiene Feature con Type=Limit
- Input: Type=Unlimited
- Resultado: Feature actualizado con Type=Unlimited, Limit=null

❌ Feature no existe
- Precondición: Plan no tiene Feature NONEXISTENT
- Resultado: NotFoundException "Feature with code 'NONEXISTENT' not found"

#### Slice: PUT /plans/{id}/features/{code}

**Request**
```csharp
public record UpdateFeatureRequest(
    string Name,
    string? Description,
    FeatureType Type,
    int? Limit,
    string? Unit
);
```

**Response**: 204 No Content

**Tests Unitarios Servicio**

✅ Obtiene el plan del repositorio
- Verifica que repository.Get es llamado con el id correcto

✅ Llama a Plan.UpdateFeature con los parámetros correctos
- Verifica que se invoca updateFeature.Execute con el code y command correctos

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

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

**Input**

*Code viene en la ruta*

**Inyecta**
- `IValidator<Plan>`

**Guards**

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Feature no existe | 404 | NotFoundGuard | "Feature with code '{Code}' not found" |
| Es el último y Plan activo | 422 | ValidationGuard | "Cannot remove last feature from active plan" |

**Lógica**
```csharp
var existing = plan.Features.FirstOrDefault(f => f.Code == code);
NotFoundGuard.ThrowIfNull(existing, $"Feature with code '{code}' not found");

ValidationGuard.ThrowIf(
    plan.IsActive && plan.Features.Count <= 1,
    "Cannot remove last feature from active plan",
    nameof(plan.Features));

plan._features.Remove(existing);

return planValidator.ValidateOrThrow(plan);
```

**Tests Unitarios Dominio**

✅ Eliminar Feature (plan tiene varios)
- Precondición: Plan con 3 Features
- Input: Code="PRIORITY_SUPPORT"
- Resultado: Feature eliminado, quedan 2

✅ Eliminar último Feature (plan inactivo)
- Precondición: Plan inactivo con 1 Feature
- Input: Code="RESERVATIONS_MONTHLY"
- Resultado: Feature eliminado, quedan 0

❌ Feature no existe
- Resultado: NotFoundException "Feature with code 'NONEXISTENT' not found"

❌ Último Feature en plan activo
- Precondición: Plan activo con 1 Feature
- Resultado: ValidationException "Cannot remove last feature from active plan"

#### Slice: DELETE /plans/{id}/features/{code}

**Response**: 204 No Content

**Tests Unitarios Servicio**

✅ Obtiene el plan del repositorio
- Verifica que repository.Get es llamado con el id correcto

✅ Llama a Plan.RemoveFeature con el code correcto
- Verifica que se invoca removeFeature.Execute con el code

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

**Tests Integración**

✅ 204 No Content

✅ Persistencia: DELETE → GET → verificar Feature eliminado

❌ 404 → Plan o Feature no encontrado

❌ 422 → Es el último en plan activo

---

### 6.8 Plan.AddProviderConfiguration

#### Event Storming
```
🟡[Admin] → 🔵(AddProviderConfig) → 🟤[[Plan]] → 🟠<ProviderConfigAdded>
                                         │
                               🟣{NoActivoDuplicado}
```

#### Dominio

**Input**

| Campo | Tipo | Default |
|-------|------|---------|
| Provider | string | |
| ExternalProductId | string | |
| ExternalPriceId | string | |
| IsActive | bool | true |

**Inyecta**
- `PaymentProviderConfig.Create`
- `IValidator<Plan>`

**Guards**

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Ya existe config activa del mismo Provider | 409 | ConflictGuard | "Active configuration for '{Provider}' already exists" |

**Lógica**
```csharp
ConflictGuard.ThrowIf(
    command.IsActive && plan.ProviderConfigurations.Any(c => c.Provider == command.Provider && c.IsActive),
    $"Active configuration for '{command.Provider}' already exists");

var config = providerConfigCreate.Execute(new CreatePaymentProviderConfigCommand(
    command.Provider,
    command.ExternalProductId,
    command.ExternalPriceId,
    command.IsActive));

plan._providerConfigurations.Add(config);

return planValidator.ValidateOrThrow(plan);
```

**Tests Unitarios Dominio**

✅ Añadir primera config (Stripe)
- Precondición: Plan sin ProviderConfigurations
- Input: Provider="Stripe", ExternalProductId="prod_xxx", ExternalPriceId="price_xxx"
- Resultado: Config añadida con IsActive=true

✅ Añadir segunda config (Paddle)
- Precondición: Plan con Stripe activa
- Input: Provider="Paddle", ExternalProductId="pro_xxx", ExternalPriceId="pri_xxx"
- Resultado: Config añadida

✅ Añadir config inactiva
- Input: Provider="Stripe", IsActive=false
- Resultado: Config añadida con IsActive=false

❌ Config activa duplicada
- Precondición: Plan con Stripe activa
- Input: Provider="Stripe", IsActive=true
- Resultado: ConflictException "Active configuration for 'Stripe' already exists"

#### Slice: POST /plans/{id}/provider-configurations

**Request**
```csharp
public record AddProviderConfigRequest(
    string Provider,
    string ExternalProductId,
    string ExternalPriceId,
    bool IsActive = true
);
```

**Response**: 201 Created → `PlanResponse`

**Tests Unitarios Servicio**

✅ Obtiene el plan del repositorio
- Verifica que repository.Get es llamado con el id correcto

✅ Llama a Plan.AddProviderConfiguration con los parámetros correctos
- Verifica que se invoca addProviderConfig.Execute con el command correcto

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

✅ Retorna Response mapeado correctamente
- Verifica que el Response contiene la Config añadida

**Tests Integración**

✅ 201 Created → PlanResponse con Config añadida

✅ Persistencia: POST → GET → verificar Config añadida

❌ 404 → Plan no encontrado

❌ 409 → Ya existe config activa para ese Provider

❌ 422 → Validación fallida

---

### 6.9 Plan.UpdateProviderConfiguration

#### Event Storming
```
🟡[Admin] → 🔵(UpdateProviderConfig) → 🟤[[Plan]] → 🟠<ProviderConfigUpdated>
                                            │
                                  🟣{ConfigExiste}
```

#### Dominio

**Input**

| Campo | Tipo |
|-------|------|
| ExternalProductId | string |
| ExternalPriceId | string |

*Provider viene en la ruta*

**Inyecta**
- `PaymentProviderConfig.Create`
- `IValidator<Plan>`

**Guards**

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Config no existe | 404 | NotFoundGuard | "Configuration for '{Provider}' not found" |

**Lógica**
```csharp
var existing = plan.ProviderConfigurations.FirstOrDefault(c => c.Provider == provider);
NotFoundGuard.ThrowIfNull(existing, $"Configuration for '{provider}' not found");

var updated = providerConfigCreate.Execute(new CreatePaymentProviderConfigCommand(
    provider,
    command.ExternalProductId,
    command.ExternalPriceId,
    existing.IsActive));

plan._providerConfigurations.Remove(existing);
plan._providerConfigurations.Add(updated);

return planValidator.ValidateOrThrow(plan);
```

**Tests Unitarios Dominio**

✅ Actualizar IDs de Stripe
- Precondición: Plan con config Stripe
- Input: ExternalProductId="prod_new", ExternalPriceId="price_new"
- Resultado: IDs actualizados, IsActive se mantiene

❌ Config no existe
- Precondición: Plan sin config Paddle
- Resultado: NotFoundException "Configuration for 'Paddle' not found"

#### Slice: PUT /plans/{id}/provider-configurations/{provider}

**Request**
```csharp
public record UpdateProviderConfigRequest(
    string ExternalProductId,
    string ExternalPriceId
);
```

**Response**: 204 No Content

**Tests Unitarios Servicio**

✅ Obtiene el plan del repositorio
- Verifica que repository.Get es llamado con el id correcto

✅ Llama a Plan.UpdateProviderConfiguration con los parámetros correctos
- Verifica que se invoca updateProviderConfig.Execute con el provider y command correctos

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

**Tests Integración**

✅ 204 No Content

✅ Persistencia: PUT → GET → verificar Config actualizada

❌ 404 → Plan o Config no encontrada

❌ 422 → Validación fallida

---

### 6.10 Plan.ActivateProviderConfiguration

#### Event Storming
```
🟡[Admin] → 🔵(ActivateProviderConfig) → 🟤[[Plan]] → 🟠<ProviderConfigActivated>
                                              │
                                    🟣{ConfigExiste}
                                    🟣{NoActivoDuplicado}
```

#### Dominio

**Input**

*Provider viene en la ruta*

**Inyecta**
- `PaymentProviderConfig.Create`
- `IValidator<Plan>`

**Guards**

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Config no existe | 404 | NotFoundGuard | "Configuration for '{Provider}' not found" |
| Ya existe otra config activa del mismo Provider | 409 | ConflictGuard | "Another active configuration for '{Provider}' already exists" |

**Lógica**
```csharp
var existing = plan.ProviderConfigurations.FirstOrDefault(c => c.Provider == provider);
NotFoundGuard.ThrowIfNull(existing, $"Configuration for '{provider}' not found");

if (existing.IsActive)
    return plan;

ConflictGuard.ThrowIf(
    plan.ProviderConfigurations.Any(c => c.Provider == provider && c.IsActive && c != existing),
    $"Another active configuration for '{provider}' already exists");

var activated = providerConfigCreate.Execute(new CreatePaymentProviderConfigCommand(
    existing.Provider,
    existing.ExternalProductId,
    existing.ExternalPriceId,
    true));

plan._providerConfigurations.Remove(existing);
plan._providerConfigurations.Add(activated);

return planValidator.ValidateOrThrow(plan);
```

**Tests Unitarios Dominio**

✅ Activar config inactiva
- Precondición: Plan con config Stripe inactiva
- Resultado: Config con IsActive=true

✅ Activar config ya activa (idempotente)
- Precondición: Plan con config Stripe activa
- Resultado: Plan sin cambios

❌ Config no existe
- Resultado: NotFoundException "Configuration for 'Paddle' not found"

#### Slice: POST /plans/{id}/provider-configurations/{provider}/activate

**Response**: 200 OK → `PlanResponse`

**Tests Unitarios Servicio**

✅ Obtiene el plan del repositorio
- Verifica que repository.Get es llamado con el id correcto

✅ Llama a Plan.ActivateProviderConfiguration con el provider correcto
- Verifica que se invoca activateProviderConfig.Execute con el provider

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

✅ Retorna Response mapeado correctamente
- Verifica que el Response contiene la Config activada

**Tests Integración**

✅ 200 OK → PlanResponse

✅ Persistencia: POST → GET → verificar Config activada

❌ 404 → Plan o Config no encontrada

❌ 409 → Ya hay otra activa del mismo Provider

---

### 6.11 Plan.DeactivateProviderConfiguration

#### Event Storming
```
🟡[Admin] → 🔵(DeactivateProviderConfig) → 🟤[[Plan]] → 🟠<ProviderConfigDeactivated>
                                                │
                                      🟣{ConfigExiste}
                                      🟣{NoEsLaÚnicaActiva}
```

#### Dominio

**Input**

*Provider viene en la ruta*

**Inyecta**
- `PaymentProviderConfig.Create`
- `IValidator<Plan>`

**Guards**

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Config no existe | 404 | NotFoundGuard | "Configuration for '{Provider}' not found" |
| Es la única activa y Plan activo | 422 | ValidationGuard | "Cannot deactivate last provider on active plan" |

**Lógica**
```csharp
var existing = plan.ProviderConfigurations.FirstOrDefault(c => c.Provider == provider);
NotFoundGuard.ThrowIfNull(existing, $"Configuration for '{provider}' not found");

if (!existing.IsActive)
    return plan;

var activeCount = plan.ProviderConfigurations.Count(c => c.IsActive);
ValidationGuard.ThrowIf(
    plan.IsActive && activeCount <= 1,
    "Cannot deactivate last provider on active plan",
    nameof(plan.ProviderConfigurations));

var deactivated = providerConfigCreate.Execute(new CreatePaymentProviderConfigCommand(
    existing.Provider,
    existing.ExternalProductId,
    existing.ExternalPriceId,
    false));

plan._providerConfigurations.Remove(existing);
plan._providerConfigurations.Add(deactivated);

return planValidator.ValidateOrThrow(plan);
```

**Tests Unitarios Dominio**

✅ Desactivar config (hay otras activas)
- Precondición: Plan con Stripe y Paddle activas
- Input: Provider="Stripe"
- Resultado: Stripe con IsActive=false

✅ Desactivar única activa (plan inactivo)
- Precondición: Plan inactivo con solo Stripe activa
- Resultado: Stripe con IsActive=false

✅ Desactivar config ya inactiva (idempotente)
- Precondición: Config Stripe ya inactiva
- Resultado: Plan sin cambios

❌ Config no existe
- Resultado: NotFoundException "Configuration for 'Paddle' not found"

❌ Última activa en plan activo
- Precondición: Plan activo con solo Stripe activa
- Resultado: ValidationException "Cannot deactivate last provider on active plan"

#### Slice: POST /plans/{id}/provider-configurations/{provider}/deactivate

**Response**: 200 OK → `PlanResponse`

**Tests Unitarios Servicio**

✅ Obtiene el plan del repositorio
- Verifica que repository.Get es llamado con el id correcto

✅ Llama a Plan.DeactivateProviderConfiguration con el provider correcto
- Verifica que se invoca deactivateProviderConfig.Execute con el provider

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

✅ Retorna Response mapeado correctamente
- Verifica que el Response contiene la Config desactivada

**Tests Integración**

✅ 200 OK → PlanResponse

✅ Persistencia: POST → GET → verificar Config desactivada

❌ 404 → Plan o Config no encontrada

❌ 422 → Es la única activa en plan activo

---

### 6.12 Plan.Activate

#### Event Storming
```
🟡[Admin] → 🔵(ActivatePlan) → 🟤[[Plan]] → 🟠<PlanActivated>
                                    │
                          🟣{TieneFeatures}
                          🟣{TieneProviderActivo}
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
| No tiene ProviderConfig activa | 422 | ValidationGuard | "Plan must have at least one active provider configuration" |

**Lógica**
```csharp
ConflictGuard.ThrowIf(plan.IsActive, "Plan is already active");
ValidationGuard.ThrowIf(!plan.Features.Any(), "Plan must have at least one feature", nameof(plan.Features));
ValidationGuard.ThrowIf(!plan.HasActiveProvider, "Plan must have at least one active provider configuration", nameof(plan.ProviderConfigurations));

plan.IsActive = true;

return planValidator.ValidateOrThrow(plan);
```

**Tests Unitarios Dominio**

✅ Activar plan completo
- Precondición: Plan con Features y ProviderConfig activa, IsActive=false
- Resultado: Plan con IsActive=true

❌ Plan ya activo
- Precondición: Plan con IsActive=true
- Resultado: ConflictException "Plan is already active"

❌ Plan sin Features
- Precondición: Plan sin Features, con ProviderConfig activa
- Resultado: ValidationException "Plan must have at least one feature"

❌ Plan sin ProviderConfig activa
- Precondición: Plan con Features, sin ProviderConfig activa
- Resultado: ValidationException "Plan must have at least one active provider configuration"

#### Slice: POST /plans/{id}/activate

**Response**: 200 OK → `PlanResponse`

**Tests Unitarios Servicio**

✅ Obtiene el plan del repositorio
- Verifica que repository.Get es llamado con el id correcto

✅ Llama a Plan.Activate
- Verifica que se invoca planActivate.Execute

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

✅ Retorna Response mapeado correctamente
- Verifica que el Response contiene IsActive=true

**Tests Integración**

✅ 200 OK → PlanResponse con IsActive=true

✅ Persistencia: POST → GET → verificar IsActive=true

❌ 404 → Plan no encontrado

❌ 409 → Ya estaba activo

❌ 422 → Falta Feature o ProviderConfig

---

### 6.13 Plan.Deactivate

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

✅ Desactivar plan activo
- Precondición: Plan con IsActive=true
- Resultado: Plan con IsActive=false

❌ Plan ya inactivo
- Precondición: Plan con IsActive=false
- Resultado: ConflictException "Plan is already inactive"

#### Slice: POST /plans/{id}/deactivate

**Response**: 200 OK → `PlanResponse`

**Tests Unitarios Servicio**

✅ Obtiene el plan del repositorio
- Verifica que repository.Get es llamado con el id correcto

✅ Llama a Plan.Deactivate
- Verifica que se invoca planDeactivate.Execute

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

✅ Retorna Response mapeado correctamente
- Verifica que el Response contiene IsActive=false

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
| 8 | POST | /plans/{id}/provider-configurations | Plan.AddProviderConfiguration | 201 → `PlanResponse` |
| 9 | PUT | /plans/{id}/provider-configurations/{provider} | Plan.UpdateProviderConfiguration | 204 |
| 10 | POST | /plans/{id}/provider-configurations/{provider}/activate | Plan.ActivateProviderConfiguration | 200 → `PlanResponse` |
| 11 | POST | /plans/{id}/provider-configurations/{provider}/deactivate | Plan.DeactivateProviderConfiguration | 200 → `PlanResponse` |
| 12 | POST | /plans/{id}/activate | Plan.Activate | 200 → `PlanResponse` |
| 13 | POST | /plans/{id}/deactivate | Plan.Deactivate | 200 → `PlanResponse` |

---

## 8. Persistencia (Firestore)

### Colección

`/plans/{planId}`

### Configuración DbContext

```csharp
modelBuilder.Entity<PlanAgg>(entity =>
{
    // Ignore: propiedades computed (no backing fields)
    entity.Ignore(p => p.HasActiveProvider);            

    // ComplexType: Price (Money con Currency anidado)
    entity.ComplexProperty(p => p.Price, price =>
    {
        // Ignore: propiedades computed de Money
        price.Ignore(m => m.IsZero);
        price.Ignore(m => m.IsPositive);
        price.Ignore(m => m.IsNegative);

        price.ComplexProperty(m => m.Currency);
    });

    // ArrayOf: Features (usa backing field _features)
    entity.ArrayOf(p => p.Features, feature =>
    {
        // Ignore: propiedades computed de Feature
        feature.Ignore(f => f.IsValid);
        feature.Ignore(f => f.DisplayValue);
    });

    // ArrayOf: ProviderConfigurations (usa backing field _providerConfigurations)
    entity.ArrayOf(p => p.ProviderConfigurations);
});
```

### Documento Ejemplo

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Plan Básico",
  "description": "Ideal para empezar",
  "price": {
    "amount": 9.99,
    "currency": {
      "code": "EUR",
      "symbol": "€",
      "decimalPlaces": 2
    }
  },
  "billingPeriod": "Monthly",
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
  "providerConfigurations": [
    {
      "provider": "Stripe",
      "externalProductId": "prod_xxx",
      "externalPriceId": "price_xxx",
      "isActive": true
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
**Autor**: Equipo Fudie