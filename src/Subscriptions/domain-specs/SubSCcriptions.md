# Domain Specification: Subscription

---

## 1. Enums

### SubscriptionStatus

```csharp
public enum SubscriptionStatus
{
    Trial,
    Active,
    PastDue,
    Cancelled,
    Expired
}
```

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

### 2.1 SubscriptionFeature

#### Estructura (Positional Record)

```csharp
public partial record SubscriptionFeature(
    string Code,
    FeatureType Type,
    int? Limit
);
```

#### Invariantes (Validator)

> Estas reglas se implementan en `SubscriptionFeatureValidator : AbstractValidator<SubscriptionFeature>`

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| Code | NotEmpty | "Code is required" |
| Code | MaxLength(50) | "Code cannot exceed 50 characters" |
| Code | Uppercase | "Code must be uppercase" |
| Code | NoSpaces | "Code cannot contain spaces" |
| Limit | NotNull when Type=Limit | "Limit is required when feature type is Limit" |
| Limit | > 0 when HasValue | "Limit must be greater than 0" |
| Limit | Null when Type=Boolean | "Limit is not allowed for Boolean feature type" |
| Limit | Null when Type=Unlimited | "Limit is not allowed for Unlimited feature type" |

#### Comando: SubscriptionFeature.Create

**Input**

| Campo | Tipo |
|-------|------|
| Code | string |
| Type | FeatureType |
| Limit | int? |

**Inyecta**: `IValidator<SubscriptionFeature>`

**Lógica**
```csharp
var feature = new SubscriptionFeature(
    command.Code,
    command.Type,
    command.Limit);

return featureValidator.ValidateOrThrow(feature);
```

#### Tests Unitarios

**Create:**

✅ Feature tipo Limit válido
- Input: Code="RESERVATIONS_MONTHLY", Type=Limit, Limit=100
- Resultado: SubscriptionFeature creado

✅ Feature tipo Boolean válido
- Input: Code="PRIORITY_SUPPORT", Type=Boolean, Limit=null
- Resultado: SubscriptionFeature creado

✅ Feature tipo Unlimited válido
- Input: Code="RESERVATIONS_MONTHLY", Type=Unlimited, Limit=null
- Resultado: SubscriptionFeature creado

❌ Code vacío
- Input: Code=""
- Resultado: ValidationException "Code is required"

❌ Code en minúsculas
- Input: Code="reservations_monthly"
- Resultado: ValidationException "Code must be uppercase"

❌ Type=Limit sin Limit
- Input: Type=Limit, Limit=null
- Resultado: ValidationException "Limit is required when feature type is Limit"

❌ Type=Boolean con Limit
- Input: Type=Boolean, Limit=100
- Resultado: ValidationException "Limit is not allowed for Boolean feature type"

---

## 3. Aggregate: Subscription

### Estructura

```
Subscription (Aggregate Root)
├─ Id: Guid
├─ TenantId: Guid                    ← Customer.Id (del customer-service)
├─ PlanId: Guid                      ← referencia al plan-service
├─ PlanName: string                  ← snapshot
├─ Status: SubscriptionStatus
├─ BillingPeriod: BillingPeriod      ← snapshot
├─ Price: Money                      ← snapshot (ComplexType)
├─ CurrentPeriodStart: DateTimeOffset
├─ CurrentPeriodEnd: DateTimeOffset
├─ TrialEndsAt: DateTimeOffset?
├─ CancelledAt: DateTimeOffset?
├─ CancellationReason: string?
├─ ExternalSubscriptionId: string?   ← Stripe sub_xxx
├─ ExternalCustomerId: string?       ← Stripe cus_xxx
├─ Provider: string?                 ← "Stripe"
└─ Features: IReadOnlyCollection<SubscriptionFeature>
```

#### Propiedades

| Propiedad | Tipo | Modificador |
|-----------|------|-------------|
| Id | Guid | init |
| TenantId | Guid | protected set |
| PlanId | Guid | protected set |
| PlanName | string | protected set |
| Status | SubscriptionStatus | protected set |
| BillingPeriod | BillingPeriod | protected set |
| Price | Money | protected set |
| CurrentPeriodStart | DateTimeOffset | protected set |
| CurrentPeriodEnd | DateTimeOffset | protected set |
| TrialEndsAt | DateTimeOffset? | protected set |
| CancelledAt | DateTimeOffset? | protected set |
| CancellationReason | string? | protected set |
| ExternalSubscriptionId | string? | protected set |
| ExternalCustomerId | string? | protected set |
| Provider | string? | protected set |

