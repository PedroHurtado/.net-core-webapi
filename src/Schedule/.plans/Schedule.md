# Domain Definition: Schedule

## 1. Estado y Estructura

### Resumen
**Schedule** representa los **horarios de apertura y cierre físicos** de un restaurante. Define cuándo el restaurante está abierto para cualquier actividad (comida en local, takeaway, etc.), pero NO define cuándo se aceptan reservas (eso es responsabilidad de ServiceSchedule).

**Responsabilidad**: Responder "¿Está abierto el restaurante en este momento?"

### Propiedades (Estado)

| Propiedad | Tipo | Modificador | Validaciones (FluentValidation) | Notas |
|-----------|------|-------------|--------------------------------|-------|
| Id | Guid | protected set | NotEmpty | Heredado de Entity |
| RestaurantId | Guid | protected set | NotEmpty | FK al restaurante |
| _weeklyHours | Dictionary<DayOfWeek, DaySchedule> | private | - | Backing field: horario semanal regular |
| WeeklyHours | IReadOnlyDictionary<DayOfWeek, DaySchedule> | get only | - | Expuesto como readonly |
| _specialDates | List<SpecialDate> | private | - | Backing field: excepciones (festivos, vacaciones) |
| SpecialDates | IReadOnlyCollection<SpecialDate> | get only | - | Expuesto como readonly |

### Value Objects Anidados

#### **DaySchedule** (Value Object)
| Propiedad | Tipo | Validaciones | Notas |
|-----------|------|-------------|-------|
| DayOfWeek | DayOfWeek | - | Lunes-Domingo |
| IsClosed | bool | - | Si true, TimeSlots debe estar vacío |
| TimeSlots | List<TimeSlot> | No vacío si IsClosed=false | Múltiples turnos posibles |

#### **TimeSlot** (Value Object)
| Propiedad | Tipo | Validaciones | Notas |
|-----------|------|-------------|-------|
| OpenTime | TimeOnly | NotEmpty | Hora de apertura |
| CloseTime | TimeOnly | NotEmpty, GreaterThan(OpenTime) | Hora de cierre |

**Validación adicional**: TimeSlots de un día NO pueden solaparse.

#### **SpecialDate** (Value Object)
| Propiedad | Tipo | Validaciones | Notas |
|-----------|------|-------------|-------|
| Date | DateOnly | NotEmpty | Fecha específica (ej: 25-Dic-2025) |
| IsClosed | bool | - | Si true, TimeSlots debe estar vacío |
| Reason | string | MaxLength(200) | Ej: "Navidad", "Vacaciones de verano" |
| TimeSlots | List<TimeSlot> | No vacío si IsClosed=false | Sobrescribe horario regular |

### Relaciones
- **Restaurant** (1:1): Un Schedule pertenece a UN Restaurant
  - Implementación: `RestaurantId` (Guid)
  - No se carga la entidad completa (solo ID)

### Invariantes / Reglas de Negocio Globales

1. **RestaurantId no puede ser vacío**: Cada Schedule debe pertenecer a un restaurante
2. **OpenTime < CloseTime**: Dentro de un TimeSlot, apertura debe ser antes del cierre
3. **Día cerrado = Sin horarios**: Si `DaySchedule.IsClosed = true`, `TimeSlots` debe estar vacío
4. **No solapamiento de TimeSlots**: En un mismo día, los TimeSlots no pueden solaparse
5. **SpecialDate sobrescribe WeeklyHours**: Para una fecha específica, se usa SpecialDate si existe, sino WeeklyHours
6. **Fechas especiales únicas**: No puede haber dos SpecialDate para la misma fecha

---

## 2. Comportamiento y Reglas (Event Storming & Example Mapping)

### Event Storming (Textual)

#### Flujo 1: Creación Inicial del Schedule
```
1. [Admin] -> (Crear Schedule) -> [Schedule] -> <ScheduleCreated>
   - Input: RestaurantId
   - Output: Schedule vacío (sin horarios definidos)
   - Constraint: RestaurantId debe ser válido (no Guid.Empty)
```

#### Flujo 2: Configurar Horario Semanal Regular
```
2. [Admin] -> (Configurar Horario Semanal) -> [Schedule] -> <WeeklyHoursUpdated>
   - Input: DayOfWeek, List<TimeSlot>
   - Validaciones:
     * OpenTime < CloseTime en cada TimeSlot
     * TimeSlots no se solapan
   - Resultado: Horario configurado para ese día

3. [Admin] -> (Marcar Día Cerrado) -> [Schedule] -> <DayMarkedAsClosed>
   - Input: DayOfWeek
   - Resultado: DaySchedule.IsClosed = true, TimeSlots = []
```

