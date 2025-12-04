# Domain Definition: Plan

## 1. Estado y Estructura

### Resumen
Plan representa un plan de suscripción disponible en la plataforma. Es agnóstico del proveedor de pagos y mantiene configuraciones para múltiples proveedores (Stripe, Paddle, etc.). Un Customer siempre tiene asociado un Plan activo que define sus características, precio y límites de uso.

### Propiedades (Estado)
| Propiedad | Tipo | Modificador | Validaciones (FluentValidation) | Notas |
|-----------|------|-------------|--------------------------------|-------|
| Name | string | protected set | NotEmpty, MaxLength(100) | Nombre del plan (ej: "Básico", "Premium") |
| Description | string | protected set | NotEmpty, MaxLength(500) | Descripción del plan |
| Price | decimal | protected set | GreaterThanOrEqualTo(0), PrecisionScale(18,2) | Precio mensual en USD |
| BillingPeriod | BillingPeriod | protected set | IsInEnum | Monthly, Quarterly, Yearly |
| IsActive | bool | protected set | - | Si el plan está disponible para nuevas suscripciones |
| Features | IReadOnlyCollection<string> | get only | NotEmpty, Each: NotEmpty, MaxLength(200) | Lista de características (ej: "100 reservas/mes") |
| MaxReservationsPerMonth | int? | protected set | GreaterThan(0) when not null | Límite de reservas (null = ilimitado) |
| MaxLocations | int? | protected set | GreaterThan(0) when not null | Límite de ubicaciones (null = ilimitado) |
| ProviderConfigurations | IReadOnlyCollection<PaymentProviderConfig> | get only | NotEmpty | Configuraciones de proveedores de pago |