#### Colecciones

```csharp
protected HashSet<SubscriptionFeature> _features = [];
public IReadOnlyCollection<SubscriptionFeature> Features => _features.ToList().AsReadOnly();
```

#### Propiedades Calculadas

| Propiedad | Tipo | Fórmula |
|-----------|------|---------|
| IsInTrial | bool | `Status == SubscriptionStatus.Trial && TrialEndsAt > DateTimeOffset.UtcNow` |
| IsUsable | bool | `Status == SubscriptionStatus.Trial \|\| Status == SubscriptionStatus.Active \|\| Status == SubscriptionStatus.PastDue \|\| Status == SubscriptionStatus.Cancelled` |
| HasExternalProvider | bool | `!string.IsNullOrEmpty(ExternalSubscriptionId)` |

### Invariantes (Validator)

> Estas reglas se implementan en `SubscriptionValidator : AbstractValidator<Subscription>`

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| Id | NotEmpty | "Id is required" |
| TenantId | NotEmpty | "TenantId is required" |
| PlanId | NotEmpty | "PlanId is required" |
| PlanName | NotEmpty | "PlanName is required" |
| PlanName | MaxLength(100) | "PlanName cannot exceed 100 characters" |
| Price | NotNull | "Price is required" |
| CurrentPeriodStart | NotEmpty | "CurrentPeriodStart is required" |
| CurrentPeriodEnd | NotEmpty | "CurrentPeriodEnd is required" |
| CurrentPeriodStart/CurrentPeriodEnd | Start < End | "CurrentPeriodStart must be earlier than CurrentPeriodEnd" |
| CancellationReason | MaxLength(500) | "CancellationReason cannot exceed 500 characters" |
| ExternalSubscriptionId | MaxLength(100) | "ExternalSubscriptionId cannot exceed 100 characters" |
| ExternalCustomerId | MaxLength(100) | "ExternalCustomerId cannot exceed 100 characters" |
| Provider | MaxLength(50) | "Provider cannot exceed 50 characters" |
| Features | NotEmpty | "Features is required" |

---

## 4. Response

```csharp
public record SubscriptionResponse(
    Guid Id,
    Guid TenantId,
    Guid PlanId,
    string PlanName,
    SubscriptionStatus Status,
    BillingPeriod BillingPeriod,
    MoneyResponse Price,
    DateTimeOffset CurrentPeriodStart,
    DateTimeOffset CurrentPeriodEnd,
    DateTimeOffset? TrialEndsAt,
    DateTimeOffset? CancelledAt,
    string? CancellationReason,
    string? ExternalSubscriptionId,
    string? ExternalCustomerId,
    string? Provider,
    bool IsInTrial,
    bool IsUsable,
    bool HasExternalProvider,
    IReadOnlyCollection<SubscriptionFeatureResponse> Features
);

public record SubscriptionFeatureResponse(
    string Code,
    FeatureType Type,
    int? Limit
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

> **Tests de dominio**: Usar `TestableSubscription` para preparar estado previo. Usar `DomainFixture` para resolver comandos y validators. **NO encadenar comandos** para crear estado.
>
> **Tests de slice**: Usar `TestableSubscription` para el estado que devuelve el repository mock. Usar `DomainFixture` para resolver el comando que la slice inyecta. Mock de `IRepository` e `IUnitOfWork`.

---

### 6.1 Subscription.Create

#### Event Storming
```
🟡[Owner/ExternalApp] → 🔵(CreateSubscription) → 🟤[[Subscription]]
                                                      │
                                            ⚡ plan-service (GetPlan)
                                            ⚡ customer-service (CreateCustomer)
                                            ⚡ auth-service (SetTenantContext)
                                                      │
                                                      → 🟠<SubscriptionCreated>
