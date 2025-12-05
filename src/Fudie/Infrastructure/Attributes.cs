namespace Fudie.Infrastructure;

/// <summary>
/// Especifica las propiedades de navegación a incluir (eager loading) en las queries del repository.
/// Los paths son validados en compile-time mediante Source Generator para garantizar type-safety.
/// </summary>
/// <typeparam name="TEntity">Tipo de la entidad raíz del repository</typeparam>
/// <remarks>
/// <para>
/// Este atributo permite configurar includes de forma declarativa usando dot notation,
/// idéntico a la sintaxis de EF Core para Include/ThenInclude.
/// </para>
/// <para>
/// El Source Generator valida que:
/// - Todas las propiedades existan en los tipos correspondientes
/// - Todas las propiedades sean navegaciones (entidades o colecciones)
/// - Los tipos intermedios sean correctos
/// </para>
/// <para>
/// Puede aplicarse múltiples veces a la misma interfaz para organizar includes lógicamente.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Include simple (1 nivel)
/// [Include&lt;Customer&gt;("Orders")]
/// public interface ICustomerRepository : IGet&lt;Customer, Guid&gt; { }
/// 
/// // Include con ThenInclude (2 niveles)
/// [Include&lt;Customer&gt;("Orders.OrderItems")]
/// public interface ICustomerRepository : IGet&lt;Customer, Guid&gt; { }
/// 
/// // Include con múltiples niveles (3 niveles)
/// [Include&lt;Order&gt;("OrderItems.Product.Category")]
/// public interface IOrderRepository : IGet&lt;Order, Guid&gt; { }
/// 
/// // Múltiples paths en un atributo
/// [Include&lt;Customer&gt;("Orders", "Address", "ContactMethods")]
/// public interface ICustomerRepository : IGet&lt;Customer, Guid&gt; { }
/// 
/// // Múltiples ramas del mismo root
/// [Include&lt;Order&gt;("OrderItems.Product", "OrderItems.Discount")]
/// public interface IOrderRepository : IGet&lt;Order, Guid&gt; { }
/// 
/// // Caso complejo con AsSplitQuery
/// [Include&lt;Order&gt;(
///     "OrderItems.Product.Category",
///     "OrderItems.Product.Images", 
///     "Customer.Address",
///     "Payment")]
/// [AsSplitQuery]
/// public interface IOrderRepository : IGet&lt;Order, Guid&gt; { }
/// 
/// // Múltiples atributos (equivalente al anterior)
/// [Include&lt;Order&gt;("OrderItems.Product.Category", "OrderItems.Product.Images")]
/// [Include&lt;Order&gt;("Customer.Address")]
/// [Include&lt;Order&gt;("Payment")]
/// [AsSplitQuery]
/// public interface IOrderRepository : IGet&lt;Order, Guid&gt; { }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Interface, AllowMultiple = true, Inherited = false)]
public sealed class IncludeAttribute<TEntity> : Attribute where TEntity : class
{
    /// <summary>
    /// Obtiene los paths de navegación a incluir usando dot notation.
    /// </summary>
    /// <value>
    /// Array de strings donde cada elemento representa un path de navegación.
    /// Los paths usan dot notation para indicar niveles de anidación (ej: "Orders.OrderItems.Product").
    /// </value>
    /// <remarks>
    /// Cada segment separado por punto (.) se traduce a un ThenInclude en EF Core.
    /// Por ejemplo:
    /// - "Orders" → Include(c => c.Orders)
    /// - "Orders.OrderItems" → Include(c => c.Orders).ThenInclude(o => o.OrderItems)
    /// - "Orders.OrderItems.Product" → Include(...).ThenInclude(...).ThenInclude(...)
    /// </remarks>
    public string[] Paths { get; }
    
    /// <summary>
    /// Obtiene o establece si se debe usar AsSplitQuery() para este include específico.
    /// </summary>
    /// <value>
    /// <c>true</c> para usar split query; <c>false</c> para usar query única (default).
    /// </value>
    /// <remarks>
    /// <para>
    /// Split Query es útil cuando se incluyen múltiples colecciones para evitar
    /// cartesian explosion en la query resultante.
    /// </para>
    /// <para>
    /// Si se usa el atributo [AsSplitQuery] a nivel de interfaz, este valor se ignora.
    /// </para>
    /// </remarks>
    public bool AsSplitQuery { get; set; }
    
    /// <summary>
    /// Inicializa una nueva instancia de <see cref="IncludeAttribute{TEntity}"/> con los paths especificados.
    /// </summary>
    /// <param name="paths">
    /// Uno o más paths de navegación usando dot notation.
    /// Cada path debe ser una cadena de propiedades separadas por punto.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Se produce si <paramref name="paths"/> es <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Se produce si <paramref name="paths"/> está vacío o contiene elementos null/vacíos.
    /// </exception>
    /// <example>
    /// <code>
    /// // Un solo path
    /// [Include&lt;Customer&gt;("Orders")]
    /// 
    /// // Múltiples paths
    /// [Include&lt;Customer&gt;("Orders", "Address", "ContactMethods")]
    /// 
    /// // Path con ThenInclude
    /// [Include&lt;Customer&gt;("Orders.OrderItems")]
    /// </code>
    /// </example>
    public IncludeAttribute(params string[] paths)
    {
        if (paths == null)
            throw new ArgumentNullException(nameof(paths));
        
        if (paths.Length == 0)
            throw new ArgumentException("At least one path is required.", nameof(paths));
        
        for (int i = 0; i < paths.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(paths[i]))
                throw new ArgumentException($"Path at index {i} cannot be null or whitespace.", nameof(paths));
        }
        
