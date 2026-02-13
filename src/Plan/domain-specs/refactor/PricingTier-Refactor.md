# Plan: Extraer PricingTier para múltiples períodos de facturación

## Contexto

El agregado Plan vincula `Price`, `BillingPeriod` y `ProviderConfigurations` como propiedades directas, forzando una relación 1:1 Plan→Precio. Para ofrecer un plan "Pro" con opciones Mensual/Anual se necesitan N documentos duplicando Features. Este refactor extrae un Value Object `PricingTier` que agrupa `BillingPeriod + Money + IsActive + ProviderConfigurations` como colección dentro del Plan, permitiendo 1 Plan = N períodos de facturación.

**Fuera de alcance**: Plan.md (especificación) y plan-api.yaml (OpenAPI) se dejan para el final.

**Guías de estilo aplicadas**: `style-valueobject.md`, `style-command-create.md`, `style-command-transform.md`, `style-command-aggregate.md`, `style-aggregate.md`, `style-slice.md`, `style-response.md`, `style-test-*.md`

---

## Fase 1: Domain — Value Object PricingTier

### 1.1 Crear `PricingTier.cs`
**Archivo**: `src/Plan/Features/Plans/Domain/PlanAggregate/ValueObjects/PricingTier.cs`

Siguiendo `style-valueobject.md`: **positional record** con constructor público implícito. NO constructor protected. NO declarar propiedades `{ get; }` manualmente.

```csharp
public partial record PricingTier(
    BillingPeriod BillingPeriod,
    Money Price,
    bool IsActive,
    IReadOnlyCollection<PaymentProviderConfig> ProviderConfigurations
)
{
    // Propiedad calculada (query, permitida en body)
    public bool HasActiveProvider => ProviderConfigurations.Any(p => p.IsActive);
}
```

- `PricingTierValidationMessages` (static class)
- `PricingTierValidator : AbstractValidator<PricingTier>`

### 1.2 Crear `PricingTier_Create.cs`
**Archivo**: `src/Plan/Features/Plans/Domain/PlanAggregate/Commands/PricingTier/PricingTier_Create.cs`

Siguiendo `style-command-create.md`: composición con `Money.Create`. Crea PricingTier con ProviderConfigurations vacío.

```csharp
public record CreatePricingTierCommand(
    BillingPeriod BillingPeriod,
    decimal Amount,
    string CurrencyCode,
    bool IsActive = false
);

public partial record PricingTier
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        Money.Create moneyCreate,
        IValidator<PricingTier> pricingTierValidator
    ) : AbstractCreateCommand<CreatePricingTierCommand, PricingTier>
    {
        public override PricingTier Execute(CreatePricingTierCommand command)
        {
            var money = moneyCreate.Execute(
                new CreateMoneyCommand(command.Amount, command.CurrencyCode));

            var tier = new PricingTier(
                command.BillingPeriod,
                money,
                command.IsActive,
                Array.Empty<PaymentProviderConfig>().AsReadOnly());

            return pricingTierValidator.ValidateOrThrow(tier);
        }
    }
}
```

### 1.3 Transform commands de PricingTier
**Carpeta**: `src/Plan/Features/Plans/Domain/PlanAggregate/Commands/PricingTier/`

Siguiendo `style-command-transform.md`: usan `with` expression, devuelven nueva instancia. NUNCA mutar el VO.

| Archivo | Clase | Tipo | Expresión `with` |
|---------|-------|------|------------------|
| `PricingTier_Activate.cs` | `PricingTier.Activate` | Sin comando | `current with { IsActive = true }` |
| `PricingTier_Deactivate.cs` | `PricingTier.Deactivate` | Sin comando | `current with { IsActive = false }` |
| `PricingTier_UpdatePrice.cs` | `PricingTier.UpdatePrice` | Con comando `UpdatePricePricingTierCommand(Amount, CurrencyCode)` | Crea nuevo Money via `Money.Create`, `current with { Price = newMoney }` |

---

## Fase 2: Domain — Modificar agregado Plan

### 2.1 Modificar `Plan.cs`
**Archivo**: `src/Plan/Features/Plans/Domain/PlanAggregate/Plan.cs`

Siguiendo `style-aggregate.md`: colecciones con backing field HashSet + IReadOnlyCollection.

