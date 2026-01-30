# Domain Specification: ServiceSchedule

---

## 1. Enums

### ServiceType

```csharp
public enum ServiceType
{
    Breakfast,
    Lunch,
    Dinner
}
```

---

## 2. Value Objects

### 2.1 ServiceDayConfig

#### Estructura

| Propiedad | Tipo |
|-----------|------|
| IsAvailable | bool |
| StartTime | TimeOnly? |
| EndTime | TimeOnly? |
| CapacityOverride | int? |

#### Validaciones

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| StartTime | NotNull when IsAvailable=true | "Start time is required when service is available" |
| EndTime | NotNull when IsAvailable=true | "End time is required when service is available" |
| EndTime | GreaterThan(StartTime) when IsAvailable=true | "End time must be after start time" |
| StartTime | Null when IsAvailable=false | "Start time must be empty when service is not available" |
| EndTime | Null when IsAvailable=false | "End time must be empty when service is not available" |
| CapacityOverride | GreaterThan(0) when HasValue | "Capacity override must be greater than 0" |

#### Propiedades Calculadas

| Propiedad | Tipo | Fórmula |
|-----------|------|---------|
| Duration | TimeSpan? | `IsAvailable ? EndTime - StartTime : null` |

#### Comando: ServiceDayConfig.Create

**Input**

| Campo | Tipo | Default |
|-------|------|---------|
| IsAvailable | bool | |
| StartTime | TimeOnly? | null |
| EndTime | TimeOnly? | null |
| CapacityOverride | int? | null |

**Inyecta**: `IValidator<ServiceDayConfig>`

**Lógica**
```csharp
var config = new ServiceDayConfig(
    command.IsAvailable,
    command.StartTime,
    command.EndTime,
    command.CapacityOverride);

return configValidator.ValidateOrThrow(config);
```

**Estáticos**: `ServiceDayConfig.Unavailable()`

#### Tests Unitarios

✅ Config disponible válida
- Input: IsAvailable=true, StartTime=13:00, EndTime=16:00
- Resultado: ServiceDayConfig creado, Duration=3h

✅ Config disponible con capacidad override
- Input: IsAvailable=true, StartTime=20:00, EndTime=23:00, CapacityOverride=60
- Resultado: ServiceDayConfig creado

✅ Config no disponible
- Input: IsAvailable=false
- Resultado: ServiceDayConfig creado con StartTime=null, EndTime=null

❌ Disponible sin StartTime
- Input: IsAvailable=true, StartTime=null
- Resultado: ValidationException "Start time is required when service is available"

❌ Disponible sin EndTime
- Input: IsAvailable=true, StartTime=13:00, EndTime=null
- Resultado: ValidationException "End time is required when service is available"

❌ EndTime antes de StartTime
- Input: IsAvailable=true, StartTime=16:00, EndTime=13:00
- Resultado: ValidationException "End time must be after start time"

❌ No disponible con StartTime
- Input: IsAvailable=false, StartTime=13:00
- Resultado: ValidationException "Start time must be empty when service is not available"

❌ CapacityOverride cero
- Input: CapacityOverride=0
- Resultado: ValidationException "Capacity override must be greater than 0"

---

### 2.2 ServiceSpecialDate

#### Estructura

| Propiedad | Tipo |
|-----------|------|
| Date | DateOnly |
| IsAvailable | bool |
| StartTime | TimeOnly? |
| EndTime | TimeOnly? |
| CapacityOverride | int? |
| Reason | string? |

#### Validaciones

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| Date | NotEmpty | "Date is required" |
| StartTime | NotNull when IsAvailable=true | "Start time is required when service is available" |
| EndTime | NotNull when IsAvailable=true | "End time is required when service is available" |
| EndTime | GreaterThan(StartTime) when IsAvailable=true | "End time must be after start time" |
| StartTime | Null when IsAvailable=false | "Start time must be empty when service is not available" |
| EndTime | Null when IsAvailable=false | "End time must be empty when service is not available" |
| CapacityOverride | GreaterThan(0) when HasValue | "Capacity override must be greater than 0" |
| Reason | Max(200) | "Reason cannot exceed 200 characters" |

#### Comando: ServiceSpecialDate.Create

**Input**

| Campo | Tipo | Default |
|-------|------|---------|
| Date | DateOnly | |
| IsAvailable | bool | |
| StartTime | TimeOnly? | null |
| EndTime | TimeOnly? | null |
| CapacityOverride | int? | null |
| Reason | string? | null |

**Inyecta**: `IValidator<ServiceSpecialDate>`

**Lógica**
```csharp
var specialDate = new ServiceSpecialDate(
    command.Date,
    command.IsAvailable,
    command.StartTime,
    command.EndTime,
    command.CapacityOverride,
    command.Reason);

return specialDateValidator.ValidateOrThrow(specialDate);
```

#### Tests Unitarios

✅ Fecha especial disponible con horario extendido
- Input: Date=2025-02-14, IsAvailable=true, StartTime=19:00, EndTime=02:00, Reason="San Valentín"
- Resultado: ServiceSpecialDate creado

✅ Fecha especial no disponible (cerrado)
- Input: Date=2025-01-01, IsAvailable=false, Reason="Año Nuevo"
- Resultado: ServiceSpecialDate creado

✅ Fecha especial con capacidad extra
- Input: Date=2025-12-31, IsAvailable=true, StartTime=20:00, EndTime=03:00, CapacityOverride=70
- Resultado: ServiceSpecialDate creado

❌ Fecha vacía
- Input: Date=default
- Resultado: ValidationException "Date is required"

❌ Razón demasiado larga
- Input: Reason=(201 caracteres)
- Resultado: ValidationException "Reason cannot exceed 200 characters"

---

### 2.3 Service

#### Estructura

| Propiedad | Tipo |
|-----------|------|
| Type | ServiceType |
| MaxCapacity | int |
| WeeklySchedule | IReadOnlyDictionary<DayOfWeek, ServiceDayConfig> |
| SpecialDates | IReadOnlyCollection<ServiceSpecialDate> |

#### Backing Fields

```csharp
protected Dictionary<DayOfWeek, ServiceDayConfig> _weeklySchedule = [];
public IReadOnlyDictionary<DayOfWeek, ServiceDayConfig> WeeklySchedule => _weeklySchedule.AsReadOnly();

protected HashSet<ServiceSpecialDate> _specialDates = [];
public IReadOnlyCollection<ServiceSpecialDate> SpecialDates => _specialDates.ToList().AsReadOnly();
```

#### Validaciones

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| Type | IsEnum | "Invalid service type" |
| MaxCapacity | GreaterThan(0) | "Max capacity must be greater than 0" |
| WeeklySchedule | AtLeastOneAvailable | "Service must be available at least one day per week" |

#### Propiedades Calculadas

| Propiedad | Tipo | Fórmula |
|-----------|------|---------|
| HasSpecialDates | bool | `_specialDates.Any()` |
| AvailableDaysCount | int | `_weeklySchedule.Count(kvp => kvp.Value.IsAvailable)` |

#### Métodos