        Paths = paths;
    }
}

/// <summary>
/// Especifica si las queries del repository deben usar change tracking de Entity Framework Core.
/// </summary>
/// <remarks>
/// <para>
/// Change tracking permite a EF Core detectar cambios en las entidades para operaciones de actualización.
/// Sin embargo, tiene un costo de performance, por lo que debe deshabilitarse para queries de solo lectura.
/// </para>
/// <para>
/// <strong>Valores por defecto automáticos si no se especifica:</strong>
/// - IGet solo: tracking = false (optimizado para lectura)
/// - IUpdate: tracking = true (necesario para detectar cambios)
/// - IRemove: tracking = true (necesario para eliminar)
/// </para>
/// <para>
/// Este atributo permite sobrescribir el comportamiento por defecto cuando sea necesario.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Tracking habilitado explícitamente
/// [Tracking(true)]
/// public interface ICustomerRepository : IGet&lt;Customer, Guid&gt; { }
/// 
/// // Tracking deshabilitado explícitamente
/// [Tracking(false)]
/// public interface ICustomerRepository : IUpdate&lt;Customer, Guid&gt; { }
/// 
/// // Mejor usar el alias AsNoTracking para deshabilitar
/// [AsNoTracking]
/// public interface ICustomerRepository : IGet&lt;Customer, Guid&gt; { }
/// </code>
/// </example>
/// <remarks>
/// Inicializa una nueva instancia de <see cref="TrackingAttribute"/> con el valor especificado.
/// </remarks>
/// <param name="enabled">
/// <c>true</c> para habilitar change tracking; <c>false</c> para deshabilitarlo.
/// </param>
/// <example>
/// <code>
/// [Tracking(true)]  // Habilitar tracking
/// [Tracking(false)] // Deshabilitar tracking
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
public class TrackingAttribute(bool enabled = true) : Attribute
{
    /// <summary>
    /// Obtiene si el change tracking está habilitado.
    /// </summary>
    /// <value>
    /// <c>true</c> para habilitar change tracking; <c>false</c> para deshabilitarlo.
    /// </value>
    public bool Enabled { get; } = enabled;
}

/// <summary>
/// Indica que las queries del repository deben ejecutarse sin change tracking (AsNoTracking).
/// </summary>
/// <remarks>
/// <para>
/// Este atributo es un alias semántico de [Tracking(false)] que hace el código más legible.
/// </para>
/// <para>
/// AsNoTracking mejora significativamente el performance de queries de solo lectura porque
/// EF Core no necesita mantener snapshots de las entidades ni rastrear cambios.
/// </para>
/// <para>
/// <strong>Cuándo usar:</strong>
/// - Queries de solo lectura (IGet sin IUpdate)
/// - Generación de reportes
/// - APIs que solo retornan DTOs
/// - Cualquier escenario donde no se modificarán las entidades
/// </para>
/// <para>
/// <strong>NO usar cuando:</strong>
/// - Se van a modificar las entidades (IUpdate)
/// - Se van a eliminar las entidades (IRemove)
/// - Se necesita lazy loading de propiedades relacionadas
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Repository de solo lectura optimizado
/// [AsNoTracking]
/// [Include&lt;Customer&gt;("Orders", "Address")]
/// public interface ICustomerRepository : IGet&lt;Customer, Guid&gt; { }
/// 
/// // ❌ NO usar con IUpdate (no podrá guardar cambios)
/// [AsNoTracking]
/// public interface ICustomerRepository : IUpdate&lt;Customer, Guid&gt; { }
/// 
/// // ✅ CORRECTO - Sin tracking para lectura, automático tracking=true para update
/// [AsNoTracking]
/// public interface ICustomerReadRepository : IGet&lt;Customer, Guid&gt; { }
/// 
/// public interface ICustomerWriteRepository : IUpdate&lt;Customer, Guid&gt; { }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
public sealed class AsNoTrackingAttribute : TrackingAttribute
{
    /// <summary>
    /// Inicializa una nueva instancia de <see cref="AsNoTrackingAttribute"/>.
    /// </summary>
    /// <remarks>
    /// Este constructor siempre pasa <c>false</c> al constructor base para deshabilitar tracking.
    /// </remarks>
    public AsNoTrackingAttribute() : base(false)
    {
    }
}