```

#### Dominio

**Input**

| Campo | Tipo |
|-------|------|
| TenantId | Guid |
| PlanId | Guid |
| PlanName | string |
| BillingPeriod | BillingPeriod |
| Price | Money |
| Features | IReadOnlyCollection\<SubscriptionFeature\> |
| CurrentPeriodStart | DateTimeOffset |
| CurrentPeriodEnd | DateTimeOffset |
| TrialEndsAt | DateTimeOffset? |
| Status | SubscriptionStatus |

**Inyecta**
- `IValidator<Subscription>`

**Guards**

Ninguno (las validaciones externas son responsabilidad de la slice).

**Lógica**
```csharp
var subscription = new Subscription(Guid.NewGuid())
{
    TenantId = command.TenantId,
    PlanId = command.PlanId,
    PlanName = command.PlanName,
    Status = command.Status,
    BillingPeriod = command.BillingPeriod,
    Price = command.Price,
    CurrentPeriodStart = command.CurrentPeriodStart,
    CurrentPeriodEnd = command.CurrentPeriodEnd,
    TrialEndsAt = command.TrialEndsAt
};

foreach (var feature in command.Features)
    subscription._features.Add(feature);

return subscriptionValidator.ValidateOrThrow(subscription);
```

**Tests Unitarios Dominio**

✅ Crear subscription con datos válidos (Trial)
- Input: PlanId=valid, PlanName="Plan Básico", Status=Trial, TrialEndsAt=futuro, Features=[RESERVATIONS_MONTHLY]
- Resultado: Subscription creada con Status=Trial, IsInTrial=true, IsUsable=true

✅ Crear subscription con datos válidos (Active)
- Input: PlanId=valid, PlanName="Plan Básico", Status=Active, TrialEndsAt=null, Features=[RESERVATIONS_MONTHLY]
- Resultado: Subscription creada con Status=Active, IsUsable=true

❌ PlanId vacío
- Input: PlanId=Guid.Empty
- Resultado: ValidationException "PlanId is required"

❌ TenantId vacío
- Input: TenantId=Guid.Empty
- Resultado: ValidationException "TenantId is required"

❌ PlanName vacío
- Input: PlanName=""
- Resultado: ValidationException "PlanName is required"

❌ Features vacío
- Input: Features=[]
- Resultado: ValidationException "Features is required"

❌ CurrentPeriodStart >= CurrentPeriodEnd
- Input: Start=2026-02-01, End=2026-01-01
- Resultado: ValidationException "CurrentPeriodStart must be earlier than CurrentPeriodEnd"

#### Slice: POST /subscriptions

**Request**
```csharp
public record CreateSubscriptionRequest(
    Guid PlanId,
    CreateCustomerRequest Customer,
    bool StartWithTrial = false
);

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

**Orquestación de la slice**

```csharp
// 1. Obtener y validar el plan
var plan = await planServiceClient.GetActivePlanAsync(request.PlanId);

// 2. Crear el customer (tenant)
var customer = await customerServiceClient.CreateCustomerAsync(request.Customer);

// 3. Establecer contexto de tenant en la session
await authServiceClient.SetTenantContextAsync(sessionId, customer.Id, isOwner: true);

// TODO: Integración con Stripe pendiente.
// Cuando se implemente, aquí se creará la subscription en Stripe
// y se obtendrán ExternalSubscriptionId, ExternalCustomerId y Provider.
// Por ahora la subscription se crea sin datos de provider externo.

// 4. Preparar datos del plan para el dominio
var features = plan.Features.Select(f =>
    createSubscriptionFeature.Execute(new(f.Code, f.Type, f.Limit))).ToList();

var price = createMoney.Execute(new(plan.Price.Amount, plan.Price.Currency.Code));

var periodStart = DateTimeOffset.UtcNow;
var periodEnd = CalculatePeriodEnd(periodStart, plan.BillingPeriod);
DateTimeOffset? trialEndsAt = request.StartWithTrial ? periodStart.AddDays(14) : null;
var status = request.StartWithTrial ? SubscriptionStatus.Trial : SubscriptionStatus.Active;

// 5. Crear la subscription
var subscription = createSubscription.Execute(new CreateSubscriptionCommand(
    TenantId: customer.Id,
    PlanId: plan.Id,
    PlanName: plan.Name,
    BillingPeriod: plan.BillingPeriod,
    Price: price,
    Features: features,
    CurrentPeriodStart: periodStart,
    CurrentPeriodEnd: request.StartWithTrial ? trialEndsAt!.Value : periodEnd,
    TrialEndsAt: trialEndsAt,
    Status: status));

repository.Add(subscription);
await unitOfWork.SaveChangesAsync();
```