- **Eliminar**: `Price`, `BillingPeriod`, `_providerConfigurations`, `ProviderConfigurations`, `HasActiveProvider`
- **Añadir**: `protected HashSet<PricingTier> _pricingTiers = []`
- **Añadir**: `public IReadOnlyCollection<PricingTier> PricingTiers => _pricingTiers.ToList().AsReadOnly()`
- **Añadir**: `public bool HasActivePricingTierWithProvider => _pricingTiers.Any(t => t.IsActive && t.HasActiveProvider)`
- **Actualizar** `PlanValidator`: eliminar reglas de Price/BillingPeriod
- **Actualizar** `PlanValidationMessages`: mensajes para PricingTier

### 2.2 Modificar `Plan_Create.cs`
Siguiendo `style-command-aggregate.md` (Create):
- `CreatePlanCommand` → solo `(Name, Description)`, sin Amount/CurrencyCode/BillingPeriod
- Eliminar dependencia de `Money.Create`

### 2.3 Modificar `Plan_Update.cs`
- `UpdatePlanCommand` → solo `(Name, Description)`
- Eliminar dependencia de `Money.Create`

### 2.4 Modificar `Plan_Activate.cs`
Siguiendo patrón Activate del style (409 ya activo → 422 requisitos):
- Cambiar guard: `PricingTiers.Any(t => t.IsActive && t.HasActiveProvider)`

### 2.5 `Plan_Deactivate.cs` — sin cambios estructurales

---

## Fase 3: Domain — Comandos de PricingTier en Plan

Siguiendo `style-command-aggregate.md` (Add/Update/Remove colecciones). Orden guards: 404 → 409 → 422 → Lógica → ValidateOrThrow.

### 3.1 `Plan_AddPricingTier.cs` (CREAR)
Patrón `Add{Item}`: usa `PricingTier.Create` (composición, NUNCA `new`).
- `AddPricingTierCommand(BillingPeriod, Amount, CurrencyCode, IsActive = false)`
- ConflictGuard: BillingPeriod único en el Plan
- `pricingTierCreate.Execute(...)` → `plan._pricingTiers.Add(...)`

### 3.2 `Plan_UpdatePricingTier.cs` (CREAR)
Patrón `Update{Item}`: busca por key, usa Transform para modificar.
- `UpdatePricingTierCommand(BillingPeriod, Amount, CurrencyCode)`
- NotFoundGuard por BillingPeriod → `PricingTier.UpdatePrice` transform → remove+add
- Preserva IsActive y ProviderConfigurations (el `with` del transform solo cambia Price)

### 3.3 `Plan_RemovePricingTier.cs` (CREAR)
Patrón `Remove{Item}`: NotFoundGuard + ValidationGuard.
- `RemovePricingTierCommand(BillingPeriod)`
- 404 si no existe → 422 si Plan activo y es el último tier activo con provider activo

### 3.4 `Plan_ActivatePricingTier.cs` (CREAR)
- `ActivatePricingTierCommand(BillingPeriod)`
- NotFoundGuard → `PricingTier.Activate` transform → remove+add

### 3.5 `Plan_DeactivatePricingTier.cs` (CREAR)
- `DeactivatePricingTierCommand(BillingPeriod)`
- 404 → 422 si Plan activo y es el último tier activo con provider activo
- `PricingTier.Deactivate` transform → remove+add

---

## Fase 4: Domain — Comandos ProviderConfig dentro de PricingTier

Patrón: Plan aggregate command → NotFoundGuard tier por BillingPeriod → opera ProviderConfig → reconstruye PricingTier con `with { ProviderConfigurations = ... }` → remove+add en plan → ValidateOrThrow.

### 4.1 `Plan_AddPricingTierProviderConfiguration.cs` (CREAR)
- `AddPricingTierProviderConfigurationCommand(BillingPeriod, Provider, ExternalProductId, ExternalPriceId, IsActive)`
- 404 tier → 409 duplicado provider → `providerConfigCreate.Execute(...)` → rebuild tier con `with` → remove+add

