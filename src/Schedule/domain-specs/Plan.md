# Domain Specification: Plan

## 1. Estado y Estructura

### Resumen
Plan representa un plan de suscripción disponible en la plataforma. Es agnóstico del proveedor de pagos y mantiene configuraciones para múltiples proveedores (Stripe, Paddle, etc.). Un Customer siempre tiene asociado un Plan activo que define sus características, precio y límites de uso. Los Features son flexibles y permiten definir cualquier tipo de límite o capacidad sin modificar el modelo.

### Propiedades (Estado)
| Propiedad | Tipo | Modificador | Validaciones (FluentValidation) | Notas |
|-----------|------|-------------|--------------------------------|-------|
| Id | Guid | protected set | Required | |
| Name | string | protected set | NotEmpty, MaxLength(100) | Nombre del plan (ej: "Básico", "Premium"). Único en el sistema |
| Description | string | protected set | NotEmpty, MaxLength(500) | Descripción del plan |
| Price | Money | protected set | NotNull, Valid | Value Object con Amount y Currency |
| BillingPeriod | BillingPeriod | protected set | IsInEnum | Monthly, Quarterly, Semester, Yearly |
| IsActive | bool | protected set | | Si el plan está disponible para nuevas suscripciones |

### Objetos de Valor Anidados

#### Money
Representa un valor monetario con su divisa.

| Propiedad | Tipo | Validaciones | Notas |
|-----------|------|--------------|-------|
| Amount | decimal | GreaterThanOrEqualTo(0), PrecisionScale(18,2) | Cantidad monetaria |
| Currency | Currency | NotNull, Valid | Divisa del importe |

```csharp
public record Money(decimal Amount, Currency Currency)
{
    public static Money Zero(Currency currency) => new(0, currency);

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot add money with different currencies");
        return new Money(Amount + other.Amount, Currency);
    }
}
```

#### Currency
Representa una divisa según ISO 4217.

| Propiedad | Tipo | Validaciones | Notas |
|-----------|------|--------------|-------|
| Code | string | NotEmpty, Length(3), Uppercase | Código ISO 4217 (EUR, USD, GBP) |
| Symbol | string | NotEmpty, MaxLength(5) | Símbolo de la divisa (€, $, £) |
| DecimalPlaces | int | InclusiveBetween(0, 4) | Decimales (2 para EUR, 0 para JPY) |

```csharp
public record Currency(string Code, string Symbol, int DecimalPlaces = 2)
{
    public static Currency EUR => new("EUR", "€", 2);
    public static Currency USD => new("USD", "$", 2);
    public static Currency GBP => new("GBP", "£", 2);

    public static Currency FromCode(string code) => code.ToUpper() switch
    {
        "EUR" => EUR,
        "USD" => USD,
        "GBP" => GBP,
        _ => throw new ArgumentException($"Currency {code} not supported")
    };
}
```

#### Feature
Representa una característica o límite del plan. Diseñado para ser extensible y permitir métricas.

| Propiedad | Tipo | Validaciones | Notas |
|-----------|------|--------------|-------|
| Code | string | NotEmpty, MaxLength(50), Uppercase, NoSpaces | Código único para métricas (RESERVATIONS_MONTHLY) |
| Name | string | NotEmpty, MaxLength(100) | Nombre legible (Reservas mensuales) |
| Description | string | MaxLength(250) | Descripción opcional |
| Type | FeatureType | IsInEnum | Boolean, Limit, Unlimited |
| Limit | int? | GreaterThan(0) when Type=Limit | Valor del límite (100, 4, etc.) |
| Unit | string? | MaxLength(50) when not null | Unidad de medida (reservas, camareros, mesas) |

```csharp
public record Feature(
    string Code,
    string Name,
    string? Description,
    FeatureType Type,
    int? Limit = null,
    string? Unit = null
)
{
    public bool IsValid => Type switch
    {
        FeatureType.Limit => Limit.HasValue && Limit > 0,
        FeatureType.Boolean => !Limit.HasValue,
        FeatureType.Unlimited => !Limit.HasValue,
        _ => false
    };

    public string DisplayValue => Type switch
    {
        FeatureType.Limit => $"{Limit} {Unit}",
        FeatureType.Unlimited => $"Ilimitado",
        FeatureType.Boolean => "Incluido",
        _ => ""
    };
}

public enum FeatureType
{
    Boolean,    // Feature activo o no (ej: "Soporte prioritario")
    Limit,      // Feature con límite numérico (ej: 100 reservas/mes)
    Unlimited   // Feature sin límite (ej: reservas ilimitadas)
}
```

