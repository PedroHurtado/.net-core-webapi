# Fudie.Generator

Source Generator para la generación automática de implementaciones de repositorios con Entity Framework Core.

## Tabla de Contenidos

- [Interfaces Base](#interfaces-base)
- [Atributos](#atributos)
- [Convenciones de Métodos Query](#convenciones-de-métodos-query)
  - [Prefijos de Query](#prefijos-de-query)
  - [Operadores](#operadores)
  - [Modificadores](#modificadores)
  - [Ejemplos Completos](#ejemplos-completos)
- [Tracking](#tracking)

---

## Interfaces Base

El generador reconoce las siguientes interfaces de infraestructura:

| Interface | Descripción | Método Generado |
|-----------|-------------|-----------------|
| `IGet<T, ID>` | Lectura por ID | `Task<T> Get(ID id)` |
| `IAdd<T>` | Inserción | `void Add(T entity)` |
| `IUpdate<T, ID>` | Actualización (hereda de IGet) | `Task<T> Get(ID id)` con tracking |
| `IRemove<T, ID>` | Eliminación (hereda de IGet) | `void Remove(T entity)` |

### Ejemplo Básico

```csharp
public interface ICustomerRepository : IGet<Customer, Guid>, IAdd<Customer>
{
}
// Genera: Get(Guid id) y Add(Customer entity)
```

---

## Atributos

### `[GenerateRepository<TEntity>]` / `[GenerateRepository<TEntity, TId>]`

Permite crear repositorios de solo consulta sin exponer métodos `Get(id)` inseguros.

```csharp
[GenerateRepository<Ingredient>]
public interface IIngredientQueries
{
    Task<List<Ingredient>> FindByRestaurantId(Guid restaurantId);
}
```

### `[Include<TEntity>("path", ...)]`

Configura eager loading con Include/ThenInclude.

```csharp
[Include<Customer>("Orders.OrderItems.Product", "Address")]
public interface ICustomerRepository : IGet<Customer, Guid> { }
```

**LINQ Generado:**
```csharp
query = query.Include(c => c.Orders)
    .ThenInclude(o => o.OrderItems)
    .ThenInclude(oi => oi.Product);
query = query.Include(c => c.Address);
```

### `[Tracking]` / `[Tracking(bool)]`

Habilita change tracking. Aplicable a **interfaces** y **métodos**.

### `[AsNoTracking]`

Deshabilita change tracking. Aplicable a **interfaces** y **métodos**.

### `[AsSplitQuery]`

Usa split queries para evitar cartesian explosion.

### `[IgnoreQueryFilters]`

Ignora los query filters globales (ej: soft delete).

---

## Convenciones de Métodos Query

El generador parsea nombres de métodos y genera código LINQ automáticamente.

### Prefijos de Query

| Prefijo | Retorno | LINQ Final |
|---------|---------|------------|
| `FindBy` | `Task<List<T>>` | `.ToListAsync()` |
| `FindFirstBy` | `Task<T?>` | `.FirstOrDefaultAsync()` |
| `FindTop{N}By` | `Task<List<T>>` | `.Take(N).ToListAsync()` |
| `CountBy` | `Task<int>` | `.CountAsync()` |
| `ExistsBy` | `Task<bool>` | `.AnyAsync()` |
| `DeleteBy` | `Task<int>` | `.ExecuteDeleteAsync()` |

---

### Operadores

| Operador | Uso en Método | LINQ Generado |
|----------|---------------|---------------|
| *(ninguno)* / `Equal` | `FindByName` | `x.Name == name` |
| `NotEqual` | `FindByStatusNotEqual` | `x.Status != status` |
| `LessThan` | `FindByPriceLessThan` | `x.Price < price` |
| `LessThanOrEqual` | `FindByAgeLessThanOrEqual` | `x.Age <= age` |
| `GreaterThan` | `FindByScoreGreaterThan` | `x.Score > score` |
| `GreaterThanOrEqual` | `FindByDateGreaterThanOrEqual` | `x.Date >= date` |
| `Between` | `FindByPriceBetween` | `x.Price >= min && x.Price <= max` |
| `In` | `FindByStatusIn` | `statuses.Contains(x.Status)` |
| `NotIn` | `FindByTypeNotIn` | `!types.Contains(x.Type)` |
| `StartsWith` | `FindByNameStartsWith` | `x.Name.StartsWith(prefix)` |
| `EndsWith` | `FindByEmailEndsWith` | `x.Email.EndsWith(suffix)` |
| `Contains` | `FindByDescriptionContains` | `x.Description.Contains(text)` |
| `Like` | `FindByNameLike` | `EF.Functions.Like(x.Name, pattern)` |
| `IsNull` | `FindByDeletedAtIsNull` | `x.DeletedAt == null` |
| `IsNotNull` | `FindByManagerIsNotNull` | `x.Manager != null` |
| `True` | `FindByIsActiveTrue` | `x.IsActive == true` |
| `False` | `FindByIsDeletedFalse` | `x.IsDeleted == false` |

---

### Modificadores

#### Conectores Lógicos

| Modificador | Uso | LINQ |
|-------------|-----|------|
| `And` | `FindByNameAndAge` | `x.Name == name && x.Age == age` |
| `Or` | `FindByNameOrEmail` | `x.Name == name \|\| x.Email == email` |

#### Case Insensitive

| Modificador | Uso | LINQ |
|-------------|-----|------|
| `IgnoreCase` | `FindByNameIgnoreCase` | `x.Name.ToLower() == name.ToLower()` |

#### Ordenamiento

| Modificador | Uso | LINQ |
|-------------|-----|------|
| `OrderBy{Prop}` | `FindByStatusOrderByName` | `.OrderBy(x => x.Name)` |
| `OrderBy{Prop}Asc` | `FindByStatusOrderByNameAsc` | `.OrderBy(x => x.Name)` |
| `OrderBy{Prop}Desc` | `FindByStatusOrderByNameDesc` | `.OrderByDescending(x => x.Name)` |

---

### Ejemplos Completos

#### 1. Búsqueda Simple

```csharp
Task<List<Customer>> FindByName(string name);
```
**LINQ:**
```csharp
return await _query.Query<Customer>()
    .Where(x => x.Name == name)
    .ToListAsync();
```

---

#### 2. Primer Resultado con Dos Condiciones

```csharp
Task<Customer?> FindFirstByIdAndRestaurantId(Guid id, Guid restaurantId);
```
**LINQ:**
```csharp
return await _query.Query<Customer>()
    .Where(x => x.Id == id && x.RestaurantId == restaurantId)
    .FirstOrDefaultAsync();
```

---

#### 3. Búsqueda con OR

```csharp
Task<List<User>> FindByEmailOrPhone(string email, string phone);
```
**LINQ:**
```csharp
return await _query.Query<User>()
    .Where(x => x.Email == email || x.Phone == phone)
    .ToListAsync();
```

---

#### 4. Rango de Valores

```csharp
Task<List<Product>> FindByPriceBetween(decimal min, decimal max);
```
**LINQ:**
```csharp
return await _query.Query<Product>()
    .Where(x => x.Price >= min && x.Price <= max)
    .ToListAsync();
```

---

#### 5. Valores en Lista

```csharp
Task<List<Order>> FindByStatusIn(List<OrderStatus> statuses);
```
**LINQ:**
```csharp
return await _query.Query<Order>()
    .Where(x => statuses.Contains(x.Status))
    .ToListAsync();
```

---

#### 6. Case Insensitive

```csharp
Task<List<Customer>> FindByNameIgnoreCase(string name);
```
**LINQ:**
```csharp
return await _query.Query<Customer>()
    .Where(x => x.Name.ToLower() == name.ToLower())
    .ToListAsync();
```

---

#### 7. Con Ordenamiento

```csharp
Task<List<Product>> FindByCategoryOrderByPriceDesc(string category);
```
**LINQ:**
```csharp
return await _query.Query<Product>()
    .Where(x => x.Category == category)
    .OrderByDescending(x => x.Price)
    .ToListAsync();
```

---

#### 8. Top N Resultados

```csharp
Task<List<Product>> FindTop5ByIsActiveTrue();
```
**LINQ:**
```csharp
return await _query.Query<Product>()
    .Where(x => x.IsActive == true)
    .Take(5)
    .ToListAsync();
```

---

#### 9. Conteo con Condición

```csharp
Task<int> CountByRestaurantIdAndIsActiveTrue(Guid restaurantId);
```
**LINQ:**
```csharp
return await _query.Query<Ingredient>()
    .Where(x => x.RestaurantId == restaurantId && x.IsActive == true)
    .CountAsync();
```

---

#### 10. Verificar Existencia

```csharp
Task<bool> ExistsByEmailIgnoreCase(string email);
```
**LINQ:**
```csharp
return await _query.Query<User>()
    .Where(x => x.Email.ToLower() == email.ToLower())
    .AnyAsync();
```

---

#### 11. Eliminación Masiva

```csharp
Task<int> DeleteByIsDeletedTrueAndDeletedAtLessThan(DateTime cutoff);
```
**LINQ:**
```csharp
return await _query.Query<AuditLog>()
    .Where(x => x.IsDeleted == true && x.DeletedAt < cutoff)
    .ExecuteDeleteAsync();
```

---

#### 12. String Operations

```csharp
Task<List<Customer>> FindByNameStartsWithAndEmailEndsWith(string prefix, string domain);
```
**LINQ:**
```csharp
return await _query.Query<Customer>()
    .Where(x => x.Name.StartsWith(prefix) && x.Email.EndsWith(domain))
    .ToListAsync();
```

---

#### 13. Nulos

```csharp
Task<List<Employee>> FindByManagerIsNullAndDepartmentIsNotNull();
```
**LINQ:**
```csharp
return await _query.Query<Employee>()
    .Where(x => x.Manager == null && x.Department != null)
    .ToListAsync();
```

---

## Tracking

El tracking se puede configurar a nivel de **interfaz** o **método**. El atributo del método tiene prioridad.

### Reglas de Prioridad

1. **Atributo en método** → Siempre gana
2. **Atributo en interfaz** → Default para métodos sin atributo
3. **Sin atributos** → Default es `AsNoTracking` (sin tracking)

### Dependencias Generadas

| Configuración | Dependencia Inyectada | Fuente de Query |
|---------------|----------------------|-----------------|
| Sin tracking | `IQuery` | `_query.Query<T>()` |
| Con tracking | `IEntityLookup` | `_entityLookup.Set<T>()` |
| Mixto | Ambas | Según cada método |

### Ejemplo: Tracking Mixto

```csharp
[GenerateRepository<Ingredient>]
public interface IIngredientRepository
{
    // Sin tracking - solo lectura
    [AsNoTracking]
    Task<List<Ingredient>> FindByName(string name);

    // Con tracking - para modificar después
    [Tracking]
    Task<Ingredient?> FindFirstByIdAndRestaurantId(Guid id, Guid restaurantId);
}
```

**Código Generado:**
```csharp
public class IngredientRepository : IIngredientRepository
{
    private readonly IEntityLookup _entityLookup;  // Para métodos con tracking
    private readonly IQuery _query;                 // Para métodos sin tracking

    public IngredientRepository(IEntityLookup entityLookup, IQuery query)
    {
        _entityLookup = entityLookup;
        _query = query;
    }

    public async Task<List<Ingredient>> FindByName(string name)
    {
        return await _query.Query<Ingredient>()  // Sin tracking
            .Where(x => x.Name == name)
            .ToListAsync();
    }

    public async Task<Ingredient?> FindFirstByIdAndRestaurantId(Guid id, Guid restaurantId)
    {
        return await _entityLookup.Set<Ingredient>()  // Con tracking
            .Where(x => x.Id == id && x.RestaurantId == restaurantId)
            .FirstOrDefaultAsync();
    }
}
```

### Ejemplo: Tracking a Nivel de Interfaz

```csharp
[GenerateRepository<Ingredient>]
[AsNoTracking]  // Default para todos los métodos
public interface IIngredientReadRepository
{
    Task<List<Ingredient>> FindByRestaurantId(Guid restaurantId);

    [Tracking]  // Override: este método SÍ usa tracking
    Task<Ingredient?> FindFirstByIdAndRestaurantId(Guid id, Guid restaurantId);
}
```

---

## Casos de Uso Multi-Tenant

Para seguridad en aplicaciones multi-tenant, usa `[GenerateRepository]` en lugar de `IGet`:

```csharp
// ❌ INSEGURO - Get(id) no filtra por tenant
public interface IIngredientRepository : IGet<Ingredient, Guid> { }

// ✅ SEGURO - Solo expone métodos que requieren tenant
[GenerateRepository<Ingredient>]
public interface IIngredientRepository
{
    Task<Ingredient?> FindFirstByIdAndRestaurantId(Guid id, Guid restaurantId);
    Task<List<Ingredient>> FindByRestaurantId(Guid restaurantId);
}
```

---

## Tabla Resumen: Método → LINQ

| Método | LINQ Generado |
|--------|---------------|
| `FindByX(v)` | `.Where(x => x.X == v).ToListAsync()` |
| `FindFirstByX(v)` | `.Where(x => x.X == v).FirstOrDefaultAsync()` |
| `FindTop10ByX(v)` | `.Where(x => x.X == v).Take(10).ToListAsync()` |
| `CountByX(v)` | `.Where(x => x.X == v).CountAsync()` |
| `ExistsByX(v)` | `.Where(x => x.X == v).AnyAsync()` |
| `DeleteByX(v)` | `.Where(x => x.X == v).ExecuteDeleteAsync()` |
| `FindByXAndY(x,y)` | `.Where(x => x.X == x && x.Y == y)...` |
| `FindByXOrY(x,y)` | `.Where(x => x.X == x \|\| x.Y == y)...` |
| `FindByXOrderByY(x)` | `.Where(...).OrderBy(x => x.Y)...` |
| `FindByXOrderByYDesc(x)` | `.Where(...).OrderByDescending(x => x.Y)...` |
| `FindByXIgnoreCase(x)` | `.Where(x => x.X.ToLower() == x.ToLower())...` |
| `FindByXBetween(min,max)` | `.Where(x => x.X >= min && x.X <= max)...` |
| `FindByXIn(list)` | `.Where(x => list.Contains(x.X))...` |
| `FindByXIsNull()` | `.Where(x => x.X == null)...` |
| `FindByXIsNotNull()` | `.Where(x => x.X != null)...` |
| `FindByXTrue()` | `.Where(x => x.X == true)...` |
| `FindByXFalse()` | `.Where(x => x.X == false)...` |
| `FindByXStartsWith(p)` | `.Where(x => x.X.StartsWith(p))...` |
| `FindByXEndsWith(s)` | `.Where(x => x.X.EndsWith(s))...` |
| `FindByXContains(t)` | `.Where(x => x.X.Contains(t))...` |
| `FindByXLike(p)` | `.Where(x => EF.Functions.Like(x.X, p))...` |
| `FindByXGreaterThan(v)` | `.Where(x => x.X > v)...` |
| `FindByXLessThanOrEqual(v)` | `.Where(x => x.X <= v)...` |