- `GetConfigForDate(DateOnly date)` → ServiceDayConfig: Retorna SpecialDate si existe, sino WeeklySchedule
- `IsAvailableOn(DateOnly date)` → bool
- `GetCapacityFor(DateOnly date)` → int: Retorna CapacityOverride si existe, sino MaxCapacity

#### Comando: Service.Create

**Input**

| Campo | Tipo |
|-------|------|
| Type | ServiceType |
| MaxCapacity | int |
| WeeklySchedule | Dictionary<DayOfWeek, ServiceDayConfigInput> |

**Inyecta**: `ServiceDayConfig.Create`, `IValidator<Service>`

**Lógica**
```csharp
var service = new Service
{
    Type = command.Type,
    MaxCapacity = command.MaxCapacity
};

foreach (var (day, configInput) in command.WeeklySchedule)
{
    var config = serviceDayConfigCreate.Execute(new CreateServiceDayConfigCommand(
        configInput.IsAvailable,
        configInput.StartTime,
        configInput.EndTime,
        configInput.CapacityOverride));
    
    service._weeklySchedule[day] = config;
}

return serviceValidator.ValidateOrThrow(service);
```

#### Tests Unitarios

✅ Service con horario semanal completo
- Input: Type=Lunch, MaxCapacity=50, WeeklySchedule con Lunes-Viernes disponible
- Resultado: Service creado

✅ Service solo fines de semana
- Input: Type=Dinner, MaxCapacity=40, WeeklySchedule con solo Sábado-Domingo disponible
- Resultado: Service creado

❌ MaxCapacity cero
- Input: MaxCapacity=0
- Resultado: ValidationException "Max capacity must be greater than 0"

❌ Ningún día disponible
- Input: WeeklySchedule con todos IsAvailable=false
- Resultado: ValidationException "Service must be available at least one day per week"

---

### 2.4 ReservationPolicy

#### Estructura

| Propiedad | Tipo |
|-----------|------|
| MinimumAdvanceTime | TimeSpan |
| MaximumAdvanceTime | TimeSpan |
| SlotInterval | TimeSpan |
| BufferBetweenReservations | TimeSpan |
| MaxPartySize | int |
| MinPartySize | int |
| StandardDurations | IReadOnlyDictionary<ServiceType, TimeSpan> |

#### Backing Fields

```csharp
protected Dictionary<ServiceType, TimeSpan> _standardDurations = [];
public IReadOnlyDictionary<ServiceType, TimeSpan> StandardDurations => _standardDurations.AsReadOnly();
```

#### Validaciones

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| MinimumAdvanceTime | GreaterThan(TimeSpan.Zero) | "Minimum advance time must be greater than 0" |
| MaximumAdvanceTime | GreaterThan(MinimumAdvanceTime) | "Maximum advance time must be greater than minimum advance time" |
| SlotInterval | GreaterThan(TimeSpan.Zero) | "Slot interval must be greater than 0" |
| SlotInterval | ValidSlotInterval | "Slot interval must be 15, 30, or 60 minutes" |
| BufferBetweenReservations | GreaterThanOrEqualTo(TimeSpan.Zero) | "Buffer cannot be negative" |
| MaxPartySize | GreaterThan(0) | "Max party size must be greater than 0" |
| MinPartySize | GreaterThan(0) | "Min party size must be greater than 0" |
| MinPartySize | LessThanOrEqualTo(MaxPartySize) | "Min party size cannot exceed max party size" |

#### Propiedades Calculadas

| Propiedad | Tipo | Fórmula |
|-----------|------|---------|
| SlotIntervalMinutes | int | `(int)SlotInterval.TotalMinutes` |
| MaxAdvanceDays | int | `(int)MaximumAdvanceTime.TotalDays` |

#### Métodos

- `IsValidSlot(TimeOnly time)` → bool: Verifica si el tiempo es múltiplo del SlotInterval
- `GetDurationFor(ServiceType type)` → TimeSpan
- `IsPartySizeValid(int partySize)` → bool
- `IsWithinAdvanceWindow(DateTime requestedTime, DateTime now)` → bool

#### Comando: ReservationPolicy.Create

**Input**

| Campo | Tipo | Default |
|-------|------|---------|
| MinimumAdvanceTime | TimeSpan | |
| MaximumAdvanceTime | TimeSpan | |
| SlotInterval | TimeSpan | |
| BufferBetweenReservations | TimeSpan | TimeSpan.Zero |
| MaxPartySize | int | |
| MinPartySize | int | 1 |
| StandardDurations | Dictionary<ServiceType, TimeSpan> | |

**Inyecta**: `IValidator<ReservationPolicy>`

**Lógica**
```csharp
var policy = new ReservationPolicy
{
    MinimumAdvanceTime = command.MinimumAdvanceTime,
    MaximumAdvanceTime = command.MaximumAdvanceTime,
    SlotInterval = command.SlotInterval,
    BufferBetweenReservations = command.BufferBetweenReservations,
    MaxPartySize = command.MaxPartySize,
    MinPartySize = command.MinPartySize
};

foreach (var (serviceType, duration) in command.StandardDurations)
{
    policy._standardDurations[serviceType] = duration;
}

return policyValidator.ValidateOrThrow(policy);
```

**Estáticos**: `ReservationPolicy.Default()`

#### Tests Unitarios

✅ Policy válida con valores estándar
- Input: MinAdvance=2h, MaxAdvance=30d, SlotInterval=15min, MaxParty=8, MinParty=1
- Resultado: ReservationPolicy creada

✅ Policy con buffer entre reservas
- Input: BufferBetweenReservations=15min
- Resultado: ReservationPolicy creada

✅ Policy con duraciones por servicio
- Input: StandardDurations={Breakfast:1h, Lunch:1.5h, Dinner:2h}
- Resultado: ReservationPolicy creada

❌ MinAdvance cero
- Input: MinimumAdvanceTime=0
- Resultado: ValidationException "Minimum advance time must be greater than 0"

❌ MaxAdvance menor que MinAdvance
- Input: MinAdvance=5h, MaxAdvance=2h
- Resultado: ValidationException "Maximum advance time must be greater than minimum advance time"

❌ SlotInterval inválido
- Input: SlotInterval=7min
- Resultado: ValidationException "Slot interval must be 15, 30, or 60 minutes"

❌ MinPartySize mayor que MaxPartySize
- Input: MinPartySize=10, MaxPartySize=8
- Resultado: ValidationException "Min party size cannot exceed max party size"

❌ Buffer negativo
- Input: BufferBetweenReservations=-5min
- Resultado: ValidationException "Buffer cannot be negative"

---

## 3. Aggregate: ServiceSchedule

### Estructura

```
ServiceSchedule (Aggregate Root)
├─ Id: Guid
├─ TenantId: Guid
├─ Name: string
├─ Description: string
├─ IsActive: bool
├─ Policy: ReservationPolicy
└─ Services: IReadOnlyCollection<Service>
```

#### Propiedades

| Propiedad | Tipo | Modificador |
|-----------|------|-------------|
| Id | Guid | init |
| TenantId | Guid | protected set |
| Name | string | protected set |
| Description | string | protected set |
| IsActive | bool | protected set |
| Policy | ReservationPolicy | protected set |

#### Colecciones

```csharp
protected HashSet<Service> _services = [];
public IReadOnlyCollection<Service> Services => _services.ToList().AsReadOnly();
```