#### Flujo 3: Gestionar Fechas Especiales
```
4. [Admin] -> (Agregar Fecha Especial) -> [Schedule] -> <SpecialDateAdded>
   - Input: Date, IsClosed, Reason, TimeSlots?
   - Constraint: Fecha no puede estar duplicada
   - Ejemplo: "25-Dic-2025, Cerrado, 'Navidad'"

5. [Admin] -> (Eliminar Fecha Especial) -> [Schedule] -> <SpecialDateRemoved>
   - Input: Date
   - Constraint: Fecha debe existir
```

#### Flujo 4: Consultas (No modifican estado)
```
6. [Cliente/Sistema] -> (¿Está Abierto?) -> [Schedule]
   - Input: DateTime
   - Lógica:
     1. Buscar SpecialDate para esa fecha
     2. Si existe: usar SpecialDate.TimeSlots
     3. Si no: usar WeeklyHours[DayOfWeek].TimeSlots
     4. Verificar si DateTime cae dentro de algún TimeSlot
   - Output: bool (true/false)

7. [Cliente] -> (Ver Horarios del Día) -> [Schedule]
   - Input: DateOnly
   - Output: List<TimeSlot> o "Cerrado"
```

#### Flujo 5: Casos de Error
```
8. [Admin] -> (Configurar TimeSlot Inválido) -> [Schedule] -> <Error: InvalidTimeSlot>
   - Ejemplo: OpenTime = 15:00, CloseTime = 13:00
   - Error: "Hora de apertura debe ser antes del cierre"

9. [Admin] -> (Agregar TimeSlots Solapados) -> [Schedule] -> <Error: OverlappingTimeSlots>
   - Ejemplo: Slot1 = 13:00-16:00, Slot2 = 15:00-18:00
   - Error: "Los horarios no pueden solaparse"

10. [Admin] -> (Marcar Día Cerrado con Horarios) -> [Schedule] -> <Error: ClosedDayCannotHaveHours>
    - Intentar: IsClosed=true + TimeSlots=[...]
    - Error: "Un día cerrado no puede tener horarios"
```

---

### Example Mapping

#### Story 1: Crear Schedule para un Restaurante

**Rule**: Un Schedule debe tener un RestaurantId válido.

- **Example (Success)**: 
  - Crear Schedule con `RestaurantId = Guid.NewGuid()`
  - Resultado: Schedule creado exitosamente

- **Example (Failure)**: 
  - Crear Schedule con `RestaurantId = Guid.Empty`
  - Error: "RestaurantId es requerido"

---

#### Story 2: Configurar Horario Regular de la Semana

**Rule**: Los TimeSlots deben tener OpenTime < CloseTime.

- **Example (Success)**: 
  - Configurar Lunes con TimeSlot(13:00, 23:00)
  - Resultado: Horario guardado correctamente

- **Example (Failure)**: 
  - Configurar Lunes con TimeSlot(23:00, 13:00)
  - Error: "Hora de apertura debe ser antes del cierre"

**Rule**: Los TimeSlots del mismo día no pueden solaparse.

- **Example (Success - Múltiples turnos SIN solapar)**: 
  - Configurar Martes:
    - TimeSlot1(13:00, 16:00) - Comida
    - TimeSlot2(20:00, 23:00) - Cena
  - Resultado: Ambos turnos configurados

- **Example (Failure - Solapamiento)**: 
  - Configurar Miércoles:
    - TimeSlot1(13:00, 16:00)
    - TimeSlot2(15:00, 18:00) ← Solapa con TimeSlot1
  - Error: "Los horarios no pueden solaparse"

**Rule**: Un día cerrado no puede tener horarios.

- **Example (Success)**: 
  - Marcar Domingo como cerrado
  - TimeSlots = []
  - Resultado: Domingo configurado como cerrado

- **Example (Failure)**: 
  - Intentar configurar Domingo:
    - IsClosed = true
    - TimeSlots = [TimeSlot(13:00, 16:00)]
  - Error: "Un día cerrado no puede tener horarios"

---

#### Story 3: Gestionar Fechas Especiales (Festivos, Vacaciones)

**Rule**: No puede haber dos SpecialDate para la misma fecha.

- **Example (Success)**: 
  - Agregar SpecialDate(25-Dic-2025, Cerrado, "Navidad")
  - Resultado: Fecha especial agregada

- **Example (Failure - Fecha duplicada)**: 
  - Agregar SpecialDate(25-Dic-2025, ...) cuando ya existe
  - Error: "Ya existe un horario especial para esta fecha"

**Rule**: SpecialDate sobrescribe WeeklyHours para esa fecha.

