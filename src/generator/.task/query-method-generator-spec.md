# Especificación Técnica: Query Method Generator

## 1. Resumen

Extender el Source Generator existente para generar implementaciones de métodos de consulta basados en convención de nombres, similar a Spring Data JPA.

**Objetivo:** Dado un método `Task<User?> FindByEmail(string email)` en una interface que implemente `IAdd<User>`, `IUpdate<User>`, `IRemove<User>` o `IGet<User, TKey>`, generar automáticamente la implementación LINQ equivalente.

---

## 2. Alcance

### En Scope

- Parser de nombres de métodos
- Validación en tiempo de compilación
- Generación de código LINQ para EF Core
- Diagnósticos descriptivos en el IDE
- Uso de `IQuery.Query<T>()` existente (ya incluye `AsNoTracking`)

### Fuera de Scope

- Soporte para Firestore (fase posterior)
- Atributo `[Tracking]` (fase posterior)
- Queries con expresiones anidadas complejas
- Proyecciones (Select)
- Joins

---

## 3. Detección de Entidad

El generator detecta la entidad `T` desde cualquiera de estas interfaces:

- `IAdd<T>`
- `IUpdate<T>`
- `IRemove<T>`
- `IGet<T, TKey>`

```csharp
// El generator extrae User de IAdd<User>
public interface IUserRepository : IAdd<User>, IUpdate<User>, IGet<User, Guid>
{
    Task<User?> FindByEmail(string email);
}
```

**No se requiere interface marcador adicional.**

---

## 4. Detección de Métodos de Query

El generator procesa métodos que comiencen con:

- `FindBy`, `FindFirstBy`, `FindTop{N}By`
- `CountBy`
- `ExistsBy`
- `DeleteBy`

---

## 5. Gramática de Nombres de Métodos

```
Método      := Prefijo Cuerpo? Orden?
Prefijo     := "FindBy" | "FindFirstBy" | "FindTop" NUMERO "By" | "CountBy" | "ExistsBy" | "DeleteBy"
Cuerpo      := Condición (Lógico Condición)*
Condición   := Propiedad Operador?
Operador    := "Is" | "Equals" | "Not" | "LessThan" | "LessThanEqual" | 
               "GreaterThan" | "GreaterThanEqual" | "Between" | "In" | "NotIn" |
               "Like" | "StartingWith" | "EndingWith" | "Containing" |
               "IsNull" | "IsNotNull" | "True" | "False" | "IgnoreCase"
Lógico      := "And" | "Or"
Orden       := "OrderBy" Propiedad ("Asc" | "Desc")?
```

---

## 6. Modelo de Datos

```csharp
public record ParsedQuery
{
    public QueryType Type { get; init; }
    public bool First { get; init; }
    public int? Top { get; init; }
    public List<Condition> Conditions { get; init; } = [];
    public List<OrderBy> OrderBy { get; init; } = [];
}

public record Condition(
    string Property,
    Operator Op,
    bool Or = false,
    bool IgnoreCase = false);

public record OrderBy(string Property, bool Descending);

public enum QueryType { Find, Count, Exists, Delete }

public enum Operator
{
    Equal, NotEqual,
    LessThan, LessThanOrEqual, GreaterThan, GreaterThanOrEqual,
    Between, In, NotIn,
    StartsWith, EndsWith, Contains, Like,
    IsNull, IsNotNull, True, False
}
```

---

## 7. Mapeo Operador a Parámetros