#### Propiedades Calculadas

| Propiedad | Tipo | Fórmula |
|-----------|------|---------|
| HasServices | bool | `_services.Any()` |
| ServiceCount | int | `_services.Count` |
| AvailableServiceTypes | IReadOnlyCollection<ServiceType> | `_services.Select(s => s.Type).ToList().AsReadOnly()` |

### Validaciones

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| Id | NotEmpty | "Id is required" |
| TenantId | NotEmpty | "Tenant id is required" |
| Name | NotEmpty | "Name is required" |
| Name | Max(100) | "Name cannot exceed 100 characters" |
| Description | NotEmpty | "Description is required" |
| Description | Max(500) | "Description cannot exceed 500 characters" |
| Policy | NotNull | "Reservation policy is required" |

---

## 4. Response

```csharp
public record ServiceScheduleResponse(
    Guid Id,
    Guid TenantId,
    string Name,
    string Description,
    bool IsActive,
    ReservationPolicyResponse Policy,
    bool HasServices,
    int ServiceCount,
    IReadOnlyCollection<ServiceType> AvailableServiceTypes,
    IReadOnlyCollection<ServiceResponse> Services
);

public record ReservationPolicyResponse(
    TimeSpan MinimumAdvanceTime,
    TimeSpan MaximumAdvanceTime,
    TimeSpan SlotInterval,
    TimeSpan BufferBetweenReservations,
    int MaxPartySize,
    int MinPartySize,
    int SlotIntervalMinutes,
    int MaxAdvanceDays,
    IReadOnlyDictionary<ServiceType, TimeSpan> StandardDurations
);

public record ServiceResponse(
    ServiceType Type,
    int MaxCapacity,
    bool HasSpecialDates,
    int AvailableDaysCount,
    IReadOnlyDictionary<DayOfWeek, ServiceDayConfigResponse> WeeklySchedule,
    IReadOnlyCollection<ServiceSpecialDateResponse> SpecialDates
);

public record ServiceDayConfigResponse(
    bool IsAvailable,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    int? CapacityOverride,
    TimeSpan? Duration
);

public record ServiceSpecialDateResponse(
    DateOnly Date,
    bool IsAvailable,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    int? CapacityOverride,
    string? Reason
);
```

---

## 5. Event Storming - Leyenda

| Color | Elemento | Símbolo | Descripción |
|-------|----------|---------|-------------|
| 🟠 Naranja | Domain Event | `<EventName>` | Algo que ocurrió (pasado) |
| 🔵 Azul | Command | `(CommandName)` | Intención/Acción (imperativo) |
| 🟡 Amarillo | Actor | `[ActorName]` | Usuario o sistema que inicia |
| 🟣 Púrpura | Policy | `{PolicyName}` | Regla de negocio/Política |
| 🟤 Marrón | Aggregate | `[[AggregateName]]` | Entidad raíz del agregado |
| 🔴 Rojo | Hot Spot | `⚠️` | Dudas o conflictos pendientes |
| 🟢 Verde | Read Model | `📊` | Vista/Proyección de datos |
| ⚪ Blanco | External System | `⚡` | Sistema externo |

---

## 6. Comandos

> ⚠️ **IMPORTANTE**: El orden de los comandos respeta las dependencias.
> - Las Queries (Get, List) van después de Create porque son necesarias para verificar persistencia
> - Update va después de las Queries
> - Los comandos de Service van después de Update porque requieren que exista el Schedule
> - Los comandos de SpecialDate van después de AddService porque requieren que exista el Service
> - Activate/Deactivate van al final porque requieren Services configurados

---

### 6.1 ServiceSchedule.Create

#### Event Storming
```
🟡[Admin] → 🔵(CreateServiceSchedule) → 🟤[[ServiceSchedule]] → 🟠<ServiceScheduleCreated>
```

#### Input

| Campo | Tipo |
|-------|------|
| Name | string |
| Description | string |
| MinimumAdvanceTime | TimeSpan |
| MaximumAdvanceTime | TimeSpan |
| SlotInterval | TimeSpan |
| BufferBetweenReservations | TimeSpan |
| MaxPartySize | int |
| MinPartySize | int |
| StandardDurations | Dictionary<ServiceType, TimeSpan> |

#### Inyecta
- `ReservationPolicy.Create`
- `IValidator<ServiceSchedule>`
- `ITenantContext` (para obtener TenantId)

#### Guards
Ninguno.

#### Lógica
```csharp
var policy = reservationPolicyCreate.Execute(new CreateReservationPolicyCommand(
    command.MinimumAdvanceTime,
    command.MaximumAdvanceTime,
    command.SlotInterval,
    command.BufferBetweenReservations,
    command.MaxPartySize,
    command.MinPartySize,
    command.StandardDurations));

var schedule = new ServiceSchedule(Guid.NewGuid())
{
    TenantId = tenantContext.TenantId,
    Name = command.Name,
    Description = command.Description,
    IsActive = false,
    Policy = policy
};

return scheduleValidator.ValidateOrThrow(schedule);
```

#### Slice: POST /service-schedules

**Request**
```csharp
public record CreateServiceScheduleRequest(
    string Name,
    string Description,
    TimeSpan MinimumAdvanceTime,
    TimeSpan MaximumAdvanceTime,
    TimeSpan SlotInterval,
    TimeSpan BufferBetweenReservations,
    int MaxPartySize,
    int MinPartySize,
    Dictionary<ServiceType, TimeSpan> StandardDurations
);
```

**Response**: 201 Created → `ServiceScheduleResponse`

#### Tests Unitarios (Dominio)

✅ Crear schedule con datos válidos
- Input: Name="Horario Verano", Description="Junio a Septiembre", MinAdvance=2h, MaxAdvance=30d
- Resultado: ServiceSchedule creado con IsActive=false, Services vacío

✅ Crear schedule con duraciones por servicio
- Input: StandardDurations={Breakfast:1h, Lunch:1.5h, Dinner:2h}
- Resultado: ServiceSchedule creado

❌ Name vacío
- Input: Name=""
- Resultado: ValidationException "Name is required"

❌ Description vacía
- Input: Description=""
- Resultado: ValidationException "Description is required"

❌ TenantId vacío
- Input: TenantId=Guid.Empty
- Resultado: ValidationException "Tenant id is required"

❌ Policy inválida (MinAdvance > MaxAdvance)
- Input: MinAdvance=5h, MaxAdvance=2h
- Resultado: ValidationException "Maximum advance time must be greater than minimum advance time"

❌ SlotInterval inválido
- Input: SlotInterval=7min
- Resultado: ValidationException "Slot interval must be 15, 30, or 60 minutes"

#### Tests Unitarios (Servicio)

✅ Llama a ReservationPolicy.Create con los parámetros correctos
- Verifica que se invoca policyCreate.Execute con los parámetros correctos

✅ Llama a ServiceSchedule.Create con los parámetros correctos
- Verifica que se invoca scheduleCreate.Execute con el command correcto

✅ Añade el schedule al repositorio
- Verifica que repository.Add es llamado con el schedule creado

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

✅ Retorna Response mapeado correctamente
- Verifica que el Response contiene los datos del schedule

