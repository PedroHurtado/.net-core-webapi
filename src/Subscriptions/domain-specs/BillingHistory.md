# Domain Specification: BillingHistory

---

## 1. Enums

### PaymentStatus

```csharp
public enum PaymentStatus
{
    Paid,
    Failed,
    Refunded
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

---

## 2. Value Objects

### 2.1 BillingFeatureSnapshot

#### Estructura (Positional Record)

```csharp
public partial record BillingFeatureSnapshot(
    string Code,
    FeatureType Type,
    int? Limit
);
```

#### Invariantes (Validator)

> Estas reglas se implementan en `BillingFeatureSnapshotValidator : AbstractValidator<BillingFeatureSnapshot>`

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| Code | NotEmpty | "Code is required" |
| Code | MaxLength(50) | "Code cannot exceed 50 characters" |
| Limit | NotNull when Type=Limit | "Limit is required when feature type is Limit" |
| Limit | > 0 when HasValue | "Limit must be greater than 0" |
| Limit | Null when Type=Boolean | "Limit is not allowed for Boolean feature type" |
| Limit | Null when Type=Unlimited | "Limit is not allowed for Unlimited feature type" |

#### Comando: BillingFeatureSnapshot.Create

**Input**

| Campo | Tipo |
|-------|------|
| Code | string |
| Type | FeatureType |
| Limit | int? |

**Inyecta**: `IValidator<BillingFeatureSnapshot>`

**Lógica**
```csharp
var snapshot = new BillingFeatureSnapshot(
    command.Code,
    command.Type,
    command.Limit);

return snapshotValidator.ValidateOrThrow(snapshot);
```

#### Tests Unitarios

**Create:**

✅ Snapshot tipo Limit válido
- Input: Code="RESERVATIONS_MONTHLY", Type=Limit, Limit=100
- Resultado: BillingFeatureSnapshot creado

✅ Snapshot tipo Boolean válido
- Input: Code="PRIORITY_SUPPORT", Type=Boolean, Limit=null
- Resultado: BillingFeatureSnapshot creado

❌ Code vacío
- Input: Code=""
- Resultado: ValidationException "Code is required"

❌ Type=Limit sin Limit
- Input: Type=Limit, Limit=null
- Resultado: ValidationException "Limit is required when feature type is Limit"

---

## 3. Aggregate: BillingHistory

### Estructura

```
BillingHistory (Aggregate Root)
├─ Id: Guid
├─ TenantId: Guid
├─ SubscriptionId: Guid
├─ PlanId: Guid
├─ PlanName: string                  ← snapshot del momento
├─ BillingPeriod: BillingPeriod      ← snapshot del momento
├─ Price: Money                      ← snapshot del momento (ComplexType)
├─ PeriodStart: DateTimeOffset
├─ PeriodEnd: DateTimeOffset
├─ PaymentStatus: PaymentStatus
├─ PaidAt: DateTimeOffset?
├─ ExternalInvoiceId: string?        ← Stripe invoice_id (in_xxx)
├─ ExternalPaymentIntentId: string?  ← Stripe payment_intent (pi_xxx)
├─ Provider: string?
└─ Features: IReadOnlyCollection<BillingFeatureSnapshot>
```

#### Propiedades

| Propiedad | Tipo | Modificador |
|-----------|------|-------------|
| Id | Guid | init |
| TenantId | Guid | protected set |
| SubscriptionId | Guid | protected set |
| PlanId | Guid | protected set |
| PlanName | string | protected set |
| BillingPeriod | BillingPeriod | protected set |
| Price | Money | protected set |
| PeriodStart | DateTimeOffset | protected set |
| PeriodEnd | DateTimeOffset | protected set |
| PaymentStatus | PaymentStatus | protected set |
| PaidAt | DateTimeOffset? | protected set |
| ExternalInvoiceId | string? | protected set |
| ExternalPaymentIntentId | string? | protected set |
| Provider | string? | protected set |

#### Colecciones

```csharp
protected HashSet<BillingFeatureSnapshot> _features = [];
public IReadOnlyCollection<BillingFeatureSnapshot> Features => _features.ToList().AsReadOnly();
```

### Invariantes (Validator)

> Estas reglas se implementan en `BillingHistoryValidator : AbstractValidator<BillingHistory>`

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| Id | NotEmpty | "Id is required" |
| TenantId | NotEmpty | "TenantId is required" |
| SubscriptionId | NotEmpty | "SubscriptionId is required" |
| PlanId | NotEmpty | "PlanId is required" |
| PlanName | NotEmpty | "PlanName is required" |
| PlanName | MaxLength(100) | "PlanName cannot exceed 100 characters" |
| Price | NotNull | "Price is required" |
| PeriodStart | NotEmpty | "PeriodStart is required" |
| PeriodEnd | NotEmpty | "PeriodEnd is required" |
| PeriodStart/PeriodEnd | Start < End | "PeriodStart must be earlier than PeriodEnd" |
| ExternalInvoiceId | MaxLength(100) | "ExternalInvoiceId cannot exceed 100 characters" |
| ExternalPaymentIntentId | MaxLength(100) | "ExternalPaymentIntentId cannot exceed 100 characters" |
| Provider | MaxLength(50) | "Provider cannot exceed 50 characters" |
| Features | NotEmpty | "Features is required" |

---

## 4. Response

```csharp
public record BillingHistoryResponse(
    Guid Id,
    Guid TenantId,
    Guid SubscriptionId,
    Guid PlanId,
    string PlanName,
    BillingPeriod BillingPeriod,
    MoneyResponse Price,
    DateTimeOffset PeriodStart,
    DateTimeOffset PeriodEnd,
    PaymentStatus PaymentStatus,
    DateTimeOffset? PaidAt,
    string? ExternalInvoiceId,
    string? ExternalPaymentIntentId,
    string? Provider,
    IReadOnlyCollection<BillingFeatureSnapshotResponse> Features
);

