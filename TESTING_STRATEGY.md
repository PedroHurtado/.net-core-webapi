# Estrategia de Testing para Comandos de Dominio y Slices

## Situacion Actual

### Problema 1: Cadena de dependencias en tests

Para testear un comando como `UpdateSpecialDate`, actualmente necesitamos:

```csharp
// Setup - 4 comandos para testear 1
var schedule = _createServiceSchedule.Execute(...);  // Comando 1
_addService.Execute(schedule, ...);                   // Comando 2
_addSpecialDate.Execute(schedule, ...);               // Comando 3
_updateSpecialDate.Execute(schedule, ...);            // Comando 4 - LO QUE REALMENTE TESTEAMOS
```

**Problemas:**
- Si falla el test, no sabes cual comando fallo
- Cambios en `Create` pueden romper tests de `UpdateSpecialDate`
- Mucho codigo de setup repetido
- Tests lentos de escribir y mantener

### Problema 2: Tests de Slice vs Tests de Dominio

Los tests de slice (API) actualmente:
1. Obtienen multiples comandos del DomainFixture
2. Ejecutan cadena de comandos para crear estado valido
3. El Repository mock devuelve esa entidad
4. Finalmente testean la slice

Pero la slice solo inyecta UN comando. Los demas son "infraestructura de test".

---

## Solucion Propuesta: Builder Pattern

### Concepto

```
Tests de Create     → Testean la creacion (usan el comando real)
Tests de otros      → Usan Builder para crear estado necesario
```

### ServiceScheduleBuilder

```csharp
public class ServiceScheduleBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _tenantId = Guid.NewGuid();
    private string _name = "Test Schedule";
    private ReservationPolicy? _policy;
    private List<Service> _services = new();
    private bool _isActive = false;

    public ServiceScheduleBuilder WithId(Guid id) { _id = id; return this; }
    public ServiceScheduleBuilder WithTenantId(Guid id) { _tenantId = id; return this; }
    public ServiceScheduleBuilder WithName(string name) { _name = name; return this; }
    public ServiceScheduleBuilder WithPolicy(ReservationPolicy policy) { _policy = policy; return this; }
    public ServiceScheduleBuilder WithService(Service service) { _services.Add(service); return this; }
    public ServiceScheduleBuilder AsActive() { _isActive = true; return this; }

    // Metodos de conveniencia
    public ServiceScheduleBuilder WithLunchService() => WithService(ServiceBuilder.Lunch().Build());
    public ServiceScheduleBuilder WithDinnerService() => WithService(ServiceBuilder.Dinner().Build());
    public ServiceScheduleBuilder WithServiceAndSpecialDate(ServiceType type, DateOnly date) { ... }

    public ServiceSchedule Build()
    {
        // Construye la entidad directamente, sin ejecutar comandos
        // Puede usar reflection o constructor interno para tests
    }
}
```

### ServiceBuilder (para Value Objects)

```csharp
public class ServiceBuilder
{
    public static ServiceBuilder Lunch() => new ServiceBuilder().WithType(ServiceType.Lunch);
    public static ServiceBuilder Dinner() => new ServiceBuilder().WithType(ServiceType.Dinner);

    public ServiceBuilder WithType(ServiceType type) { ... }
    public ServiceBuilder WithMaxCapacity(int capacity) { ... }
    public ServiceBuilder WithWeeklySchedule(Dictionary<DayOfWeek, ServiceDayConfig> schedule) { ... }
    public ServiceBuilder WithSpecialDate(ServiceSpecialDate date) { ... }
    public ServiceBuilder WithNoAvailableDays() { ... }  // Para testear guards

    public Service Build() { ... }
}
```

---

## Casos de Uso

### Test de Create (usa comando real)

```csharp
public class ServiceSchedule_CreateTests
{
    [Fact]
    public void Execute_WithValidData_CreatesSchedule()
    {
        var command = new CreateServiceScheduleCommand(...);

        var result = _create.Execute(command);

        result.Should().NotBeNull();
        result.Name.Should().Be("Test");
    }

    [Fact]
    public void Execute_WithEmptyName_ThrowsValidationException()
    {
        var command = new CreateServiceScheduleCommand(Name: "");

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }
}
```

### Test de UpdateSpecialDate (usa Builder)

