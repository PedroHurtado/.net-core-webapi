# Análisis de Lecturas para Generación de Tests Unitarios

## Contexto del Experimento

**Objetivo:** Crear tests unitarios para las slices del MenuAggregate (12 Commands + 2 Queries)

**Documentos proporcionados:**
- `style-slice-test-unit.md` - Estilo de tests unitarios
- `AsyncQueryableExtensions.cs` - Helper para queries async

---

## Resumen de Lecturas

| Categoría | Archivos | Porcentaje |
|-----------|----------|------------|
| Slices a testear | 14 | 50% |
| Comandos de dominio | 12 | 43% |
| Entidades/ValueObjects | 4 | 14% |
| Tests de referencia | 3 | 11% |
| Configuración | 1 | 4% |
| **Total** | **28** | - |

---

## Detalle de Archivos Leídos

### 1. Slices a Testear (14 archivos) - INEVITABLES

```
src/Customer/Features/Menus/Api/MenuAggregate/Commands/
├── CreateMenu.cs
├── UpdateMenu.cs
├── ActivateMenu.cs
├── DeactivateMenu.cs
├── AddMenuCategory.cs
├── UpdateMenuCategory.cs
├── RemoveMenuCategory.cs
├── SetMenuDepositPolicy.cs
├── RemoveMenuDepositPolicy.cs
├── AddItemToCategory.cs
├── UpdateCategoryItem.cs
└── RemoveItemFromCategory.cs

src/Customer/Features/Menus/Api/MenuAggregate/Queries/
├── GetMenu.cs
└── GetMenus.cs
```

**Razón:** Contienen el código a testear (Request, Service, Handler, IRepository)

---

### 2. Comandos de Dominio (12 archivos) - REDUCIBLES

```
src/Customer/Features/Menus/Domain/MenuAggregate/Commands/Menu/
├── Menu_Create.cs
├── Menu_Update.cs
├── Menu_Activate.cs
├── Menu_Deactivate.cs
├── Menu_AddCategory.cs
├── Menu_UpdateCategory.cs
├── Menu_RemoveCategory.cs
├── Menu_SetDepositPolicy.cs
├── Menu_RemoveDepositPolicy.cs
├── Menu_AddItemToCategory.cs
├── Menu_UpdateCategoryItem.cs
└── Menu_RemoveItemFromCategory.cs
```

**Razón:** Necesarios para entender las dependencias de cada comando (validators, sub-comandos)

**Problema:** Cada comando tiene dependencias anidadas que no son evidentes desde la slice

---

### 3. Entidades y ValueObjects (4 archivos) - REDUCIBLES

```
src/Customer/Features/Menus/Domain/MenuAggregate/
├── Menu.cs                           # Entidad + MenuValidator
├── Entities/MenuCategory.cs          # Entidad + MenuCategoryValidator
├── ValueObjects/DepositPolicy.cs     # VO + DepositPolicyValidator
└── ...

src/Customer/Features/Menus/Domain/Shared/ValueObjects/
└── CategoryItem.cs                   # VO + CategoryItemValidator
```

**Razón:** Para instanciar validators y entender reglas de validación

---

### 4. Comandos de Entidades Secundarias (6 archivos) - REDUCIBLES

```
src/Customer/Features/Menus/Domain/MenuAggregate/Commands/
├── MenuCategory/MenuCategory_Create.cs
├── MenuCategory/MenuCategory_AddItem.cs
├── MenuCategory/MenuCategory_RemoveItem.cs
├── MenuCategory/MenuCategory_UpdateItem.cs
└── DepositPolicy/DepositPolicy_Create.cs
```

**Razón:** Dependencias transitivas de los comandos de Menu

---

### 5. Tests de Referencia (3 archivos) - EVITABLES

```
tests/Customer.UnitTests/Features/Menus/Api/MenuItemAggregate/
├── Commands/CreateMenuItemTests.cs   # Patrón de tests de commands
└── Queries/GetMenuItemTests.cs       # Patrón IRepository
└── Queries/GetMenuItemsTests.cs      # Patrón IQuery + AsAsyncQueryable
```

