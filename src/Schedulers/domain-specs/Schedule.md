# Domain Specification: Schedule

---

## 1. Enums

*No hay enums específicos para este agregado. Se usa `DayOfWeek` de System.*

---

## 2. Value Objects

### 2.1 TimeSlot

#### Estructura

| Propiedad | Tipo |
|-----------|------|
| OpenTime | TimeOnly |
| CloseTime | TimeOnly |

#### Validaciones

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| OpenTime | NotEmpty | "Open time is required" |
| CloseTime | NotEmpty | "Close time is required" |
| CloseTime | GreaterThan(OpenTime) | "Close time must be after open time" |

#### Propiedades Calculadas

| Propiedad | Tipo | Fórmula |
|-----------|------|---------|
| Duration | TimeSpan | `CloseTime - OpenTime` |

#### Métodos

- `Contains(TimeOnly time)` → bool: `time >= OpenTime && time <= CloseTime`
- `OverlapsWith(TimeSlot other)` → bool: `OpenTime < other.CloseTime && CloseTime > other.OpenTime`

#### Comando: TimeSlot.Create

**Input**

| Campo | Tipo |
|-------|------|
| OpenTime | TimeOnly |
| CloseTime | TimeOnly |

**Inyecta**: `IValidator<TimeSlot>`

**Lógica**
```csharp
var timeSlot = new TimeSlot(command.OpenTime, command.CloseTime);

return timeSlotValidator.ValidateOrThrow(timeSlot);
```

#### Tests Unitarios

✅ TimeSlot válido
- Input: OpenTime=13:00, CloseTime=23:00
- Resultado: TimeSlot creado, Duration=10h

✅ TimeSlot de turno corto
- Input: OpenTime=13:00, CloseTime=16:00
- Resultado: TimeSlot creado, Duration=3h

✅ TimeSlot 24h (máximo permitido)
- Input: OpenTime=00:00, CloseTime=23:59
- Resultado: TimeSlot creado

✅ Contains devuelve true para hora dentro del rango
- Input: TimeSlot(13:00, 23:00), time=15:00
- Resultado: Contains(15:00)=true

✅ Contains devuelve false para hora fuera del rango
- Input: TimeSlot(13:00, 23:00), time=11:00
- Resultado: Contains(11:00)=false

✅ OverlapsWith detecta solapamiento
- Input: Slot1(13:00, 16:00), Slot2(15:00, 18:00)
- Resultado: Slot1.OverlapsWith(Slot2)=true

✅ OverlapsWith detecta no solapamiento
- Input: Slot1(13:00, 16:00), Slot2(20:00, 23:00)
- Resultado: Slot1.OverlapsWith(Slot2)=false

❌ CloseTime antes de OpenTime
- Input: OpenTime=23:00, CloseTime=13:00
- Resultado: ValidationException "Close time must be after open time"

❌ CloseTime igual a OpenTime
- Input: OpenTime=13:00, CloseTime=13:00
- Resultado: ValidationException "Close time must be after open time"

---

### 2.2 DaySchedule

#### Estructura

| Propiedad | Tipo |
|-----------|------|
| DayOfWeek | DayOfWeek |
| IsClosed | bool |
| TimeSlots | IReadOnlyCollection<TimeSlot> |

#### Validaciones

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| DayOfWeek | IsEnum | "Invalid day of week" |
| TimeSlots | Empty when IsClosed=true | "Closed day cannot have time slots" |
| TimeSlots | NotEmpty when IsClosed=false | "Open day must have at least one time slot" |
| TimeSlots | NoOverlapping | "Time slots cannot overlap" |

#### Propiedades Calculadas

| Propiedad | Tipo | Fórmula |
|-----------|------|---------|
| TotalOpenHours | TimeSpan | `TimeSlots.Sum(ts => ts.Duration)` |

#### Métodos

- `IsOpenAt(TimeOnly time)` → bool: `!IsClosed && TimeSlots.Any(ts => ts.Contains(time))`

#### Comando: DaySchedule.Create

**Input**

| Campo | Tipo |
|-------|------|
| DayOfWeek | DayOfWeek |
| IsClosed | bool |
| TimeSlots | TimeSlot[] |

**Inyecta**: `IValidator<DaySchedule>`

**Lógica**
```csharp
var daySchedule = new DaySchedule(command.DayOfWeek, command.IsClosed, command.TimeSlots);

return dayScheduleValidator.ValidateOrThrow(daySchedule);
```

#### Tests Unitarios

✅ DaySchedule abierto con un turno
- Input: DayOfWeek=Monday, IsClosed=false, TimeSlots=[{13:00, 23:00}]
- Resultado: DaySchedule creado