**Ejemplos de Features:**
```csharp
// 100 reservas al mes
new Feature("RESERVATIONS_MONTHLY", "Reservas mensuales", null, FeatureType.Limit, 100, "reservas/mes")

// 4 camareros activos
new Feature("ACTIVE_WAITERS", "Camareros activos", null, FeatureType.Limit, 4, "camareros")

// 2 ubicaciones
new Feature("LOCATIONS", "Ubicaciones", "Número de locales", FeatureType.Limit, 2, "ubicaciones")

// Soporte prioritario (boolean)
new Feature("PRIORITY_SUPPORT", "Soporte prioritario", "Respuesta en 24h", FeatureType.Boolean)

// Reservas ilimitadas
new Feature("RESERVATIONS_MONTHLY", "Reservas mensuales", null, FeatureType.Unlimited)

// Reportes avanzados (boolean)
new Feature("ADVANCED_REPORTS", "Reportes avanzados", "Analytics detallado", FeatureType.Boolean)
```

#### PaymentProviderConfig
Representa la configuración de un proveedor de pagos específico para este plan.

| Propiedad | Tipo | Validaciones | Notas |
|-----------|------|--------------|-------|
| Provider | string | NotEmpty, MaxLength(50) | Nombre del proveedor: "Stripe", "Paddle", etc. |
| ExternalProductId | string | NotEmpty, MaxLength(100) | ID del producto en el proveedor externo |
| ExternalPriceId | string | NotEmpty, MaxLength(100) | ID del precio en el proveedor externo |
| IsActive | bool | | Si esta configuración está activa |

```csharp
public record PaymentProviderConfig(
    string Provider,
    string ExternalProductId,
    string ExternalPriceId,
    bool IsActive = true
);
```

#### BillingPeriod (Enum)
```csharp
public enum BillingPeriod
{
    Monthly,    // Mensual
    Quarterly,  // Trimestral (3 meses)
    Semester,   // Semestral (6 meses)
    Yearly      // Anual
}
```

### Colecciones

```csharp
protected List<Feature> _features = [];
public IReadOnlyCollection<Feature> Features => _features.ToList().AsReadOnly();

protected List<PaymentProviderConfig> _providerConfigurations = [];
public IReadOnlyCollection<PaymentProviderConfig> ProviderConfigurations => _providerConfigurations.ToList().AsReadOnly();
```

### Propiedades Calculadas
- `HasActiveProvider`: `bool` (get only) → `_providerConfigurations.Any(p => p.IsActive)`

### Invariantes / Reglas de Negocio Globales
- ✅ Un plan debe tener al menos una característica (Feature) definida
- ✅ Un plan debe tener al menos una configuración de proveedor activa
- ✅ No puede haber dos configuraciones para el mismo proveedor ambas activas
- ✅ No puede haber dos Features con el mismo Code
- ✅ El precio (Amount) debe ser mayor o igual a 0
- ✅ Name debe ser único en el sistema
- ✅ Features de tipo Limit deben tener Limit > 0

---

## 2. Comportamiento y Reglas (Event Storming)

### Leyenda de Colores
| Color | Elemento | Símbolo | Descripción |
|-------|----------|---------|-------------|
| 🟠 Naranja | Domain Event | `<EventName>` | Algo que ocurrió (pasado) |
| 🔵 Azul | Command | `(CommandName)` | Intención/Acción (imperativo) |
| 🟡 Amarillo | Actor | `[ActorName]` | Usuario o sistema que inicia |
| 🟣 Púrpura | Policy | `{PolicyName}` | Regla de negocio/Política |
| 🟤 Marrón | Aggregate | `[[AggregateName]]` | Entidad raíz del agregado |
| 🔴 Rojo | Hot Spot | `⚠️` | Dudas o conflictos pendientes |
| 🟢 Verde | Read Model | `📊` | Vista/Proyección de datos |
| 🩷 Rosa | External System | `⚡` | Sistema externo |

---

### Flujo 1: Creación de Plan

#### 1.1 Crear Plan
```
🟡[Admin] → 🔵(CreatePlan) → 🟤[[Plan]] → 🟠<PlanCreated>
                                    │
                          🟣{ValidacionDatosBasicos}
                          🟣{ValidacionFeatures}
                          🟣{ValidacionProviderConfig}
```

**Input**: Name, Description, Price (Money), BillingPeriod, Features[], ProviderConfigurations[], IsActive?

**Validaciones** 🟣{ValidacionDatosBasicos}:
- Name no vacío, máximo 100 caracteres
- Description no vacía, máximo 500 caracteres
- Price.Amount >= 0
- BillingPeriod válido (Monthly, Quarterly, Semester, Yearly)

**Validaciones** 🟣{ValidacionFeatures}:
- Al menos un Feature
- Cada Feature con Code único
- Features de tipo Limit deben tener Limit > 0
- Features de tipo Boolean/Unlimited no deben tener Limit

**Validaciones** 🟣{ValidacionProviderConfig}:
- Al menos una configuración de proveedor
- No puede haber dos configuraciones activas para el mismo proveedor

**Resultado**: Plan creado (IsActive = true por defecto)

**Flujo de Error**:
```
🟡[Admin] → 🔵(CreatePlan) → 🟤[[Plan]] → 🔴<Error: PlanDebeTeberAlMenosUnFeature>
                                    │
                          🟣{ValidacionFeatures} ❌
```

---

