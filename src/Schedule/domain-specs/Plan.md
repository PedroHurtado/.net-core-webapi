# Domain Definition: Plan

## 1. Estado y Estructura

### Resumen
Plan representa un plan de suscripción disponible en la plataforma. Es agnóstico del proveedor de pagos y mantiene configuraciones para múltiples proveedores (Stripe, Paddle, etc.). Un Customer siempre tiene asociado un Plan activo que define sus características, precio y límites de uso. Los Features son flexibles y permiten definir cualquier tipo de límite o capacidad sin modificar el modelo.

### Propiedades (Estado)
| Propiedad | Tipo | Modificador | Validaciones (FluentValidation) | Notas |
|-----------|------|-------------|--------------------------------|-------|
| Name | string | protected set | NotEmpty, MaxLength(100) | Nombre del plan (ej: "Básico", "Premium") |
| Description | string | protected set | NotEmpty, MaxLength(500) | Descripción del plan |
| Price | Money | protected set | NotNull, Valid | Value Object con Amount y Currency |
| BillingPeriod | BillingPeriod | protected set | IsInEnum | Monthly, Quarterly, Semester, Yearly |
| IsActive | bool | protected set | - | Si el plan está disponible para nuevas suscripciones |
| Features | IReadOnlyCollection<Feature> | get only | NotEmpty | Características y límites del plan |
| ProviderConfigurations | IReadOnlyCollection<PaymentProviderConfig> | get only | NotEmpty | Configuraciones de proveedores de pago |

### Value Objects

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
    // Validación: Si Type es Limit, Limit debe tener valor
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
| IsActive | bool | - | Si esta configuración está activa |

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

### Relaciones
- **Features**: `List<Feature>` (backing field: `_features`), expuesto como `IReadOnlyCollection<Feature>`
- **ProviderConfigurations**: `List<PaymentProviderConfig>` (backing field: `_providerConfigurations`), expuesto como `IReadOnlyCollection<PaymentProviderConfig>`

### Invariantes / Reglas de Negocio Globales
- Un plan debe tener al menos una característica (Feature) definida
- Un plan debe tener al menos una configuración de proveedor activa
- No puede haber dos configuraciones para el mismo proveedor ambas activas
- No puede haber dos Features con el mismo Code
- El precio (Amount) debe ser mayor o igual a 0
- Name debe ser único en el sistema
- Features de tipo Limit deben tener Limit > 0

---

## 2. Comportamiento y Reglas (Event Storming & Example Mapping)

### Event Storming (Textual)

#### Flujo Principal: Creación de Plan
1. [Admin] -> (Crear Plan) -> [Plan] -> <PlanCreado>
2. [Admin] -> (Agregar Configuración de Proveedor) -> [Plan] -> <ConfiguraciónProveedorAgregada>
   *Constraint*: No debe existir otra configuración activa para el mismo proveedor
3. [Admin] -> (Activar Plan) -> [Plan] -> <PlanActivado>
4. [Customer] -> (Consultar Planes Disponibles) -> [Sistema] -> <PlanesDisponiblesConsultados>

#### Flujo Secundario: Gestión de Features
1. [Admin] -> (Agregar Feature) -> [Plan] -> <FeatureAgregado>
   *Constraint*: Code del Feature no debe existir ya en el plan
2. [Admin] -> (Actualizar Feature) -> [Plan] -> <FeatureActualizado>
3. [Admin] -> (Eliminar Feature) -> [Plan] -> <FeatureEliminado>
   *Constraint*: Debe quedar al menos un Feature

#### Flujo Secundario: Consulta de Límites (para Métricas)
1. [Sistema Métricas] -> (Consultar Límite por Code) -> [Plan] -> <LímiteConsultado>
   *Uso*: El sistema de métricas consulta GetFeatureByCode("RESERVATIONS_MONTHLY") para saber el límite

#### Flujo de Error
1. [Admin] -> (Crear Plan sin Features) -> [Plan] -> <Error: PlanDebeTeberAlMenosUnFeature>
2. [Admin] -> (Agregar Feature con Code duplicado) -> [Plan] -> <Error: FeatureCodeYaExiste>
3. [Admin] -> (Agregar Feature Limit sin valor) -> [Plan] -> <Error: FeatureLimitRequiereValor>