✅ DaySchedule abierto con múltiples turnos (comida y cena)
- Input: DayOfWeek=Tuesday, IsClosed=false, TimeSlots=[{13:00, 16:00}, {20:00, 23:00}]
- Resultado: DaySchedule creado, TotalOpenHours=6h

✅ DaySchedule cerrado
- Input: DayOfWeek=Sunday, IsClosed=true, TimeSlots=[]
- Resultado: DaySchedule creado con IsClosed=true

✅ IsOpenAt devuelve true dentro de horario
- Precondición: DaySchedule(Monday, false, [{13:00, 23:00}])
- Input: time=15:00
- Resultado: IsOpenAt(15:00)=true

✅ IsOpenAt devuelve false fuera de horario
- Precondición: DaySchedule(Monday, false, [{13:00, 23:00}])
- Input: time=11:00
- Resultado: IsOpenAt(11:00)=false

✅ IsOpenAt devuelve false entre turnos
- Precondición: DaySchedule(Monday, false, [{13:00, 16:00}, {20:00, 23:00}])
- Input: time=18:00
- Resultado: IsOpenAt(18:00)=false

❌ Día cerrado con TimeSlots
- Input: IsClosed=true, TimeSlots=[{13:00, 23:00}]
- Resultado: ValidationException "Closed day cannot have time slots"

❌ Día abierto sin TimeSlots
- Input: IsClosed=false, TimeSlots=[]
- Resultado: ValidationException "Open day must have at least one time slot"

❌ TimeSlots solapados
- Input: TimeSlots=[{13:00, 16:00}, {15:00, 18:00}]
- Resultado: ValidationException "Time slots cannot overlap"

---

### 2.3 SpecialDate

#### Estructura

| Propiedad | Tipo |
|-----------|------|
| Date | DateOnly |
| IsClosed | bool |
| Reason | string |
| TimeSlots | IReadOnlyCollection<TimeSlot> |

#### Validaciones

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| Date | NotEmpty | "Date is required" |
| Reason | NotEmpty | "Reason is required" |
| Reason | Max(200) | "Reason cannot exceed 200 characters" |
| TimeSlots | Empty when IsClosed=true | "Closed date cannot have time slots" |
| TimeSlots | NotEmpty when IsClosed=false | "Open date must have at least one time slot" |
| TimeSlots | NoOverlapping | "Time slots cannot overlap" |

#### Propiedades Calculadas

| Propiedad | Tipo | Fórmula |
|-----------|------|---------|
| TotalOpenHours | TimeSpan | `IsClosed ? TimeSpan.Zero : TimeSlots.Sum(ts => ts.Duration)` |

#### Métodos

- `IsOpenAt(TimeOnly time)` → bool: `!IsClosed && TimeSlots.Any(ts => ts.Contains(time))`

#### Comando: SpecialDate.Create

**Input**

| Campo | Tipo |
|-------|------|
| Date | DateOnly |
| IsClosed | bool |
| Reason | string |
| TimeSlots | TimeSlot[] |

**Inyecta**: `IValidator<SpecialDate>`

**Lógica**
```csharp
var specialDate = new SpecialDate(command.Date, command.IsClosed, command.Reason, command.TimeSlots);

return specialDateValidator.ValidateOrThrow(specialDate);
```

#### Tests Unitarios

✅ SpecialDate cerrado (festivo)
- Input: Date=2025-12-25, IsClosed=true, Reason="Navidad", TimeSlots=[]
- Resultado: SpecialDate creado

✅ SpecialDate con horario especial
- Input: Date=2025-02-14, IsClosed=false, Reason="San Valentín", TimeSlots=[{13:00, 02:00}]
- Resultado: SpecialDate creado

✅ SpecialDate de vacaciones
- Input: Date=2025-08-15, IsClosed=true, Reason="Vacaciones de verano", TimeSlots=[]
- Resultado: SpecialDate creado

❌ Reason vacío
- Input: Reason=""
- Resultado: ValidationException "Reason is required"

❌ Reason demasiado largo
- Input: Reason=(201 caracteres)
- Resultado: ValidationException "Reason cannot exceed 200 characters"

❌ Fecha cerrada con TimeSlots
- Input: IsClosed=true, TimeSlots=[{13:00, 23:00}]
- Resultado: ValidationException "Closed date cannot have time slots"

❌ Fecha abierta sin TimeSlots
- Input: IsClosed=false, TimeSlots=[]
- Resultado: ValidationException "Open date must have at least one time slot"

---

## 3. Aggregate: Schedule

### Estructura

```
Schedule (Aggregate Root)
├─ Id: Guid
├─ TenantId: Guid
├─ Name: string
├─ Description: string?
├─ IsActive: bool
├─ WeeklyHours: IReadOnlyDictionary<DayOfWeek, DaySchedule>
└─ SpecialDates: IReadOnlyCollection<SpecialDate>
```

#### Propiedades