#### Tests Integración

✅ 201 Created → ServiceScheduleResponse con IsActive=false

❌ 422 → Validación fallida

---

### 6.2 GetServiceSchedule

#### Event Storming
```
🟡[Admin] → 🔵(GetServiceSchedule) → 🟤[[ServiceSchedule]] → 📊 ServiceScheduleResponse
```

#### Slice: GET /service-schedules/{id}

**Response**: 200 OK → `ServiceScheduleResponse`

#### Tests Unitarios (Servicio)

✅ Obtiene el schedule del repositorio con el id correcto
- Verifica que repository.GetByIdAsync es llamado con el id

✅ Retorna Response mapeado correctamente
- Verifica que el Response contiene los datos del schedule

#### Tests Integración

✅ 200 OK → ServiceScheduleResponse

❌ 404 → No encontrado

---

### 6.3 ListServiceSchedules

#### Event Storming
```
🟡[Admin] → 🔵(ListServiceSchedules) → 🟤[[ServiceSchedule]] → 📊 ServiceScheduleResponse[]
```

#### Slice: GET /service-schedules

**QueryParams**: `?isActive=true` (opcional)

**Response**: 200 OK → `ServiceScheduleResponse[]`

#### Tests Unitarios (Servicio)

✅ Retorna lista de schedules mapeados correctamente
- Verifica que el Response contiene los datos de los schedules

✅ Filtra por isActive cuando se proporciona
- Verifica que solo retorna schedules con el estado indicado

#### Tests Integración

✅ 200 OK → Array de ServiceScheduleResponse

✅ 200 OK → Array vacío si no hay schedules

---

### 6.4 ServiceSchedule.Update

#### Event Storming
```
🟡[Admin] → 🔵(UpdateServiceSchedule) → 🟤[[ServiceSchedule]] → 🟠<ServiceScheduleUpdated>
```

#### Input

| Campo | Tipo |
|-------|------|
| Name | string |
| Description | string |
| MinimumAdvanceTime | TimeSpan |
| MaximumAdvanceTime | TimeSpan |
| SlotInterval | TimeSpan |
| BufferBetweenReservations | TimeSpan |
| MaxPartySize | int |
| MinPartySize | int |
| StandardDurations | Dictionary<ServiceType, TimeSpan> |

#### Inyecta
- `ReservationPolicy.Create`
- `IValidator<ServiceSchedule>`

#### Guards
Ninguno.

#### Lógica
```csharp
var policy = reservationPolicyCreate.Execute(new CreateReservationPolicyCommand(
    command.MinimumAdvanceTime,
    command.MaximumAdvanceTime,
    command.SlotInterval,
    command.BufferBetweenReservations,
    command.MaxPartySize,
    command.MinPartySize,
    command.StandardDurations));

schedule.Name = command.Name;
schedule.Description = command.Description;
schedule.Policy = policy;

return scheduleValidator.ValidateOrThrow(schedule);
```

#### Slice: PUT /service-schedules/{id}

**Request**
```csharp
public record UpdateServiceScheduleRequest(
    string Name,
    string Description,
    TimeSpan MinimumAdvanceTime,
    TimeSpan MaximumAdvanceTime,
    TimeSpan SlotInterval,
    TimeSpan BufferBetweenReservations,
    int MaxPartySize,
    int MinPartySize,
    Dictionary<ServiceType, TimeSpan> StandardDurations
);
```

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

✅ Actualizar schedule existente
- Precondición: ServiceSchedule existe
- Input: Name="Horario Invierno", Description="Octubre a Mayo"
- Resultado: Name y Description actualizados

✅ Actualizar policy
- Input: MinAdvance=3h, MaxAdvance=60d
- Resultado: Policy actualizada

✅ Actualizar duraciones por servicio
- Input: StandardDurations={Breakfast:45min, Lunch:2h, Dinner:2.5h}
- Resultado: StandardDurations actualizadas

❌ Name vacío
- Input: Name=""
- Resultado: ValidationException "Name is required"

❌ Policy inválida
- Input: MinAdvance=5h, MaxAdvance=2h
- Resultado: ValidationException "Maximum advance time must be greater than minimum advance time"

#### Tests Unitarios (Servicio)

✅ Obtiene el schedule del repositorio
- Verifica que repository.GetByIdAsync es llamado con el id correcto

✅ Llama a ServiceSchedule.Update con los parámetros correctos
- Verifica que se invoca update.Execute con el command correcto

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

#### Tests Integración

✅ 204 No Content

❌ 404 → ServiceSchedule no encontrado

❌ 422 → Validación fallida

---

### 6.5 ServiceSchedule.AddService

#### Event Storming
```
🟡[Admin] → 🔵(AddService) → 🟤[[ServiceSchedule]] → 🟠<ServiceAdded>
                                    │
                          🟣{ServiceTypeÚnico}
```

#### Input

| Campo | Tipo |
|-------|------|
| Type | ServiceType |
| MaxCapacity | int |
| WeeklySchedule | Dictionary<DayOfWeek, ServiceDayConfigInput> |

#### Inyecta
- `Service.Create`
- `IValidator<ServiceSchedule>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| ServiceType ya existe | 409 | ConflictGuard | "Service of type '{Type}' already exists" |

#### Lógica
```csharp
ConflictGuard.ThrowIf(
    schedule.Services.Any(s => s.Type == command.Type),
    $"Service of type '{command.Type}' already exists");

var service = serviceCreate.Execute(new CreateServiceCommand(
    command.Type,
    command.MaxCapacity,
    command.WeeklySchedule));

schedule._services.Add(service);

return scheduleValidator.ValidateOrThrow(schedule);
```

#### Slice: POST /service-schedules/{id}/services

**Request**
```csharp
public record AddServiceRequest(
    ServiceType Type,
    int MaxCapacity,
    Dictionary<DayOfWeek, ServiceDayConfigInput> WeeklySchedule
);

public record ServiceDayConfigInput(
    bool IsAvailable,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    int? CapacityOverride
);
```

**Response**: 201 Created → `ServiceScheduleResponse`

#### Tests Unitarios (Dominio)

✅ Añadir primer servicio (Lunch)
- Precondición: ServiceSchedule sin servicios
- Input: Type=Lunch, MaxCapacity=50, WeeklySchedule con Lunes-Viernes
- Resultado: Service añadido

✅ Añadir segundo servicio (Dinner)
- Precondición: ServiceSchedule con Lunch
- Input: Type=Dinner, MaxCapacity=40, WeeklySchedule con Lunes-Domingo
- Resultado: Service añadido, total 2 servicios

✅ Añadir servicio solo fines de semana
- Input: WeeklySchedule con solo Sábado-Domingo disponible
- Resultado: Service añadido

❌ ServiceType duplicado
- Precondición: ServiceSchedule ya tiene Lunch
- Input: Type=Lunch
- Resultado: ConflictException "Service of type 'Lunch' already exists"

❌ MaxCapacity cero
- Input: MaxCapacity=0
- Resultado: ValidationException "Max capacity must be greater than 0"

❌ Ningún día disponible
- Input: WeeklySchedule con todos IsAvailable=false
- Resultado: ValidationException "Service must be available at least one day per week"

#### Tests Unitarios (Servicio)

✅ Obtiene el schedule del repositorio
- Verifica que repository.GetByIdAsync es llamado con el id correcto

✅ Llama a ServiceSchedule.AddService con los parámetros correctos
- Verifica que se invoca addService.Execute con el command correcto

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

✅ Retorna Response mapeado correctamente
- Verifica que el Response contiene el Service añadido

#### Tests Integración

✅ 201 Created → ServiceScheduleResponse con Service añadido

❌ 404 → ServiceSchedule no encontrado

❌ 409 → ServiceType duplicado

❌ 422 → Validación fallida

---

### 6.6 ServiceSchedule.UpdateService

#### Event Storming
```
🟡[Admin] → 🔵(UpdateService) → 🟤[[ServiceSchedule]] → 🟠<ServiceUpdated>
                                      │
                            🟣{ServiceExiste}