/// <summary>
/// Indica que las queries del repository deben ejecutarse usando split queries (AsSplitQuery).
/// </summary>
/// <remarks>
/// <para>
/// Split queries divide una query compleja con múltiples includes en queries separadas,
/// evitando el problema de "cartesian explosion" que ocurre cuando se incluyen múltiples colecciones.
/// </para>
/// <para>
/// <strong>Cartesian Explosion:</strong>
/// Cuando incluyes dos colecciones (Orders y Addresses), EF Core genera un JOIN que produce
/// todas las combinaciones posibles (producto cartesiano), resultando en datos duplicados
/// y queries muy grandes.
/// </para>
/// <para>
/// <strong>Cuándo usar:</strong>
/// - Cuando incluyes 2+ colecciones en la misma query
/// - Cuando notas queries muy lentas con múltiples includes
/// - Cuando ves warnings de EF Core sobre cartesian explosion
/// </para>
/// <para>
/// <strong>Trade-off:</strong>
/// - ✅ Ventaja: Elimina duplicación de datos, queries más rápidas
/// - ⚠️ Desventaja: Múltiples roundtrips a la base de datos
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // ❌ SIN AsSplitQuery - Genera cartesian explosion
/// [Include&lt;Customer&gt;("Orders", "Addresses", "ContactMethods")]
/// public interface ICustomerRepository : IGet&lt;Customer, Guid&gt; { }
/// // Query resultante: 10 orders × 3 addresses × 5 contacts = 150 filas!
/// 
/// // ✅ CON AsSplitQuery - Queries separadas eficientes
/// [Include&lt;Customer&gt;("Orders", "Addresses", "ContactMethods")]
/// [AsSplitQuery]
/// public interface ICustomerRepository : IGet&lt;Customer, Guid&gt; { }
/// // Query 1: Customer (1 fila)
/// // Query 2: Orders (10 filas)
/// // Query 3: Addresses (3 filas)
/// // Query 4: ContactMethods (5 filas)
/// // Total: 19 filas en lugar de 150
/// 
/// // Ejemplo real: E-commerce
/// [Include&lt;Order&gt;(
///     "OrderItems.Product.Category",
///     "OrderItems.Product.Images",
///     "Customer.Address",
///     "Payment")]
/// [AsSplitQuery] // ✅ NECESARIO - múltiples colecciones anidadas
/// public interface IOrderRepository : IGet&lt;Order, Guid&gt; { }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
public sealed class AsSplitQueryAttribute : Attribute
{
    /// <summary>
    /// Inicializa una nueva instancia de <see cref="AsSplitQueryAttribute"/>.
    /// </summary>
    /// <remarks>
    /// Este atributo no requiere parámetros. Su sola presencia activa el comportamiento de split query.
    /// </remarks>
    public AsSplitQueryAttribute()
    {
    }
}

/// <summary>
/// Indica que las queries del repository deben ignorar los filtros globales de Entity Framework Core.
/// </summary>
/// <remarks>
/// <para>
/// Los filtros globales (Global Query Filters) son condiciones WHERE que EF Core aplica automáticamente
/// a todas las queries de ciertos tipos de entidades. Ejemplos comunes: soft deletes, multi-tenancy.
/// </para>
/// <para>
/// Este atributo hace que las queries generadas incluyan .IgnoreQueryFilters(), permitiendo
/// acceder a entidades que normalmente estarían ocultas por los filtros.
/// </para>
/// <para>
/// <strong>Casos de uso comunes:</strong>
/// - Ver entidades eliminadas lógicamente (soft deleted)
/// - Acceder a datos de otros tenants (en sistemas multi-tenant)
/// - Operaciones administrativas que necesitan ver todos los datos
/// - Auditoría y reporting
/// </para>
/// <para>
/// <strong>⚠️ PRECAUCIÓN:</strong>
/// Usar este atributo puede exponer datos sensibles. Asegúrate de implementar
/// controles de autorización adicionales a nivel de aplicación.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Configuración de filtro global (en DbContext)
/// modelBuilder.Entity&lt;Customer&gt;()
///     .HasQueryFilter(c => !c.IsDeleted); // Soft delete
/// 
/// // Repository normal - solo ve clientes activos
/// public interface ICustomerRepository : IGet&lt;Customer, Guid&gt; { }
/// 
/// // Repository administrativo - ve TODOS los clientes (incluso eliminados)
/// [IgnoreQueryFilters]
/// public interface ICustomerAdminRepository : IGet&lt;Customer, Guid&gt; { }
/// 
/// // Ejemplo multi-tenancy
/// modelBuilder.Entity&lt;Order&gt;()
///     .HasQueryFilter(o => o.TenantId == _currentTenantId);
/// 
/// // Repository normal - solo ve órdenes del tenant actual
/// public interface IOrderRepository : IGet&lt;Order, Guid&gt; { }
/// 
/// // Repository admin - ve órdenes de TODOS los tenants
/// [IgnoreQueryFilters]
/// public interface IOrderCrossTenanRepository : IGet&lt;Order, Guid&gt; { }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
public sealed class IgnoreQueryFiltersAttribute : Attribute
{
    /// <summary>
    /// Inicializa una nueva instancia de <see cref="IgnoreQueryFiltersAttribute"/>.
    /// </summary>
    /// <remarks>
    /// Este atributo no requiere parámetros. Su sola presencia hace que se ignoren todos los filtros globales.
    /// </remarks>
    public IgnoreQueryFiltersAttribute()
    {
    }
}