| Propiedad | Tipo | Modificador |
|-----------|------|-------------|
| Id | Guid | init |
| TenantId | Guid | protected set |
| Name | string | protected set |
| Description | string? | protected set |
| IsActive | bool | protected set |

#### Colecciones

```csharp
protected Dictionary<DayOfWeek, DaySchedule> _weeklyHours = [];
public IReadOnlyDictionary<DayOfWeek, DaySchedule> WeeklyHours => _weeklyHours.AsReadOnly();

protected HashSet<SpecialDate> _specialDates = [];
public IReadOnlyCollection<SpecialDate> SpecialDates => _specialDates.ToList().AsReadOnly();
```

#### Propiedades Calculadas

| Propiedad | Tipo | Fórmula |
|-----------|------|---------|
| HasWeeklyHours | bool | `_weeklyHours.Any()` |
| HasSpecialDates | bool | `_specialDates.Any()` |
| IsFullyConfigured | bool | `_weeklyHours.Count == 7` |

### Validaciones

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| Id | NotEmpty | "Id is required" |
| TenantId | NotEmpty | "Tenant id is required" |
| Name | NotEmpty | "Name is required" |
| Name | Max(100) | "Name cannot exceed 100 characters" |
| Description | Max(500) | "Description cannot exceed 500 characters" |

---

## 4. Response

```csharp
public record ScheduleResponse(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    bool HasWeeklyHours,
    bool HasSpecialDates,
    bool IsFullyConfigured,
    IReadOnlyDictionary<DayOfWeek, DayScheduleResponse> WeeklyHours,
    IReadOnlyCollection<SpecialDateResponse> SpecialDates
);

public record DayScheduleResponse(
    DayOfWeek DayOfWeek,
    bool IsClosed,
    TimeSpan TotalOpenHours,
    IReadOnlyCollection<TimeSlotResponse> TimeSlots
);

public record SpecialDateResponse(
    DateOnly Date,
    bool IsClosed,
    string Reason,
    TimeSpan TotalOpenHours,
    IReadOnlyCollection<TimeSlotResponse> TimeSlots
);

public record TimeSlotResponse(
    TimeOnly OpenTime,
    TimeOnly CloseTime,
    TimeSpan Duration
);

public record IsOpenResponse(
    bool IsOpen,
    DateOnly Date,
    TimeOnly Time,
    bool IsSpecialDate,
    string? SpecialDateReason
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

---

### 6.1 Schedule.Create

#### Event Storming
```
🟡[Admin] → 🔵(CreateSchedule) → 🟤[[Schedule]] → 🟠<ScheduleCreated>
```

#### Input

| Campo | Tipo |
|-------|------|
| TenantId | Guid |
| Name | string |
| Description | string? |

#### Inyecta
- `IValidator<Schedule>`

#### Guards
Ninguno.

#### Lógica
```csharp
var schedule = new Schedule(Guid.NewGuid())
{
    TenantId = command.TenantId,
    Name = command.Name,
    Description = command.Description,
    IsActive = false
};

return scheduleValidator.ValidateOrThrow(schedule);
```

#### Slice: POST /schedules

**Request**
```csharp
public record CreateScheduleRequest(
    string Name,
    string? Description
);
```

**Response**: 201 Created → `ScheduleResponse`

#### Tests Unitarios (Dominio)

✅ Crear schedule con datos válidos
- Input: TenantId=valid-guid, Name="Horario de Verano", Description="Del 15 junio al 15 septiembre"
- Resultado: Schedule creado con IsActive=false, WeeklyHours vacío, SpecialDates vacío

✅ Crear schedule sin descripción
- Input: TenantId=valid-guid, Name="Horario de Invierno", Description=null
- Resultado: Schedule creado

❌ TenantId vacío
- Input: TenantId=Guid.Empty
- Resultado: ValidationException "Tenant id is required"

❌ Name vacío
- Input: Name=""
- Resultado: ValidationException "Name is required"

❌ Name demasiado largo
- Input: Name=(101 caracteres)
- Resultado: ValidationException "Name cannot exceed 100 characters"

#### Tests Unitarios (Servicio)

✅ Llama a Schedule.Create con los parámetros correctos
- Verifica que se invoca scheduleCreate.Execute con el command correcto

✅ Añade el schedule al repositorio
- Verifica que repository.Add es llamado con el schedule creado

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

✅ Retorna Response mapeado correctamente
- Verifica que el Response contiene los datos del schedule

#### Tests Integración

✅ 201 Created → ScheduleResponse con WeeklyHours vacío

❌ 422 → Validación fallida

---

### 6.2 GetSchedule

#### Event Storming
```
🟡[Admin] → 🔵(GetSchedule) → 🟤[[Schedule]] → 📊 ScheduleResponse
```

#### Slice: GET /schedules/{id}

**Response**: 200 OK → `ScheduleResponse`

#### Tests Unitarios (Servicio)

✅ Obtiene el schedule del repositorio con el id correcto
- Verifica que repository.GetByIdAsync es llamado con el id

✅ Retorna Response mapeado correctamente
- Verifica que el Response contiene los datos del schedule

#### Tests Integración

✅ 200 OK → ScheduleResponse

❌ 404 → No encontrado

---

### 6.3 ListSchedules

#### Event Storming
```
🟡[Admin] → 🔵(ListSchedules) → 🟤[[Schedule]] → 📊 ScheduleResponse[]
```

#### Slice: GET /schedules

**Response**: 200 OK → `ScheduleResponse[]`

#### Tests Unitarios (Servicio)

✅ Retorna lista de schedules mapeados correctamente
- Verifica que el Response contiene los datos de los schedules

#### Tests Integración

✅ 200 OK → Array de ScheduleResponse

✅ 200 OK → Array vacío si no hay schedules

---

### 6.4 Schedule.Update

#### Event Storming
```
🟡[Admin] → 🔵(UpdateSchedule) → 🟤[[Schedule]] → 🟠<ScheduleUpdated>
```

#### Input

| Campo | Tipo |
|-------|------|
| Name | string |
| Description | string? |

#### Inyecta
- `IValidator<Schedule>`

#### Guards
Ninguno.

#### Lógica
```csharp
schedule.Name = command.Name;
schedule.Description = command.Description;