---

### Example Mapping

#### Story: Crear un nuevo Plan

**Rule 1**: El plan debe tener datos básicos válidos (Name, Description, Price, BillingPeriod)
- *Example (Success)*: Crear plan "Básico" con precio Money(9.99, EUR), BillingPeriod.Monthly
- *Example (Failure)*: Crear plan con Name vacío → Error "El nombre es requerido"
- *Example (Failure)*: Crear plan con precio negativo → Error "El precio debe ser mayor o igual a 0"
- *Example (Failure)*: Crear plan con descripción de 600 caracteres → Error "La descripción no puede exceder 500 caracteres"

**Rule 2**: El plan debe tener al menos un Feature
- *Example (Success)*: Crear plan con Feature RESERVATIONS_MONTHLY (Limit: 100)
- *Example (Failure)*: Crear plan con lista de Features vacía → Error "El plan debe tener al menos una característica"

**Rule 3**: Los Features deben ser válidos
- *Example (Success)*: Feature con Type=Limit y Limit=100 → Válido
- *Example (Failure)*: Feature con Type=Limit y Limit=null → Error "Feature de tipo Limit requiere un valor"
- *Example (Failure)*: Feature con Type=Boolean y Limit=50 → Error "Feature de tipo Boolean no debe tener límite"
- *Example (Failure)*: Feature con Code vacío → Error "El código del Feature es requerido"

**Rule 4**: El plan debe tener al menos una configuración de proveedor
- *Example (Success)*: Crear plan con configuración Stripe (productId: "prod_xxx", priceId: "price_xxx")
- *Example (Failure)*: Crear plan sin configuraciones de proveedor → Error "El plan debe tener al menos una configuración de proveedor"

---

#### Story: Gestión de Features

**Rule 1**: No puede haber Features con el mismo Code
- *Example (Success)*: Agregar Feature ACTIVE_WAITERS a plan que no lo tiene
- *Example (Failure)*: Agregar Feature RESERVATIONS_MONTHLY a plan que ya lo tiene → Error "Ya existe un Feature con código RESERVATIONS_MONTHLY"

**Rule 2**: Siempre debe haber al menos un Feature
- *Example (Success)*: Eliminar Feature PRIORITY_SUPPORT de plan con 3 features (quedan 2)
- *Example (Failure)*: Eliminar último Feature del plan → Error "El plan debe tener al menos una característica"

**Rule 3**: Actualizar Feature existente
- *Example (Success)*: Actualizar límite de RESERVATIONS_MONTHLY de 100 a 200
- *Example (Success)*: Cambiar RESERVATIONS_MONTHLY de Limit(100) a Unlimited
- *Example (Failure)*: Actualizar Feature que no existe → Error "Feature no encontrado"

**Rule 4**: Code debe ser uppercase sin espacios
- *Example (Success)*: Code "RESERVATIONS_MONTHLY" → Válido
- *Example (Failure)*: Code "reservations monthly" → Error "El código debe ser mayúsculas sin espacios"
- *Example (Success)*: Code "active_waiters" → Se convierte automáticamente a "ACTIVE_WAITERS"

---

#### Story: Consultar Feature para Métricas

**Rule 1**: Obtener Feature por Code
- *Example (Success)*: GetFeatureByCode("RESERVATIONS_MONTHLY") → Retorna Feature
- *Example (Success)*: GetFeatureByCode("NONEXISTENT") → Retorna null
- *Example (Success)*: GetLimitForFeature("RESERVATIONS_MONTHLY") cuando es Limit(100) → Retorna 100
- *Example (Success)*: GetLimitForFeature("RESERVATIONS_MONTHLY") cuando es Unlimited → Retorna null (sin límite)

**Rule 2**: Verificar si Feature tiene límite
- *Example (Success)*: HasFeature("PRIORITY_SUPPORT") en plan Premium → true
- *Example (Success)*: HasFeature("PRIORITY_SUPPORT") en plan Básico → false
- *Example (Success)*: IsFeatureUnlimited("RESERVATIONS_MONTHLY") en plan Enterprise → true

---