public record BillingFeatureSnapshotResponse(
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

> **Nota**: BillingHistory es mayoritariamente un registro inmutable. Se crea cuando se renueva una subscription o se procesa un pago, y solo se actualiza el PaymentStatus si cambia (Failed → Paid, Paid → Refunded).

> **Tests de dominio**: Usar `TestableBillingHistory` para preparar estado previo. Usar `DomainFixture` para resolver comandos y validators. **NO encadenar comandos** para crear estado.
>
> **Tests de slice**: Usar `TestableBillingHistory` para el estado que devuelve el repository mock. Usar `DomainFixture` para resolver el comando que la slice inyecta. Mock de `IRepository` e `IUnitOfWork`.

---

### 6.1 BillingHistory.Create

#### Event Storming
```
🟡[⚡Subscription.Renew / ⚡Stripe Webhook] → 🔵(CreateBillingHistory) → 🟤[[BillingHistory]] → 🟠<BillingHistoryCreated>
```

> Se crea cuando se renueva una subscription (snapshot del período que finaliza) o cuando se recibe un pago de Stripe.

#### Dominio

**Input**

| Campo | Tipo |
|-------|------|
| TenantId | Guid |
| SubscriptionId | Guid |
| PlanId | Guid |
| PlanName | string |
| BillingPeriod | BillingPeriod |
| Price | Money |
| PeriodStart | DateTimeOffset |
| PeriodEnd | DateTimeOffset |
| PaymentStatus | PaymentStatus |
| PaidAt | DateTimeOffset? |
| ExternalInvoiceId | string? |
| ExternalPaymentIntentId | string? |
| Provider | string? |
| Features | IReadOnlyCollection\<BillingFeatureSnapshot\> |

**Inyecta**
- `IValidator<BillingHistory>`

**Guards**

Ninguno.

**Lógica**
```csharp
var record = new BillingHistory(Guid.NewGuid())
{
    TenantId = command.TenantId,
    SubscriptionId = command.SubscriptionId,
    PlanId = command.PlanId,
    PlanName = command.PlanName,
    BillingPeriod = command.BillingPeriod,
    Price = command.Price,
    PeriodStart = command.PeriodStart,
    PeriodEnd = command.PeriodEnd,
    PaymentStatus = command.PaymentStatus,
    PaidAt = command.PaidAt,
    ExternalInvoiceId = command.ExternalInvoiceId,
    ExternalPaymentIntentId = command.ExternalPaymentIntentId,
    Provider = command.Provider
};

foreach (var feature in command.Features)
    record._features.Add(feature);

return billingHistoryValidator.ValidateOrThrow(record);
```

**Tests Unitarios Dominio**

✅ Crear registro con pago exitoso
- Input: PaymentStatus=Paid, PaidAt=now, Features=[RESERVATIONS_MONTHLY]
- Resultado: BillingHistory creado

✅ Crear registro con pago fallido
- Input: PaymentStatus=Failed, PaidAt=null
- Resultado: BillingHistory creado

❌ SubscriptionId vacío
- Input: SubscriptionId=Guid.Empty
- Resultado: ValidationException "SubscriptionId is required"

❌ TenantId vacío
- Input: TenantId=Guid.Empty
- Resultado: ValidationException "TenantId is required"

❌ PlanName vacío
- Input: PlanName=""
- Resultado: ValidationException "PlanName is required"

❌ Features vacío
- Input: Features=[]
- Resultado: ValidationException "Features is required"

❌ PeriodStart >= PeriodEnd
- Input: PeriodStart=2026-03-01, PeriodEnd=2026-02-01
- Resultado: ValidationException "PeriodStart must be earlier than PeriodEnd"

#### Slice: No tiene endpoint HTTP propio

> Se crea internamente desde la slice de Subscription.Renew o desde el webhook handler de Stripe.

---

### 6.2 GetBillingHistory

#### Event Storming
```
🟡[Owner/Admin] → 🔵(GetBillingHistory) → 🟤[[BillingHistory]] → 📊 BillingHistoryResponse
```

#### Slice: GET /billing-history/{id}

**Response**: 200 OK → `BillingHistoryResponse`

**Tests Unitarios Servicio**

✅ Obtiene el registro del repositorio con el id correcto
- Verifica que repository.Get es llamado con el id

✅ Retorna Response mapeado correctamente

**Tests Integración**

✅ 200 OK → BillingHistoryResponse

❌ 404 → No encontrado

---

### 6.3 ListBillingHistoryByTenant

#### Event Storming
```
🟡[Owner/Admin] → 🔵(ListBillingHistoryByTenant) → 🟤[[BillingHistory]] → 📊 BillingHistoryResponse[]
```

#### Slice: GET /billing-history/by-tenant/{tenantId}

**QueryParams**: `?from=2026-01-01&to=2026-12-31` (opcional, filtro por PeriodStart)

**Response**: 200 OK → `BillingHistoryResponse[]`

**Tests Unitarios Servicio**

✅ Retorna lista filtrada por tenantId

✅ Filtra por rango de fechas cuando se proporciona

**Tests Integración**

✅ 200 OK → Array de BillingHistoryResponse

✅ 200 OK → Array vacío si no hay registros

---

### 6.4 ListBillingHistoryBySubscription

#### Event Storming
```
🟡[Owner/Admin] → 🔵(ListBillingHistoryBySubscription) → 🟤[[BillingHistory]] → 📊 BillingHistoryResponse[]
```

#### Slice: GET /billing-history/by-subscription/{subscriptionId}

**Response**: 200 OK → `BillingHistoryResponse[]`

**Tests Unitarios Servicio**

✅ Retorna lista filtrada por subscriptionId

**Tests Integración**

✅ 200 OK → Array de BillingHistoryResponse

✅ 200 OK → Array vacío si no hay registros

---

### 6.5 BillingHistory.MarkAsPaid

#### Event Storming
```
🟡[⚡Stripe Webhook] → 🔵(MarkAsPaid) → 🟤[[BillingHistory]] → 🟠<BillingHistoryPaid>
```

> Un registro que estaba como Failed se marca como Paid cuando Stripe reintenta el cobro y tiene éxito.

#### Dominio

**Input**

| Campo | Tipo |
|-------|------|
| PaidAt | DateTimeOffset |
| ExternalPaymentIntentId | string? |

**Inyecta**
- `IValidator<BillingHistory>`

**Guards**

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| PaymentStatus no es Failed | 409 | ConflictGuard | "Only failed payments can be marked as paid" |

**Lógica**
```csharp
ConflictGuard.ThrowIf(
    record.PaymentStatus != PaymentStatus.Failed,
    "Only failed payments can be marked as paid");

record.PaymentStatus = PaymentStatus.Paid;
record.PaidAt = command.PaidAt;

if (command.ExternalPaymentIntentId != null)
    record.ExternalPaymentIntentId = command.ExternalPaymentIntentId;

return billingHistoryValidator.ValidateOrThrow(record);
```

**Tests Unitarios Dominio**

> Estado previo: `TestableBillingHistory` con PaymentStatus=Failed/Paid.

✅ Marcar como pagado un registro fallido
- Precondición: PaymentStatus=Failed
- Input: PaidAt=now
- Resultado: PaymentStatus=Paid, PaidAt set

❌ Marcar como pagado un registro ya pagado
- Precondición: PaymentStatus=Paid
- Resultado: ConflictException "Only failed payments can be marked as paid"

#### Slice: Webhook interno

---

### 6.6 BillingHistory.Refund

#### Event Storming
```
🟡[⚡Stripe Webhook] → 🔵(RefundBillingHistory) → 🟤[[BillingHistory]] → 🟠<BillingHistoryRefunded>
```

#### Dominio

**Input**

Ninguno

**Inyecta**
- `IValidator<BillingHistory>`

**Guards**

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| PaymentStatus no es Paid | 409 | ConflictGuard | "Only paid records can be refunded" |

**Lógica**
```csharp
ConflictGuard.ThrowIf(
    record.PaymentStatus != PaymentStatus.Paid,
    "Only paid records can be refunded");

record.PaymentStatus = PaymentStatus.Refunded;

return billingHistoryValidator.ValidateOrThrow(record);
```

**Tests Unitarios Dominio**

> Estado previo: `TestableBillingHistory` con PaymentStatus=Paid/Failed.

✅ Reembolsar registro pagado
- Precondición: PaymentStatus=Paid
- Resultado: PaymentStatus=Refunded

❌ Reembolsar registro no pagado
- Precondición: PaymentStatus=Failed
- Resultado: ConflictException "Only paid records can be refunded"

#### Slice: Webhook interno

---

## 7. Descripciones de Permisos

### Scopes atómicos

| Scope (nombre de clase) | Descripción (es) |
|--------------------------|-------------------|
| `GetBillingHistory` | Ver el detalle de un registro de facturación |
| `ListBillingHistoryByTenant` | Ver el historial de facturación del establecimiento |
| `ListBillingHistoryBySubscription` | Ver el historial de facturación de una suscripción |

> Los comandos de creación y cambio de estado (Create, MarkAsPaid, Refund) no tienen scope — son disparados por el sistema.

---

## 8. Resumen de Endpoints (Orden de Implementación)

| # | Método | Ruta | Comando/Query | Response |
|---|--------|------|---------------|----------|
| 1 | GET | /billing-history/{id} | GetBillingHistory | 200 → `BillingHistoryResponse` |
| 2 | GET | /billing-history/by-tenant/{tenantId} | ListBillingHistoryByTenant | 200 → `BillingHistoryResponse[]` |
| 3 | GET | /billing-history/by-subscription/{subscriptionId} | ListBillingHistoryBySubscription | 200 → `BillingHistoryResponse[]` |

---

## 9. Persistencia (Firestore)

### Colección

`/billing-history/{billingHistoryId}`

### Configuración DbContext

```csharp
modelBuilder.Entity<BillingHistoryAgg>(entity =>
{
    // ComplexType: Price (Money con Currency anidado)
    entity.ComplexProperty(b => b.Price, price =>
    {
        price.Ignore(m => m.IsZero);
        price.Ignore(m => m.IsPositive);
        price.Ignore(m => m.IsNegative);

        price.ComplexProperty(m => m.Currency);
    });

    // ArrayOf: Features
    entity.ArrayOf(b => b.Features);
});
```

### Documento Ejemplo

```json
{
  "id": "b2c3d4e5-6789-0123-4567-890123456789",
  "tenantId": "customer-001-guid",
  "subscriptionId": "a1b2c3d4-5678-9012-3456-789012345678",
  "planId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "planName": "Plan Básico",
  "billingPeriod": "Monthly",
  "price": {
    "amount": 9.99,
    "currency": {
      "code": "EUR",
      "symbol": "€",
      "decimalPlaces": 2
    }
  },
  "periodStart": "2026-01-01T00:00:00Z",
  "periodEnd": "2026-01-31T23:59:59Z",
  "paymentStatus": "Paid",
  "paidAt": "2026-01-01T00:05:23Z",
  "externalInvoiceId": "in_1OXbFJ2eZvKYlo2C5004ogvj",
  "externalPaymentIntentId": "pi_3OXbFJ2eZvKYlo2C1234abcd",
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

## 10. Hot Spots ⚠️

| # | Pregunta | Estado |
|---|----------|--------|
| 1 | ¿Se guardan los intentos de cobro fallidos como registros separados o solo el resultado final del período? | Pendiente |
| 2 | ¿Retención de datos — se purgan registros antiguos o se mantienen indefinidamente? | Pendiente |

---

**Fecha**: 2026-02-12
**Autor**: Equipo Fudie
