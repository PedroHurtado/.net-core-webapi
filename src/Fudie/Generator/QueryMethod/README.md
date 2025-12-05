# Query Method Generator - Componentes Básicos

## Resumen

Este directorio contiene los componentes básicos del **Query Method Generator**, un Source Generator que permite definir métodos de consulta en interfaces de repositorio usando convenciones de nombres (similar a Spring Data JPA).

## Archivos Creados

### 1. **Models.cs**
Define los modelos de datos fundamentales:

- **`QueryType`** (enum): Tipos de query soportados
  - `Find` - Consultas que retornan entidades
  - `Count` - Conteo de registros
  - `Exists` - Verificación de existencia
  - `Delete` - Operaciones de eliminación

- **`Operator`** (enum): 17 operadores de comparación
  - Comparación: `Equal`, `NotEqual`, `LessThan`, `GreaterThan`, etc.
  - Rangos: `Between`, `In`, `NotIn`
  - Strings: `StartsWith`, `EndsWith`, `Contains`, `Like`
  - Nulos: `IsNull`, `IsNotNull`
  - Booleanos: `True`, `False`

- **`Condition`** (record): Representa una condición de filtrado
  - `Property`: Nombre de la propiedad
  - `Op`: Operador a aplicar
  - `Or`: Flag para unión con OR
  - `IgnoreCase`: Flag para comparación case-insensitive

- **`OrderBy`** (record): Representa un ordenamiento
  - `Property`: Propiedad por la que ordenar
  - `Descending`: Dirección del ordenamiento

- **`ParsedQuery`** (record): Query parseada completa
  - `Type`: Tipo de query
  - `First`: Flag para retornar solo el primer resultado
  - `Top`: Límite de resultados
  - `Conditions`: Lista de condiciones
  - `OrderBy`: Lista de ordenamientos

- **`ParseResult`** (record): Resultado del parsing
  - `Success`: Indica si el parsing fue exitoso
  - `Query`: Query parseada (si success)
  - `ErrorMessage`: Mensaje de error (si falla)
  - `ErrorPosition`: Posición del error en el nombre del método

### 2. **Diagnostics.cs**
Define los diagnósticos de compilación:

- **REPO001**: Propiedad no existe en la entidad
- **REPO002**: Tipo de parámetro incompatible
- **REPO003**: Falta parámetro requerido
- **REPO004**: Cantidad incorrecta de parámetros
- **REPO005**: Tipo de retorno incorrecto
- **REPO006**: Operador incompatible con tipo de propiedad
- **REPO007**: Error al parsear nombre del método

Incluye métodos helper para crear cada tipo de diagnóstico con mensajes formateados.

### 3. **QueryParser.cs** ✅
Parser de nombres de métodos que tokeniza y extrae información de queries:

**Características:**
- **Tokenización PascalCase mejorada**: Separa correctamente letras y números (ej: `FindTop10By` → `["Find", "Top", "10", "By"]`)
- **Detección de prefijos**: `FindBy`, `FindFirstBy`, `FindTopNBy`, `CountBy`, `ExistsBy`, `DeleteBy`
- **17 operadores soportados**: Equal (implícito), NotEqual, LessThan, GreaterThan, Between, In, NotIn, StartsWith, EndsWith, Contains, Like, IsNull, IsNotNull, True, False, etc.
- **Operadores lógicos**: `And`, `Or`
- **OrderBy**: Con soporte para `Asc`/`Desc`
- **Propiedades compuestas**: Reconoce propiedades como `CreatedAt`, `FirstName`, etc.
- **IgnoreCase**: Soporte para comparaciones case-insensitive
- **Manejo de errores**: Retorna `ParseResult` con mensajes descriptivos

**Métodos principales:**
- `Parse(methodName, entityProperties)` - Punto de entrada principal
- `TokenizePascalCase(input)` - Tokenización inteligente
- `DetectPrefix(tokens, position)` - Identifica el tipo de query
- `ParseConditions(tokens, position, properties)` - Extrae condiciones
- `ParseOrderBy(tokens, position, properties)` - Extrae ordenamiento
- `FindProperty(tokens, position, properties)` - Encuentra propiedades (incluso compuestas)

**Ejemplos de parsing:**
```csharp
// FindByEmail → { Type: Find, Conditions: [{ Property: "Email", Op: Equal }] }
// FindTop10ByActiveTrue → { Type: Find, Top: 10, Conditions: [{ Property: "Active", Op: True }] }
// FindByAgeGreaterThanAndActiveTrueOrderByCreatedAtDesc → 
//   { Type: Find, 
//     Conditions: [
//       { Property: "Age", Op: GreaterThan },
//       { Property: "Active", Op: True }
//     ],
//     OrderBy: [{ Property: "CreatedAt", Descending: true }]
//   }
```

## Tests Unitarios

### **ModelsTests.cs** (25 tests) ✅
- ✅ Validación de todos los valores de enums
- ✅ Creación de Conditions con diferentes configuraciones
- ✅ Creación de OrderBy ascendente/descendente
- ✅ Creación de ParsedQuery para todos los tipos
- ✅ ParseResult para casos de éxito y error
- ✅ Igualdad de records

### **DiagnosticsTests.cs** (12 tests) ✅
- ✅ Validación de IDs de diagnósticos (REPO001-REPO007)
- ✅ Severidad y habilitación por defecto
- ✅ Creación de diagnósticos con mensajes correctos
- ✅ Categoría consistente para todos los diagnósticos

### **QueryParserTests.cs** (45 tests) ✅
- ✅ Detección de prefijos (FindBy, FindFirstBy, FindTopNBy, CountBy, ExistsBy, DeleteBy)
- ✅ Condiciones simples (And, Or)
- ✅ Todos los 17 operadores
- ✅ IgnoreCase
- ✅ OrderBy (Asc, Desc)
- ✅ Propiedades compuestas (CreatedAt, FirstName)
- ✅ Queries complejas con múltiples condiciones y ordenamiento
- ✅ Casos de error (prefijo inválido, nombre vacío, solo prefijo)

## Resultados

```
✅ Compilación exitosa
✅ 82 tests pasando (37 Models/Diagnostics + 45 QueryParser)
✅ 0 errores
✅ 0 advertencias
```

## Próximos Pasos

1. **QueryValidator.cs** - Validador de queries (propiedades, tipos, parámetros)
2. **LinqEmitter.cs** - Generador de código LINQ
3. **Integración** - Modificar RepositorySourceGenerator

## Ejemplo de Uso Futuro

```csharp
public interface IUserRepository : IAdd<User>, IGet<User, Guid>
{
    // El generator parseará este nombre y generará la implementación LINQ
    Task<User?> FindByEmail(string email);
    
    // FindBy + Age + GreaterThan + And + Active + True + OrderBy + CreatedAt + Desc
    Task<List<User>> FindByAgeGreaterThanAndActiveTrueOrderByCreatedAtDesc(int age);
    
    // CountBy + Status
    Task<int> CountByStatus(Status status);
    
    // ExistsBy + Email + IgnoreCase
    Task<bool> ExistsByEmailIgnoreCase(string email);
}
```

## Notas Técnicas

- **Agnóstico del Provider**: El generador produce LINQ estándar compatible con cualquier provider de EF Core
- **Firestore**: Las limitaciones de Firestore (documentadas en `Comparativa-Metodos-Spring-Data.md`) son responsabilidad del provider, no del generator
- **Validación en Tiempo de Compilación**: Todos los errores se reportan como diagnósticos de Roslyn en el IDE