```

#### Input

| Campo | Tipo |
|-------|------|
| MaxCapacity | int |
| WeeklySchedule | Dictionary<DayOfWeek, ServiceDayConfigInput> |

*Type viene en la ruta*

#### Inyecta
- `Service.Create`
- `IValidator<ServiceSchedule>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Service no existe | 404 | NotFoundGuard | "Service of type '{Type}' not found" |

#### Lógica
```csharp
var existing = schedule.Services.FirstOrDefault(s => s.Type == type);
NotFoundGuard.ThrowIfNull(existing, $"Service of type '{type}' not found");

var updated = serviceCreate.Execute(new CreateServiceCommand(
    type,
    command.MaxCapacity,
    command.WeeklySchedule));

// Copiar SpecialDates del servicio existente
foreach (var specialDate in existing.SpecialDates)
{
    updated._specialDates.Add(specialDate);
}

schedule._services.Remove(existing);
schedule._services.Add(updated);

return scheduleValidator.ValidateOrThrow(schedule);
```

#### Slice: PUT /service-schedules/{id}/services/{type}

**Request**
```csharp
public record UpdateServiceRequest(
    int MaxCapacity,
    Dictionary<DayOfWeek, ServiceDayConfigInput> WeeklySchedule
);
```

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

✅ Actualizar capacidad de servicio
- Precondición: ServiceSchedule tiene Lunch con MaxCapacity=50
- Input: MaxCapacity=60
- Resultado: Service actualizado con MaxCapacity=60

✅ Actualizar horario semanal
- Precondición: ServiceSchedule tiene Lunch
- Input: WeeklySchedule con nuevos horarios
- Resultado: WeeklySchedule actualizado

✅ Mantiene SpecialDates existentes
- Precondición: Lunch tiene SpecialDate para San Valentín
- Input: MaxCapacity=60
- Resultado: SpecialDates se mantienen

❌ Service no existe
- Precondición: ServiceSchedule no tiene Breakfast
- Input: Type=Breakfast
- Resultado: NotFoundException "Service of type 'Breakfast' not found"

#### Tests Unitarios (Servicio)

✅ Obtiene el schedule del repositorio
- Verifica que repository.GetByIdAsync es llamado con el id correcto

✅ Llama a ServiceSchedule.UpdateService con los parámetros correctos
- Verifica que se invoca updateService.Execute con type y command correctos

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

#### Tests Integración

✅ 204 No Content

❌ 404 → ServiceSchedule o Service no encontrado

❌ 422 → Validación fallida

---

### 6.7 ServiceSchedule.RemoveService

#### Event Storming
```
🟡[Admin] → 🔵(RemoveService) → 🟤[[ServiceSchedule]] → 🟠<ServiceRemoved>
                                      │
                            🟣{ServiceExiste}
```

#### Input
*Type viene en la ruta*

#### Inyecta
- `IValidator<ServiceSchedule>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Service no existe | 404 | NotFoundGuard | "Service of type '{Type}' not found" |

#### Lógica
```csharp
var existing = schedule.Services.FirstOrDefault(s => s.Type == type);
NotFoundGuard.ThrowIfNull(existing, $"Service of type '{type}' not found");

schedule._services.Remove(existing);

return scheduleValidator.ValidateOrThrow(schedule);
```

#### Slice: DELETE /service-schedules/{id}/services/{type}

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

✅ Eliminar servicio existente
- Precondición: ServiceSchedule con Lunch y Dinner
- Input: Type=Lunch
- Resultado: Lunch eliminado, queda Dinner

✅ Eliminar último servicio
- Precondición: ServiceSchedule con solo Lunch
- Input: Type=Lunch
- Resultado: Services vacío

❌ Service no existe
- Precondición: ServiceSchedule sin Breakfast
- Input: Type=Breakfast
- Resultado: NotFoundException "Service of type 'Breakfast' not found"

#### Tests Unitarios (Servicio)

✅ Obtiene el schedule del repositorio
- Verifica que repository.GetByIdAsync es llamado con el id correcto

✅ Llama a ServiceSchedule.RemoveService con el type correcto
- Verifica que se invoca removeService.Execute con el type

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

#### Tests Integración

✅ 204 No Content

❌ 404 → ServiceSchedule o Service no encontrado

---

### 6.8 ServiceSchedule.ConfigureServiceDay

#### Event Storming
```
🟡[Admin] → 🔵(ConfigureServiceDay) → 🟤[[ServiceSchedule]] → 🟠<ServiceDayConfigured>
                                            │
                                  🟣{ServiceExiste}
```

#### Input

| Campo | Tipo |
|-------|------|
| IsAvailable | bool |
| StartTime | TimeOnly? |
| EndTime | TimeOnly? |
| CapacityOverride | int? |

*Type y Day vienen en la ruta*

#### Inyecta
- `ServiceDayConfig.Create`
- `IValidator<ServiceSchedule>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Service no existe | 404 | NotFoundGuard | "Service of type '{Type}' not found" |

#### Lógica
```csharp
var service = schedule.Services.FirstOrDefault(s => s.Type == type);
NotFoundGuard.ThrowIfNull(service, $"Service of type '{type}' not found");

var config = serviceDayConfigCreate.Execute(new CreateServiceDayConfigCommand(
    command.IsAvailable,
    command.StartTime,
    command.EndTime,
    command.CapacityOverride));

service._weeklySchedule[day] = config;

return scheduleValidator.ValidateOrThrow(schedule);
```

#### Slice: PUT /service-schedules/{id}/services/{type}/days/{day}

**Request**
```csharp
public record ConfigureServiceDayRequest(
    bool IsAvailable,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    int? CapacityOverride
);
```

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

✅ Configurar día específico
- Precondición: Lunch con horario estándar
- Input: Day=Friday, IsAvailable=true, StartTime=12:00, EndTime=17:00, CapacityOverride=60
- Resultado: Viernes con horario extendido y más capacidad

✅ Desactivar servicio en día específico
- Precondición: Lunch disponible todos los días
- Input: Day=Sunday, IsAvailable=false
- Resultado: Domingo no disponible

❌ Service no existe
- Input: Type=Breakfast (no existe)
- Resultado: NotFoundException "Service of type 'Breakfast' not found"

❌ Config inválida
- Input: IsAvailable=true, StartTime=null
- Resultado: ValidationException "Start time is required when service is available"