### Flujo 2: Gestión de Features

#### 2.1 Agregar Feature
```
🟡[Admin] → 🔵(AddFeature) → 🟤[[Plan]] → 🟠<FeatureAdded>
                                    │
                          🟣{FeatureCodeUnico}
                          🟣{FeatureTipoValido}
```

**Input**: Feature (Code, Name, Description?, Type, Limit?, Unit?)

**Validaciones** 🟣{FeatureCodeUnico}:
- Code no debe existir ya en el plan

**Validaciones** 🟣{FeatureTipoValido}:
- Si Type=Limit → Limit debe tener valor > 0
- Si Type=Boolean/Unlimited → Limit debe ser null

**Resultado**: Feature agregado al plan

**Flujo de Error**:
```
🟡[Admin] → 🔵(AddFeature) → 🟤[[Plan]] → 🔴<Error: FeatureCodeYaExiste>
                                    │
                          🟣{FeatureCodeUnico} ❌
```

---

#### 2.2 Actualizar Feature
```
🟡[Admin] → 🔵(UpdateFeature) → 🟤[[Plan]] → 🟠<FeatureUpdated>
                                    │
                          🟣{FeatureExiste}
```

**Input**: Code (existente), UpdatedFeature

**Validaciones** 🟣{FeatureExiste}:
- El Feature con ese Code debe existir

**Resultado**: Feature actualizado

---

#### 2.3 Eliminar Feature
```
🟡[Admin] → 🔵(RemoveFeature) → 🟤[[Plan]] → 🟠<FeatureRemoved>
                                    │
                          🟣{MinimoUnFeature}
```

**Input**: Code

**Validaciones** 🟣{MinimoUnFeature}:
- Debe quedar al menos un Feature después de eliminar

**Resultado**: Feature eliminado

**Flujo de Error**:
```
🟡[Admin] → 🔵(RemoveFeature) → 🟤[[Plan]] → 🔴<Error: PlanDebeTeberAlMenosUnFeature>
                                    │
                          🟣{MinimoUnFeature} ❌
```

---

### Flujo 3: Gestión de Configuraciones de Proveedor

#### 3.1 Agregar Configuración de Proveedor
```
🟡[Admin] → 🔵(AddProviderConfiguration) → 🟤[[Plan]] → 🟠<ProviderConfigAdded>
                                                │
                                      🟣{NoProveedorDuplicadoActivo}
```

**Input**: PaymentProviderConfig (Provider, ExternalProductId, ExternalPriceId, IsActive?)

**Validaciones** 🟣{NoProveedorDuplicadoActivo}:
- No debe existir otra configuración activa para el mismo proveedor

**Resultado**: Configuración agregada

**Flujo de Error**:
```
🟡[Admin] → 🔵(AddProviderConfiguration) → 🟤[[Plan]] → 🔴<Error: ProveedorYaTieneConfigActiva>
                                                │
                                      🟣{NoProveedorDuplicadoActivo} ❌
```

---

#### 3.2 Desactivar Configuración de Proveedor
```
🟡[Admin] → 🔵(DeactivateProviderConfiguration) → 🟤[[Plan]] → 🟠<ProviderConfigDeactivated>
                                                        │
                                              🟣{MinimoUnaConfigActiva}
```

**Input**: Provider (nombre del proveedor)

**Validaciones** 🟣{MinimoUnaConfigActiva}:
- Debe quedar al menos una configuración activa

**Resultado**: Configuración desactivada

**Flujo de Error**:
```
🟡[Admin] → 🔵(DeactivateProviderConfiguration) → 🟤[[Plan]] → 🔴<Error: DebeHaberAlMenosUnaConfigActiva>
                                                        │
                                              🟣{MinimoUnaConfigActiva} ❌
```

---

### Flujo 4: Activación/Desactivación de Plan

#### 4.1 Activar Plan
```
🟡[Admin] → 🔵(ActivatePlan) → 🟤[[Plan]] → 🟠<PlanActivated>
```

**Input**: (ninguno)

**Resultado**: Plan.IsActive = true, disponible para nuevas suscripciones

---

#### 4.2 Desactivar Plan
```
🟡[Admin] → 🔵(DeactivatePlan) → 🟤[[Plan]] → 🟠<PlanDeactivated>
```

**Input**: (ninguno)

**Resultado**: Plan.IsActive = false, no disponible para nuevas suscripciones

**Nota**: Desactivar un plan NO afecta a customers existentes con ese plan

---

### Flujo 5: Consulta de Límites (para Métricas)

#### 5.1 Consultar Feature por Code
```
🟡[SistemaMetricas] → 🔵(GetFeatureByCode) → 🟤[[Plan]] → 📊 FeatureView
```

**Input**: Code (string)

**Resultado**: Feature o null si no existe

---

#### 5.2 Verificar Límite de Feature
```
🟡[SistemaMetricas] → 🔵(GetLimitForFeature) → 🟤[[Plan]] → 📊 LimitValue
                                                    │
                                          🟣{CalculoLimite}
```