### 4.2 `Plan_UpdatePricingTierProviderConfiguration.cs` (CREAR)
- `UpdatePricingTierProviderConfigurationCommand(BillingPeriod, Provider, ExternalProductId, ExternalPriceId)`
- 404 tier → 404 config → create new config preservando IsActive → rebuild tier → remove+add

### 4.3 `Plan_ActivatePricingTierProviderConfiguration.cs` (CREAR)
- `ActivatePricingTierProviderConfigurationCommand(BillingPeriod, Provider)`
- 404 tier → 404 config → rebuild config con IsActive=true → rebuild tier → remove+add

### 4.4 `Plan_DeactivatePricingTierProviderConfiguration.cs` (CREAR)
- `DeactivatePricingTierProviderConfigurationCommand(BillingPeriod, Provider)`
- 404 tier → 404 config → 422 si último provider activo del último tier activo de Plan activo
- Rebuild config con IsActive=false → rebuild tier → remove+add

---

## Fase 5: Domain — Eliminar comandos obsoletos

**ELIMINAR** estos 4 archivos:
- `Commands/Plan/Plan_AddProviderConfiguration.cs`
- `Commands/Plan/Plan_UpdateProviderConfiguration.cs`
- `Commands/Plan/Plan_ActivateProviderConfiguration.cs`
- `Commands/Plan/Plan_DeactivateProviderConfiguration.cs`

---

## Fase 6: API — Responses y slices existentes

### 6.1 Modificar `PlanResponse.cs`
Siguiendo `style-response.md`: un archivo con todos los records, Map estático con named arguments.

- **Eliminar** de PlanResponse: `MoneyResponse Price`, `BillingPeriod`, `HasActiveProvider`, `IReadOnlyCollection<ProviderConfigResponse>`
- **Añadir** a PlanResponse: `IReadOnlyCollection<PricingTierResponse> PricingTiers`
- **Crear** record (entre MoneyResponse y FeatureResponse en el archivo):

```csharp
public record PricingTierResponse(
    BillingPeriod BillingPeriod,
    MoneyResponse Price,
    bool IsActive,
    bool HasActiveProvider,
    IReadOnlyCollection<ProviderConfigResponse> ProviderConfigurations)
{
    public static PricingTierResponse Map(PricingTier tier) => new(
        BillingPeriod: tier.BillingPeriod,
        Price: MoneyResponse.Map(tier.Price),
        IsActive: tier.IsActive,
        HasActiveProvider: tier.HasActiveProvider,
        ProviderConfigurations: tier.ProviderConfigurations
            .Select(ProviderConfigResponse.Map)
            .ToList()
            .AsReadOnly());
}
```

- PlanResponse.Map actualizado: `PricingTiers: entity.PricingTiers.Select(PricingTierResponse.Map).ToList().AsReadOnly()`
- MoneyResponse, CurrencyResponse, FeatureResponse, ProviderConfigResponse sin cambios

### 6.2 Modificar `CreatePlan.cs` — Request sin Amount/CurrencyCode/BillingPeriod
### 6.3 Modificar `UpdatePlan.cs` — Request sin Amount/CurrencyCode/BillingPeriod
### 6.4 Modificar `ActivatePlan.cs` — Include → `"Features", "PricingTiers"`
### 6.5 Actualizar Include en `AddPlanFeature.cs`, `UpdatePlanFeature.cs`, `RemovePlanFeature.cs` → `"Features", "PricingTiers"`
### 6.6 Actualizar queries `GetPlan.cs`, `GetPlans.cs` si tienen Include

Todos siguen `style-slice.md`. Response compartido: `PlanResponse.Map(entity)`.

---

## Fase 7: API — Nuevos slices PricingTier

Siguiendo `style-slice.md` con los patrones correspondientes:

| Archivo | Ruta HTTP | Patrón slice | Handler params |
|---------|-----------|--------------|----------------|
| `AddPlanPricingTier.cs` | `POST /plans/{id}/pricing-tiers` | Add{Item} | `(IService, Guid, Request)` → `Results.Created` |
| `UpdatePlanPricingTier.cs` | `PUT /plans/{id}/pricing-tiers/{billingPeriod}` | Update{Item} | `(IService, Guid, BillingPeriod, Request)` → `Results.Ok` |
| `RemovePlanPricingTier.cs` | `DELETE /plans/{id}/pricing-tiers/{billingPeriod}` | Delete | `(IService, Guid, BillingPeriod)` → `Results.NoContent` |
| `ActivatePlanPricingTier.cs` | `POST /plans/{id}/pricing-tiers/{billingPeriod}/activate` | Action | `(IService, Guid, BillingPeriod)` → `Results.Ok` |
| `DeactivatePlanPricingTier.cs` | `POST /plans/{id}/pricing-tiers/{billingPeriod}/deactivate` | Action | `(IService, Guid, BillingPeriod)` → `Results.Ok` |