#### Tests Unitarios (Servicio)

✅ Obtiene el schedule del repositorio
- Verifica que repository.GetByIdAsync es llamado con el id correcto

✅ Llama a ServiceSchedule.ConfigureServiceDay con los parámetros correctos
- Verifica que se invoca configureServiceDay.Execute con type, day y command correctos

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

#### Tests Integración

✅ 204 No Content

❌ 404 → ServiceSchedule o Service no encontrado

❌ 422 → Validación fallida

---

### 6.9 ServiceSchedule.AddSpecialDate

#### Event Storming
```
🟡[Admin] → 🔵(AddSpecialDate) → 🟤[[ServiceSchedule]] → 🟠<SpecialDateAdded>
                                        │
                              🟣{ServiceExiste}
                              🟣{FechaÚnica}
```

#### Input

| Campo | Tipo |
|-------|------|
| Date | DateOnly |
| IsAvailable | bool |
| StartTime | TimeOnly? |
| EndTime | TimeOnly? |
| CapacityOverride | int? |
| Reason | string? |

*Type viene en la ruta*

#### Inyecta
- `ServiceSpecialDate.Create`
- `IValidator<ServiceSchedule>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Service no existe | 404 | NotFoundGuard | "Service of type '{Type}' not found" |
| Fecha ya existe | 409 | ConflictGuard | "Special date '{Date}' already exists for this service" |

#### Lógica
```csharp
var service = schedule.Services.FirstOrDefault(s => s.Type == type);
NotFoundGuard.ThrowIfNull(service, $"Service of type '{type}' not found");

ConflictGuard.ThrowIf(
    service.SpecialDates.Any(sd => sd.Date == command.Date),
    $"Special date '{command.Date}' already exists for this service");

var specialDate = specialDateCreate.Execute(new CreateServiceSpecialDateCommand(
    command.Date,
    command.IsAvailable,
    command.StartTime,
    command.EndTime,
    command.CapacityOverride,
    command.Reason));

service._specialDates.Add(specialDate);

return scheduleValidator.ValidateOrThrow(schedule);
```

#### Slice: POST /service-schedules/{id}/services/{type}/special-dates

**Request**
```csharp
public record AddSpecialDateRequest(
    DateOnly Date,
    bool IsAvailable,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    int? CapacityOverride,
    string? Reason
);
```

**Response**: 201 Created → `ServiceScheduleResponse`

#### Tests Unitarios (Dominio)

✅ Añadir fecha especial con horario extendido
- Precondición: Dinner existe
- Input: Date=2025-02-14, IsAvailable=true, StartTime=19:00, EndTime=02:00, Reason="San Valentín"
- Resultado: SpecialDate añadida

✅ Añadir fecha especial cerrada
- Input: Date=2025-01-01, IsAvailable=false, Reason="Año Nuevo"
- Resultado: SpecialDate añadida

✅ Añadir fecha especial con capacidad extra
- Input: Date=2025-12-31, CapacityOverride=70
- Resultado: SpecialDate añadida con capacidad override

❌ Service no existe
- Input: Type=Breakfast (no existe)
- Resultado: NotFoundException "Service of type 'Breakfast' not found"

❌ Fecha duplicada
- Precondición: Dinner ya tiene SpecialDate para 2025-02-14
- Input: Date=2025-02-14
- Resultado: ConflictException "Special date '2025-02-14' already exists for this service"

#### Tests Unitarios (Servicio)

✅ Obtiene el schedule del repositorio
- Verifica que repository.GetByIdAsync es llamado con el id correcto

✅ Llama a ServiceSchedule.AddSpecialDate con los parámetros correctos
- Verifica que se invoca addSpecialDate.Execute con type y command correctos

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

✅ Retorna Response mapeado correctamente
- Verifica que el Response contiene la SpecialDate añadida

#### Tests Integración

✅ 201 Created → ServiceScheduleResponse con SpecialDate añadida

❌ 404 → ServiceSchedule o Service no encontrado

❌ 409 → Fecha duplicada

❌ 422 → Validación fallida

---

### 6.10 ServiceSchedule.UpdateSpecialDate

#### Event Storming
```
🟡[Admin] → 🔵(UpdateSpecialDate) → 🟤[[ServiceSchedule]] → 🟠<SpecialDateUpdated>
                                          │
                                🟣{ServiceExiste}
                                🟣{FechaExiste}
```

#### Input

| Campo | Tipo |
|-------|------|
| IsAvailable | bool |
| StartTime | TimeOnly? |
| EndTime | TimeOnly? |
| CapacityOverride | int? |
| Reason | string? |

*Type y Date vienen en la ruta*

#### Inyecta
- `ServiceSpecialDate.Create`
- `IValidator<ServiceSchedule>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Service no existe | 404 | NotFoundGuard | "Service of type '{Type}' not found" |
| SpecialDate no existe | 404 | NotFoundGuard | "Special date '{Date}' not found for this service" |

#### Lógica
```csharp
var service = schedule.Services.FirstOrDefault(s => s.Type == type);
NotFoundGuard.ThrowIfNull(service, $"Service of type '{type}' not found");

var existing = service.SpecialDates.FirstOrDefault(sd => sd.Date == date);
NotFoundGuard.ThrowIfNull(existing, $"Special date '{date}' not found for this service");

var updated = specialDateCreate.Execute(new CreateServiceSpecialDateCommand(
    date,
    command.IsAvailable,
    command.StartTime,
    command.EndTime,
    command.CapacityOverride,
    command.Reason));

service._specialDates.Remove(existing);
service._specialDates.Add(updated);

return scheduleValidator.ValidateOrThrow(schedule);
```

#### Slice: PUT /service-schedules/{id}/services/{type}/special-dates/{date}

**Request**
```csharp
public record UpdateSpecialDateRequest(
    bool IsAvailable,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    int? CapacityOverride,
    string? Reason
);
```

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

✅ Actualizar horario de fecha especial
- Precondición: Dinner tiene SpecialDate para San Valentín
- Input: StartTime=18:00, EndTime=03:00
- Resultado: Horario actualizado

✅ Cambiar fecha especial de disponible a cerrada
- Precondición: SpecialDate con IsAvailable=true
- Input: IsAvailable=false, Reason="Cerrado por reformas"
- Resultado: Fecha ahora cerrada

❌ Service no existe
- Resultado: NotFoundException "Service of type 'Breakfast' not found"

❌ SpecialDate no existe
- Resultado: NotFoundException "Special date '2025-03-15' not found for this service"

#### Tests Unitarios (Servicio)

✅ Obtiene el schedule del repositorio
- Verifica que repository.GetByIdAsync es llamado con el id correcto

✅ Llama a ServiceSchedule.UpdateSpecialDate con los parámetros correctos
- Verifica que se invoca updateSpecialDate.Execute con type, date y command correctos

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

#### Tests Integración

✅ 204 No Content

❌ 404 → ServiceSchedule, Service o SpecialDate no encontrado

❌ 422 → Validación fallida

---

### 6.11 ServiceSchedule.RemoveSpecialDate

