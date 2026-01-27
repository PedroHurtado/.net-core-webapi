# Reporte de Análisis: Plan Domain

Este documento detalla los errores, violaciones de estilo y archivos faltantes encontrados en `src/Plan` tras comparar con `src/Plan/domain-specs/Plan.md` y `.plans/templates/styles`.

## 1. Errores de Estructura y Archivos Faltantes

### ❌ Tests Inexistentes
No se ha encontrado ninguna estructura de pruebas. Según el estándar, deberían existir tests unitarios para el dominio y la capa de aplicación.
*   **Falta**: Carpeta `src/Plan/test` o `src/Plan/Features/Plans/Tests`.
*   **Falta**: Tests unitarios para `Plan`, `Feature`, `Money`, `Currency`, `PaymentProviderConfig`.
*   **Falta**: Tests para todos los comandos (`Create`, `AddFeature`, etc.).

### ❌ Slices (API) Inexistentes
No existe la capa de API (Slices) definida en `style-slice.md`.
*   **Falta**: Carpeta `src/Plan/Features/Plans/Api`.
*   **Faltan Endpoints**:
    *   `CreatePlan.cs` (POST /plans)
    *   `GetPlan.cs` (GET /plans/{id})
    *   `ListPlans.cs` (GET /plans)
    *   `UpdatePlan.cs` (PUT /plans/{id})
    *   `ActivatePlan.cs` (POST /plans/{id}/activate)
    *   `DeactivatePlan.cs` (POST /plans/{id}/deactivate)
    *   `AddFeature.cs` (POST /plans/{id}/features)
    *   `UpdateFeature.cs` (PUT /plans/{id}/features/{code})
    *   `RemoveFeature.cs` (DELETE /plans/{id}/features/{code})
    *   `AddProviderConfig.cs` (POST /plans/{id}/provider-configurations)
    *   `UpdateProviderConfig.cs` (PUT /plans/{id}/provider-configurations/{provider})
    *   `ActivateProviderConfig.cs` (POST /plans/{id}/provider-configurations/{provider}/activate)
    *   `DeactivateProviderConfig.cs` (POST /plans/{id}/provider-configurations/{provider}/deactivate)

## 2. Violaciones de Especificación (Plan.md)

### ❌ Plan_Create.cs
*   **Error**: El comando `CreatePlanCommand` incluye propiedades `Features` y `ProviderConfigurations`.
    *   **Especificación (6.1)**: `Plan.Create` solo debe recibir `Name`, `Description`, `Amount`, `CurrencyCode` y `BillingPeriod`.
    *   **Corrección**: Eliminar `Features` y `ProviderConfigurations` del comando. El plan debe nacer sin features y con `IsActive = false`.

### ❌ Plan.cs
*   **Error**: `Price` se inicializa con `Money.Zero(Currency.EUR)`.
    *   **Especificación**: `Price` es requerido y debe asignarse en el constructor o via comando.
*   **Error**: `Name` y `Description` se inicializan con `string.Empty`.
    *   **Estilo**: Deberían ser `required` o asignarse en el constructor si no son nullable.

## 3. Violaciones de Estilo (Templates)

### ❌ Feature.cs (Value Object)
*   **Error**: Contiene un método estático `public static Feature New(...)`.
    *   **Estilo (`style-valueobject.md`)**: Prohíbe explícitamente métodos estáticos de creación (`New`, `From`, `Create`) en el VO. La creación debe hacerse vía constructor protegido invocado desde el comando `Feature.Create`.

### ❌ Plan_AddFeature.cs (Command)
*   **Error**: Usa `Feature.New(...)` para instanciar el objeto.
    *   **Estilo (`style-command-aggregate.md`)**: Debe inyectar `Feature.Create` y usar `featureCreate.Execute(...)`.
*   **Error**: Validación manual de duplicados (`plan.Features.Any(...)`).
    *   **Estilo**: Debe usar `ConflictGuard.ThrowIf(...)` de forma más limpia o delegar si es posible, aunque la lógica actual no es incorrecta per se, el estilo de instanciación es el crítico.

### ❌ Plan_Create.cs (Command)
*   **Error**: Lógica compleja de iteración para añadir Features y ProviderConfigs en el comando de creación.
    *   **Estilo**: Al violar la especificación de input, también viola el principio de responsabilidad única del comando `Create`.

## 4. Resumen de Archivos a Crear/Corregir

### Modificar
*   `src/Plan/Features/Plans/Domain/PlanAggregate/Plan.cs`
*   `src/Plan/Features/Plans/Domain/PlanAggregate/ValueObjects/Feature.cs`
*   `src/Plan/Features/Plans/Domain/PlanAggregate/Commands/Plan/Plan_Create.cs`
*   `src/Plan/Features/Plans/Domain/PlanAggregate/Commands/Plan/Plan_AddFeature.cs`

### Crear
*   Todo el directorio `src/Plan/Features/Plans/Api/` con sus 13 endpoints.
*   Todo el directorio de tests `src/Plan/test/` con tests unitarios y de integración.