return scheduleValidator.ValidateOrThrow(schedule);
```

#### Slice: PUT /schedules/{id}

**Request**
```csharp
public record UpdateScheduleRequest(
    string Name,
    string? Description
);
```

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

✅ Actualizar schedule existente
- Precondición: Schedule existe
- Input: Name="Horario de Verano Actualizado", Description="Nueva descripción"
- Resultado: Schedule actualizado

❌ Name vacío
- Input: Name=""
- Resultado: ValidationException "Name is required"

#### Tests Unitarios (Servicio)

✅ Obtiene el schedule del repositorio
- Verifica que repository.GetByIdAsync es llamado con el id correcto

✅ Llama a Schedule.Update con los parámetros correctos
- Verifica que se invoca scheduleUpdate.Execute con el command correcto

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

#### Tests Integración

✅ 204 No Content

❌ 404 → Schedule no encontrado

❌ 422 → Validación fallida

---

### 6.5 Schedule.Activate

#### Event Storming
```
🟡[Admin] → 🔵(ActivateSchedule) → 🟤[[Schedule]] → 🟠<ScheduleActivated>
                                        │
                              🟣{TieneWeeklyHours}
```

#### Input
Ninguno

#### Inyecta
- `IScheduleRepository`
- `IValidator<Schedule>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Ya está activo | 409 | ConflictGuard | "Schedule is already active" |
| No tiene WeeklyHours | 422 | ValidationGuard | "Schedule must have at least one day configured" |

#### Lógica
```csharp
ConflictGuard.ThrowIf(schedule.IsActive, "Schedule is already active");
ValidationGuard.ThrowIf(!schedule.HasWeeklyHours, "Schedule must have at least one day configured", nameof(schedule.WeeklyHours));

// Desactivar el schedule activo actual (si existe)
var currentActive = await scheduleRepository.GetActiveAsync();
if (currentActive != null && currentActive.Id != schedule.Id)
{
    currentActive.IsActive = false;
}

schedule.IsActive = true;

return scheduleValidator.ValidateOrThrow(schedule);
```

#### Slice: POST /schedules/{id}/activate

**Response**: 200 OK → `ScheduleResponse`

#### Tests Unitarios (Dominio)

✅ Activar schedule con WeeklyHours configurados
- Precondición: Schedule con al menos un día configurado, IsActive=false
- Resultado: Schedule con IsActive=true

✅ Activar schedule desactiva el anterior
- Precondición: Otro Schedule está activo
- Resultado: Anterior con IsActive=false, nuevo con IsActive=true

❌ Schedule ya activo
- Precondición: Schedule con IsActive=true
- Resultado: ConflictException "Schedule is already active"

❌ Schedule sin WeeklyHours
- Precondición: Schedule sin ningún día configurado
- Resultado: ValidationException "Schedule must have at least one day configured"

#### Tests Unitarios (Servicio)

✅ Obtiene el schedule del repositorio
- Verifica que repository.GetByIdAsync es llamado con el id correcto

✅ Llama a Schedule.Activate
- Verifica que se invoca scheduleActivate.Execute

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

✅ Retorna Response mapeado correctamente
- Verifica que el Response contiene IsActive=true

#### Tests Integración

✅ 200 OK → ScheduleResponse con IsActive=true

❌ 404 → Schedule no encontrado

❌ 409 → Ya estaba activo

