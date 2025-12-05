# Fudie Source Generators

Source generators de Roslyn para automatizar la generación de código en Fudie.

## 📦 Componentes

### **RepositorySourceGenerator.cs**
Generador principal que crea implementaciones automáticas de repositorios basándose en interfaces.

**Funcionalidades:**
- ✅ Genera implementaciones de `IGet<T, ID>`, `IAdd<T>`, `IUpdate<T, ID>`, `IRemove<T, ID>`
- ✅ Procesa atributos de configuración (`Include`, `AsNoTracking`, `AsSplitQuery`, etc.)
- ✅ Valida paths de navegación para `Include`/`ThenInclude`
- ✅ Genera métodos de query automáticos (ver [Query Method Generator](#query-method-generator))
- ✅ Inyección de dependencias automática con `[Injectable]`

**Ejemplo:**
```csharp
[Include("Orders.Items")]
[AsNoTracking]
public interface ICustomerRepository : IGet<Customer, Guid>, IAdd<Customer>
{
    Task<List<Customer>> FindByCity(string city);
}

// Genera automáticamente CustomerRepository con:
// - Método Get(Guid id) con Include de Orders.Items
// - Método Add(Customer entity)
// - Método FindByCity(string city) con LINQ generado
```

### **CodeBuilder.cs**
Constructor de código C# para generar clases de repositorio.

**Responsabilidades:**
- Genera código de clases completas
- Crea cadenas de `Include`/`ThenInclude`
- Genera métodos CRUD (Get, Add, Remove)
- Integra query methods generados
- Aplica modificadores de query (AsNoTracking, AsSplitQuery, etc.)

### **PathValidator.cs**
Validador de paths de navegación para atributos `Include`.

**Validaciones:**
- ✅ Verifica que las propiedades existan en las entidades
- ✅ Valida tipos de navegación (colecciones vs referencias)
- ✅ Detecta paths inválidos y reporta diagnósticos claros
- ✅ Soporta navegación multinivel (ej: `Orders.Items.Product`)

**Ejemplo de validación:**
```csharp
[Include("Orders.InvalidProperty")]  // ❌ Error: InvalidProperty no existe
[Include("Orders.Items")]            // ✅ Válido
public interface ICustomerRepository : IGet<Customer, Guid> { }
```

### **Query Method Generator** 📂

Sistema completo para generar métodos de consulta basados en convenciones de nombres.

**¿Qué hace?**  
Permite definir métodos de query en interfaces usando nombres descriptivos (ej: `FindByEmail`, `CountByActiveTrue`), y el generador crea automáticamente la implementación LINQ.

**Componentes:**
- `QueryParser` - Parsea nombres de métodos
- `QueryValidator` - Valida queries contra entidades
- `LinqEmitter` - Genera código LINQ
- `Models` - Estructuras de datos
- `Diagnostics` - Definiciones de errores

**Ejemplos:**
```csharp
// Defines la firma - el generador crea la implementación
Task<List<User>> FindByEmail(string email);
Task<User?> FindFirstByAge(int age);
Task<List<User>> FindByAgeGreaterThanAndActiveTrueOrderByCreatedAtDesc(int age);
Task<int> CountByActiveTrue();
Task<bool> ExistsByEmail(string email);
```

**📖 [Ver documentación completa →](./QueryMethod/README.md)**

## 🔄 Flujo de Generación

```
1. Interfaz de Repositorio con atributos
   ↓
2. RepositorySourceGenerator detecta la interfaz
   ↓
3. PathValidator valida paths de Include
   ↓
4. Query Method Generator procesa métodos de query
   ↓
5. CodeBuilder genera la clase completa
   ↓
6. Código C# generado y compilado
```

## 🎯 Características

### Atributos Soportados

| Atributo | Descripción |
|---|---|
| `[Include("Path")]` | Incluye navegación relacionada |
| `[AsNoTracking]` | Deshabilita tracking de cambios |
| `[AsSplitQuery]` | Usa queries separadas para colecciones |
| `[IgnoreQueryFilters]` | Ignora filtros globales |
| `[Tracking(false)]` | Control explícito de tracking |

### Interfaces de Infraestructura

| Interfaz | Método Generado | Descripción |
|---|---|---|
| `IGet<T, ID>` | `Task<T> Get(ID id)` | Obtiene entidad por ID |
| `IAdd<T>` | `void Add(T entity)` | Agrega entidad |
| `IUpdate<T, ID>` | `Task<T> Get(ID id)` | Get con tracking habilitado |
| `IRemove<T, ID>` | `void Remove(T entity)` | Elimina entidad |

### Diagnósticos

El generador reporta errores claros durante la compilación:

| ID | Descripción |
|---|---|
| `FUDIE001` | Atributo Include inválido |
| `FUDIE002` | No se encontró tipo de entidad |
| `FUDIE003` | Tipo de entidad no encontrado en compilación |
| `REPO001-007` | Errores de query methods (ver [QueryMethod](./QueryMethod/README.md)) |

## 💡 Ejemplo Completo

```csharp
using Fudie.Infrastructure;
using Fudie.Attributes;

namespace MyApp.Repositories;

public class Customer
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string City { get; set; }
    public List<Order> Orders { get; set; }
}

[Include("Orders.Items")]
[AsNoTracking]
public interface ICustomerRepository : 
    IGet<Customer, Guid>, 
    IAdd<Customer>
{
    // Query methods - implementación generada automáticamente
    Task<List<Customer>> FindByCity(string city);
    Task<Customer?> FindFirstByName(string name);
    Task<int> CountByCityAndActiveTrue(string city);
}

// Código generado:
[Injectable(ServiceLifetime.Scoped)]
public class CustomerRepository : 
    IGet<Customer, Guid>, 
    IAdd<Customer>,
    ICustomerRepository
{
    private readonly IEntityLookup _entityLookup;
    private readonly IChangeTracker _changeTracker;

    public CustomerRepository(IEntityLookup entityLookup, IChangeTracker changeTracker)
    {
        _entityLookup = entityLookup;
        _changeTracker = changeTracker;
    }

    public async Task<Customer> Get(Guid id)
    {
        var query = _entityLookup.Query<Customer>();
        
        // Apply includes
        query = query.Include(c => c.Orders)
            .ThenInclude(o => o.Items);
        
        // Apply query modifiers
        query = query.AsNoTracking();
        
        var entity = await query.FirstOrDefaultAsync(c => c.Id == id);
        
        if (entity == null)
            throw new KeyNotFoundException($"Customer with ID '{id}' not found.");
        
        return entity;
    }

    public void Add(Customer entity)
    {
        _changeTracker.Entry(entity).State = EntityState.Added;
    }

    public async Task<List<Customer>> FindByCity(string city)
    {
        return await _query.Query<Customer>()
            .Where(x => x.City == city)
            .ToListAsync();
    }

    public async Task<Customer?> FindFirstByName(string name)
    {
        return await _query.Query<Customer>()
            .Where(x => x.Name == name)
            .FirstOrDefaultAsync();
    }

    public async Task<int> CountByCityAndActiveTrue(string city)
    {
        return await _query.Query<Customer>()
            .Where(x => x.City == city && x.Active == true)
            .CountAsync();
    }
}
```

## 🧪 Testing

El sistema de generadores incluye **145 tests unitarios** (100% pasando):
- Tests de generación de código
- Tests de validación de paths
- Tests de query methods
- Tests end-to-end de generación completa

## 📚 Documentación Adicional

- **[Query Method Generator](./QueryMethod/README.md)** - Generación de métodos de consulta
- **[Especificación de Query Methods](./.task/query-method-generator-spec.md)** - Gramática completa
- **[Comparativa con Spring Data](./.task/Comparativa-Metodos-Spring-Data.md)** - Análisis comparativo

## 🔧 Desarrollo

### Agregar Nuevo Generador

1. Crear clase que herede de `IIncrementalGenerator`
2. Implementar `Initialize` con pipeline incremental
3. Implementar `Execute` con lógica de generación
4. Registrar en el proyecto Fudie.csproj

### Agregar Nuevo Diagnóstico

1. Definir `DiagnosticDescriptor` en clase de diagnósticos
2. Crear método factory para el diagnóstico
3. Reportar usando `context.ReportDiagnostic()`

## 🎯 Roadmap

- [ ] Soporte para navegación en query methods
- [ ] Soporte para `ThenBy` múltiple
- [ ] Generación de métodos async batch
- [ ] Soporte para proyecciones (Select)
- [ ] Generación de specifications pattern
