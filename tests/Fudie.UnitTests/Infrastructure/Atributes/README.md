# Tests Unitarios - Infrastructure Attributes

Este directorio contiene tests unitarios completos para todos los atributos definidos en `Fudie.Infrastructure.Atributes`.

## 📋 Resumen de Cobertura

Se han creado **109 tests unitarios** que cubren los siguientes atributos:

### 1. IncludeAttribute&lt;TEntity&gt; (21 tests)
**Archivo:** `IncludeAttributeTests.cs`

Atributo para especificar propiedades de navegación a incluir (eager loading) en queries del repository.

**Áreas cubiertas:**
- ✅ Constructor con un solo path
- ✅ Constructor con múltiples paths
- ✅ Paths anidados (ThenInclude)
- ✅ Validación de parámetros nulos
- ✅ Validación de arrays vacíos
- ✅ Validación de strings vacíos/whitespace
- ✅ Propiedad `AsSplitQuery`
- ✅ Metadata de `AttributeUsage`
- ✅ Restricciones de tipos genéricos
- ✅ Casos edge (paths largos, caracteres especiales)
- ✅ Inmutabilidad de la propiedad `Paths`

### 2. TrackingAttribute (16 tests)
**Archivo:** `TrackingAttributeTests.cs`

Atributo para especificar si las queries deben usar change tracking de EF Core.

**Áreas cubiertas:**
- ✅ Constructor con parámetro por defecto
- ✅ Constructor con `true`/`false` explícito
- ✅ Propiedad `Enabled` (lectura)
- ✅ Inmutabilidad de `Enabled`
- ✅ Metadata de `AttributeUsage`
- ✅ Herencia de `Attribute`
- ✅ Múltiples instancias independientes
- ✅ Comportamiento por defecto
- ✅ Type safety
- ✅ Semántica del atributo

### 3. AsNoTrackingAttribute (26 tests)
**Archivo:** `AsNoTrackingAttributeTests.cs`

Atributo alias de `TrackingAttribute(false)` para mejorar la legibilidad.

**Áreas cubiertas:**
- ✅ Constructor sin parámetros
- ✅ Herencia de `TrackingAttribute`
- ✅ Clase sealed
- ✅ Propiedad `Enabled` siempre `false`
- ✅ Metadata de `AttributeUsage`
- ✅ Equivalencia semántica con `Tracking(false)`
- ✅ Compatibilidad de tipos
- ✅ Polimorfismo
- ✅ Inmutabilidad
- ✅ Claridad semántica

### 4. AsSplitQueryAttribute (23 tests)
**Archivo:** `AsSplitQueryAttributeTests.cs`

Atributo marker para indicar que las queries deben usar split queries (evitar cartesian explosion).

**Áreas cubiertas:**
- ✅ Constructor sin parámetros
- ✅ Herencia de `Attribute`
- ✅ Clase sealed
- ✅ Metadata de `AttributeUsage`
- ✅ Aplicabilidad solo a interfaces
- ✅ No permite múltiples instancias
- ✅ No es heredable
- ✅ Atributo marker (sin propiedades/campos)
- ✅ Type safety
- ✅ Reflection capabilities
- ✅ Compatibilidad

### 5. IgnoreQueryFiltersAttribute (29 tests)
**Archivo:** `IgnoreQueryFiltersAttributeTests.cs`

Atributo marker para ignorar filtros globales de EF Core (soft deletes, multi-tenancy, etc.).

**Áreas cubiertas:**
- ✅ Constructor sin parámetros
- ✅ Herencia de `Attribute`
- ✅ Clase sealed
- ✅ Metadata de `AttributeUsage`
- ✅ Aplicabilidad solo a interfaces
- ✅ No permite múltiples instancias
- ✅ No es heredable
- ✅ Atributo marker (sin propiedades/campos)
- ✅ Type safety
- ✅ Reflection capabilities
- ✅ Consideraciones de seguridad
- ✅ Casos de uso documentados (soft delete, multi-tenancy, auditoría)

## 🎯 Estrategia de Testing

Los tests siguen estos principios:

### 1. **Arrange-Act-Assert (AAA)**
Todos los tests siguen el patrón AAA para claridad y mantenibilidad.

### 2. **Nombres Descriptivos**
Los nombres de los tests describen claramente:
- El escenario que se está probando
- La acción que se realiza
- El resultado esperado

Ejemplo: `Constructor_WithNullPaths_ShouldThrowArgumentNullException`

### 3. **Cobertura Completa**
Cada atributo tiene tests para:
- **Constructor:** Validación de parámetros y inicialización
- **Propiedades:** Getters, setters, valores por defecto
- **Metadata:** `AttributeUsage` (ValidOn, AllowMultiple, Inherited)
- **Herencia:** Verificación de jerarquía de clases
- **Type Safety:** Verificación de tipos y restricciones
- **Edge Cases:** Casos límite y escenarios inusuales
- **Inmutabilidad:** Verificación de propiedades read-only