**Razón:** El documento de estilo no tiene ejemplos completos reales

---

### 6. Configuración (1 archivo) - NECESARIO

```
tests/Customer.UnitTests/GlobalUsings.cs
```

**Razón:** Para agregar los usings faltantes

---

## Propuestas de Mejora

### Propuesta 1: Mapa de Dependencias por Tipo de Slice

Añadir al documento de estilo una sección con el grafo de dependencias típico:

```markdown
## Dependencias por Tipo de Comando

### Create (agrega nueva entidad)
- `{Entity}.Create` comando de dominio
- `{Entity}Validator` validator de la entidad
- `IRepository : IAdd<{Entity}>`
- `IUnitOfWork`

### Update (modifica entidad existente)
- `{Entity}.Update` comando de dominio
- `{Entity}Validator` validator
- `IRepository : IUpdate<{Entity}, {Id}>`
- `IUnitOfWork`

### Activate/Deactivate (cambia estado)
- `{Entity}.Activate` o `{Entity}.Deactivate`
- `{Entity}Validator`
- Puede tener validaciones de estado (ConflictException)
- `IRepository : IUpdate<{Entity}, {Id}>`

### AddChild (agrega entidad hija)
- `{Entity}.Add{Child}` comando padre
- `{Child}.Create` comando hijo (transitivo)
- `{Child}Validator` validator hijo
- `{Entity}Validator` validator padre
- Puede tener validación de duplicados (ConflictException)

### RemoveChild (elimina entidad hija)
- `{Entity}.Remove{Child}` comando padre
- `{Child}Validator` (solo para validación post-remove)
- Puede tener validación de dependencias (ValidationException)
```

**Impacto:** Evitaría leer los 12 comandos de dominio para descubrir dependencias

---

### Propuesta 2: Ejemplos Completos de Setup en el Documento

Añadir sección con setup real para cada patrón:

```markdown
## Setup por Patrón

### Patrón Create
```csharp
private readonly {Entity}Validator _{entity}Validator = new();
private readonly {Entity}.Create _create{Entity};
private readonly Mock<Create{Entity}.IRepository> _repositoryMock;
private readonly Mock<IUnitOfWork> _unitOfWorkMock;
private readonly Create{Entity}.Service _service;

public Create{Entity}Tests()
{
    _create{Entity} = new(_{entity}Validator);
    _repositoryMock = new Mock<Create{Entity}.IRepository>();
    _unitOfWorkMock = new Mock<IUnitOfWork>();
    _service = new Create{Entity}.Service(
        /* tenantId si aplica */,
        _create{Entity},
        _repositoryMock.Object,
        _unitOfWorkMock.Object
    );
}
```

### Patrón Query con IRepository
```csharp
private readonly Mock<Get{Entity}.IRepository> _repositoryMock;
private readonly Get{Entity}.Service _service;

public Get{Entity}Tests()
{
    _repositoryMock = new Mock<Get{Entity}.IRepository>();
    _service = new Get{Entity}.Service(_repositoryMock.Object);
}
```

### Patrón Query con IQuery (listas)
```csharp
private readonly Mock<IQuery> _queryMock;
private readonly Get{Entities}.Service _service;

public Get{Entities}Tests()
{
    _queryMock = new Mock<IQuery>();
    _service = new Get{Entities}.Service(_queryMock.Object);
}

// En los tests usar:
var items = new List<{Entity}> { ... }.AsAsyncQueryable();
_queryMock.Setup(q => q.Query<{Entity}>()).Returns(items);
```
```

**Impacto:** Evitaría leer los 3 tests de referencia

---

### Propuesta 3: Convención de Nombres de Dependencias

Documentar la convención que el código sigue:

```markdown
## Convención de Dependencias

Las dependencias de un comando de dominio se infieren del nombre:

| Comando | Dependencias |
|---------|-------------|
| `Menu.Create` | `IValidator<Menu>` (MenuValidator) |
| `Menu.AddCategory` | `MenuCategory.Create`, `IValidator<Menu>` |
| `Menu.SetDepositPolicy` | `DepositPolicy.Create`, `IValidator<Menu>` |
| `MenuCategory.AddItem` | `CategoryItem.Create`, `IValidator<MenuCategory>` |

**Regla general:**
- Si el comando "agrega" algo → necesita el `.Create` de ese algo
- Siempre necesita el validator de la entidad que modifica
```

**Impacto:** Evitaría leer los 6 comandos de entidades secundarias

---

### Propuesta 4: Estructura de Archivos de Estilo por Feature

En lugar de un solo documento de estilo, tener uno por aggregate:

```
.plans/templates/styles/slice/
├── style-slice-test-unit.md          # Documento base
└── features/
    └── MenuAggregate/
        ├── dependencies.md            # Mapa de dependencias específico
        └── examples/
            ├── CreateMenuTests.cs     # Ejemplo real completo
            └── GetMenusTests.cs       # Ejemplo de query con IQuery
```

**Impacto:** Documentación específica reduce ambigüedad

---

### Propuesta 5: Generador de Esqueleto de Tests

Script/tool que genere el esqueleto de tests basándose en la slice:

```bash
# Comando hipotético
dotnet generate-test --slice CreateMenu --output tests/.../Commands/
```

Generaría:
- Imports correctos
- Setup con dependencias inferidas
- Métodos de test vacíos con nombres según convención

**Impacto:** Automatizaría la parte mecánica, reduciría lecturas a 0

---

## Resumen de Impacto

| Propuesta | Lecturas Evitadas | Complejidad |
|-----------|-------------------|-------------|
| Mapa de dependencias | ~12 | Baja |
| Ejemplos de setup | ~3 | Baja |
| Convención de nombres | ~6 | Media |
| Docs por feature | ~5 | Media |
| Generador de esqueleto | ~28 (todas) | Alta |

**Recomendación:** Implementar propuestas 1, 2 y 3 primero (bajo esfuerzo, alto impacto)

---

## Incidencias de Compilación - Lecciones Adicionales

Durante la ejecución del build y tests surgieron errores que revelan información faltante en el proceso:

### Incidencia 1: Firma completa de IEntityLookup.GetRequiredAsync

**Error:** `CS1503: Argument 2: cannot convert from 'CancellationToken' to 'bool'`

**Causa:** Desconocimiento de la firma completa del método:

```csharp
Task<T> GetRequiredAsync<T, TId>(
    TId id,
    bool tracking = true,
    CancellationToken cancellationToken = default,
    params string[] includeProperties)
```

**Solución requerida:** En Moq, los expression trees NO permiten parámetros opcionales. Hay que especificar TODOS:

```csharp
// ❌ INCORRECTO - Expression tree con parámetros opcionales
_entityLookupMock.Setup(e => e.GetRequiredAsync<MenuItemAgg, Guid>(menuItem.Id))

// ✅ CORRECTO - Todos los parámetros explícitos
_entityLookupMock.Setup(e => e.GetRequiredAsync<MenuItemAgg, Guid>(
    It.IsAny<Guid>(),
    It.IsAny<bool>(),
    It.IsAny<CancellationToken>(),
    It.IsAny<string[]>()
)).ReturnsAsync(menuItem);
```

---

### Incidencia 2: Firmas de comandos con muchos parámetros

**Error:** `CS7036: No argument given that corresponds to the required formal parameter`

**Causa:** `CreateMenuItemCommand` tiene 12 parámetros requeridos que no son evidentes:

```csharp
public record CreateMenuItemCommand(
    Guid TenantId,
    string Name,
    string? Description,
    string? ImageUrl,
    int DisplayOrder,
    bool IsHighRiskItem,
    bool RequiresAdvanceOrder,
    int? MinimumAdvanceOrderQuantity,
    bool IsAlwaysAvailable,
    DayOfWeek[] AvailableDays,
    string? AllergenNotes,
    CreatePriceOptionCommand[] PriceOptions
);
```

---

### Incidencia 3: Constructor de comandos de entidades secundarias