**Algoritmo** 🟣{CalculoLimite}:
```
1. Buscar Feature por Code
2. Si no existe → Retornar null
3. Si Type == Unlimited → Retornar null (sin límite)
4. Si Type == Limit → Retornar Limit value
5. Si Type == Boolean → Retornar null
```

**Ejemplos Visuales**:

```
📊 Ejemplo 1 - Feature con Límite
┌────────────────────────────────────────┐
│ Feature: RESERVATIONS_MONTHLY          │
│ Type: Limit                            │
│ Limit: 100                             │
├────────────────────────────────────────┤
│ GetLimitForFeature("RESERVATIONS...")  │
│ Resultado: 100                         │
└────────────────────────────────────────┘

📊 Ejemplo 2 - Feature Ilimitado
┌────────────────────────────────────────┐
│ Feature: RESERVATIONS_MONTHLY          │
│ Type: Unlimited                        │
├────────────────────────────────────────┤
│ GetLimitForFeature("RESERVATIONS...")  │
│ Resultado: null (sin límite)           │
└────────────────────────────────────────┘
```

---

### Hot Spots ⚠️ (Preguntas Pendientes)

| # | Pregunta | Estado |
|---|----------|--------|
| 1 | ⚠️ ¿Los customers existentes se ven afectados cuando cambias los Features del plan? | Pendiente |
| 2 | ⚠️ ¿Permitimos "grandfathering" (customers antiguos mantienen límites viejos)? | Pendiente |
| 3 | ⚠️ ¿Hay período de prueba gratuito? ¿Se configura en Plan o en Customer? | Pendiente |
| 4 | ⚠️ ¿Los límites se resetean al inicio de cada período de facturación? | Pendiente |
| 5 | ⚠️ ¿Cómo manejamos cambios de plan (upgrade/downgrade)? | Pendiente |
| 6 | ⚠️ ¿Se pueden eliminar planes o solo desactivar? | Pendiente |
| 7 | ⚠️ ¿Histórico de cambios de Features del plan? | Pendiente |
| 8 | ⚠️ ¿Qué pasa si un Feature se elimina y el Customer lo estaba usando? | Pendiente |

---

### Resumen de Políticas 🟣

| Política | Trigger | Descripción |
|----------|---------|-------------|
| `{ValidacionDatosBasicos}` | `(CreatePlan)`, `(UpdatePlan)` | Valida Name, Description, Price, BillingPeriod |
| `{ValidacionFeatures}` | `(CreatePlan)`, `(AddFeature)` | Al menos un Feature, Codes únicos, tipos válidos |
| `{ValidacionProviderConfig}` | `(CreatePlan)`, `(AddProviderConfiguration)` | Al menos una config, no duplicados activos |
| `{FeatureCodeUnico}` | `(AddFeature)` | Code no debe existir ya en el plan |
| `{FeatureTipoValido}` | `(AddFeature)`, `(UpdateFeature)` | Limit con valor si Type=Limit |
| `{FeatureExiste}` | `(UpdateFeature)`, `(RemoveFeature)` | El Feature debe existir |
| `{MinimoUnFeature}` | `(RemoveFeature)` | Debe quedar al menos un Feature |
| `{NoProveedorDuplicadoActivo}` | `(AddProviderConfiguration)` | No dos configs activas del mismo proveedor |
| `{MinimoUnaConfigActiva}` | `(DeactivateProviderConfiguration)` | Al menos una config activa |
| `{CalculoLimite}` | `(GetLimitForFeature)` | Lógica para obtener límite según tipo |

---

### Read Models 📊

| Vista | Propósito | Actualizado por |
|-------|-----------|-----------------|
| `FeatureView` | Obtener Feature por Code para métricas | `<FeatureAdded>`, `<FeatureUpdated>`, `<FeatureRemoved>` |
| `LimitValue` | Obtener límite numérico de un Feature | `<FeatureAdded>`, `<FeatureUpdated>` |
| `ActivePlansView` | Listar planes disponibles para suscripción | `<PlanCreated>`, `<PlanActivated>`, `<PlanDeactivated>` |

---

## 3. Example Mapping

### Story 1: Crear un nuevo Plan

**Rule**: El plan debe tener datos básicos válidos (Name, Description, Price, BillingPeriod)

✅ **Example (Success)**:
- Crear plan "Básico" con precio Money(9.99, EUR), BillingPeriod.Monthly
- **Acción**: `Plan.Create(...)`
- **Resultado**: Plan creado correctamente

❌ **Example (Failure - Name vacío)**:
- **Acción**: `Plan.Create(name: "", ...)`
- **Resultado**: Error "El nombre es requerido"

❌ **Example (Failure - Precio negativo)**:
- **Acción**: `Plan.Create(price: Money(-5, EUR), ...)`
- **Resultado**: Error "El precio debe ser mayor o igual a 0"

❌ **Example (Failure - Descripción muy larga)**:
- **Acción**: `Plan.Create(description: "[600 caracteres]", ...)`
- **Resultado**: Error "La descripción no puede exceder 500 caracteres"