Todos usan `PlanResponse.Map(entity)`. IRepository: `[Include<Plan>("Features", "PricingTiers")] IUpdate<Plan, Guid>`.

---

## Fase 8: API — Nuevos slices ProviderConfig dentro de PricingTier

| Archivo | Ruta HTTP | Result |
|---------|-----------|--------|
| `AddPricingTierProviderConfiguration.cs` | `POST /plans/{id}/pricing-tiers/{billingPeriod}/provider-configurations` | Created |
| `UpdatePricingTierProviderConfiguration.cs` | `PUT /plans/{id}/pricing-tiers/{billingPeriod}/provider-configurations/{provider}` | Ok |
| `ActivatePricingTierProviderConfiguration.cs` | `POST .../provider-configurations/{provider}/activate` | Ok |
| `DeactivatePricingTierProviderConfiguration.cs` | `POST .../provider-configurations/{provider}/deactivate` | Ok |

Todos usan `PlanResponse.Map(entity)`. IRepository: `[Include<Plan>("Features", "PricingTiers")] IUpdate<Plan, Guid>`.

---

## Fase 9: API — Eliminar slices obsoletos

**ELIMINAR** estos 4 archivos:
- `Api/PlanAggregate/Commands/AddPlanProviderConfiguration.cs`
- `Api/PlanAggregate/Commands/UpdateProviderConfiguration.cs`
- `Api/PlanAggregate/Commands/ActivateProviderConfiguration.cs`
- `Api/PlanAggregate/Commands/DeactivateProviderConfiguration.cs`

---

## Fase 10: Infrastructure — PlanDbContext.cs

Reconfigurar el modelo:
- **Eliminar**: `ComplexProperty` de Price, `ArrayOf` de ProviderConfigurations
- **Añadir**: `ArrayOf(p => p.PricingTiers, tier => { ... })` con:
  - `tier.Ignore(t => t.HasActiveProvider)`
  - `tier.ComplexProperty(t => t.Price, ...)` con Currency anidado e ignores de IsZero/IsPositive/IsNegative
  - `tier.ArrayOf(t => t.ProviderConfigurations)` (ArrayOf anidado)

> **Riesgo**: Verificar que `Fudie.Firestore.EntityFrameworkCore` soporta `ArrayOf` anidado dentro de `ArrayOf`. Si no, será necesario ajustar el framework.

---

## Fase 11: Tests

### 11.1 Tests del Value Object PricingTier
Siguiendo `style-test-valueobject.md` y `style-test-validator.md`:

- `PricingTierTests.cs` → propiedades con `new PricingTier(...)` (constructor público)
- `PricingTierValidatorTests.cs` → `_validator = new PricingTierValidator()`, `#region` por propiedad

### 11.2 Tests de comandos PricingTier
Siguiendo `style-test-command.md`: usan `DomainFixture`, `IClassFixture<DomainFixture>`, `fixture.Get<>()`.
**NO usar `new` de validators ni comandos. NO usar Testable para VOs.**

- `PricingTierCreateTests.cs` → `fixture.Get<PricingTier.Create>()`
- `PricingTierActivateTests.cs` → `fixture.Get<PricingTier.Activate>()`
- `PricingTierDeactivateTests.cs`
- `PricingTierUpdatePriceTests.cs`

### 11.3 Tests de comandos de Plan (aggregate)
Siguiendo `style-test-aggregate.md`:

- **Actualizar**: CreatePlanTests, UpdatePlanTests, ActivatePlanTests, DeactivatePlanTests
- **Crear**: Tests para Add/Update/Remove/Activate/Deactivate PricingTier
- **Crear**: Tests para Add/Update/Activate/Deactivate PricingTierProviderConfiguration