**Response**: 201 Created → `SubscriptionResponse`

**Tests Unitarios Servicio**

✅ Orquestación completa sin trial
- Mock: planServiceClient devuelve plan activo
- Mock: customerServiceClient devuelve customer con Id
- Mock: authServiceClient.SetTenantContext completa
- Verifica: repository.Add con subscription Status=Active

✅ Orquestación completa con trial
- Input: StartWithTrial=true
- Verifica: subscription con Status=Trial, TrialEndsAt no null

❌ Plan no encontrado
- Mock: planServiceClient lanza NotFoundException
- Resultado: NotFoundException

❌ Plan inactivo
- Mock: planServiceClient lanza ValidationException "Plan is not active"
- Resultado: ValidationException

**Tests Integración**

✅ 201 Created → SubscriptionResponse

❌ 404 → Plan no encontrado

❌ 422 → Plan inactivo o sin provider

---

### 6.2 GetSubscription

#### Event Storming
```
🟡[Owner/Admin] → 🔵(GetSubscription) → 🟤[[Subscription]] → 📊 SubscriptionResponse
```

#### Slice: GET /subscriptions/{id}

**Response**: 200 OK → `SubscriptionResponse`

**Tests Unitarios Servicio**

✅ Obtiene la subscription del repositorio con el id correcto
- Verifica que repository.Get es llamado con el id

✅ Retorna Response mapeado correctamente
- Verifica que el Response contiene los datos de la subscription

**Tests Integración**

✅ 200 OK → SubscriptionResponse

❌ 404 → No encontrado

---

### 6.3 GetSubscriptionByTenant

#### Event Storming
```
🟡[System/Microservice] → 🔵(GetSubscriptionByTenant) → 🟤[[Subscription]] → 📊 SubscriptionResponse
```

> Endpoint clave para que otros microservicios consulten qué subscription (y qué features) tiene un tenant.

#### Slice: GET /subscriptions/by-tenant/{tenantId}

**Response**: 200 OK → `SubscriptionResponse`

**Tests Unitarios Servicio**

✅ Obtiene la subscription del repositorio por tenantId
- Verifica que repository.GetByTenantIdAsync es llamado con el tenantId

✅ Retorna Response mapeado correctamente

**Tests Integración**

✅ 200 OK → SubscriptionResponse

❌ 404 → No encontrada subscription para ese tenant

---

### 6.4 ListSubscriptions

#### Event Storming
```
🟡[Admin] → 🔵(ListSubscriptions) → 🟤[[Subscription]] → 📊 SubscriptionResponse[]
```

#### Slice: GET /subscriptions?status=Active

**QueryParams**: `?status=Active` (opcional)

**Response**: 200 OK → `SubscriptionResponse[]`

**Tests Unitarios Servicio**

✅ Retorna lista de subscriptions mapeadas correctamente

✅ Filtra por status cuando se proporciona

**Tests Integración**

✅ 200 OK → Array de SubscriptionResponse

✅ 200 OK → Array vacío si no hay subscriptions

---

### 6.5 Subscription.Activate

#### Event Storming
```
🟡[⚡Stripe Webhook] → 🔵(ActivateSubscription) → 🟤[[Subscription]] → 🟠<SubscriptionActivated>
```

> Disparado por webhook de Stripe: `checkout.session.completed` o `invoice.paid` (primera factura).

#### Dominio

**Input**

| Campo | Tipo |
|-------|------|
| ExternalSubscriptionId | string? |
| ExternalCustomerId | string? |
| CurrentPeriodStart | DateTimeOffset |
| CurrentPeriodEnd | DateTimeOffset |

**Inyecta**
- `IValidator<Subscription>`

**Guards**

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Status no es Trial ni PastDue | 409 | ConflictGuard | "Subscription cannot be activated from current status" |