- **Example (Success - Horario especial diferente)**: 
  - WeeklyHours: Viernes 13:00-23:00
  - SpecialDate: 14-Feb-2025 (Viernes San Valentín): 13:00-02:00
  - Consulta: ¿Abierto 14-Feb-2025 a las 01:00?
  - Resultado: TRUE (usa SpecialDate, no WeeklyHours)

- **Example (Success - Día normalmente abierto, especial cerrado)**: 
  - WeeklyHours: Lunes 13:00-23:00
  - SpecialDate: 01-Ene-2025 (Lunes Año Nuevo): Cerrado
  - Consulta: ¿Abierto 01-Ene-2025 a las 14:00?
  - Resultado: FALSE (SpecialDate dice cerrado)

---

#### Story 4: Consultar si el Restaurante está Abierto

**Rule**: Se prioriza SpecialDate sobre WeeklyHours.

- **Example (Success - Día normal)**: 
  - WeeklyHours: Martes 13:00-23:00
  - SpecialDates: []
  - Consulta: ¿Abierto Martes 15-Abr-2025 a las 15:00?
  - Resultado: TRUE (dentro del TimeSlot)

- **Example (Success - Fuera de horario)**: 
  - WeeklyHours: Martes 13:00-23:00
  - Consulta: ¿Abierto Martes 15-Abr-2025 a las 11:00?
  - Resultado: FALSE (antes de apertura)

- **Example (Success - Día cerrado regular)**: 
  - WeeklyHours: Domingo CERRADO
  - Consulta: ¿Abierto Domingo 20-Abr-2025 a las 15:00?
  - Resultado: FALSE

- **Example (Edge - Múltiples turnos)**: 
  - WeeklyHours: Viernes [13:00-16:00, 20:00-23:00]
  - Consulta: ¿Abierto Viernes 18-Abr-2025 a las 17:00?
  - Resultado: FALSE (entre turnos)

---

#### Story 5: Casos Edge - Horarios que Cruzan Medianoche

**Rule**: NO soportamos horarios que crucen medianoche en un solo TimeSlot.

- **Example (Failure - Intento de horario cruzando medianoche)**: 
  - Intentar: TimeSlot(22:00, 02:00)
  - Error: "Hora de cierre debe ser posterior a hora de apertura"

- **Example (Success - Alternativa con 2 días)**: 
  - Viernes: TimeSlot(13:00, 23:59)
  - Sábado: TimeSlot(00:00, 02:00)
  - Consulta: ¿Abierto Viernes 23:30? → TRUE
  - Consulta: ¿Abierto Sábado 01:00? → TRUE

**Justificación**: Simplifica la lógica y evita edge cases complejos. En la práctica, se divide en 2 días.

---

#### Story 6: Casos Edge - Restaurante 24 Horas

**Rule**: Para un día abierto 24h, usar TimeSlot(00:00, 23:59).

- **Example (Success - Día 24h)**: 
  - Configurar Viernes: TimeSlot(00:00, 23:59)
  - Consulta: ¿Abierto Viernes a las 03:00? → TRUE
  - Consulta: ¿Abierto Viernes a las 23:45? → TRUE

---

## 3. Invariantes Críticos (Resumen para Tests)

| Invariante | Test |
|------------|------|
| RestaurantId no vacío | `Create_WithEmptyRestaurantId_ShouldReturnFailure()` |
| OpenTime < CloseTime | `SetWeeklyHours_WithInvalidTimeSlot_ShouldReturnFailure()` |
| TimeSlots no solapan | `SetWeeklyHours_WithOverlappingSlots_ShouldReturnFailure()` |
| Día cerrado sin horarios | `MarkAsClosed_ThenAddTimeSlot_ShouldReturnFailure()` |
| SpecialDate única | `AddSpecialDate_Duplicate_ShouldReturnFailure()` |
| SpecialDate sobrescribe | `IsOpen_WithSpecialDate_ShouldUseSpecialDateNotWeekly()` |

---

## 4. Comandos y Queries del Dominio

### Comandos (Modifican estado)
```csharp
// Factory
public static Result<Schedule> Create(Guid restaurantId)

// Gestión semanal
public Result SetWeeklyHours(DayOfWeek day, List<TimeSlot> slots)
public Result MarkDayAsClosed(DayOfWeek day)

// Gestión especial
public Result AddSpecialDate(DateOnly date, bool isClosed, string reason, List<TimeSlot>? slots = null)
public Result RemoveSpecialDate(DateOnly date)
public Result UpdateSpecialDate(DateOnly date, bool isClosed, string reason, List<TimeSlot>? slots = null)
```

### Queries (Solo lectura)
```csharp
public bool IsOpen(DateTime dateTime)
public List<TimeSlot> GetHoursFor(DateOnly date)
public bool IsSpecialDate(DateOnly date)
```