### 11.4 Tests de slices (unit + integration)
Siguiendo `style-slice-test-unit.md` y `style-slice-test-integration.md`:

- **Actualizar** tests existentes de CreatePlan, UpdatePlan, ActivatePlan
- **Crear** tests para cada nuevo slice
- **Eliminar** tests de ProviderConfiguration directa

### 11.5 Fixture de integración (`PlanWebApplicationFixture.cs`)
- `CreatePlanAsync` sin Amount/CurrencyCode/BillingPeriod
- Eliminar `AddProviderConfigToPlanAsync`
- Añadir `AddPricingTierToPlanAsync`, `AddPricingTierProviderConfigToPlanAsync`
- Actualizar `CreateCompletePlanAsync`

---

## Orden de ejecución (checkpoints compilables)

| Batch | Fases | Resultado |
|-------|-------|-----------|
| **A** | 1-5 | Domain completo, API no compila aún |
| **B** | 6-9 | API completo, proyecto compila |
| **C** | 10 | Infrastructure, Firestore config |
| **D** | 11 | Tests actualizados y nuevos |

---

## Resumen de archivos

### CREAR
| Archivo | Tipo |
|---------|------|
| `ValueObjects/PricingTier.cs` | VO positional record + Validator |
| `Commands/PricingTier/PricingTier_Create.cs` | VO Create command |
| `Commands/PricingTier/PricingTier_Activate.cs` | VO Transform sin comando |
| `Commands/PricingTier/PricingTier_Deactivate.cs` | VO Transform sin comando |
| `Commands/PricingTier/PricingTier_UpdatePrice.cs` | VO Transform con comando |
| `Commands/Plan/Plan_AddPricingTier.cs` | Aggregate Add{Item} |
| `Commands/Plan/Plan_UpdatePricingTier.cs` | Aggregate Update{Item} |
| `Commands/Plan/Plan_RemovePricingTier.cs` | Aggregate Remove{Item} |
| `Commands/Plan/Plan_ActivatePricingTier.cs` | Aggregate Modify |
| `Commands/Plan/Plan_DeactivatePricingTier.cs` | Aggregate Modify |
| `Commands/Plan/Plan_AddPricingTierProviderConfiguration.cs` | Aggregate Modify |
| `Commands/Plan/Plan_UpdatePricingTierProviderConfiguration.cs` | Aggregate Modify |
| `Commands/Plan/Plan_ActivatePricingTierProviderConfiguration.cs` | Aggregate Modify |
| `Commands/Plan/Plan_DeactivatePricingTierProviderConfiguration.cs` | Aggregate Modify |
| `Api/.../AddPlanPricingTier.cs` | Slice POST |
| `Api/.../UpdatePlanPricingTier.cs` | Slice PUT |
| `Api/.../RemovePlanPricingTier.cs` | Slice DELETE |
| `Api/.../ActivatePlanPricingTier.cs` | Slice Action |
| `Api/.../DeactivatePlanPricingTier.cs` | Slice Action |
| `Api/.../AddPricingTierProviderConfiguration.cs` | Slice POST |
| `Api/.../UpdatePricingTierProviderConfiguration.cs` | Slice PUT |
| `Api/.../ActivatePricingTierProviderConfiguration.cs` | Slice Action |
| `Api/.../DeactivatePricingTierProviderConfiguration.cs` | Slice Action |

### MODIFICAR
- `Plan.cs`, `Plan_Create.cs`, `Plan_Update.cs`, `Plan_Activate.cs`
- `PlanResponse.cs`, `CreatePlan.cs`, `UpdatePlan.cs`, `ActivatePlan.cs`
- `PlanDbContext.cs`
- Slices existentes (Include attributes)
- Tests existentes

### ELIMINAR (8 archivos)
- 4 domain commands de ProviderConfiguration directa
- 4 API slices de ProviderConfiguration directa

---

## Verificación

1. `dotnet build` del proyecto Plan tras Batch B
2. `dotnet test` tras Batch D
3. Prueba manual: crear plan → añadir pricing tier Monthly → añadir pricing tier Yearly → añadir provider config a cada tier → activar tiers → activar plan
4. Verificar JSON de Firestore con estructura anidada correcta