**Lógica**
```csharp
ConflictGuard.ThrowIf(
    subscription.Status != SubscriptionStatus.Trial && subscription.Status != SubscriptionStatus.PastDue,
    "Subscription cannot be activated from current status");

subscription.Status = SubscriptionStatus.Active;
subscription.CurrentPeriodStart = command.CurrentPeriodStart;
subscription.CurrentPeriodEnd = command.CurrentPeriodEnd;
subscription.TrialEndsAt = null;

if (command.ExternalSubscriptionId != null)
    subscription.ExternalSubscriptionId = command.ExternalSubscriptionId;

if (command.ExternalCustomerId != null)
    subscription.ExternalCustomerId = command.ExternalCustomerId;

return subscriptionValidator.ValidateOrThrow(subscription);
```

**Tests Unitarios Dominio**

✅ Activar desde Trial
- Precondición: Status=Trial
- Resultado: Status=Active, TrialEndsAt=null

✅ Activar desde PastDue (pago recuperado)
- Precondición: Status=PastDue
- Resultado: Status=Active

❌ Activar desde Active
- Precondición: Status=Active
- Resultado: ConflictException

❌ Activar desde Expired
- Precondición: Status=Expired
- Resultado: ConflictException

#### Slice: Webhook interno

> Se consume desde el endpoint de webhook de Stripe: `POST /subscriptions/webhooks/stripe`

---

### 6.6 Subscription.Renew

#### Event Storming
```
🟡[⚡Stripe Webhook] → 🔵(RenewSubscription) → 🟤[[Subscription]] → 🟠<SubscriptionRenewed>
```

> Disparado por webhook de Stripe: `invoice.paid` (renovación periódica). La slice es responsable de crear el BillingHistory antes de renovar.

#### Dominio

**Input**

| Campo | Tipo |
|-------|------|
| NewPeriodStart | DateTimeOffset |
| NewPeriodEnd | DateTimeOffset |

**Inyecta**
- `IValidator<Subscription>`

**Guards**

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Status no es Active | 409 | ConflictGuard | "Only active subscriptions can be renewed" |

**Lógica**
```csharp
ConflictGuard.ThrowIf(
    subscription.Status != SubscriptionStatus.Active,
    "Only active subscriptions can be renewed");

subscription.CurrentPeriodStart = command.NewPeriodStart;
subscription.CurrentPeriodEnd = command.NewPeriodEnd;

return subscriptionValidator.ValidateOrThrow(subscription);
```

**Tests Unitarios Dominio**

✅ Renovar subscription activa
- Precondición: Status=Active, CurrentPeriodEnd=2026-02-28
- Input: NewPeriodStart=2026-03-01, NewPeriodEnd=2026-03-31
- Resultado: Períodos actualizados

❌ Renovar subscription no activa
- Precondición: Status=Cancelled
- Resultado: ConflictException

#### Slice: Webhook interno

---

### 6.7 Subscription.MarkPastDue

#### Event Storming
```
🟡[⚡Stripe Webhook] → 🔵(MarkPastDue) → 🟤[[Subscription]] → 🟠<SubscriptionPastDue>
```

> Disparado por webhook de Stripe: `invoice.payment_failed`.

#### Dominio

**Input**

Ninguno

**Inyecta**
- `IValidator<Subscription>`

**Guards**

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Status no es Active ni Trial | 409 | ConflictGuard | "Subscription cannot be marked as past due from current status" |

**Lógica**
```csharp
ConflictGuard.ThrowIf(
    subscription.Status != SubscriptionStatus.Active && subscription.Status != SubscriptionStatus.Trial,
    "Subscription cannot be marked as past due from current status");

subscription.Status = SubscriptionStatus.PastDue;

return subscriptionValidator.ValidateOrThrow(subscription);
```

**Tests Unitarios Dominio**

✅ Marcar como PastDue desde Active
- Precondición: Status=Active
- Resultado: Status=PastDue, IsUsable=true (período de gracia)

✅ Marcar como PastDue desde Trial
- Precondición: Status=Trial
- Resultado: Status=PastDue

❌ Marcar como PastDue desde Expired
- Precondición: Status=Expired
- Resultado: ConflictException