| Operador | Parámetros | Ejemplo Método | Ejemplo Firma |
|----------|:----------:|----------------|---------------|
| Equal | 1 | `FindByName` | `(string name)` |
| NotEqual | 1 | `FindByNameNot` | `(string name)` |
| LessThan | 1 | `FindByAgeLessThan` | `(int age)` |
| LessThanEqual | 1 | `FindByAgeLessThanEqual` | `(int age)` |
| GreaterThan | 1 | `FindByAgeGreaterThan` | `(int age)` |
| GreaterThanEqual | 1 | `FindByAgeGreaterThanEqual` | `(int age)` |
| Between | 2 | `FindByAgeBetween` | `(int min, int max)` |
| In | 1 | `FindByStatusIn` | `(IEnumerable<Status> statuses)` |
| NotIn | 1 | `FindByStatusNotIn` | `(IEnumerable<Status> statuses)` |
| Like | 1 | `FindByNameLike` | `(string pattern)` |
| StartsWith | 1 | `FindByNameStartingWith` | `(string prefix)` |
| EndsWith | 1 | `FindByNameEndingWith` | `(string suffix)` |
| Contains | 1 | `FindByNameContaining` | `(string text)` |
| IsNull | 0 | `FindByNameIsNull` | `()` |
| IsNotNull | 0 | `FindByNameIsNotNull` | `()` |
| True | 0 | `FindByActiveTrue` | `()` |
| False | 0 | `FindByActiveFalse` | `()` |

---

## 8. Mapeo Operador a LINQ

| Operador | LINQ |
|----------|------|
| Equal | `x => x.Prop == value` |
| NotEqual | `x => x.Prop != value` |
| LessThan | `x => x.Prop < value` |
| LessThanEqual | `x => x.Prop <= value` |
| GreaterThan | `x => x.Prop > value` |
| GreaterThanEqual | `x => x.Prop >= value` |
| Between | `x => x.Prop >= min && x.Prop <= max` |
| In | `x => values.Contains(x.Prop)` |
| NotIn | `x => !values.Contains(x.Prop)` |
| Like | `x => EF.Functions.Like(x.Prop, pattern)` |
| StartsWith | `x => x.Prop.StartsWith(value)` |
| EndsWith | `x => x.Prop.EndsWith(value)` |
| Contains | `x => x.Prop.Contains(value)` |
| IsNull | `x => x.Prop == null` |
| IsNotNull | `x => x.Prop != null` |
| True | `x => x.Prop == true` |
| False | `x => x.Prop == false` |

**IgnoreCase:** Aplicar `.ToLower()` a ambos lados de la comparación.

---

## 9. Mapeo Prefijo a Ejecución

| Prefijo | Retorno Esperado | Ejecución LINQ |
|---------|------------------|----------------|
| FindBy | `Task<List<T>>` | `.ToListAsync()` |
| FindFirstBy | `Task<T?>` | `.FirstOrDefaultAsync()` |
| FindTopNBy | `Task<List<T>>` | `.Take(N).ToListAsync()` |
| CountBy | `Task<int>` | `.CountAsync()` |
| ExistsBy | `Task<bool>` | `.AnyAsync()` |
| DeleteBy | `Task<int>` | `.ExecuteDeleteAsync()` |

---

## 10. Dependencia: IQuery

El código generado utiliza la interface `IQuery` existente:

```csharp
public interface IQuery
{
    IQueryable<T> Query<T>() where T : Entity;
}

// Implementación (ya incluye AsNoTracking)
public IQueryable<T> Query<T>() where T : Entity
{
    return Set<T>().AsQueryable().AsNoTracking();
}
```

**Todo el código generado usa `_query.Query<T>()` como base.**

---

## 11. Validaciones

### 11.1 Propiedad Existe

| ID | Severidad | Mensaje |
|----|-----------|---------|
| REPO001 | Error | `'{0}' no existe en '{1}'.{2}` |

- Verificar que cada propiedad en condiciones y OrderBy existe en la entidad
- Incluir propiedades heredadas de clases base
- Sugerir corrección si distancia Levenshtein <= 3

### 11.2 Tipo Compatible

| ID | Severidad | Mensaje |
|----|-----------|---------|
| REPO002 | Error | `Parámetro '{0}' es '{1}' pero '{2}' es '{3}'` |