---

**Rule**: El plan debe tener al menos un Feature

✅ **Example (Success)**:
- Crear plan con Feature RESERVATIONS_MONTHLY (Limit: 100)
- **Acción**: `Plan.Create(features: [reservationFeature], ...)`
- **Resultado**: Plan creado con Feature

❌ **Example (Failure - Sin Features)**:
- **Acción**: `Plan.Create(features: [], ...)`
- **Resultado**: Error "El plan debe tener al menos una característica"

---

**Rule**: Los Features deben ser válidos

✅ **Example (Success - Feature Limit válido)**:
- **Acción**: `new Feature("CODE", "Name", null, FeatureType.Limit, 100, "units")`
- **Resultado**: Feature válido

❌ **Example (Failure - Feature Limit sin valor)**:
- **Acción**: `new Feature("CODE", "Name", null, FeatureType.Limit, null, null)`
- **Resultado**: Error "Feature de tipo Limit requiere un valor"

❌ **Example (Failure - Feature Boolean con Limit)**:
- **Acción**: `new Feature("CODE", "Name", null, FeatureType.Boolean, 50, null)`
- **Resultado**: Error "Feature de tipo Boolean no debe tener límite"

❌ **Example (Failure - Code vacío)**:
- **Acción**: `new Feature("", "Name", null, FeatureType.Limit, 100, "units")`
- **Resultado**: Error "El código del Feature es requerido"

---

**Rule**: El plan debe tener al menos una configuración de proveedor

✅ **Example (Success)**:
- **Acción**: `Plan.Create(providerConfigurations: [stripeConfig], ...)`
- **Resultado**: Plan creado con configuración Stripe

❌ **Example (Failure - Sin configuraciones)**:
- **Acción**: `Plan.Create(providerConfigurations: [], ...)`
- **Resultado**: Error "El plan debe tener al menos una configuración de proveedor"

---

### Story 2: Gestión de Features

**Rule**: No puede haber Features con el mismo Code

✅ **Example (Success)**:
- Agregar Feature ACTIVE_WAITERS a plan que no lo tiene
- **Acción**: `plan.AddFeature(new Feature("ACTIVE_WAITERS", ...))`
- **Resultado**: Feature agregado

❌ **Example (Failure - Code duplicado)**:
- **Precondición**: Plan ya tiene Feature RESERVATIONS_MONTHLY
- **Acción**: `plan.AddFeature(new Feature("RESERVATIONS_MONTHLY", ...))`
- **Resultado**: Error "Ya existe un Feature con código RESERVATIONS_MONTHLY"

---

**Rule**: Siempre debe haber al menos un Feature

✅ **Example (Success)**:
- **Precondición**: Plan con 3 Features
- **Acción**: `plan.RemoveFeature("PRIORITY_SUPPORT")`
- **Resultado**: Feature eliminado (quedan 2)

❌ **Example (Failure - Eliminar último)**:
- **Precondición**: Plan con 1 Feature
- **Acción**: `plan.RemoveFeature("LAST_FEATURE")`
- **Resultado**: Error "El plan debe tener al menos una característica"

---

**Rule**: Actualizar Feature existente

✅ **Example (Success - Cambiar límite)**:
- **Acción**: `plan.UpdateFeature("RESERVATIONS_MONTHLY", newFeature with Limit=200)`
- **Resultado**: Límite actualizado de 100 a 200

✅ **Example (Success - Cambiar tipo)**:
- **Acción**: `plan.UpdateFeature("RESERVATIONS_MONTHLY", newFeature with Type=Unlimited)`
- **Resultado**: Feature cambiado de Limit(100) a Unlimited

❌ **Example (Failure - Feature no existe)**:
- **Acción**: `plan.UpdateFeature("NONEXISTENT", ...)`
- **Resultado**: Error "Feature no encontrado"

---

**Rule**: Code debe ser uppercase sin espacios

✅ **Example (Success)**:
- **Acción**: `new Feature("RESERVATIONS_MONTHLY", ...)`
- **Resultado**: Code válido

❌ **Example (Failure - Con espacios)**:
- **Acción**: `new Feature("reservations monthly", ...)`
- **Resultado**: Error "El código debe ser mayúsculas sin espacios"

✅ **Example (Success - Conversión automática)**:
- **Acción**: `new Feature("active_waiters", ...)`
- **Resultado**: Se convierte automáticamente a "ACTIVE_WAITERS"

---

### Story 3: Consultar Feature para Métricas

**Rule**: Obtener Feature por Code

✅ **Example (Success - Feature existe)**:
- **Acción**: `plan.GetFeatureByCode("RESERVATIONS_MONTHLY")`
- **Resultado**: Retorna Feature

✅ **Example (Success - Feature no existe)**:
- **Acción**: `plan.GetFeatureByCode("NONEXISTENT")`
- **Resultado**: Retorna null