❌ 422 → Falta WeeklyHours

---

### 6.6 Schedule.Deactivate

#### Event Storming
```
🟡[Admin] → 🔵(DeactivateSchedule) → 🟤[[Schedule]] → 🟠<ScheduleDeactivated>
```

#### Input
Ninguno

#### Inyecta
- `IValidator<Schedule>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Ya está inactivo | 409 | ConflictGuard | "Schedule is already inactive" |

#### Lógica
```csharp
ConflictGuard.ThrowIf(!schedule.IsActive, "Schedule is already inactive");

schedule.IsActive = false;

return scheduleValidator.ValidateOrThrow(schedule);
```

#### Slice: POST /schedules/{id}/deactivate

**Response**: 200 OK → `ScheduleResponse`

#### Tests Unitarios (Dominio)

✅ Desactivar schedule activo
- Precondición: Schedule con IsActive=true
- Resultado: Schedule con IsActive=false

❌ Schedule ya inactivo
- Precondición: Schedule con IsActive=false
- Resultado: ConflictException "Schedule is already inactive"

#### Tests Unitarios (Servicio)

✅ Obtiene el schedule del repositorio
- Verifica que repository.GetByIdAsync es llamado con el id correcto

✅ Llama a Schedule.Deactivate
- Verifica que se invoca scheduleDeactivate.Execute

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

✅ Retorna Response mapeado correctamente
- Verifica que el Response contiene IsActive=false

#### Tests Integración

✅ 200 OK → ScheduleResponse con IsActive=false

❌ 404 → Schedule no encontrado

❌ 409 → Ya estaba inactivo

---

### 6.7 Schedule.SetWeeklyHours

#### Event Storming
```
🟡[Admin] → 🔵(SetWeeklyHours) → 🟤[[Schedule]] → 🟠<WeeklyHoursUpdated>
                                      │
                            🟣{TimeSlotsNoSolapan}
```

#### Input

| Campo | Tipo |
|-------|------|
| DayOfWeek | DayOfWeek |
| IsClosed | bool |
| TimeSlots | CreateTimeSlotCommand[] |

#### Inyecta
- `TimeSlot.Create`
- `DaySchedule.Create`
- `IValidator<Schedule>`

#### Guards
Ninguno.

#### Lógica
```csharp
var timeSlots = command.TimeSlots
    .Select(ts => timeSlotCreate.Execute(ts))
    .ToList();

var daySchedule = dayScheduleCreate.Execute(new CreateDayScheduleCommand(
    command.DayOfWeek,
    command.IsClosed,
    timeSlots));

schedule._weeklyHours[command.DayOfWeek] = daySchedule;

return scheduleValidator.ValidateOrThrow(schedule);
```

#### Slice: PUT /schedules/{id}/weekly-hours/{dayOfWeek}

**Request**
```csharp
public record SetWeeklyHoursRequest(
    bool IsClosed,
    SetTimeSlotRequest[] TimeSlots
);

public record SetTimeSlotRequest(
    TimeOnly OpenTime,
    TimeOnly CloseTime
);
```

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

✅ Configurar día con un turno
- Precondición: Schedule existe
- Input: DayOfWeek=Monday, IsClosed=false, TimeSlots=[{13:00, 23:00}]
- Resultado: WeeklyHours[Monday] configurado

✅ Configurar día con múltiples turnos
- Input: DayOfWeek=Tuesday, IsClosed=false, TimeSlots=[{13:00, 16:00}, {20:00, 23:00}]
- Resultado: WeeklyHours[Tuesday] con 2 TimeSlots

✅ Sobrescribir configuración existente
- Precondición: Schedule con Monday configurado
- Input: DayOfWeek=Monday, IsClosed=false, TimeSlots=[{12:00, 22:00}]
- Resultado: WeeklyHours[Monday] actualizado

❌ TimeSlots solapados
- Input: TimeSlots=[{13:00, 16:00}, {15:00, 18:00}]
- Resultado: ValidationException "Time slots cannot overlap"

❌ CloseTime antes de OpenTime
- Input: TimeSlots=[{23:00, 13:00}]
- Resultado: ValidationException "Close time must be after open time"

#### Tests Unitarios (Servicio)

✅ Obtiene el schedule del repositorio
- Verifica que repository.GetByIdAsync es llamado con el id correcto

✅ Llama a Schedule.SetWeeklyHours con los parámetros correctos
- Verifica que se invoca setWeeklyHours.Execute con el command correcto

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

#### Tests Integración

✅ 204 No Content

❌ 404 → Schedule no encontrado

❌ 422 → Validación fallida

---

### 6.8 Schedule.MarkDayAsClosed

#### Event Storming
```
🟡[Admin] → 🔵(MarkDayAsClosed) → 🟤[[Schedule]] → 🟠<DayMarkedAsClosed>
```

#### Input

| Campo | Tipo |
|-------|------|
| DayOfWeek | DayOfWeek |

#### Inyecta
- `DaySchedule.Create`
- `IValidator<Schedule>`

#### Guards
Ninguno.

#### Lógica
```csharp
var daySchedule = dayScheduleCreate.Execute(new CreateDayScheduleCommand(
    command.DayOfWeek,
    isClosed: true,
    timeSlots: []));