- El tipo del parámetro debe ser compatible con el tipo de la propiedad
- Para `In`/`NotIn`: parámetro debe ser `IEnumerable<T>` donde T es compatible
- Considerar conversiones implícitas numéricas (int -> long)
- Considerar nullables (T? compatible con T)

### 11.3 Cantidad de Parámetros

| ID | Severidad | Mensaje |
|----|-----------|---------|
| REPO003 | Error | `Falta parámetro para '{0}' en '{1}'` |
| REPO004 | Error | `'{0}' tiene {1} parámetros pero se esperaban {2}` |

### 11.4 Tipo de Retorno

| ID | Severidad | Mensaje |
|----|-----------|---------|
| REPO005 | Error | `'{0}' debe retornar '{1}', no '{2}'` |

| Prefijo | Retorno Válido |
|---------|----------------|
| FindBy | `Task<List<T>>`, `Task<IEnumerable<T>>` |
| FindFirstBy, FindTop1By | `Task<T?>`, `Task<T>` |
| CountBy | `Task<int>`, `Task<long>` |
| ExistsBy | `Task<bool>` |
| DeleteBy | `Task<int>`, `Task` |

### 11.5 Operador Compatible con Tipo

| ID | Severidad | Mensaje |
|----|-----------|---------|
| REPO006 | Error | `Operador '{0}' no es válido para tipo '{1}'` |

| Operador | Tipos Válidos |
|----------|---------------|
| LessThan, GreaterThan, Between | Numéricos, DateTime, DateOnly, TimeOnly |
| StartsWith, EndsWith, Contains, Like | string |
| True, False | bool |
| IgnoreCase | Solo con propiedades string |

### 11.6 Parse Error

| ID | Severidad | Mensaje |
|----|-----------|---------|
| REPO007 | Error | `No se pudo parsear '{0}': {1}` |

---

## 12. Componentes

```
+-----------------------------------------------------+
|                  RepositoryGenerator                 |
|        (Entry point - IIncrementalGenerator)         |
+------------------------+----------------------------+
                         |
         +---------------+---------------+
         |               |               |
         v               v               v
+----------------+ +-----------+ +----------------+
|  QueryParser   | | Validator | |  LinqEmitter   |
|                | |           | |                |
|  string ->     | | Verifica  | |  ParsedQuery   |
|  ParsedQuery   | | errores   | |  -> string     |
+----------------+ +-----------+ +----------------+
```

### 12.1 QueryParser

**Input:** `string methodName`, `IReadOnlyList<string> entityProperties`

**Output:** `ParseResult` (success con `ParsedQuery` o failure con errores)

**Responsabilidades:**

- Tokenizar por PascalCase
- Identificar prefijo
- Extraer condiciones con propiedades y operadores
- Extraer OrderBy
- Manejar propiedades compuestas (ej: `CreatedAt` = tokens `Created` + `At`)

### 12.2 QueryValidator

**Input:** `ParsedQuery`, `IMethodSymbol`, `ITypeSymbol entityType`

**Output:** `List<Diagnostic>`

**Responsabilidades:**

- Validar existencia de propiedades
- Validar compatibilidad de tipos
- Validar cantidad de parámetros
- Validar tipo de retorno
- Validar operador compatible con tipo de propiedad

### 12.3 LinqEmitter

**Input:** `ParsedQuery`, `IMethodSymbol`, `string entityName`