✅ **Example (Success - Obtener límite de Feature Limit)**:
- **Precondición**: Feature RESERVATIONS_MONTHLY con Type=Limit, Limit=100
- **Acción**: `plan.GetLimitForFeature("RESERVATIONS_MONTHLY")`
- **Resultado**: Retorna 100

✅ **Example (Success - Obtener límite de Feature Unlimited)**:
- **Precondición**: Feature RESERVATIONS_MONTHLY con Type=Unlimited
- **Acción**: `plan.GetLimitForFeature("RESERVATIONS_MONTHLY")`
- **Resultado**: Retorna null (sin límite)

---

**Rule**: Verificar si Feature tiene límite

✅ **Example (Success - HasFeature en plan Premium)**:
- **Precondición**: Plan Premium con PRIORITY_SUPPORT
- **Acción**: `plan.HasFeature("PRIORITY_SUPPORT")`
- **Resultado**: true

✅ **Example (Success - HasFeature en plan Básico)**:
- **Precondición**: Plan Básico sin PRIORITY_SUPPORT
- **Acción**: `plan.HasFeature("PRIORITY_SUPPORT")`
- **Resultado**: false

✅ **Example (Success - IsFeatureUnlimited)**:
- **Precondición**: Plan Enterprise con RESERVATIONS_MONTHLY Unlimited
- **Acción**: `plan.IsFeatureUnlimited("RESERVATIONS_MONTHLY")`
- **Resultado**: true

---

### Story 4: Gestión de Configuraciones de Proveedor

**Rule**: No puede haber dos configuraciones activas para el mismo proveedor

✅ **Example (Success - Agregar Stripe a plan sin Stripe)**:
- **Acción**: `plan.AddProviderConfiguration(stripeConfig)`
- **Resultado**: Configuración agregada

✅ **Example (Success - Agregar Paddle a plan con Stripe)**:
- **Precondición**: Plan con configuración Stripe activa
- **Acción**: `plan.AddProviderConfiguration(paddleConfig)`
- **Resultado**: Configuración Paddle agregada

❌ **Example (Failure - Segunda configuración Stripe activa)**:
- **Precondición**: Plan con configuración Stripe activa
- **Acción**: `plan.AddProviderConfiguration(otraStripeConfig with IsActive=true)`
- **Resultado**: Error "Ya existe una configuración activa para Stripe"

---

**Rule**: Siempre debe haber al menos una configuración activa

✅ **Example (Success - Desactivar con otra activa)**:
- **Precondición**: Plan con Stripe activa y Paddle activa
- **Acción**: `plan.DeactivateProviderConfiguration("Stripe")`
- **Resultado**: Stripe desactivada (Paddle sigue activa)

❌ **Example (Failure - Desactivar única activa)**:
- **Precondición**: Plan solo con Stripe activa
- **Acción**: `plan.DeactivateProviderConfiguration("Stripe")`
- **Resultado**: Error "El plan debe tener al menos una configuración de proveedor activa"

---

### Story 5: Activar/Desactivar Plan

**Rule**: Un plan puede activarse/desactivarse para nuevas suscripciones

✅ **Example (Success - Activar)**:
- **Precondición**: Plan.IsActive = false
- **Acción**: `plan.Activate()`
- **Resultado**: Plan.IsActive = true

✅ **Example (Success - Desactivar)**:
- **Precondición**: Plan.IsActive = true
- **Acción**: `plan.Deactivate()`
- **Resultado**: Plan.IsActive = false

✅ **Example (Note - Desactivar no afecta customers existentes)**:
- **Precondición**: Plan con customers suscritos
- **Acción**: `plan.Deactivate()`
- **Resultado**: Customers existentes mantienen el plan

---

## 4. Notas de Implementación

### Aggregate Boundary

**Plan es el Aggregate Root**:
```
Plan (Root)
 ├─ Money (Value Object)
 │   └─ Currency (Value Object)
 ├─ Features: IReadOnlyCollection<Feature> (Value Objects)
 │   └─ Feature (Value Object)
 └─ ProviderConfigurations: IReadOnlyCollection<PaymentProviderConfig> (Value Objects)
     └─ PaymentProviderConfig (Value Object)
```

**Reglas de Acceso**:
- Toda modificación pasa por `Plan`
- Los Value Objects son inmutables (reemplazar, no modificar)
- Features y ProviderConfigurations se modifican a través de métodos del Plan

---

### Métodos de Comportamiento

#### Factory Method
```csharp
public static Result<Plan> Create(
    Guid id,
    string name,
    string description,
    Money price,
    BillingPeriod billingPeriod,
    IEnumerable<Feature> features,
    IEnumerable<PaymentProviderConfig> providerConfigurations,
    bool isActive = true)
```

#### Métodos de Gestión de Features
```csharp
public Result AddFeature(Feature feature)
public Result UpdateFeature(string code, Feature updatedFeature)
public Result RemoveFeature(string code)
public Feature? GetFeatureByCode(string code)
public int? GetLimitForFeature(string code)
public bool HasFeature(string code)
public bool IsFeatureUnlimited(string code)
```