schedule._weeklyHours[command.DayOfWeek] = daySchedule;

return scheduleValidator.ValidateOrThrow(schedule);
```

#### Slice: POST /schedules/{id}/weekly-hours/{dayOfWeek}/close

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

✅ Marcar día como cerrado
- Precondición: Schedule existe
- Input: DayOfWeek=Sunday
- Resultado: WeeklyHours[Sunday].IsClosed=true, TimeSlots=[]

✅ Marcar día previamente abierto como cerrado
- Precondición: Schedule con Monday abierto [{13:00, 23:00}]
- Input: DayOfWeek=Monday
- Resultado: WeeklyHours[Monday].IsClosed=true, TimeSlots=[]

#### Tests Unitarios (Servicio)

✅ Obtiene el schedule del repositorio
- Verifica que repository.GetByIdAsync es llamado con el id correcto

✅ Llama a Schedule.MarkDayAsClosed con los parámetros correctos
- Verifica que se invoca markDayAsClosed.Execute con el command correcto

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

#### Tests Integración

✅ 204 No Content

❌ 404 → Schedule no encontrado

---

### 6.9 Schedule.AddSpecialDate

#### Event Storming
```
🟡[Admin] → 🔵(AddSpecialDate) → 🟤[[Schedule]] → 🟠<SpecialDateAdded>
                                      │
                            🟣{FechaÚnica}
```

#### Input

| Campo | Tipo |
|-------|------|
| Date | DateOnly |
| IsClosed | bool |
| Reason | string |
| TimeSlots | CreateTimeSlotCommand[] |

#### Inyecta
- `TimeSlot.Create`
- `SpecialDate.Create`
- `IValidator<Schedule>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Fecha ya existe | 409 | ConflictGuard | "Special date for '{Date}' already exists" |

#### Lógica
```csharp
ConflictGuard.ThrowIf(
    schedule.SpecialDates.Any(sd => sd.Date == command.Date),
    $"Special date for '{command.Date}' already exists");

var timeSlots = command.TimeSlots
    .Select(ts => timeSlotCreate.Execute(ts))
    .ToList();

var specialDate = specialDateCreate.Execute(new CreateSpecialDateCommand(
    command.Date,
    command.IsClosed,
    command.Reason,
    timeSlots));

schedule._specialDates.Add(specialDate);

return scheduleValidator.ValidateOrThrow(schedule);
```

#### Slice: POST /schedules/{id}/special-dates

**Request**
```csharp
public record AddSpecialDateRequest(
    DateOnly Date,
    bool IsClosed,
    string Reason,
    SetTimeSlotRequest[] TimeSlots
);
```

**Response**: 201 Created → `ScheduleResponse`

#### Tests Unitarios (Dominio)

✅ Añadir fecha especial cerrada (festivo)
- Precondición: Schedule sin SpecialDates
- Input: Date=2025-12-25, IsClosed=true, Reason="Navidad", TimeSlots=[]
- Resultado: SpecialDate añadido

✅ Añadir fecha especial con horario extendido
- Input: Date=2025-02-14, IsClosed=false, Reason="San Valentín", TimeSlots=[{13:00, 02:00}]
- Resultado: SpecialDate añadido con horario especial

✅ Añadir múltiples fechas especiales
- Precondición: Schedule con 2025-12-25 configurado
- Input: Date=2025-12-31, IsClosed=false, Reason="Nochevieja", TimeSlots=[{20:00, 03:00}]
- Resultado: 2 SpecialDates en el schedule

❌ Fecha duplicada
- Precondición: Schedule ya tiene 2025-12-25
- Input: Date=2025-12-25
- Resultado: ConflictException "Special date for '2025-12-25' already exists"

❌ Reason vacío
- Input: Reason=""
- Resultado: ValidationException "Reason is required"

#### Tests Unitarios (Servicio)

✅ Obtiene el schedule del repositorio
- Verifica que repository.GetByIdAsync es llamado con el id correcto

✅ Llama a Schedule.AddSpecialDate con los parámetros correctos
- Verifica que se invoca addSpecialDate.Execute con el command correcto

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

✅ Retorna Response mapeado correctamente
- Verifica que el Response contiene el SpecialDate añadido

#### Tests Integración

✅ 201 Created → ScheduleResponse con SpecialDate añadido

❌ 404 → Schedule no encontrado