#### Slice: Webhook interno

---

### 6.8 Subscription.Cancel

#### Event Storming
```
🟡[Owner] → 🔵(CancelSubscription) → 🟤[[Subscription]] → 🟠<SubscriptionCancelled>
```

> El usuario solicita cancelación. La subscription sigue activa hasta el fin del período actual.

#### Dominio

**Input**

| Campo | Tipo |
|-------|------|
| Reason | string? |

**Inyecta**
- `IValidator<Subscription>`

**Guards**

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Status no es Active ni Trial ni PastDue | 409 | ConflictGuard | "Subscription cannot be cancelled from current status" |

**Lógica**
```csharp
ConflictGuard.ThrowIf(
    subscription.Status != SubscriptionStatus.Active &&
    subscription.Status != SubscriptionStatus.Trial &&
    subscription.Status != SubscriptionStatus.PastDue,
    "Subscription cannot be cancelled from current status");

subscription.Status = SubscriptionStatus.Cancelled;
subscription.CancelledAt = DateTimeOffset.UtcNow;
subscription.CancellationReason = command.Reason;

return subscriptionValidator.ValidateOrThrow(subscription);
```

**Tests Unitarios Dominio**

✅ Cancelar subscription activa
- Precondición: Status=Active
- Input: Reason="Too expensive"
- Resultado: Status=Cancelled, CancelledAt set, IsUsable=true

✅ Cancelar subscription en trial
- Precondición: Status=Trial
- Resultado: Status=Cancelled

✅ Cancelar sin razón
- Input: Reason=null
- Resultado: Status=Cancelled, CancellationReason=null

❌ Cancelar subscription expirada
- Precondición: Status=Expired
- Resultado: ConflictException

❌ Cancelar subscription ya cancelada
- Precondición: Status=Cancelled
- Resultado: ConflictException

#### Slice: POST /subscriptions/{id}/cancel

**Request**
```csharp
public record CancelSubscriptionRequest(
    string? Reason
);
```

**Response**: 200 OK → `SubscriptionResponse`

**Tests Unitarios Servicio**

✅ Cancela subscription
- Mock: repository devuelve subscription activa
- Verifica: cancelSubscription.Execute llamado

❌ Subscription no encontrada
- Mock: repository devuelve null
- Resultado: NotFoundException

**Tests Integración**

✅ 200 OK → SubscriptionResponse con Status=Cancelled

❌ 404 → Subscription no encontrada

❌ 409 → Status no permite cancelación

---

### 6.9 Subscription.Expire

#### Event Storming
```
🟡[⚡Stripe Webhook / ⚡Scheduler] → 🔵(ExpireSubscription) → 🟤[[Subscription]] → 🟠<SubscriptionExpired>
```

> Disparado por webhook de Stripe: `customer.subscription.deleted`, o por un job programado cuando el período de una subscription Cancelled termina.

#### Dominio

**Input**

Ninguno

**Inyecta**
- `IValidator<Subscription>`

**Guards**

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Status no es Cancelled ni PastDue | 409 | ConflictGuard | "Subscription cannot be expired from current status" |

**Lógica**
```csharp
ConflictGuard.ThrowIf(
    subscription.Status != SubscriptionStatus.Cancelled &&
    subscription.Status != SubscriptionStatus.PastDue,
    "Subscription cannot be expired from current status");

subscription.Status = SubscriptionStatus.Expired;

return subscriptionValidator.ValidateOrThrow(subscription);
```

**Tests Unitarios Dominio**

✅ Expirar subscription cancelada
- Precondición: Status=Cancelled
- Resultado: Status=Expired, IsUsable=false

✅ Expirar subscription PastDue
- Precondición: Status=PastDue
- Resultado: Status=Expired

❌ Expirar subscription activa
- Precondición: Status=Active
- Resultado: ConflictException

#### Slice: Webhook interno / Scheduled job

---

### 6.10 Subscription.SyncPlan

#### Event Storming
```
🟡[⚡Event: PlanUpdated] → 🔵(SyncPlan) → 🟤[[Subscription]] → 🟠<SubscriptionPlanSynced>
```