#### Métodos de Gestión de Configuraciones de Proveedor
```csharp
public Result AddProviderConfiguration(PaymentProviderConfig config)
public Result UpdateProviderConfiguration(string provider, string newProductId, string newPriceId)
public Result DeactivateProviderConfiguration(string provider)
public Result ActivateProviderConfiguration(string provider)
public PaymentProviderConfig? GetActiveConfigForProvider(string provider)
public bool HasActiveProviderConfiguration(string provider)
```

#### Métodos de Actualización
```csharp
public Result Update(
    string name,
    string description,
    Money price,
    BillingPeriod billingPeriod)
public Result Activate()
public Result Deactivate()
```

---

### Persistencia

**Colección** (Firestore): `/plans/{planId}`

**Estructura Recomendada**:
```json
{
  "id": "guid",
  "name": "Plan Básico",
  "description": "Plan ideal para empezar",
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
    },
    {
      "code": "ACTIVE_WAITERS",
      "name": "Camareros activos",
      "description": null,
      "type": "Limit",
      "limit": 4,
      "unit": "camareros"
    },
    {
      "code": "LOCATIONS",
      "name": "Ubicaciones",
      "description": "Número de locales",
      "type": "Limit",
      "limit": 1,
      "unit": "ubicaciones"
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

**Índices Requeridos**:
- `name` (único)
- `isActive` (para filtrar planes disponibles)
- Compuesto: `isActive + price.amount` (para ordenar planes activos por precio)

**Queries Comunes**:
1. Obtener todos los planes activos: `WHERE isActive = true ORDER BY price.amount ASC`
2. Buscar plan por nombre: `WHERE name = "Plan Básico"`
3. Obtener plan específico: `GET /plans/{planId}`

---

### Integración con Sistema de Métricas

**Flujo de Verificación de Límites**:
```
1. Customer hace una reserva
2. Sistema de Métricas consulta:
   - Plan del Customer
   - Feature "RESERVATIONS_MONTHLY" del Plan
   - Uso actual del Customer este mes
3. Si Feature.Type == Unlimited → Permitir
4. Si Feature.Type == Limit:
   - Si uso_actual < Feature.Limit → Permitir
   - Si uso_actual >= Feature.Limit → Bloquear/Notificar
```

**Códigos de Feature Estándar (Sugeridos)**:
| Code | Descripción | Tipo Común |
|------|-------------|------------|
| RESERVATIONS_MONTHLY | Reservas por mes | Limit/Unlimited |
| RESERVATIONS_DAILY | Reservas por día | Limit/Unlimited |
| ACTIVE_WAITERS | Camareros activos | Limit |
| LOCATIONS | Número de ubicaciones | Limit |
| TABLES | Número de mesas | Limit |
| PRIORITY_SUPPORT | Soporte prioritario | Boolean |
| ADVANCED_REPORTS | Reportes avanzados | Boolean |
| API_ACCESS | Acceso a API | Boolean |
| CUSTOM_BRANDING | Marca personalizada | Boolean |
| SMS_NOTIFICATIONS | Notificaciones SMS | Limit/Boolean |

---

## 5. Casos Edge y Consideraciones

### Casos Edge

**Edge 1: Agregar nuevo tipo de límite sin cambiar el modelo**
- **Comportamiento**: El sistema de Features permite agregar cualquier nuevo límite sin modificar la estructura del Plan
- **Resultado**: Solo agregar Feature con nuevo Code (ej: "TABLES")

**Edge 2: Customer con plan desactivado**
- **Comportamiento**: El customer mantiene su plan actual
- **Resultado**: Sigue funcionando normalmente, solo no pueden suscribirse nuevos customers

**Edge 3: Cambio de Feature de Limit a Unlimited**
- **Comportamiento**: Se puede actualizar un Feature existente
- **Resultado**: Customers con ese plan obtienen el beneficio inmediatamente

---

### Consideraciones de Negocio

**¿Qué pasa cuando se modifica un Feature de un plan activo?**
- Pendiente de decisión: ¿afecta a customers existentes o solo a nuevos?

**¿Se permite eliminar planes o solo desactivar?**
- Recomendación: Solo desactivar para mantener integridad referencial

**¿Cómo funciona el upgrade/downgrade de plan?**
- Pendiente de diseño en flujo de Customer

---

### Dependencias

**Dependencias de Otros Aggregates**:
- **Customer**: Referencia Plan por PlanId, usa Features para validar límites
- **Sistema de Métricas**: Consulta Features para conocer límites

**Dependencias Externas**:
- **Stripe API**: Para sincronizar Products/Prices
- **Sistema de Eventos**: Publicar PlanCreated, PlanUpdated, FeatureChanged

---

## 6. Resumen de Invariantes Críticos

### Plan
- ✅ Name no vacío, máximo 100 caracteres, único en sistema
- ✅ Description no vacía, máximo 500 caracteres
- ✅ Price.Amount >= 0
- ✅ Al menos un Feature
- ✅ Al menos una configuración de proveedor activa

### Feature
- ✅ Code no vacío, uppercase, sin espacios, máximo 50 caracteres
- ✅ Name no vacío, máximo 100 caracteres
- ✅ Si Type=Limit → Limit debe ser > 0
- ✅ Si Type=Boolean/Unlimited → Limit debe ser null
- ✅ Code único dentro del Plan

### PaymentProviderConfig
- ✅ Provider no vacío
- ✅ ExternalProductId y ExternalPriceId no vacíos
- ✅ No puede haber dos configuraciones activas para el mismo proveedor

### Money
- ✅ Amount >= 0
- ✅ Currency válida

### Currency
- ✅ Code de 3 caracteres (ISO 4217)
- ✅ Symbol no vacío

---

## 7. Diagrama Conceptual

```
Plan (Aggregate Root)
├─ Id: Guid
├─ Name: string (único)
├─ Description: string
├─ Price: Money (Value Object)
│   ├─ Amount: decimal
│   └─ Currency: Currency (Value Object)
│       ├─ Code: string (ISO 4217)
│       ├─ Symbol: string
│       └─ DecimalPlaces: int
│
├─ BillingPeriod: BillingPeriod (Enum)
├─ IsActive: bool
│
├─ Features: IReadOnlyCollection<Feature>
│   └─ Feature (Value Object)
│       ├─ Code: string (uppercase, único)
│       ├─ Name: string
│       ├─ Description: string?
│       ├─ Type: FeatureType (Enum)
│       ├─ Limit: int?
│       └─ Unit: string?
│
└─ ProviderConfigurations: IReadOnlyCollection<PaymentProviderConfig>
    └─ PaymentProviderConfig (Value Object)
        ├─ Provider: string
        ├─ ExternalProductId: string
        ├─ ExternalPriceId: string
        └─ IsActive: bool