❌ 409 → Fecha duplicada

❌ 422 → Validación fallida

---

### 6.10 Schedule.UpdateSpecialDate

#### Event Storming
```
🟡[Admin] → 🔵(UpdateSpecialDate) → 🟤[[Schedule]] → 🟠<SpecialDateUpdated>
                                         │
                               🟣{SpecialDateExiste}
```

#### Input

| Campo | Tipo |
|-------|------|
| IsClosed | bool |
| Reason | string |
| TimeSlots | CreateTimeSlotCommand[] |

*Date viene en la ruta*

#### Inyecta
- `TimeSlot.Create`
- `SpecialDate.Create`
- `IValidator<Schedule>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| SpecialDate no existe | 404 | NotFoundGuard | "Special date for '{Date}' not found" |

#### Lógica
```csharp
var existing = schedule.SpecialDates.FirstOrDefault(sd => sd.Date == date);
NotFoundGuard.ThrowIfNull(existing, $"Special date for '{date}' not found");

var timeSlots = command.TimeSlots
    .Select(ts => timeSlotCreate.Execute(ts))
    .ToList();

var updated = specialDateCreate.Execute(new CreateSpecialDateCommand(
    date,
    command.IsClosed,
    command.Reason,
    timeSlots));

schedule._specialDates.Remove(existing);
schedule._specialDates.Add(updated);

return scheduleValidator.ValidateOrThrow(schedule);
```

#### Slice: PUT /schedules/{id}/special-dates/{date}

**Request**
```csharp
public record UpdateSpecialDateRequest(
    bool IsClosed,
    string Reason,
    SetTimeSlotRequest[] TimeSlots
);
```

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

✅ Actualizar fecha especial
- Precondición: Schedule tiene 2025-12-25 cerrado
- Input: IsClosed=false, Reason="Navidad (horario especial)", TimeSlots=[{13:00, 18:00}]
- Resultado: SpecialDate actualizado

✅ Cambiar de abierto a cerrado
- Precondición: Schedule tiene 2025-02-14 con horario especial
- Input: IsClosed=true, Reason="San Valentín cancelado", TimeSlots=[]
- Resultado: SpecialDate actualizado a cerrado

❌ SpecialDate no existe
- Precondición: Schedule no tiene 2025-08-15
- Resultado: NotFoundException "Special date for '2025-08-15' not found"

#### Tests Unitarios (Servicio)

✅ Obtiene el schedule del repositorio
- Verifica que repository.GetByIdAsync es llamado con el id correcto

✅ Llama a Schedule.UpdateSpecialDate con los parámetros correctos
- Verifica que se invoca updateSpecialDate.Execute con date y command correctos

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

#### Tests Integración

✅ 204 No Content

❌ 404 → Schedule o SpecialDate no encontrado

❌ 422 → Validación fallida

---

### 6.11 Schedule.RemoveSpecialDate

#### Event Storming
```
🟡[Admin] → 🔵(RemoveSpecialDate) → 🟤[[Schedule]] → 🟠<SpecialDateRemoved>
                                         │
                               🟣{SpecialDateExiste}
```

#### Input
*Date viene en la ruta*

#### Inyecta
- `IValidator<Schedule>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| SpecialDate no existe | 404 | NotFoundGuard | "Special date for '{Date}' not found" |

#### Lógica
```csharp
var existing = schedule.SpecialDates.FirstOrDefault(sd => sd.Date == date);
NotFoundGuard.ThrowIfNull(existing, $"Special date for '{date}' not found");

schedule._specialDates.Remove(existing);

return scheduleValidator.ValidateOrThrow(schedule);
```

#### Slice: DELETE /schedules/{id}/special-dates/{date}

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

✅ Eliminar fecha especial existente
- Precondición: Schedule con 2025-12-25 configurado
- Input: Date=2025-12-25
- Resultado: SpecialDate eliminado

✅ Eliminar una de varias fechas especiales
- Precondición: Schedule con 3 SpecialDates
- Input: Date=2025-12-25
- Resultado: Quedan 2 SpecialDates

❌ SpecialDate no existe
- Precondición: Schedule no tiene 2025-08-15
- Resultado: NotFoundException "Special date for '2025-08-15' not found"

#### Tests Unitarios (Servicio)

✅ Obtiene el schedule del repositorio
- Verifica que repository.GetByIdAsync es llamado con el id correcto

✅ Llama a Schedule.RemoveSpecialDate con el date correcto
- Verifica que se invoca removeSpecialDate.Execute con el date

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

#### Tests Integración

✅ 204 No Content

❌ 404 → Schedule o SpecialDate no encontrado

---

## 7. Queries

### IsOpen

**Slice**: GET /schedules/{id}/is-open?dateTime={dateTime}

**Response**: 200 OK → `IsOpenResponse`

#### Tests Integración