> Disparado por evento `PlanUpdated` del plan-service. Actualiza masivamente los snapshots de todas las subscriptions usables de ese plan.

#### Dominio

**Input**

| Campo | Tipo |
|-------|------|
| PlanName | string |
| BillingPeriod | BillingPeriod |
| Price | Money |
| Features | IReadOnlyCollection\<SubscriptionFeature\> |

**Inyecta**
- `IValidator<Subscription>`

**Guards**

Ninguno.

**Lógica**
```csharp
subscription.PlanName = command.PlanName;
subscription.BillingPeriod = command.BillingPeriod;
subscription.Price = command.Price;

subscription._features.Clear();
foreach (var feature in command.Features)
    subscription._features.Add(feature);

return subscriptionValidator.ValidateOrThrow(subscription);
```

**Tests Unitarios Dominio**

✅ Sincronizar plan con nuevos datos
- Precondición: Subscription con PlanName="Plan Básico", Price=9.99, Features=[RESERVATIONS_MONTHLY Limit=100]
- Input: PlanName="Plan Básico Plus", Price=12.99, Features=[RESERVATIONS_MONTHLY Limit=200, PRIORITY_SUPPORT Boolean]
- Resultado: Todos los snapshots actualizados

✅ Sincronizar reduce features
- Precondición: Subscription con 3 features
- Input: Features con 1 feature
- Resultado: Solo queda 1 feature

#### Slice: Event handler (no endpoint HTTP)

> Se consume desde un event handler que escucha `PlanUpdated`. La slice carga todas las subscriptions con el `PlanId` afectado y ejecuta `SyncPlan` en cada una.

---

### 6.11 Subscription.SetExternalIds

#### Event Storming
```
🟡[⚡Stripe Webhook] → 🔵(SetExternalIds) → 🟤[[Subscription]] → 🟠<ExternalIdsSet>
```

> Para casos donde los IDs externos se resuelven después de la creación (checkout async).

#### Dominio

**Input**

| Campo | Tipo |
|-------|------|
| ExternalSubscriptionId | string |
| ExternalCustomerId | string |
| Provider | string |

**Inyecta**
- `IValidator<Subscription>`

**Guards**

Ninguno.

**Lógica**
```csharp
subscription.ExternalSubscriptionId = command.ExternalSubscriptionId;
subscription.ExternalCustomerId = command.ExternalCustomerId;
subscription.Provider = command.Provider;

return subscriptionValidator.ValidateOrThrow(subscription);
```

**Tests Unitarios Dominio**

✅ Establecer IDs externos
- Input: ExternalSubscriptionId="sub_xxx", ExternalCustomerId="cus_xxx", Provider="Stripe"
- Resultado: IDs actualizados

#### Slice: Webhook interno

---

## 7. Webhook Endpoint

### POST /subscriptions/webhooks/stripe

> Endpoint único para recibir todos los eventos de Stripe. Valida la firma del webhook, identifica el tipo de evento y despacha al comando de dominio correspondiente.

**Request**: Raw body (Stripe event JSON)

**Headers**: `Stripe-Signature`

**Mapeo de eventos Stripe → Comandos**

| Evento Stripe | Comando |
|---------------|---------|
| `checkout.session.completed` | Subscription.Activate o Subscription.SetExternalIds |
| `invoice.paid` | Subscription.Activate (primera) o Subscription.Renew (renovación) |
| `invoice.payment_failed` | Subscription.MarkPastDue |
| `customer.subscription.deleted` | Subscription.Expire |

**Response**: 200 OK (siempre, para que Stripe no reintente)

---

## 8. Descripciones de Permisos

> Las descripciones son **responsabilidad de producto**. Se definen en español durante la sesión de diseño. Claude Code genera el archivo de descripciones del microservicio con el español como base y traduce automáticamente al resto de idiomas necesarios.
>
> Deben ser claras, concisas y comprensibles para alguien sin conocimientos técnicos — es lo que el administrador del restaurante ve cuando configura roles.

### Scopes atómicos

| Scope (nombre de clase) | Descripción (es) |
|--------------------------|-------------------|
| `CreateSubscription` | Contratar una suscripción para el establecimiento |
| `GetSubscription` | Ver los detalles de la suscripción |
| `GetSubscriptionByTenant` | Consultar la suscripción de un establecimiento |
| `ListSubscriptions` | Ver el listado de suscripciones |
| `CancelSubscription` | Cancelar la suscripción del establecimiento |