```

---

## 8. Testing Strategy

### Tests Unitarios (Obligatorios)
- ✅ Create con datos válidos → Success
- ✅ Create con nombre vacío → Failure
- ✅ Create sin features → Failure
- ✅ Create sin provider configs → Failure
- ✅ Create con Feature Limit sin valor → Failure
- ✅ Create con Feature Boolean con valor → Failure
- ✅ AddFeature con Code duplicado → Failure
- ✅ RemoveFeature último → Failure
- ✅ GetFeatureByCode existente → Retorna Feature
- ✅ GetFeatureByCode inexistente → Retorna null
- ✅ GetLimitForFeature con Limit → Retorna valor
- ✅ GetLimitForFeature con Unlimited → Retorna null
- ✅ AddProviderConfiguration duplicado activo → Failure
- ✅ DeactivateProviderConfiguration último activo → Failure
- ✅ Money con Amount negativo → Failure
- ✅ Currency con Code inválido → Failure

### Tests de Integración (Recomendados)
- ✅ Crear plan y persistir en Firestore
- ✅ Consultar planes activos
- ✅ Actualizar Features y verificar persistencia
- ✅ Unicidad de nombre

---

## 9. Casos de Uso

### Caso de Uso 1: Crear Plan Básico para MVP
```
Input:
- Name: "Plan Básico"
- Description: "Perfecto para comenzar"
- Price: Money(9.99, EUR)
- BillingPeriod: Monthly
- Features:
  - RESERVATIONS_MONTHLY: Limit(100)
  - LOCATIONS: Limit(1)
  - ACTIVE_WAITERS: Limit(2)
- ProviderConfigurations: [Stripe: prod_basic, price_basic]

Output: Plan creado y activo
```

### Caso de Uso 2: Crear Plan Premium
```
Input:
- Name: "Plan Premium"
- Description: "Para negocios en crecimiento"
- Price: Money(29.99, EUR)
- BillingPeriod: Monthly
- Features:
  - RESERVATIONS_MONTHLY: Unlimited
  - LOCATIONS: Limit(5)
  - ACTIVE_WAITERS: Limit(10)
  - PRIORITY_SUPPORT: Boolean
  - ADVANCED_REPORTS: Boolean
- ProviderConfigurations: [Stripe: prod_premium, price_premium]

Output: Plan creado y activo
```

### Caso de Uso 3: Agregar Nuevo Tipo de Límite
```
Escenario: Mañana decides limitar el número de mesas

Step 1: Definir nuevo Feature
  - Code: "TABLES"
  - Name: "Mesas"
  - Type: Limit
  - Limit: 10
  - Unit: "mesas"

Step 2: Agregar a planes existentes
  - plan.AddFeature(new Feature("TABLES", "Mesas", null, FeatureType.Limit, 10, "mesas"))

Step 3: Sistema de Métricas
  - Consultar plan.GetLimitForFeature("TABLES")
  - Comparar con uso actual

Nota: NO requiere cambios en el modelo de Plan
```

---

**Fin del Domain Specification**

---

**Fecha**: 2025-01-04
**Autor**: Equipo de Arquitectura
**Versión**: 2.0