#### Story: Gestión de Configuraciones de Proveedor

**Rule 1**: No puede haber dos configuraciones activas para el mismo proveedor
- *Example (Success)*: Agregar configuración Stripe activa a plan sin configuración Stripe
- *Example (Success)*: Agregar configuración Paddle activa a plan que ya tiene Stripe activa
- *Example (Failure)*: Agregar segunda configuración Stripe activa → Error "Ya existe una configuración activa para Stripe"

**Rule 2**: Siempre debe haber al menos una configuración activa
- *Example (Success)*: Desactivar configuración Stripe cuando existe configuración Paddle activa
- *Example (Failure)*: Desactivar única configuración activa → Error "El plan debe tener al menos una configuración de proveedor activa"

---

#### Story: Activar/Desactivar Plan

**Rule 1**: Un plan puede activarse/desactivarse para nuevas suscripciones
- *Example (Success)*: Activar plan inactivo → Plan.IsActive = true
- *Example (Success)*: Desactivar plan activo → Plan.IsActive = false
- *Example (Note)*: Desactivar un plan NO afecta a customers existentes con ese plan

---

## 3. Métodos de Comportamiento

### Factory Method
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

### Métodos de Gestión de Features
```csharp
public Result AddFeature(Feature feature)
public Result UpdateFeature(string code, Feature updatedFeature)
public Result RemoveFeature(string code)
public Feature? GetFeatureByCode(string code)
public int? GetLimitForFeature(string code)
public bool HasFeature(string code)
public bool IsFeatureUnlimited(string code)
```

### Métodos de Gestión de Configuraciones de Proveedor
```csharp
public Result AddProviderConfiguration(PaymentProviderConfig config)
public Result UpdateProviderConfiguration(string provider, string newProductId, string newPriceId)
public Result DeactivateProviderConfiguration(string provider)
public Result ActivateProviderConfiguration(string provider)
public PaymentProviderConfig? GetActiveConfigForProvider(string provider)
public bool HasActiveProviderConfiguration(string provider)
```

### Métodos de Actualización
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

## 4. Consideraciones Técnicas

### Persistencia (Firestore)
- **Colección**: `/plans/{planId}`
- **Estructura del documento**:
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

### Índices Requeridos
- `name` (único)
- `isActive` (para filtrar planes disponibles)
- Compuesto: `isActive + price.amount` (para ordenar planes activos por precio)

### Queries Comunes
1. Obtener todos los planes activos: `WHERE isActive = true ORDER BY price.amount ASC`
2. Buscar plan por nombre: `WHERE name = "Plan Básico"`
3. Obtener plan específico: `GET /plans/{planId}`

---

## 5. Integración con Sistema de Métricas

### Flujo de Verificación de Límites
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

### Códigos de Feature Estándar (Sugeridos)
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

## 6. Casos de Uso

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

## 7. Preguntas y Decisiones Pendientes

### ❓ Decisiones de Producto
- [ ] ¿Los customers existentes se ven afectados cuando cambias los Features del plan?
- [ ] ¿Permitimos "grandfathering" (customers antiguos mantienen límites viejos)?
- [ ] ¿Hay período de prueba gratuito? ¿Se configura en Plan o en Customer?
- [ ] ¿Los límites se resetean al inicio de cada período de facturación?

### ❓ Decisiones Técnicas
- [ ] ¿Cómo manejamos cambios de plan (upgrade/downgrade)?
- [ ] ¿Se pueden eliminar planes o solo desactivar?
- [ ] ¿Histórico de cambios de Features del plan?
- [ ] ¿Qué pasa si un Feature se elimina y el Customer lo estaba usando?

---

## 8. Dependencias

### Dependencias de Otros Aggregates
- **Customer**: Referencia Plan por PlanId, usa Features para validar límites
- **Sistema de Métricas**: Consulta Features para conocer límites

### Dependencias Externas
- **Stripe API**: Para sincronizar Products/Prices
- **Sistema de Eventos**: Publicar PlanCreated, PlanUpdated, FeatureChanged

---

## 9. Testing Strategy

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

**Versión**: 2.0  
**Fecha**: 2025-01-04  
**Autor**: Equipo de Arquitectura