#### Event Storming
```
🟡[Admin] → 🔵(RemoveSpecialDate) → 🟤[[ServiceSchedule]] → 🟠<SpecialDateRemoved>
                                          │
                                🟣{ServiceExiste}
                                🟣{FechaExiste}
```

#### Input
*Type y Date vienen en la ruta*

#### Inyecta
- `IValidator<ServiceSchedule>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Service no existe | 404 | NotFoundGuard | "Service of type '{Type}' not found" |
| SpecialDate no existe | 404 | NotFoundGuard | "Special date '{Date}' not found for this service" |

#### Lógica
```csharp
var service = schedule.Services.FirstOrDefault(s => s.Type == type);
NotFoundGuard.ThrowIfNull(service, $"Service of type '{type}' not found");

var existing = service.SpecialDates.FirstOrDefault(sd => sd.Date == date);
NotFoundGuard.ThrowIfNull(existing, $"Special date '{date}' not found for this service");

service._specialDates.Remove(existing);

return scheduleValidator.ValidateOrThrow(schedule);
```

#### Slice: DELETE /service-schedules/{id}/services/{type}/special-dates/{date}

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

✅ Eliminar fecha especial existente
- Precondición: Dinner tiene SpecialDate para 2025-02-14
- Input: Date=2025-02-14
- Resultado: SpecialDate eliminada

❌ Service no existe
- Resultado: NotFoundException "Service of type 'Breakfast' not found"

❌ SpecialDate no existe
- Resultado: NotFoundException "Special date '2025-03-15' not found for this service"

#### Tests Unitarios (Servicio)

✅ Obtiene el schedule del repositorio
- Verifica que repository.GetByIdAsync es llamado con el id correcto

✅ Llama a ServiceSchedule.RemoveSpecialDate con los parámetros correctos
- Verifica que se invoca removeSpecialDate.Execute con type y date

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

#### Tests Integración

✅ 204 No Content

❌ 404 → ServiceSchedule, Service o SpecialDate no encontrado

---

### 6.12 ServiceSchedule.Activate

> ⚠️ **Dependencias**: Requiere que existan Services configurados.
> Por eso este comando va después de AddService.

#### Event Storming
```
🟡[Admin] → 🔵(ActivateServiceSchedule) → 🟤[[ServiceSchedule]] → 🟠<ServiceScheduleActivated>
                                                │
                                      🟣{TieneServices}
                                      🟣{SoloUnoActivoPorTenant}
```

#### Input
Ninguno

#### Inyecta
- `IServiceScheduleRepository`
- `IValidator<ServiceSchedule>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Ya está activo | 409 | ConflictGuard | "Service schedule is already active" |
| No tiene Services | 422 | ValidationGuard | "Service schedule must have at least one service" |
| Ya existe otro activo en el Tenant | 409 | ConflictGuard | "Another service schedule is already active. Deactivate it first." |

#### Lógica
```csharp
ConflictGuard.ThrowIf(schedule.IsActive, "Service schedule is already active");
ValidationGuard.ThrowIf(!schedule.HasServices, "Service schedule must have at least one service", nameof(schedule.Services));

var activeSchedule = await repository.GetActiveByTenantAsync(schedule.TenantId);
ConflictGuard.ThrowIf(activeSchedule != null && activeSchedule.Id != schedule.Id, 
    "Another service schedule is already active. Deactivate it first.");

schedule.IsActive = true;

return scheduleValidator.ValidateOrThrow(schedule);
```

#### Slice: POST /service-schedules/{id}/activate

**Response**: 200 OK → `ServiceScheduleResponse`

#### Tests Unitarios (Dominio)

✅ Activar schedule con services configurados
- Precondición: ServiceSchedule con Services, IsActive=false, ningún otro activo
- Resultado: ServiceSchedule con IsActive=true

❌ Schedule ya activo
- Precondición: ServiceSchedule con IsActive=true
- Resultado: ConflictException "Service schedule is already active"

❌ Schedule sin Services
- Precondición: ServiceSchedule sin Services
- Resultado: ValidationException "Service schedule must have at least one service"

❌ Ya existe otro activo
- Precondición: Otro ServiceSchedule del mismo Tenant está activo
- Resultado: ConflictException "Another service schedule is already active. Deactivate it first."

#### Tests Unitarios (Servicio)

✅ Obtiene el schedule del repositorio
- Verifica que repository.GetByIdAsync es llamado con el id correcto

✅ Verifica que no hay otro schedule activo
- Verifica que repository.GetActiveByTenantAsync es llamado

✅ Llama a ServiceSchedule.Activate
- Verifica que se invoca activate.Execute

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

✅ Retorna Response mapeado correctamente
- Verifica que el Response contiene IsActive=true

#### Tests Integración

✅ 200 OK → ServiceScheduleResponse con IsActive=true

❌ 404 → ServiceSchedule no encontrado

❌ 409 → Ya estaba activo

❌ 409 → Ya existe otro activo

❌ 422 → Falta Services

---

### 6.13 ServiceSchedule.Deactivate

#### Event Storming
```
🟡[Admin] → 🔵(DeactivateServiceSchedule) → 🟤[[ServiceSchedule]] → 🟠<ServiceScheduleDeactivated>
```

#### Input
Ninguno

#### Inyecta
- `IValidator<ServiceSchedule>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Ya está inactivo | 409 | ConflictGuard | "Service schedule is already inactive" |

#### Lógica
```csharp
ConflictGuard.ThrowIf(!schedule.IsActive, "Service schedule is already inactive");

schedule.IsActive = false;