> Los comandos de webhook (Activate, Renew, MarkPastDue, Expire, SetExternalIds, SyncPlan) no tienen scope — son disparados por el sistema, no por usuarios.

### Agrupaciones custom

| Agrupación | Descripción (es) | Scopes que incluye |
|------------|-------------------|-------------------|
| `subscription:billing` | Gestión de facturación y suscripción | `CreateSubscription`, `CancelSubscription` |

> Las agrupaciones automáticas (`subscription:read` y `subscription:write`) se generan por reflexión a partir del verbo HTTP.

---

## 9. Resumen de Endpoints (Orden de Implementación)

| # | Método | Ruta | Comando/Query | Response |
|---|--------|------|---------------|----------|
| 1 | POST | /subscriptions | Subscription.Create | 201 → `SubscriptionResponse` |
| 2 | GET | /subscriptions/{id} | GetSubscription | 200 → `SubscriptionResponse` |
| 3 | GET | /subscriptions/by-tenant/{tenantId} | GetSubscriptionByTenant | 200 → `SubscriptionResponse` |
| 4 | GET | /subscriptions | ListSubscriptions | 200 → `SubscriptionResponse[]` |
| 5 | POST | /subscriptions/{id}/cancel | Subscription.Cancel | 200 → `SubscriptionResponse` |
| 6 | POST | /subscriptions/webhooks/stripe | Webhook dispatcher | 200 |

---

## 10. Persistencia (Firestore)

### Colección

`/subscriptions/{subscriptionId}`

### Configuración DbContext

```csharp
modelBuilder.Entity<SubscriptionAgg>(entity =>
{
    // Ignore: propiedades computed
    entity.Ignore(s => s.IsInTrial);
    entity.Ignore(s => s.IsUsable);
    entity.Ignore(s => s.HasExternalProvider);

    // ComplexType: Price (Money con Currency anidado)
    entity.ComplexProperty(s => s.Price, price =>
    {
        price.Ignore(m => m.IsZero);
        price.Ignore(m => m.IsPositive);
        price.Ignore(m => m.IsNegative);

        price.ComplexProperty(m => m.Currency);
    });

    // ArrayOf: Features
    entity.ArrayOf(s => s.Features);
});
```

### Documento Ejemplo

```json
{
  "id": "a1b2c3d4-5678-9012-3456-789012345678",
  "tenantId": "customer-001-guid",
  "planId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "planName": "Plan Básico",
  "status": "Active",
  "billingPeriod": "Monthly",
  "price": {
    "amount": 9.99,
    "currency": {
      "code": "EUR",
      "symbol": "€",
      "decimalPlaces": 2
    }
  },
  "currentPeriodStart": "2026-02-01T00:00:00Z",
  "currentPeriodEnd": "2026-02-28T23:59:59Z",
  "trialEndsAt": null,
  "cancelledAt": null,
  "cancellationReason": null,
  "externalSubscriptionId": "sub_1OXbFJ2eZvKYlo2C5004ogvj",
  "externalCustomerId": "cus_PZ8qx5N3VBn5DP",
  "provider": "Stripe",
  "features": [
    {
      "code": "RESERVATIONS_MONTHLY",
      "type": "Limit",
      "limit": 100
    },
    {
      "code": "PRIORITY_SUPPORT",
      "type": "Boolean",
      "limit": null
    }
  ]
}
```

---

## 11. Hot Spots ⚠️

| # | Pregunta | Estado |
|---|----------|--------|
| 1 | ¿Duración del trial? (14 días propuesto en la slice) | Pendiente |
| 2 | ¿Cuántos días de gracia en PastDue antes de pasar a Expired? | Pendiente |
| 3 | ¿Se permite re-suscribirse después de Expired? (crear nueva subscription) | Pendiente |
| 4 | ¿Cómo se transporta el evento PlanUpdated entre microservicios? (pub/sub, polling) | Pendiente |

---

**Fecha**: 2026-02-12
**Autor**: Equipo Fudie