### Value Objects

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
    Monthly,
    Quarterly,
    Yearly
}
```

### Relaciones
- **Features**: `List<string>` (backing field: `_features`), expuesto como `IReadOnlyCollection<string>`
- **ProviderConfigurations**: `List<PaymentProviderConfig>` (backing field: `_providerConfigurations`), expuesto como `IReadOnlyCollection<PaymentProviderConfig>`

### Invariantes / Reglas de Negocio Globales
- Un plan debe tener al menos una característica definida
- Un plan debe tener al menos una configuración de proveedor activa
- No puede haber dos configuraciones para el mismo proveedor ambas activas
- El precio debe ser mayor o igual a 0
- Si MaxReservationsPerMonth o MaxLocations son nulos, se interpreta como "ilimitado"
- Name debe ser único en el sistema

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
   *Constraint*: Feature no debe estar duplicado
2. [Admin] -> (Eliminar Feature) -> [Plan] -> <FeatureEliminado>
   *Constraint*: Debe quedar al menos un feature

#### Flujo Secundario: Gestión de Configuraciones de Proveedor
1. [Admin] -> (Actualizar Configuración Stripe) -> [Plan] -> <ConfiguraciónStripeActualizada>
2. [Admin] -> (Desactivar Configuración Stripe) -> [Plan] -> <ConfiguraciónStripeDesactivada>
3. [Admin] -> (Agregar Configuración Paddle) -> [Plan] -> <ConfiguraciónPaddleAgregada>
   *Uso*: Preparación para migración de proveedor

#### Flujo de Error
1. [Admin] -> (Crear Plan sin Features) -> [Plan] -> <Error: PlanDebeTeberAlMenosUnFeature>
2. [Admin] -> (Agregar Configuración Duplicada) -> [Plan] -> <Error: ConfiguraciónProveedorYaExiste>
3. [Admin] -> (Desactivar Última Configuración) -> [Plan] -> <Error: PlanDebeTeberAlMenosUnaConfiguraciónActiva>

---

### Example Mapping

#### Story: Crear un nuevo Plan

**Rule 1**: El plan debe tener datos básicos válidos (Name, Description, Price, BillingPeriod)
- *Example (Success)*: Crear plan "Básico" con descripción, precio 9.99, BillingPeriod.Monthly
- *Example (Failure)*: Crear plan con Name vacío → Error "El nombre es requerido"
- *Example (Failure)*: Crear plan con precio negativo → Error "El precio debe ser mayor o igual a 0"
- *Example (Failure)*: Crear plan con descripción de 600 caracteres → Error "La descripción no puede exceder 500 caracteres"

**Rule 2**: El plan debe tener al menos un feature
- *Example (Success)*: Crear plan con features ["100 reservas/mes", "1 ubicación", "Soporte básico"]
- *Example (Failure)*: Crear plan con lista de features vacía → Error "El plan debe tener al menos una característica"
- *Example (Failure)*: Crear plan con feature vacío "" → Error "La característica no puede estar vacía"

**Rule 3**: El plan debe tener al menos una configuración de proveedor
- *Example (Success)*: Crear plan con configuración Stripe (productId: "prod_xxx", priceId: "price_xxx")
- *Example (Failure)*: Crear plan sin configuraciones de proveedor → Error "El plan debe tener al menos una configuración de proveedor"

---

#### Story: Gestión de Configuraciones de Proveedor

**Rule 1**: No puede haber dos configuraciones activas para el mismo proveedor
- *Example (Success)*: Agregar configuración Stripe activa a plan sin configuración Stripe
- *Example (Success)*: Agregar configuración Paddle activa a plan que ya tiene Stripe activa
- *Example (Failure)*: Agregar segunda configuración Stripe activa cuando ya existe una activa → Error "Ya existe una configuración activa para Stripe"
- *Example (Success)*: Agregar segunda configuración Stripe inactiva (IsActive=false) → Permitido (preparación para migración)

**Rule 2**: Siempre debe haber al menos una configuración activa
- *Example (Success)*: Desactivar configuración Stripe cuando existe configuración Paddle activa
- *Example (Failure)*: Desactivar única configuración activa → Error "El plan debe tener al menos una configuración de proveedor activa"

**Rule 3**: Actualizar configuración existente
- *Example (Success)*: Actualizar ExternalPriceId de configuración Stripe de "price_old" a "price_new"
- *Example (Success)*: Activar configuración Paddle inactiva (después de preparar migración)

---

#### Story: Gestión de Features

**Rule 1**: No se pueden agregar features duplicados
- *Example (Success)*: Agregar feature "Soporte prioritario" a plan que no lo tiene
- *Example (Failure)*: Agregar feature "100 reservas/mes" a plan que ya lo tiene → Error "La característica ya existe en el plan"
- *Example (Success)*: Agregar feature "100 Reservas/Mes" (diferente capitalización) → Permitido, case-sensitive

**Rule 2**: Siempre debe haber al menos un feature
- *Example (Success)*: Eliminar feature "Soporte prioritario" de plan con 3 features (quedan 2)
- *Example (Failure)*: Eliminar último feature del plan → Error "El plan debe tener al menos una característica"

**Rule 3**: Features no pueden estar vacíos
- *Example (Failure)*: Agregar feature con string vacío "" → Error "La característica no puede estar vacía"
- *Example (Failure)*: Agregar feature con solo espacios "   " → Error "La característica no puede estar vacía"

---

#### Story: Consultar Configuración para Proveedor Específico

**Rule 1**: Obtener configuración activa para un proveedor
- *Example (Success)*: Obtener configuración para "Stripe" cuando existe configuración activa → Retorna PaymentProviderConfig de Stripe
- *Example (Success)*: Obtener configuración para "Paddle" cuando no existe → Retorna null
- *Example (Success)*: Obtener configuración para "Stripe" cuando existe pero está inactiva → Retorna null

**Rule 2**: Case-sensitivity del nombre del proveedor
- *Example (Success)*: Obtener configuración para "Stripe" (exacto) → Retorna config
- *Example (Failure)*: Obtener configuración para "stripe" (minúsculas) → Retorna null (case-sensitive)

---

#### Story: Activar/Desactivar Plan

**Rule 1**: Un plan puede activarse/desactivarse para nuevas suscripciones
- *Example (Success)*: Activar plan inactivo → Plan.IsActive = true
- *Example (Success)*: Desactivar plan activo → Plan.IsActive = false
- *Example (Note)*: Desactivar un plan NO afecta a customers existentes con ese plan

**Rule 2**: Plan desactivado no aparece en listados públicos
- *Example (Success)*: Consultar planes activos cuando hay 3 activos y 2 inactivos → Retorna solo 3
- *Example (Success)*: Admin puede ver todos los planes (activos e inactivos)

---

#### Story: Actualizar Límites del Plan

**Rule 1**: Los límites pueden ser nulos (ilimitado) o números positivos
- *Example (Success)*: Actualizar MaxReservationsPerMonth de 100 a 500
- *Example (Success)*: Actualizar MaxReservationsPerMonth de 100 a null (ilimitado)
- *Example (Failure)*: Actualizar MaxReservationsPerMonth a 0 → Error "El límite debe ser mayor a 0"
- *Example (Failure)*: Actualizar MaxReservationsPerMonth a -50 → Error "El límite debe ser mayor a 0"

**Rule 2**: MaxLocations funciona igual que MaxReservationsPerMonth
- *Example (Success)*: Actualizar MaxLocations de 1 a 5
- *Example (Success)*: Actualizar MaxLocations de 5 a null (ilimitado)

---

## 3. Métodos de Comportamiento

### Factory Method
```csharp
public static Result<Plan> Create(
    Guid id,
    string name,
    string description,
    decimal price,
    BillingPeriod billingPeriod,
    IEnumerable<string> features,
    IEnumerable<PaymentProviderConfig> providerConfigurations,
    int? maxReservationsPerMonth = null,
    int? maxLocations = null,
    bool isActive = true)