**Output:** `string` (código C# generado)

**Responsabilidades:**

- Generar firma del método
- Generar query base con `_query.Query<T>()`
- Generar expresiones Where
- Generar OrderBy/ThenBy
- Generar Take si aplica
- Generar ejecución final

---

## 13. Casos de Prueba

### 13.1 Parser

| Input | Output Esperado |
|-------|-----------------|
| `FindByName` | Type=Find, Conditions=[{Name, Equal}] |
| `FindByNameAndAge` | Conditions=[{Name, Equal}, {Age, Equal}] |
| `FindByNameOrAge` | Conditions=[{Name, Equal, Or=true}, {Age, Equal}] |
| `FindByAgeLessThan` | Conditions=[{Age, LessThan}] |
| `FindByAgeBetween` | Conditions=[{Age, Between}] |
| `FindByActiveTrue` | Conditions=[{Active, True}] |
| `FindByNameIgnoreCase` | Conditions=[{Name, Equal, IgnoreCase=true}] |
| `FindByNameOrderByAgeDesc` | Conditions=[{Name, Equal}], OrderBy=[{Age, Desc=true}] |
| `FindFirstByEmail` | Type=Find, First=true |
| `FindTop10ByActiveTrue` | Type=Find, Top=10 |
| `CountByActiveTrue` | Type=Count |
| `ExistsByEmail` | Type=Exists |
| `DeleteByActiveFalse` | Type=Delete |

### 13.2 Validator

| Escenario | Diagnóstico |
|-----------|-------------|
| `FindByEmial` (User tiene Email) | REPO001 + sugerencia |
| `FindByAge(string x)` (Age es int) | REPO002 |
| `FindByNameAndAge(string x)` | REPO003 |
| `FindByName(string x, int y)` | REPO004 |
| `Task<User> CountByX()` | REPO005 |
| `FindByNameGreaterThan` (Name es string) | REPO006 |

### 13.3 Emitter

| Input | Output |
|-------|--------|
| FindByName | `query.Where(x => x.Name == name)` |
| FindByAgeGreaterThan | `query.Where(x => x.Age > age)` |
| FindByAgeBetween | `query.Where(x => x.Age >= min && x.Age <= max)` |
| FindByStatusIn | `query.Where(x => statuses.Contains(x.Status))` |
| FindByActiveTrue | `query.Where(x => x.Active == true)` |
| FindByNameIgnoreCase | `query.Where(x => x.Name.ToLower() == name.ToLower())` |
| FindByNameAndAgeOrStatus | `query.Where(x => x.Name == name && x.Age == age \|\| x.Status == status)` |

---

## 14. Ejemplo Completo

### Input

```csharp
public interface IUserRepository : IAdd<User>, IUpdate<User>, IGet<User, Guid>
{
    Task<User?> FindByEmail(string email);
    Task<List<User>> FindByAgeGreaterThanAndActiveTrueOrderByCreatedAtDesc(int age);
    Task<int> CountByStatus(Status status);
    Task<bool> ExistsByEmailIgnoreCase(string email);
    Task<User?> FindFirstByRoleOrderByCreatedAtDesc(Role role);
    Task<List<User>> FindTop10ByActiveTrueOrderByScoreDesc();
}
```

### Output Generado

```csharp
public partial class UserRepository
{
    private readonly IQuery _query;

    public async Task<User?> FindByEmail(string email)
    {
        return await _query.Query<User>()
            .Where(x => x.Email == email)
            .FirstOrDefaultAsync();
    }

    public async Task<List<User>> FindByAgeGreaterThanAndActiveTrueOrderByCreatedAtDesc(int age)
    {
        return await _query.Query<User>()
            .Where(x => x.Age > age && x.Active == true)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> CountByStatus(Status status)
    {
        return await _query.Query<User>()
            .Where(x => x.Status == status)
            .CountAsync();
    }

    public async Task<bool> ExistsByEmailIgnoreCase(string email)
    {
        return await _query.Query<User>()
            .Where(x => x.Email.ToLower() == email.ToLower())
            .AnyAsync();
    }

    public async Task<User?> FindFirstByRoleOrderByCreatedAtDesc(Role role)
    {
        return await _query.Query<User>()
            .Where(x => x.Role == role)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<List<User>> FindTop10ByActiveTrueOrderByScoreDesc()
    {
        return await _query.Query<User>()
            .Where(x => x.Active == true)
            .OrderByDescending(x => x.Score)
            .Take(10)
            .ToListAsync();
    }
}
```

---

## 15. Errores en IDE (Ejemplos)

```csharp
public interface IUserRepository : IAdd<User>, IGet<User, Guid>
{
    // REPO001: 'Emial' no existe en 'User'. Did you mean 'Email'?
    Task<User?> FindByEmial(string email);
    
    // REPO002: Parámetro 'age' es 'string' pero 'Age' es 'int'
    Task<User?> FindByAge(string age);
    
    // REPO002: Parámetro 'ids' es 'List<string>' pero 'Id' es 'Guid'
    Task<List<User>> FindByIdIn(List<string> ids);
    
    // REPO003: Falta parámetro para 'Age' en 'FindByNameAndAge'
    Task<User?> FindByNameAndAge(string name);
    
    // REPO004: 'FindByName' tiene 2 parámetros pero se esperaban 1
    Task<User?> FindByName(string name, int extra);
    
    // REPO005: 'CountByActiveTrue' debe retornar 'int', no 'User'
    Task<User> CountByActiveTrue();
    
    // REPO005: 'FindFirstByEmail' debe retornar 'User?', no 'List<User>'
    Task<List<User>> FindFirstByEmail(string email);
    
    // REPO006: Operador 'GreaterThan' no es válido para tipo 'string'
    Task<List<User>> FindByNameGreaterThan(string name);
    
    // OK
    Task<User?> FindByEmail(string email);
    Task<List<User>> FindByAgeGreaterThan(int age);
    Task<List<User>> FindByIdIn(List<Guid> ids);
    Task<int> CountByActiveTrue();
    Task<bool> ExistsByEmail(string email);
}
```

---

## 16. Entregables

1. **QueryParser.cs** - Parser de nombres de métodos
2. **QueryValidator.cs** - Validaciones con diagnósticos
3. **LinqEmitter.cs** - Generador de código LINQ
4. **Diagnostics.cs** - Definición de diagnósticos
5. **Models.cs** - Records y enums
6. **Tests/** - Unit tests para cada componente

---

## 17. Criterios de Aceptación

- [ ] Parser reconoce todos los prefijos y operadores de la gramática
- [ ] Parser maneja propiedades compuestas (CreatedAt, FirstName)
- [ ] Validador detecta propiedades inexistentes con sugerencia
- [ ] Validador detecta tipos incompatibles
- [ ] Validador detecta cantidad incorrecta de parámetros
- [ ] Validador detecta tipo de retorno incorrecto
- [ ] Validador detecta operador incompatible con tipo de propiedad
- [ ] Emitter genera LINQ correcto para todas las combinaciones
- [ ] Emitter usa `_query.Query<T>()` como base
- [ ] Errores aparecen en el IDE con ubicación correcta
- [ ] Integración con generators existentes (IAdd, IUpdate, IRemove, IGet)

---

## 18. Fases de Implementación

| Fase | Tarea | Prioridad |
|------|-------|-----------|
| 1 | Parser básico (FindBy + Equal, And) | Alta |
| 2 | Validador de propiedades en entidad | Alta |
| 3 | LinqEmitter básico | Alta |
| 4 | Integración con generator existente | Alta |
| 5 | Operadores de comparación (LessThan, GreaterThan, etc.) | Media |
| 6 | Between, In, NotIn | Media |
| 7 | IsNull, IsNotNull, True, False | Media |
| 8 | StartsWith, EndsWith, Contains, Like | Media |
| 9 | OrderBy con múltiples campos | Media |
| 10 | First, TopN | Media |
| 11 | Or (operador lógico) | Media |
| 12 | IgnoreCase | Baja |
| 13 | Count, Exists, Delete | Baja |
| 14 | Validación de tipos | Baja |
| 15 | Validación de operador compatible con tipo | Baja |
| 16 | Diagnósticos con sugerencias (Levenshtein) | Baja |
