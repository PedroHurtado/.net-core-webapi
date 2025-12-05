# Query Method Generator

Generador automático de métodos de consulta basado en convenciones de nombres, similar a Spring Data JPA pero para .NET con EF Core.

## 🎯 ¿Qué hace?

Permite definir métodos de consulta en interfaces de repositorio usando nombres descriptivos, y el generador crea automáticamente la implementación LINQ correspondiente.

**Ejemplo:**

```csharp
public interface IUserRepository : IGet<User, Guid>
{
    // Solo defines la firma - el generador crea la implementación
    Task<List<User>> FindByEmail(string email);
    Task<User?> FindFirstByAge(int age);
    Task<int> CountByActiveTrue();
}

// Código generado automáticamente:
public async Task<List<User>> FindByEmail(string email)
{
    return await _query.Query<User>()
        .Where(x => x.Email == email)
        .ToListAsync();
}
```

## 📁 Componentes

### **Models.cs**
Define las estructuras de datos del sistema:
- `QueryType` - Tipos de operación (Find, Count, Exists, Delete)
- `Operator` - 17 operadores de comparación (Equal, GreaterThan, StartsWith, etc.)
- `Condition` - Representa una condición de filtrado
- `OrderBy` - Representa ordenamiento
- `ParsedQuery` - Query parseada completa
- `ParseResult` - Resultado del parsing

### **Diagnostics.cs**
Define los 7 diagnósticos de error que el generador puede reportar:
- `REPO001` - Propiedad no encontrada (con sugerencias)
- `REPO002` - Tipo de parámetro incompatible
- `REPO003` - Faltan parámetros
- `REPO004` - Parámetros extras
- `REPO005` - Tipo de retorno incorrecto
- `REPO006` - Operador incompatible con tipo
- `REPO007` - Error de parsing

### **QueryParser.cs**
Parsea nombres de métodos y extrae la información de la query:
- Detecta prefijos: `FindBy`, `FindFirstBy`, `FindTopNBy`, `CountBy`, `ExistsBy`, `DeleteBy`
- Reconoce 17 operadores de comparación
- Maneja condiciones lógicas: `And`, `Or`
- Extrae ordenamiento: `OrderBy` con `Asc`/`Desc`
- Soporta modificadores: `IgnoreCase`

**Ejemplo de parsing:**
```
FindByAgeGreaterThanAndActiveTrueOrderByCreatedAtDesc
  ↓
- Tipo: Find
- Condiciones:
  * Age > [parámetro]
  * Active == true
- OrderBy: CreatedAt DESC
```

### **QueryValidator.cs**
Valida queries parseadas contra la estructura real de las entidades:
- Verifica existencia de propiedades (incluyendo heredadas)
- Sugiere correcciones usando algoritmo de Levenshtein
- Valida compatibilidad de operadores con tipos
- Verifica cantidad y tipos de parámetros
- Valida tipos de retorno según el prefijo

**Validaciones:**
- ✅ Propiedades existen en la entidad
- ✅ Operadores compatibles con tipos (ej: `GreaterThan` solo para números)
- ✅ Cantidad correcta de parámetros
- ✅ Tipos de parámetros compatibles
- ✅ Tipo de retorno correcto según operación

### **LinqEmitter.cs**
Genera el código LINQ ejecutable:
- Convierte condiciones en expresiones `Where`
- Genera `OrderBy`/`OrderByDescending`
- Agrega `Take` para límites
- Genera ejecución final: `ToListAsync`, `FirstOrDefaultAsync`, `CountAsync`, `AnyAsync`, `ExecuteDeleteAsync`
- Usa `_query.Query<T>()` (incluye AsNoTracking automático)

**Mapeo de operadores:**
- `Equal` → `x.Prop == value`
- `GreaterThan` → `x.Prop > value`
- `Between` → `x.Prop >= min && x.Prop <= max`
- `StartsWith` → `x.Prop.StartsWith(value)`
- `Like` → `EF.Functions.Like(x.Prop, pattern)`
- `IsNull` → `x.Prop == null`
- `True` → `x.Prop == true`