```csharp
public class ServiceSchedule_UpdateSpecialDateTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly ServiceSchedule.UpdateSpecialDate _updateSpecialDate =
        fixture.Get<ServiceSchedule.UpdateSpecialDate>();

    [Fact]
    public void Execute_WithValidData_UpdatesSpecialDate()
    {
        // Arrange - Builder crea estado necesario
        var targetDate = new DateOnly(2025, 2, 14);
        var schedule = new ServiceScheduleBuilder()
            .WithServiceAndSpecialDate(ServiceType.Dinner, targetDate)
            .Build();

        var command = new UpdateServiceScheduleSpecialDateCommand(
            Type: ServiceType.Dinner,
            Date: targetDate,
            IsAvailable: true,
            StartTime: new TimeOnly(18, 0),
            EndTime: new TimeOnly(23, 0));

        // Act
        var result = _updateSpecialDate.Execute(schedule, command);

        // Assert
        var specialDate = result.Services.First().SpecialDates.First();
        specialDate.StartTime.Should().Be(new TimeOnly(18, 0));
    }

    [Fact]
    public void Execute_WhenServiceNotExists_ThrowsKeyNotFoundException()
    {
        // Builder crea schedule SIN servicios - para testear guard
        var schedule = new ServiceScheduleBuilder()
            .Build();  // Sin servicios

        var command = new UpdateServiceScheduleSpecialDateCommand(
            Type: ServiceType.Dinner, ...);

        var act = () => _updateSpecialDate.Execute(schedule, command);

        act.Should().Throw<KeyNotFoundException>()
            .WithMessage("*Service of type 'Dinner' not found*");
    }

    [Fact]
    public void Execute_WhenSpecialDateNotExists_ThrowsKeyNotFoundException()
    {
        // Builder crea schedule con servicio pero SIN special date
        var schedule = new ServiceScheduleBuilder()
            .WithDinnerService()  // Tiene servicio
            .Build();             // Pero no tiene special dates

        var command = new UpdateServiceScheduleSpecialDateCommand(
            Type: ServiceType.Dinner,
            Date: new DateOnly(2025, 3, 15), ...);

        var act = () => _updateSpecialDate.Execute(schedule, command);

        act.Should().Throw<KeyNotFoundException>()
            .WithMessage("*Special date '2025-03-15' not found*");
    }
}
```

### Test de Activate (guards complejos)

```csharp
[Fact]
public void Execute_WhenNoServicesConfigured_ThrowsValidationException()
{
    var schedule = new ServiceScheduleBuilder()
        .Build();  // Sin servicios

    var act = () => _activate.Execute(schedule, new ActivateServiceScheduleCommand(null));

    act.Should().Throw<ValidationException>()
        .WithMessage("*at least one service*");
}

[Fact]
public void Execute_WhenNoAvailableDays_ThrowsValidationException()
{
    var schedule = new ServiceScheduleBuilder()
        .WithService(ServiceBuilder.Lunch().WithNoAvailableDays().Build())
        .Build();

    var act = () => _activate.Execute(schedule, new ActivateServiceScheduleCommand(null));

    act.Should().Throw<ValidationException>()
        .WithMessage("*at least one day available*");
}

[Fact]
public void Execute_WithValidConfiguration_ActivatesSchedule()
{
    var schedule = new ServiceScheduleBuilder()
        .WithLunchService()  // Servicio con dias disponibles
        .Build();

    var result = _activate.Execute(schedule, new ActivateServiceScheduleCommand(null));

    result.IsActive.Should().BeTrue();
}
```

### Test de Slice (solo comando inyectado)

```csharp
public class UpdateServiceScheduleSpecialDateSliceTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly ServiceSchedule.UpdateSpecialDate _updateSpecialDate =
        fixture.Get<ServiceSchedule.UpdateSpecialDate>();
    private readonly Mock<UpdateServiceScheduleSpecialDate.IRepository> _repositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    [Fact]
    public async Task HandleAsync_WithValidData_UpdatesAndSaves()
    {
        // Arrange
        var scheduleId = Guid.NewGuid();
        var targetDate = new DateOnly(2025, 2, 14);

        // Builder crea el estado que el Repository devolvera
        var existingSchedule = new ServiceScheduleBuilder()
            .WithId(scheduleId)
            .WithServiceAndSpecialDate(ServiceType.Lunch, targetDate)
            .Build();

        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);

        var service = new UpdateServiceScheduleSpecialDate.Service(
            _updateSpecialDate,
            _repositoryMock.Object,
            _unitOfWorkMock.Object);

        var request = new UpdateServiceScheduleSpecialDate.Request(
            IsAvailable: true,
            StartTime: new TimeOnly(11, 0),
            EndTime: new TimeOnly(16, 0));

        // Act
        await service.HandleAsync(scheduleId, ServiceType.Lunch, targetDate, request);

        // Assert
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
```