return scheduleValidator.ValidateOrThrow(schedule);
```

#### Slice: POST /service-schedules/{id}/deactivate

**Response**: 200 OK → `ServiceScheduleResponse`

#### Tests Unitarios (Dominio)

✅ Desactivar schedule activo
- Precondición: ServiceSchedule con IsActive=true
- Resultado: ServiceSchedule con IsActive=false

❌ Schedule ya inactivo
- Precondición: ServiceSchedule con IsActive=false
- Resultado: ConflictException "Service schedule is already inactive"

#### Tests Unitarios (Servicio)

✅ Obtiene el schedule del repositorio
- Verifica que repository.GetByIdAsync es llamado con el id correcto

✅ Llama a ServiceSchedule.Deactivate
- Verifica que se invoca deactivate.Execute

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

✅ Retorna Response mapeado correctamente
- Verifica que el Response contiene IsActive=false

#### Tests Integración

✅ 200 OK → ServiceScheduleResponse con IsActive=false

❌ 404 → ServiceSchedule no encontrado

❌ 409 → Ya estaba inactivo

---

## 7. Queries

### CanReserve

**Slice**: GET /service-schedules/{id}/can-reserve?type=Lunch&dateTime=2025-02-14T13:00&partySize=4

**Response**: 200 OK → `CanReserveResponse`

```csharp
public record CanReserveResponse(
    bool CanReserve,
    string? Reason
);
```

**Lógica**:
1. Verificar servicio existe y disponible en esa fecha
2. Verificar DateTime en ventana de antelación (MinAdvance < X < MaxAdvance)
3. Verificar PartySize válido (MinPartySize <= X <= MaxPartySize)
4. Verificar slot válido (múltiplo de SlotInterval)
5. Verificar tiempo suficiente para duración del servicio

**Nota**: Esta query es la "fuente de verdad" para validar reservas. Evita que cada cliente tenga que implementar la lógica de validación.

#### Tests Integración

✅ 200 OK → CanReserve=true

✅ 200 OK → CanReserve=false, Reason="Service is not available on this date"

✅ 200 OK → CanReserve=false, Reason="Must book at least 2 hours in advance"

✅ 200 OK → CanReserve=false, Reason="Maximum 8 guests per reservation"

✅ 200 OK → CanReserve=false, Reason="Reservations only accepted at :00, :15, :30, :45"

❌ 404 → ServiceSchedule no encontrado

---

## 8. Resumen de Endpoints

| Método | Ruta | Comando/Query | Response |
|--------|------|---------------|----------|
| POST | /service-schedules | ServiceSchedule.Create | 201 → `ServiceScheduleResponse` |
| GET | /service-schedules/{id} | GetServiceSchedule | 200 → `ServiceScheduleResponse` |
| GET | /service-schedules | ListServiceSchedules | 200 → `ServiceScheduleResponse[]` |
| PUT | /service-schedules/{id} | ServiceSchedule.Update | 204 |
| POST | /service-schedules/{id}/services | ServiceSchedule.AddService | 201 → `ServiceScheduleResponse` |
| PUT | /service-schedules/{id}/services/{type} | ServiceSchedule.UpdateService | 204 |
| DELETE | /service-schedules/{id}/services/{type} | ServiceSchedule.RemoveService | 204 |
| PUT | /service-schedules/{id}/services/{type}/days/{day} | ServiceSchedule.ConfigureServiceDay | 204 |
| POST | /service-schedules/{id}/services/{type}/special-dates | ServiceSchedule.AddSpecialDate | 201 → `ServiceScheduleResponse` |
| PUT | /service-schedules/{id}/services/{type}/special-dates/{date} | ServiceSchedule.UpdateSpecialDate | 204 |
| DELETE | /service-schedules/{id}/services/{type}/special-dates/{date} | ServiceSchedule.RemoveSpecialDate | 204 |
| POST | /service-schedules/{id}/activate | ServiceSchedule.Activate | 200 → `ServiceScheduleResponse` |
| POST | /service-schedules/{id}/deactivate | ServiceSchedule.Deactivate | 200 → `ServiceScheduleResponse` |
| GET | /service-schedules/{id}/can-reserve | CanReserve | 200 → `CanReserveResponse` |

---

## 9. Persistencia (Firestore)

### Colección

`/service-schedules/{scheduleId}`

### Configuración DbContext

```csharp
modelBuilder.Entity<ServiceScheduleAgg>(entity =>
{
    // Ignore: propiedades computed (no backing fields)
    entity.Ignore(s => s.HasServices);
    entity.Ignore(s => s.ServiceCount);
    entity.Ignore(s => s.AvailableServiceTypes);

    // ComplexType: Policy
    entity.ComplexProperty(s => s.Policy, policy =>
    {
        // Ignore: propiedades computed de Policy
        policy.Ignore(p => p.SlotIntervalMinutes);
        policy.Ignore(p => p.MaxAdvanceDays);

        // MapOf: StandardDurations
        policy.MapOf(p => p.StandardDurations);
    });

    // ArrayOf: Services (usa backing field _services)
    entity.ArrayOf(s => s.Services, service =>
    {
        // Ignore: propiedades computed de Service
        service.Ignore(srv => srv.HasSpecialDates);
        service.Ignore(srv => srv.AvailableDaysCount);

        // MapOf: WeeklySchedule
        service.MapOf(srv => srv.WeeklySchedule, dayConfig =>
        {
            dayConfig.Ignore(dc => dc.Duration);
        });

        // ArrayOf: SpecialDates
        service.ArrayOf(srv => srv.SpecialDates);
    });
});
```

### Documento Ejemplo

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "tenantId": "tenant-001-guid",
  "name": "Horario Verano",
  "description": "Junio a Septiembre - Horarios adaptados al calor",
  "isActive": true,
  "policy": {
    "minimumAdvanceTime": "02:00:00",
    "maximumAdvanceTime": "30.00:00:00",
    "slotInterval": "00:15:00",
    "bufferBetweenReservations": "00:15:00",
    "maxPartySize": 8,
    "minPartySize": 1,
    "standardDurations": {
      "Breakfast": "01:00:00",
      "Lunch": "01:30:00",
      "Dinner": "02:00:00"
    }
  },
  "services": [
    {
      "type": "Lunch",
      "maxCapacity": 50,
      "weeklySchedule": {
        "Monday": {
          "isAvailable": true,
          "startTime": "13:00:00",
          "endTime": "16:00:00",
          "capacityOverride": null
        },
        "Tuesday": {
          "isAvailable": true,
          "startTime": "13:00:00",
          "endTime": "16:00:00",
          "capacityOverride": null
        },
        "Friday": {
          "isAvailable": true,
          "startTime": "12:00:00",
          "endTime": "17:00:00",
          "capacityOverride": 60
        },
        "Sunday": {
          "isAvailable": false,
          "startTime": null,
          "endTime": null,
          "capacityOverride": null
        }
      },
      "specialDates": [
        {
          "date": "2025-02-14",
          "isAvailable": true,
          "startTime": "12:00:00",
          "endTime": "18:00:00",
          "capacityOverride": 70,
          "reason": "San Valentín - horario extendido"
        },
        {
          "date": "2025-01-01",
          "isAvailable": false,
          "startTime": null,
          "endTime": null,
          "capacityOverride": null,
          "reason": "Año Nuevo"
        }
      ]
    },
    {
      "type": "Dinner",
      "maxCapacity": 40,
      "weeklySchedule": {
        "Monday": {
          "isAvailable": true,
          "startTime": "20:00:00",
          "endTime": "23:00:00",
          "capacityOverride": null
        }
      },
      "specialDates": []
    }
  ]
}
```

---

## 10. Hot Spots ⚠️

| # | Pregunta | Estado |
|---|----------|--------|
| 1 | ¿Los horarios de servicio deben validarse contra el horario de apertura del restaurante (Schedule)? | Decidido: Validación en Application Layer, no en Domain |
| 2 | ¿Cómo se maneja la disponibilidad real (capacidad ocupada)? | Decidido: Query CanReserve consulta Reservations, no es responsabilidad de ServiceSchedule |
| 3 | ¿Se pueden solapar horarios de diferentes servicios? | Decidido: Permitido, responsabilidad del admin |
| 4 | ¿Qué pasa si se elimina un servicio con reservas existentes? | Pendiente: Validación cross-aggregate |
| 5 | ¿TenantId se expone en URLs? | Decidido: No, viene del contexto de autenticación |
| 6 | ¿Queries de slots disponibles? | Decidido: No, datos ya están en Response. Solo CanReserve como fuente de verdad |
| 7 | ¿Cuántos ServiceSchedules puede tener un Tenant? | Decidido: Múltiples, pero solo UNO activo a la vez |
| 8 | ¿Se puede activar un schedule si ya hay otro activo? | Decidido: No, debe desactivar el actual primero (guard de conflicto) |

---

**Fecha**: 2025-01-29
**Autor**: Equipo Fudie