## 🔄 Flujo de Procesamiento

```
1. Interfaz de Repositorio
   ↓
2. QueryParser → Parsea nombre del método
   ↓
3. QueryValidator → Valida contra entidad
   ↓
4. LinqEmitter → Genera código LINQ
   ↓
5. Código C# generado
```

## 📊 Capacidades

### Prefijos Soportados
- `FindBy` - Retorna lista de entidades
- `FindFirstBy` - Retorna primera entidad o null
- `FindTopNBy` - Retorna top N entidades
- `CountBy` - Retorna cantidad (int)
- `ExistsBy` - Retorna si existe (bool)
- `DeleteBy` - Elimina y retorna cantidad eliminada

### Operadores Soportados (17)
- **Comparación**: `Equal`, `NotEqual`, `LessThan`, `LessThanOrEqual`, `GreaterThan`, `GreaterThanOrEqual`
- **Rango**: `Between`, `In`, `NotIn`
- **String**: `StartsWith`, `EndsWith`, `Contains`, `Like`
- **Null**: `IsNull`, `IsNotNull`
- **Boolean**: `True`, `False`

### Características Adicionales
- ✅ Condiciones lógicas: `And` (default), `Or`
- ✅ Ordenamiento: `OrderBy` con `Asc`/`Desc`
- ✅ Límites: `Top10`, `Top20`, etc.
- ✅ Case-insensitive: `IgnoreCase`
- ✅ Propiedades compuestas: `CreatedAt`, `FirstName`

## 💡 Ejemplos de Uso

### Queries Simples
```csharp
Task<List<User>> FindByEmail(string email);
Task<User?> FindFirstByAge(int age);
Task<int> CountByActiveTrue();
Task<bool> ExistsByEmail(string email);
```

### Queries con Operadores
```csharp
Task<List<User>> FindByAgeGreaterThan(int age);
Task<List<User>> FindByAgeBetween(int min, int max);
Task<List<User>> FindByNameStartingWith(string prefix);
Task<List<User>> FindByStatusIn(IEnumerable<Status> statuses);
```

### Queries con Lógica
```csharp
Task<List<User>> FindByNameAndAge(string name, int age);
Task<List<User>> FindByNameOrEmail(string name, string email);
Task<List<User>> FindByAgeGreaterThanAndActiveTrue(int age);
```

### Queries con Ordenamiento
```csharp
Task<List<User>> FindByActiveTrueOrderByCreatedAt();
Task<List<User>> FindByActiveTrueOrderByCreatedAtDesc();
```

### Queries con Límites
```csharp
Task<List<User>> FindTop10ByActiveTrueOrderByScoreDesc();
Task<List<User>> FindTop20ByCreatedAtGreaterThan(DateTime date);
```

### Queries Complejas
```csharp
Task<List<User>> FindByAgeGreaterThanAndActiveTrueOrderByCreatedAtDesc(int age);
Task<List<User>> FindTop10ByEmailContainingAndActiveTrueOrderByNameAsc(string text);
```

## 🧪 Testing

El sistema incluye 145 tests unitarios (100% pasando):
- **Models & Diagnostics**: 37 tests
- **QueryParser**: 45 tests
- **QueryValidator**: 17 tests
- **LinqEmitter**: 28 tests
- **CodeBuilder Integration**: 12 tests
- **End-to-End**: 6 tests

## 📝 Notas Técnicas

### Decisiones de Diseño
- **Provider-agnostic**: Genera LINQ estándar, no código específico de provider
- **Type-safe**: Validación en tiempo de compilación
- **Roslyn-based**: Usa análisis de símbolos para validación precisa
- **Levenshtein**: Sugerencias inteligentes para errores tipográficos

### Limitaciones
- Solo soporta propiedades directas de la entidad (no navegación)
- Máximo un `OrderBy` por query
- `Between` siempre es inclusivo (>=, <=)

## 🔗 Ver También

- [Especificación Completa](./query-method-generator-spec.md) - Gramática y detalles técnicos
- [Resumen de Implementación](./IMPLEMENTATION_SUMMARY.md) - Estado y métricas del proyecto