**Error:** Constructor de `MenuCategoryEntity.UpdateItem` mal inferido

**Causa:** Se asumió que solo necesitaba el validator, pero requiere:

```csharp
// ❌ INCORRECTO - Asunción errónea
new MenuCategoryEntity.UpdateItem(_categoryValidator)

// ✅ CORRECTO - Dependencias reales
var createCategoryItem = new CategoryItemVO.Create(_categoryItemValidator);
new MenuCategoryEntity.UpdateItem(createCategoryItem, _categoryValidator)
```

---

### Propuesta 6: Documentar Interfaces de Infraestructura

Añadir al documento de estilo las firmas de interfaces comunes con sus mocks:

```markdown
## Interfaces de Infraestructura - Firmas para Mocks

### IEntityLookup
```csharp
// Firma completa (todos los parámetros obligatorios en mocks)
Task<T> GetRequiredAsync<T, TId>(
    TId id,
    bool tracking = true,
    CancellationToken ct = default,
    params string[] includes)

// Mock correcto
_entityLookupMock.Setup(e => e.GetRequiredAsync<TEntity, TId>(
    It.IsAny<TId>(),
    It.IsAny<bool>(),
    It.IsAny<CancellationToken>(),
    It.IsAny<string[]>()
)).ReturnsAsync(entity);
```

### IUnitOfWork
```csharp
Task SaveChangesAsync(CancellationToken ct = default)

// Mock
_unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
```
```

**Impacto:** Evitaría errores de compilación por firmas desconocidas

---

### Propuesta 7: Catálogo de Records/Commands con Muchos Parámetros

Documentar los commands que tienen firmas complejas para facilitar la creación de helpers:

```markdown
## Commands con Firmas Complejas

### CreateMenuItemCommand (12 params)
```csharp
// Helper recomendado en tests
private static MenuItemAgg CreateMenuItem(
    string name = "Test Item",
    decimal price = 10.00m)
{
    return create.Execute(new CreateMenuItemCommand(
        TenantId: Guid.NewGuid(),
        Name: name,
        Description: null,
        ImageUrl: null,
        DisplayOrder: 0,
        IsHighRiskItem: false,
        RequiresAdvanceOrder: false,
        MinimumAdvanceOrderQuantity: null,
        IsAlwaysAvailable: true,
        AvailableDays: [],
        AllergenNotes: null,
        PriceOptions: [new CreatePriceOptionCommand(PortionType.Full, price)]
    ));
}
```
```

**Impacto:** Evitaría errores CS7036 por parámetros faltantes

---

## Resumen de Impacto Actualizado

| Propuesta | Lecturas Evitadas | Errores Evitados | Complejidad |
|-----------|-------------------|------------------|-------------|
| 1. Mapa de dependencias | ~12 | - | Baja |
| 2. Ejemplos de setup | ~3 | - | Baja |
| 3. Convención de nombres | ~6 | - | Media |
| 4. Docs por feature | ~5 | - | Media |
| 5. Generador de esqueleto | ~28 (todas) | Todos | Alta |
| **6. Firmas de interfaces** | ~2 | CS1503, CS0854 | Baja |
| **7. Catálogo de commands** | ~3 | CS7036 | Baja |

**Recomendación actualizada:** Implementar propuestas 1, 2, 3, **6 y 7** primero

---

## Conclusión

El 50% de las lecturas (14/28) son inevitables (las slices a testear). El otro 50% podría reducirse significativamente con mejor documentación de dependencias y ejemplos completos en el documento de estilo.

**Conclusión adicional de las incidencias:** Más allá de las lecturas, los errores de compilación revelan que falta documentar:
- Firmas completas de interfaces de infraestructura (especialmente para mocks con Moq)
- Records/Commands con muchos parámetros que requieren helpers predefinidos
- Restricción de C#: expression trees no soportan parámetros opcionales

La inversión en documentación se amortiza rápidamente considerando que:
- Cada nuevo aggregate tendrá múltiples slices
- Los tests siguen patrones predecibles
- La IA puede seguir reglas documentadas sin necesidad de "descubrir" patrones