### 4. **FluentAssertions**
Se utiliza FluentAssertions para aserciones más legibles:
```csharp
attribute.Paths.Should().NotBeNull();
attribute.Paths.Should().HaveCount(3);
attribute.Enabled.Should().BeTrue();
```

### 5. **Organización por Regiones**
Los tests están organizados en regiones lógicas:
- `#region Constructor Tests`
- `#region Validation Tests`
- `#region Property Tests`
- `#region Attribute Usage Tests`
- etc.

## 🚀 Ejecutar los Tests

### Ejecutar todos los tests de atributos:
```bash
dotnet test tests/Fudie.UnitTests/Fudie.UnitTests.csproj --filter "FullyQualifiedName~Infrastructure.Atributes"
```

### Ejecutar tests de un atributo específico:
```bash
# IncludeAttribute
dotnet test --filter "FullyQualifiedName~IncludeAttributeTests"

# TrackingAttribute
dotnet test --filter "FullyQualifiedName~TrackingAttributeTests"

# AsNoTrackingAttribute
dotnet test --filter "FullyQualifiedName~AsNoTrackingAttributeTests"

# AsSplitQueryAttribute
dotnet test --filter "FullyQualifiedName~AsSplitQueryAttributeTests"

# IgnoreQueryFiltersAttribute
dotnet test --filter "FullyQualifiedName~IgnoreQueryFiltersAttributeTests"
```

### Ejecutar con verbosidad detallada:
```bash
dotnet test --filter "FullyQualifiedName~Infrastructure.Atributes" --verbosity detailed
```

### Listar todos los tests sin ejecutarlos:
```bash
dotnet test --filter "FullyQualifiedName~Infrastructure.Atributes" --list-tests
```

## 📊 Resultados

```
Pruebas totales: 109
     Correcto: 109
     Fallidas: 0
 Tiempo total: ~1.4 segundos
```

## 🔍 Ejemplos de Tests

### Ejemplo 1: Validación de Constructor
```csharp
[Fact]
public void Constructor_WithNullPaths_ShouldThrowArgumentNullException()
{
    // Arrange & Act
    var act = () => new IncludeAttribute<TestEntity>(null!);

    // Assert
    act.Should().Throw<ArgumentNullException>()
        .WithParameterName("paths");
}
```

### Ejemplo 2: Verificación de Metadata
```csharp
[Fact]
public void IncludeAttribute_ShouldHaveCorrectAttributeUsage()
{
    // Arrange
    var attributeType = typeof(IncludeAttribute<TestEntity>);

    // Act
    var attributeUsage = attributeType.GetCustomAttributes(typeof(AttributeUsageAttribute), false)
        .Cast<AttributeUsageAttribute>()
        .FirstOrDefault();

    // Assert
    attributeUsage.Should().NotBeNull();
    attributeUsage!.ValidOn.Should().Be(AttributeTargets.Interface);
    attributeUsage.AllowMultiple.Should().BeTrue();
    attributeUsage.Inherited.Should().BeFalse();
}
```

### Ejemplo 3: Verificación de Herencia
```csharp
[Fact]
public void AsNoTrackingAttribute_ShouldInheritFromTrackingAttribute()
{
    // Arrange
    var attributeType = typeof(AsNoTrackingAttribute);

    // Act & Assert
    attributeType.Should().BeAssignableTo<TrackingAttribute>();
}
```

## 📝 Notas Importantes

### IncludeAttribute&lt;TEntity&gt;
- ✅ Valida que los paths no sean nulos, vacíos o whitespace
- ✅ Soporta múltiples paths en un solo atributo
- ✅ Soporta paths anidados con dot notation
- ✅ Propiedad `AsSplitQuery` opcional

### TrackingAttribute
- ✅ Valor por defecto es `true` (tracking habilitado)
- ✅ Propiedad `Enabled` es read-only
- ✅ Clase base para `AsNoTrackingAttribute`

### AsNoTrackingAttribute
- ✅ Siempre pasa `false` al constructor base
- ✅ Clase sealed (no se puede heredar)
- ✅ Alias semántico más legible que `Tracking(false)`

### AsSplitQueryAttribute
- ✅ Atributo marker (sin propiedades)
- ✅ Su presencia activa el comportamiento split query
- ✅ Clase sealed

### IgnoreQueryFiltersAttribute
- ✅ Atributo marker (sin propiedades)
- ✅ Tiene implicaciones de seguridad
- ✅ Casos de uso: soft delete, multi-tenancy, auditoría
- ✅ Clase sealed

## 🔗 Referencias

- **Código fuente:** `src/Fudie/Infrastructure/Atributes/Attributes.cs`
- **Tests:** `tests/Fudie.UnitTests/Infrastructure/Atributes/`
- **Framework de testing:** xUnit + FluentAssertions