---

## Comparativa

| Aspecto | Ahora | Con Builder |
|---------|-------|-------------|
| Comandos en setup | 3-4 | 0 |
| Lineas de setup | 15-20 | 3-5 |
| Aislamiento | Bajo | Total |
| Si falla Create | Rompe todos los tests | Solo tests de Create |
| Claridad | "Que hace este setup?" | "Builder con X estado" |
| Velocidad escribir | Lento | Rapido |
| Mantenimiento | Alto | Bajo |

---

## Pros

1. **Aislamiento total**: Cada test solo testea UN comando
2. **Claridad**: El Builder expresa claramente el estado necesario
3. **Mantenimiento**: Cambios en un comando no afectan tests de otros
4. **Velocidad**: Menos codigo de setup
5. **Flexibilidad**: Builder puede crear estados "imposibles" para testear guards
6. **Reutilizacion**: Builders se reutilizan en todos los tests

---

## Contras

1. **Codigo inicial**: Hay que crear los Builders
2. **Sincronizacion**: Si el agregado cambia, hay que actualizar el Builder
3. **Posibles estados invalidos**: El Builder podria crear estados que no son alcanzables via comandos
4. **Bypass de validaciones**: El Builder no ejecuta validaciones de dominio al construir

---

## Mitigaciones de Contras

### Para "estados invalidos"
- El Builder usa los Value Objects reales (Service, ServiceDayConfig, etc.)
- Solo bypasea la cadena de comandos, no las validaciones de los Value Objects

### Para "sincronizacion"
- El Builder esta en el proyecto de tests
- Cuando el agregado cambia, los tests fallan y actualizas el Builder

### Para "bypass de validaciones"
- Los tests de Create SI ejecutan validaciones completas
- Los demas tests asumen que el estado es valido (viene del Repository)
- En produccion, el estado siempre viene de comandos validados

---

## Implementacion Tecnica

### Opcion 1: Constructor interno para tests

```csharp
public partial class ServiceSchedule
{
    // Constructor interno solo para tests
    internal ServiceSchedule(
        Guid id,
        Guid tenantId,
        string name,
        ReservationPolicy policy,
        IEnumerable<Service> services,
        bool isActive)
    {
        Id = id;
        TenantId = tenantId;
        Name = name;
        ReservationPolicy = policy;
        _services = services.ToList();
        _isActive = isActive;
    }
}
```

### Opcion 2: Reflection en el Builder

```csharp
public ServiceSchedule Build()
{
    var schedule = (ServiceSchedule)FormatterServices.GetUninitializedObject(typeof(ServiceSchedule));

    typeof(ServiceSchedule).GetProperty("Id")!.SetValue(schedule, _id);
    typeof(ServiceSchedule).GetField("_services", BindingFlags.NonPublic | BindingFlags.Instance)!
        .SetValue(schedule, _services);
    // etc.

    return schedule;
}
```

### Opcion 3: InternalsVisibleTo

```csharp
// En el proyecto de dominio
[assembly: InternalsVisibleTo("Schedules.UnitTests")]
```

---

## Conclusion

El patron Builder para tests de agregados DDD es una practica estandar que:

1. Simplifica enormemente el codigo de test
2. Aisla cada test para probar exactamente lo que debe
3. Reduce el mantenimiento a largo plazo
4. Permite crear cualquier estado necesario para testear guards

La inversion inicial (crear Builders) se amortiza rapidamente con la cantidad de tests que se simplifican.

---

## Proximos Pasos

1. Decidir metodo de construccion (constructor interno, reflection, InternalsVisibleTo)
2. Crear `ServiceScheduleBuilder` basico
3. Crear builders auxiliares (`ServiceBuilder`, `ServiceDayConfigBuilder`, etc.)
4. Migrar un test existente como prueba de concepto
5. Si funciona, migrar resto de tests gradualmente