```

### Métodos de Gestión de Features
```csharp
public Result AddFeature(string feature)
public Result RemoveFeature(string feature)
public bool HasFeature(string feature)
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
    decimal price,
    BillingPeriod billingPeriod,
    int? maxReservationsPerMonth,
    int? maxLocations)
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
  "price": 9.99,
  "billingPeriod": "Monthly",
  "isActive": true,
  "features": ["100 reservas/mes", "1 ubicación"],
  "maxReservationsPerMonth": 100,
  "maxLocations": 1,
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
- Compuesto: `isActive + price` (para ordenar planes activos por precio)

### Queries Comunes
1. Obtener todos los planes activos: `WHERE isActive = true ORDER BY price ASC`
2. Buscar plan por nombre: `WHERE name = "Plan Básico"`
3. Obtener plan específico: `GET /plans/{planId}`

---

## 5. Casos de Uso

### Caso de Uso 1: Crear Plan Básico para MVP
```
Input:
- Name: "Plan Básico"
- Description: "Perfecto para comenzar"
- Price: 9.99
- BillingPeriod: Monthly
- Features: ["100 reservas/mes", "1 ubicación", "Soporte por email"]
- ProviderConfigurations: [Stripe: prod_basic, price_basic]

Output: Plan creado y activo
```

### Caso de Uso 2: Preparar Migración de Stripe a Paddle
```
Step 1: Agregar configuración Paddle inactiva
  - AddProviderConfiguration(Paddle, prod_paddle, price_paddle, isActive: false)

Step 2: Probar con algunos customers en Paddle

Step 3: Activar configuración Paddle
  - ActivateProviderConfiguration("Paddle")

Step 4: Desactivar configuración Stripe (opcional, para nuevos customers)
  - DeactivateProviderConfiguration("Stripe")

Nota: Customers existentes con Stripe siguen usando Stripe
```

### Caso de Uso 3: Actualizar Precio del Plan
```
Input:
- Plan existente con Price: 9.99
- Nuevo precio: 12.99

Proceso:
1. Update(name, description, 12.99, ...)
2. En Stripe: Crear nuevo Price (price_new)
3. UpdateProviderConfiguration("Stripe", prod_xxx, price_new)
4. Nuevos customers usan nuevo precio
5. Customers existentes mantienen precio antiguo (según política)
```

---

## 6. Preguntas y Decisiones Pendientes

### ❓ Decisiones de Producto
- [ ] ¿Los customers existentes se ven afectados cuando cambias el precio del plan?
- [ ] ¿Permitimos "grandfathering" (customers antiguos mantienen precio viejo)?
- [ ] ¿Hay período de prueba gratuito? ¿Se configura en Plan o en Customer?
- [ ] ¿Los límites (MaxReservations) se resetean mensualmente?

### ❓ Decisiones Técnicas
- [ ] ¿Cómo manejamos cambios de plan (upgrade/downgrade)?
- [ ] ¿Se pueden eliminar planes o solo desactivar?
- [ ] ¿Historico de cambios de precio del plan?
- [ ] ¿Auditoría de quién modificó el plan y cuándo?

---

## 7. Dependencias

### Dependencias de Otros Aggregates
- **Customer**: Referencia Plan por PlanId
- **Reservation** (futuro): Puede consultar límites del plan del customer

### Dependencias Externas
- **Stripe API**: Para sincronizar Products/Prices
- **Sistema de Eventos**: Publicar PlanCreated, PlanUpdated para sincronización

---

## 8. Testing Strategy

### Tests Unitarios (Obligatorios)
- ✅ Create con datos válidos → Success
- ✅ Create con nombre vacío → Failure
- ✅ Create sin features → Failure
- ✅ Create sin provider configs → Failure
- ✅ AddFeature duplicado → Failure
- ✅ RemoveFeature último → Failure
- ✅ AddProviderConfiguration duplicado activo → Failure
- ✅ DeactivateProviderConfiguration último activo → Failure
- ✅ GetActiveConfigForProvider existente → Retorna config
- ✅ GetActiveConfigForProvider inexistente → Retorna null
- ✅ Update con precio negativo → Failure

### Tests de Integración (Recomendados)
- ✅ Crear plan y persistir en Firestore
- ✅ Consultar planes activos
- ✅ Actualizar plan y verificar persistencia
- ✅ Unicidad de nombre

---

**Versión**: 1.0  
**Fecha**: 2025-01-04  
**Autor**: Equipo de Arquitectura