✅ 200 OK → IsOpen=true (dentro de horario regular)

✅ 200 OK → IsOpen=false (fuera de horario)

✅ 200 OK → IsOpen=false (día cerrado)

✅ 200 OK → IsOpen=false (festivo cerrado, sobrescribe semanal)

✅ 200 OK → IsOpen=true (fecha especial abierta, sobrescribe semanal)

✅ 200 OK → IsOpen=false (entre turnos)

❌ 404 → Schedule no encontrado

---

## 8. Resumen de Endpoints

| Método | Ruta | Comando/Query | Response |
|--------|------|---------------|----------|
| POST | /schedules | Schedule.Create | 201 → `ScheduleResponse` |
| GET | /schedules/{id} | GetSchedule | 200 → `ScheduleResponse` |
| GET | /schedules | ListSchedules | 200 → `ScheduleResponse[]` |
| PUT | /schedules/{id} | Schedule.Update | 204 |
| POST | /schedules/{id}/activate | Schedule.Activate | 200 → `ScheduleResponse` |
| POST | /schedules/{id}/deactivate | Schedule.Deactivate | 200 → `ScheduleResponse` |
| PUT | /schedules/{id}/weekly-hours/{dayOfWeek} | Schedule.SetWeeklyHours | 204 |
| POST | /schedules/{id}/weekly-hours/{dayOfWeek}/close | Schedule.MarkDayAsClosed | 204 |
| POST | /schedules/{id}/special-dates | Schedule.AddSpecialDate | 201 → `ScheduleResponse` |
| PUT | /schedules/{id}/special-dates/{date} | Schedule.UpdateSpecialDate | 204 |
| DELETE | /schedules/{id}/special-dates/{date} | Schedule.RemoveSpecialDate | 204 |
| GET | /schedules/{id}/is-open | IsOpen | 200 → `IsOpenResponse` |

---

## 9. Persistencia (Firestore)

### Colección

`/schedules/{scheduleId}`

### Configuración DbContext

```csharp
modelBuilder.Entity<ScheduleAgg>(entity =>
{
    entity.HasQueryFilter(s => s.TenantId == tenantId);

    entity.Ignore(s => s.HasWeeklyHours);
    entity.Ignore(s => s.HasSpecialDates);
    entity.Ignore(s => s.IsFullyConfigured);

    entity.MapOf(s => s.WeeklyHours, daySchedule =>
    {
        daySchedule.Ignore(ds => ds.TotalOpenHours);

        daySchedule.ArrayOf(ds => ds.TimeSlots, timeSlot =>
        {
            timeSlot.Ignore(ts => ts.Duration);
        });
    });

    entity.ArrayOf(s => s.SpecialDates, specialDate =>
    {
        specialDate.Ignore(sd => sd.TotalOpenHours);

        specialDate.ArrayOf(sd => sd.TimeSlots, timeSlot =>
        {
            timeSlot.Ignore(ts => ts.Duration);
        });
    });
});
```

### Documento Ejemplo

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "tenantId": "tenant-001-guid",
  "name": "Horario de Verano",
  "description": "Del 15 de junio al 15 de septiembre",
  "isActive": true,
  "weeklyHours": {
    "Monday": {
      "dayOfWeek": 1,
      "isClosed": false,
      "timeSlots": [
        { "openTime": "13:00:00", "closeTime": "16:00:00" },
        { "openTime": "20:00:00", "closeTime": "23:00:00" }
      ]
    },
    "Tuesday": {
      "dayOfWeek": 2,
      "isClosed": false,
      "timeSlots": [
        { "openTime": "13:00:00", "closeTime": "23:00:00" }
      ]
    },
    "Sunday": {
      "dayOfWeek": 0,
      "isClosed": true,
      "timeSlots": []
    }
  },
  "specialDates": [
    {
      "date": "2025-12-25",
      "isClosed": true,
      "reason": "Navidad",
      "timeSlots": []
    },
    {
      "date": "2025-02-14",
      "isClosed": false,
      "reason": "San Valentín",
      "timeSlots": [
        { "openTime": "13:00:00", "closeTime": "02:00:00" }
      ]
    }
  ]
}
```

---

## 10. Hot Spots ⚠️

| # | Pregunta | Estado |
|---|----------|--------|
| 1 | ¿Se soportan horarios que cruzan medianoche en un solo TimeSlot? | Decidido: NO. Se divide en 2 días |
| 2 | ¿Cómo se manejan restaurantes 24h? | Decidido: TimeSlot(00:00, 23:59) |
| 3 | ¿Se puede eliminar un Schedule o solo dejar vacío? | Pendiente |
| 4 | ¿Las SpecialDates tienen fecha de expiración automática? | Pendiente |

---

**Fecha**: 2025-01-29
**Autor**: Equipo Fudie
