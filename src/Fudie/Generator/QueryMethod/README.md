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

## Tests Unitarios

### **ModelsTests.cs** (37 tests)
- ✅ Validación de todos los valores de enums
- ✅ Creación de Conditions con diferentes configuraciones
- ✅ Creación de OrderBy ascendente/descendente
- ✅ Creación de ParsedQuery para todos los tipos
- ✅ ParseResult para casos de éxito y error
- ✅ Igualdad de records

### **DiagnosticsTests.cs** (incluido en los 37 tests)
- ✅ Validación de IDs de diagnósticos (REPO001-REPO007)
- ✅ Severidad y habilitación por defecto
- ✅ Creación de diagnósticos con mensajes correctos
- ✅ Categoría consistente para todos los diagnósticos

## Resultados

```
✅ Compilación exitosa
✅ 37 tests pasando
✅ 0 errores
✅ 0 advertencias
```

## Próximos Pasos

1. **QueryParser.cs** - Parser de nombres de métodos
2. **QueryValidator.cs** - Validador de queries
3. **LinqEmitter.cs** - Generador de código LINQ
4. **Integración** - Modificar RepositorySourceGenerator

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
