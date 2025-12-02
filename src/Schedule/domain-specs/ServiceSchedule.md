# Domain Definition: ServiceSchedule

## 1. Estado y Estructura

### Resumen
**ServiceSchedule** representa los **horarios y configuración de servicios reservables** de un restaurante. Define CUÁNDO se aceptan reservas, para QUÉ servicios (Desayuno, Comida, Cena), y bajo QUÉ políticas (tiempo mínimo de antelación, duración, capacidad, etc.).

**Responsabilidad**: Responder "¿Puedo reservar para [servicio] el [fecha] a las [hora] para [X personas]?"

**Diferencia con Schedule**: 
- Schedule = "¿Está abierto el restaurante?"
- ServiceSchedule = "¿Puedo hacer una reserva?"

### Propiedades (Estado)

| Propiedad | Tipo | Modificador | Validaciones (FluentValidation) | Notas |
|-----------|------|-------------|--------------------------------|-------|
| Id | Guid | protected set | NotEmpty | Heredado de Entity |
| RestaurantId | Guid | protected set | NotEmpty | FK al restaurante |
| _services | List<Service> | private | - | Backing field: servicios configurados |
| Services | IReadOnlyCollection<Service> | get only | - | Expuesto como readonly |
| Policy | ReservationPolicy | protected set | NotNull | Políticas globales de reserva |

### Value Objects Principales

#### **Service** (Value Object)
Representa un tipo de servicio reservable (Desayuno, Comida, Cena).

| Propiedad | Tipo | Validaciones | Notas |
|-----------|------|-------------|-------|
| Type | ServiceType (enum) | Required | Breakfast, Lunch, Dinner |
| _weeklySchedule | Dictionary<DayOfWeek, ServiceDayConfig> | - | Backing field |
| WeeklySchedule | IReadOnlyDictionary<DayOfWeek, ServiceDayConfig> | - | Expuesto como readonly |
| MaxCapacity | int | GreaterThan(0) | Personas simultáneas en este servicio |
| _specialDates | List<ServiceSpecialDate> | - | Excepciones de horario |
| SpecialDates | IReadOnlyCollection<ServiceSpecialDate> | - | Expuesto como readonly |

**ServiceType Enum**:
```csharp
public enum ServiceType
{
    Breakfast,
    Lunch,
    Dinner
}
```

#### **ServiceDayConfig** (Value Object)
Configuración de un servicio para un día específico de la semana.

| Propiedad | Tipo | Validaciones | Notas |
|-----------|------|-------------|-------|
| IsAvailable | bool | - | Si false, no hay servicio este día |
| StartTime | TimeOnly | Required si IsAvailable=true | Hora inicio de reservas |
| EndTime | TimeOnly | Required si IsAvailable=true, GreaterThan(StartTime) | Hora fin de reservas |
| CapacityOverride | int? | GreaterThan(0) si no null | null = usa MaxCapacity del Service |

**Validación adicional**: Si `IsAvailable=false`, StartTime y EndTime deben ser default.

#### **ServiceSpecialDate** (Value Object)
Excepción de horario para un servicio en una fecha específica.

| Propiedad | Tipo | Validaciones | Notas |
|-----------|------|-------------|-------|
| Date | DateOnly | NotEmpty | Fecha específica |
| IsAvailable | bool | - | Si false, servicio no disponible este día |
| StartTime | TimeOnly | Required si IsAvailable=true | Sobrescribe WeeklySchedule |
| EndTime | TimeOnly | Required si IsAvailable=true, GreaterThan(StartTime) | Sobrescribe WeeklySchedule |
| CapacityOverride | int? | GreaterThan(0) si no null | Sobrescribe MaxCapacity o DayConfig |
| Reason | string | MaxLength(200) | Ej: "San Valentín - horario extendido" |

#### **ReservationPolicy** (Value Object)
Políticas globales que aplican a TODAS las reservas del restaurante.

| Propiedad | Tipo | Validaciones | Notas |
|-----------|------|-------------|-------|
| MinimumAdvanceTime | TimeSpan | GreaterThan(0) | Tiempo mínimo de antelación (ej: 2 horas) |
| MaximumAdvanceTime | TimeSpan | GreaterThan(MinimumAdvanceTime) | Tiempo máximo de antelación (ej: 30 días) |
| SlotInterval | TimeSpan | GreaterThan(0) | Intervalo entre slots (ej: 15 minutos) |
| StandardDurations | Dictionary<ServiceType, TimeSpan> | Required para cada ServiceType | Duración estándar por servicio |
| BufferBetweenReservations | TimeSpan | GreaterThanOrEqualTo(0) | Tiempo de limpieza/preparación entre reservas |
| MaxPartySize | int | GreaterThan(0) | Máximo de personas por reserva |
| MinPartySize | int | GreaterThan(0), LessThanOrEqualTo(MaxPartySize) | Mínimo de personas por reserva |

**Valores típicos recomendados**:
```csharp
MinimumAdvanceTime: 2 horas
MaximumAdvanceTime: 30 días
SlotInterval: 15 minutos
StandardDurations: 
  - Breakfast: 1 hora
  - Lunch: 1.5 horas
  - Dinner: 2 horas
BufferBetweenReservations: 15 minutos
MaxPartySize: 8
MinPartySize: 1
```

### Relaciones
- **Restaurant** (1:1): Un ServiceSchedule pertenece a UN Restaurant
  - Implementación: `RestaurantId` (Guid)
- **Schedule** (referencia lógica): Valida que los horarios de servicio estén dentro del horario de apertura
  - No hay FK directo, validación a nivel de aplicación

### Invariantes / Reglas de Negocio Globales

1. **RestaurantId no puede ser vacío**: Cada ServiceSchedule debe pertenecer a un restaurante
2. **Policy es obligatoria**: Debe haber una ReservationPolicy configurada
3. **ServiceType único**: No puede haber dos Services con el mismo ServiceType
4. **StartTime < EndTime**: En ServiceDayConfig y ServiceSpecialDate
5. **Capacidad positiva**: MaxCapacity y CapacityOverride deben ser > 0
6. **SlotInterval válido**: Debe ser divisible en minutos (ej: 15, 30, 60)
7. **Duración estándar coherente**: Debe haber duración para cada ServiceType configurado
8. **MaxPartySize >= MinPartySize**: Validación lógica
9. **SpecialDate sobrescribe WeeklySchedule**: Para una fecha específica
10. **Horarios dentro de apertura del restaurante**: Validación cruzada (a nivel de aplicación)

---

## 2. Comportamiento y Reglas (Event Storming & Example Mapping)

### Event Storming (Textual)

#### Flujo 1: Creación Inicial del ServiceSchedule

```
1. [Admin] -> (Crear ServiceSchedule) -> [ServiceSchedule] -> <ServiceScheduleCreated>
   - Input: RestaurantId, ReservationPolicy
   - Output: ServiceSchedule sin servicios configurados
   - Constraint: 
     * RestaurantId no vacío
     * Policy válida (MinAdvance < MaxAdvance, etc.)
```

#### Flujo 2: Configurar Servicios

```
2. [Admin] -> (Agregar Servicio) -> [ServiceSchedule] -> <ServiceAdded>
   - Input: ServiceType, WeeklySchedule, MaxCapacity
   - Validaciones:
     * ServiceType no duplicado
     * MaxCapacity > 0
     * StartTime < EndTime en cada día
   - Resultado: Servicio agregado (ej: Lunch 13:00-16:00)

3. [Admin] -> (Actualizar Servicio) -> [ServiceSchedule] -> <ServiceUpdated>
   - Input: ServiceType, WeeklySchedule actualizado, MaxCapacity
   - Constraint: ServiceType debe existir
   - Resultado: Horarios/capacidad actualizados

4. [Admin] -> (Eliminar Servicio) -> [ServiceSchedule] -> <ServiceRemoved>
   - Input: ServiceType
   - Constraint: ServiceType debe existir
   - Resultado: Servicio eliminado
```

#### Flujo 3: Configurar Horarios Semanales de un Servicio

```
5. [Admin] -> (Configurar Día de Servicio) -> [ServiceSchedule] -> <ServiceDayConfigured>
   - Input: ServiceType, DayOfWeek, ServiceDayConfig
   - Ejemplo: Lunch disponible Lunes 13:00-16:00, capacidad 50
   - Resultado: Día configurado

6. [Admin] -> (Desactivar Servicio en Día) -> [ServiceSchedule] -> <ServiceDayDisabled>
   - Input: ServiceType, DayOfWeek
   - Ejemplo: Lunch no disponible los Domingos
   - Resultado: IsAvailable = false para ese día
```

#### Flujo 4: Gestionar Fechas Especiales para Servicios

```
7. [Admin] -> (Agregar Fecha Especial a Servicio) -> [ServiceSchedule] -> <ServiceSpecialDateAdded>
   - Input: ServiceType, Date, IsAvailable, StartTime?, EndTime?, CapacityOverride?, Reason
   - Ejemplo: Dinner el 14-Feb (San Valentín): 19:00-02:00, capacidad 60
   - Constraint: Fecha no duplicada para ese servicio
   - Resultado: Horario especial agregado

8. [Admin] -> (Eliminar Fecha Especial) -> [ServiceSchedule] -> <ServiceSpecialDateRemoved>
   - Input: ServiceType, Date
   - Resultado: Fecha especial eliminada
```

#### Flujo 5: Actualizar Políticas de Reserva

```
9. [Admin] -> (Actualizar Policy) -> [ServiceSchedule] -> <ReservationPolicyUpdated>
   - Input: Nueva ReservationPolicy
   - Validaciones:
     * MinAdvance < MaxAdvance
     * SlotInterval > 0
     * Duraciones configuradas para todos los servicios
   - Resultado: Policy actualizada
```

#### Flujo 6: Consultas de Disponibilidad (Cliente)

```
10. [Cliente] -> (¿Servicios disponibles en fecha?) -> [ServiceSchedule]
    - Input: DateOnly
    - Lógica:
      1. Para cada Service:
         a. Buscar ServiceSpecialDate para esa fecha
         b. Si existe y IsAvailable=false: servicio no disponible
         c. Si existe y IsAvailable=true: servicio disponible con horario especial
         d. Si no existe: usar WeeklySchedule[DayOfWeek]
      2. Filtrar servicios disponibles
    - Output: List<ServiceType> disponibles

11. [Cliente] -> (¿Puedo reservar?) -> [ServiceSchedule]
    - Input: ServiceType, DateTime, PartySize
    - Validaciones:
      a. Servicio existe
      b. Servicio disponible en esa fecha/hora
      c. DateTime dentro de ventana de antelación (MinAdvance < X < MaxAdvance)
      d. PartySize dentro de límites (MinPartySize <= X <= MaxPartySize)
      e. DateTime es un slot válido (múltiplo de SlotInterval)
    - Output: bool + razón si false

12. [Cliente] -> (Ver slots disponibles) -> [ServiceSchedule]
    - Input: ServiceType, DateOnly, PartySize
    - Lógica:
      1. Obtener horario del servicio para esa fecha
      2. Generar slots cada SlotInterval entre StartTime y EndTime
      3. Filtrar slots válidos (antelación, capacidad)
    - Output: List<TimeOnly> con slots disponibles
```

#### Flujo 7: Validación Cruzada con Schedule (Aplicación)

```
13. [Sistema] -> (Validar Horarios contra Schedule) -> [ServiceSchedule + Schedule]
    - Input: ServiceSchedule, Schedule
    - Validación:
      1. Para cada Service y cada día:
         a. Verificar que StartTime/EndTime estén dentro de horario de apertura
      2. Para cada ServiceSpecialDate:
         a. Verificar que la fecha tenga horario de apertura en Schedule
    - Output: Result con errores si hay inconsistencias
    
    Nota: Esta validación es a nivel de Application Layer, no de Domain.
```

#### Flujo 8: Casos de Error

```
14. [Admin] -> (Agregar Servicio Duplicado) -> [ServiceSchedule] -> <Error: DuplicateServiceType>
    - Intentar agregar Lunch cuando ya existe
    - Error: "Ya existe un servicio de tipo Lunch"

15. [Admin] -> (Configurar Horario Inválido) -> [ServiceSchedule] -> <Error: InvalidTimeRange>
    - Intentar: StartTime = 15:00, EndTime = 13:00
    - Error: "Hora de inicio debe ser antes de hora de fin"

16. [Cliente] -> (Reservar fuera de antelación) -> [ServiceSchedule] -> <Error: OutsideAdvanceWindow>
    - Intentar reservar para dentro de 30 minutos (MinAdvance = 2h)
    - Error: "Debe reservar con al menos 2 horas de antelación"

17. [Cliente] -> (Reservar grupo muy grande) -> [ServiceSchedule] -> <Error: PartySize>
    - Intentar reservar para 12 personas (MaxPartySize = 8)
    - Error: "Máximo 8 personas por reserva"

18. [Cliente] -> (Reservar en slot inválido) -> [ServiceSchedule] -> <Error: InvalidSlot>
    - Intentar reservar a las 13:07 (SlotInterval = 15 min)
    - Error: "Solo se aceptan reservas cada 15 minutos (:00, :15, :30, :45)"
```

---

### Example Mapping

#### Story 1: Crear ServiceSchedule

**Rule**: Debe tener RestaurantId y ReservationPolicy válidos.

- **Example (Success)**: 
  - Crear ServiceSchedule con RestaurantId válido y policy estándar
  - Resultado: ServiceSchedule creado sin servicios

- **Example (Failure - RestaurantId vacío)**: 
  - Crear con RestaurantId = Guid.Empty
  - Error: "RestaurantId es requerido"

- **Example (Failure - Policy inválida)**: 
  - Crear con MinAdvance = 5 horas, MaxAdvance = 2 horas
  - Error: "Tiempo máximo debe ser mayor que tiempo mínimo"

---

#### Story 2: Configurar Servicios de Reserva

**Rule**: No puede haber dos servicios del mismo tipo.

- **Example (Success)**: 
  - Agregar Lunch (13:00-16:00)
  - Agregar Dinner (20:00-23:00)
  - Resultado: Dos servicios configurados

- **Example (Failure - Duplicado)**: 
  - Agregar Lunch
  - Intentar agregar Lunch de nuevo
  - Error: "Ya existe un servicio de tipo Lunch"

**Rule**: StartTime debe ser menor que EndTime.

- **Example (Success)**: 
  - Configurar Breakfast: 08:00-11:00
  - Resultado: Servicio configurado

- **Example (Failure - Horario invertido)**: 
  - Configurar Breakfast: 11:00-08:00
  - Error: "Hora de inicio debe ser antes de hora de fin"

**Rule**: MaxCapacity debe ser mayor que 0.

- **Example (Success)**: 
  - Agregar Dinner con MaxCapacity = 50
  - Resultado: Servicio configurado

- **Example (Failure - Capacidad cero)**: 
  - Agregar Dinner con MaxCapacity = 0
  - Error: "Capacidad debe ser mayor que 0"

---

#### Story 3: Configurar Horarios Semanales por Servicio

**Rule**: Cada día de la semana puede tener configuración diferente.

- **Example (Success - Horarios estándar)**: 
  - Lunch: Lunes-Viernes 13:00-16:00
  - Resultado: 5 días configurados igual

- **Example (Success - Horario diferente fin de semana)**: 
  - Lunch: Lunes-Viernes 13:00-16:00
  - Lunch: Sábado-Domingo 12:00-17:00 (Brunch extendido)
  - Resultado: Horarios diferentes por día

- **Example (Success - Servicio no disponible ciertos días)**: 
  - Breakfast: Lunes-Viernes disponible
  - Breakfast: Sábado-Domingo NO disponible
  - Consulta: ¿Breakfast disponible Domingo? → FALSE

**Rule**: CapacityOverride por día sobrescribe MaxCapacity.

- **Example (Success)**: 
  - Dinner: MaxCapacity = 50
  - Viernes: CapacityOverride = 60 (más demanda)
  - Lunes: CapacityOverride = null (usa 50)
  - Consulta: Capacidad Viernes → 60
  - Consulta: Capacidad Lunes → 50

---

#### Story 4: Fechas Especiales para Servicios

**Rule**: ServiceSpecialDate sobrescribe WeeklySchedule para esa fecha.

- **Example (Success - Horario extendido San Valentín)**: 
  - Dinner regular: 20:00-23:00
  - SpecialDate 14-Feb: 19:00-02:00
  - Consulta: Horario 14-Feb → 19:00-02:00
  - Consulta: Horario 15-Feb → 20:00-23:00 (regular)

- **Example (Success - Servicio cerrado día especial)**: 
  - Lunch regular: Lunes 13:00-16:00
  - SpecialDate 01-Ene (Lunes): IsAvailable=false (Año Nuevo)
  - Consulta: ¿Lunch disponible 01-Ene? → FALSE

- **Example (Success - Capacidad especial)**: 
  - Dinner: MaxCapacity = 50
  - SpecialDate 31-Dic (Nochevieja): CapacityOverride = 70
  - Consulta: Capacidad 31-Dic → 70

**Rule**: No puede haber fechas especiales duplicadas para un servicio.

- **Example (Failure)**: 
  - Agregar SpecialDate Lunch 25-Dic
  - Intentar agregar otra SpecialDate Lunch 25-Dic
  - Error: "Ya existe horario especial para esta fecha"

---

#### Story 5: Validar Tiempo de Antelación

**Rule**: MinimumAdvanceTime debe cumplirse.

- **Example (Success)**: 
  - Policy: MinAdvance = 2 horas
  - Ahora: 10:00
  - Reservar para hoy 13:00 (3h adelante)
  - Resultado: Válido

- **Example (Failure - Muy pronto)**: 
  - Policy: MinAdvance = 2 horas
  - Ahora: 10:00
  - Reservar para hoy 11:00 (1h adelante)
  - Error: "Debe reservar con al menos 2 horas de antelación"

**Rule**: MaximumAdvanceTime debe cumplirse.

- **Example (Success)**: 
  - Policy: MaxAdvance = 30 días
  - Hoy: 01-Ene
  - Reservar para 15-Ene (14 días adelante)
  - Resultado: Válido

- **Example (Failure - Muy adelante)**: 
  - Policy: MaxAdvance = 30 días
  - Hoy: 01-Ene
  - Reservar para 15-Mar (73 días adelante)
  - Error: "Solo se aceptan reservas hasta 30 días de anticipación"

---

#### Story 6: Validar Slots de Tiempo

**Rule**: Solo se aceptan reservas en múltiplos de SlotInterval.

- **Example (Success - Slots válidos)**: 
  - Policy: SlotInterval = 15 minutos
  - Lunch: 13:00-16:00
  - Slots válidos: 13:00, 13:15, 13:30, 13:45, 14:00, ..., 15:45
  - Reservar para 13:30 → Válido

- **Example (Failure - Slot inválido)**: 
  - Policy: SlotInterval = 15 minutos
  - Reservar para 13:07
  - Error: "Solo se aceptan reservas cada 15 minutos"

**Rule**: Último slot debe permitir duración completa del servicio.

- **Example (Success)**: 
  - Lunch: 13:00-16:00
  - Duración: 1.5 horas
  - Último slot válido: 14:30 (termina a 16:00)
  - Reservar para 14:30 → Válido

- **Example (Failure - Slot demasiado tarde)**: 
  - Lunch: 13:00-16:00
  - Duración: 1.5 horas
  - Reservar para 15:00 (terminaría a 16:30, fuera del horario)
  - Error: "No hay tiempo suficiente para completar el servicio"

---

#### Story 7: Validar Tamaño de Grupo

**Rule**: PartySize debe estar entre MinPartySize y MaxPartySize.

- **Example (Success)**: 
  - Policy: MinPartySize=1, MaxPartySize=8
  - Reservar para 4 personas
  - Resultado: Válido

- **Example (Failure - Muy pequeño)**: 
  - Policy: MinPartySize=2, MaxPartySize=8
  - Reservar para 1 persona
  - Error: "Mínimo 2 personas por reserva"

- **Example (Failure - Muy grande)**: 
  - Policy: MinPartySize=1, MaxPartySize=8
  - Reservar para 12 personas
  - Error: "Máximo 8 personas por reserva. Contacte para grupos grandes."

---

#### Story 8: Calcular Disponibilidad de Slots

**Rule**: Un slot está disponible si hay capacidad suficiente.

- **Example (Success - Capacidad disponible)**: 
  - Lunch: MaxCapacity = 50 personas
  - Reservas existentes para 13:00: 30 personas
  - Nueva reserva: 15 personas
  - Total: 45 personas (< 50)
  - Resultado: Slot disponible

- **Example (Failure - Sin capacidad)**: 
  - Lunch: MaxCapacity = 50 personas
  - Reservas existentes para 13:00: 45 personas
  - Nueva reserva: 10 personas
  - Total: 55 personas (> 50)
  - Resultado: Slot NO disponible

**Nota**: Esta validación requiere consultar Reservations (otro agregado).

---

#### Story 9: Considerar Buffer entre Reservas

**Rule**: El buffer se suma a la duración estándar.

- **Example (Success - Sin buffer)**: 
  - Lunch: 13:00-16:00
  - Duración: 1.5h
  - Buffer: 0 min
  - Reserva mesa A: 13:00-14:30
  - Mesa A disponible de nuevo: 14:30

- **Example (Success - Con buffer)**: 
  - Lunch: 13:00-16:00
  - Duración: 1.5h
  - Buffer: 15 min
  - Reserva mesa A: 13:00-14:30 + 15min buffer = 14:45
  - Mesa A disponible de nuevo: 14:45

**Nota**: Esta lógica es más relevante para el agregado Reservation/Table, pero Policy la define.

---

#### Story 10: Casos Edge - Servicios Múltiples en un Día

**Rule**: Múltiples servicios pueden existir el mismo día sin conflicto.

- **Example (Success)**: 
  - Breakfast: 08:00-11:00
  - Lunch: 13:00-16:00
  - Dinner: 20:00-23:00
  - Todos el mismo día (Lunes)
  - Resultado: Los 3 servicios configurados

- **Example (Edge - Servicios solapados en horario)**: 
  - Brunch: 10:00-15:00
  - Lunch: 13:00-16:00
  - ¿Problema? NO a nivel de ServiceSchedule
  - Responsabilidad del admin no configurarlos así

**Decisión**: ServiceSchedule NO valida solapamiento entre servicios (flexibilidad al admin).

---

## 3. Invariantes Críticos (Resumen para Tests)

| Invariante | Test |
|------------|------|
| RestaurantId no vacío | `Create_WithEmptyRestaurantId_ShouldReturnFailure()` |
| Policy válida | `Create_WithInvalidPolicy_ShouldReturnFailure()` |
| ServiceType único | `AddService_Duplicate_ShouldReturnFailure()` |
| StartTime < EndTime | `AddService_WithInvalidTimeRange_ShouldReturnFailure()` |
| MaxCapacity > 0 | `AddService_WithZeroCapacity_ShouldReturnFailure()` |
| SpecialDate sobrescribe | `GetAvailableSlots_WithSpecialDate_ShouldUseSpecialDate()` |
| Antelación mínima | `CanReserve_WithinMinAdvance_ShouldReturnFalse()` |
| Antelación máxima | `CanReserve_BeyondMaxAdvance_ShouldReturnFalse()` |
| Slot válido | `CanReserve_InvalidSlot_ShouldReturnFalse()` |
| PartySize válido | `CanReserve_PartyTooLarge_ShouldReturnFalse()` |
| Duración suficiente | `GetAvailableSlots_ShouldExcludeSlotsWithInsufficientTime()` |

---

## 4. Comandos y Queries del Dominio

### Comandos (Modifican estado)
```csharp
// Factory
public static Result<ServiceSchedule> Create(Guid restaurantId, ReservationPolicy policy)

// Gestión de servicios
public Result AddService(ServiceType type, Dictionary<DayOfWeek, ServiceDayConfig> weeklySchedule, int maxCapacity)
public Result UpdateService(ServiceType type, Dictionary<DayOfWeek, ServiceDayConfig> weeklySchedule, int maxCapacity)
public Result RemoveService(ServiceType type)

// Gestión de días específicos de un servicio
public Result ConfigureServiceDay(ServiceType type, DayOfWeek day, ServiceDayConfig config)

// Gestión de fechas especiales
public Result AddSpecialDate(ServiceType type, DateOnly date, bool isAvailable, TimeOnly? startTime, TimeOnly? endTime, int? capacityOverride, string reason)
public Result RemoveSpecialDate(ServiceType type, DateOnly date)

// Actualizar política
public Result UpdatePolicy(ReservationPolicy newPolicy)
```

### Queries (Solo lectura)
```csharp
// Consultas de disponibilidad
public List<ServiceType> GetAvailableServices(DateOnly date)
public bool IsServiceAvailable(ServiceType type, DateOnly date)

// Validación de reservas
public Result<bool> CanReserve(ServiceType type, DateTime requestedDateTime, int partySize)

// Obtener slots disponibles
public List<TimeOnly> GetAvailableSlots(ServiceType type, DateOnly date, int partySize)
// Nota: Este método NO verifica capacidad real (requiere consultar Reservations)
// Retorna slots válidos según horario y política

// Calcular hora de fin
public DateTime CalculateReservationEndTime(ServiceType type, DateTime startTime)

// Obtener configuración
public ServiceDayConfig GetServiceConfig(ServiceType type, DateOnly date)
public int GetCapacity(ServiceType type, DateOnly date)
```

---

## 5. Interacción con Otros Agregados

### ServiceSchedule → Schedule (Lectura)
- **Validación recomendada**: Al configurar horarios de servicio, verificar que estén dentro del horario de apertura
- **Nivel**: Application Layer (no Domain)
- **Implementación**: Command Handler valida antes de llamar a ServiceSchedule.AddService()

### ServiceSchedule → Reservation (Lectura)
- **Para calcular disponibilidad real**: ServiceSchedule da los slots válidos, pero Reservation tiene las reservas existentes
- **Nivel**: Application Layer (Query Handler)
- **Flujo**:
  1. ServiceSchedule.GetAvailableSlots() → slots teóricos
  2. ReservationRepository.GetReservationsFor(date, service) → reservas existentes
  3. Filtrar slots con capacidad disponible

---

## 6. Decisiones de Diseño Importantes

### ¿Por qué ServiceSchedule NO valida contra Schedule?

**Razón**: Separación de responsabilidades (SRP) y evitar acoplamiento.

- Schedule y ServiceSchedule son agregados independientes
- ServiceSchedule NO tiene referencia directa a Schedule
- Validación cruzada se hace en Application Layer

**Ventajas**:
- ✅ Agregados más simples
- ✅ Sin dependencias circulares
- ✅ Testeable independientemente

**Desventaja**:
- ❌ Posible inconsistencia si admin configura mal

**Solución**: Validación en Command Handler antes de persistir.

### ¿Por qué ReservationPolicy es Value Object y no entidad?

**Razón**: No tiene identidad propia, es parte de ServiceSchedule.

- Policy cambia completa (no se modifica parcialmente)
- No se consulta Policy independientemente
- Inmutabilidad simplifica la lógica

### ¿Por qué Service es Value Object y no agregado?

**Razón**: Service no tiene ciclo de vida independiente.

- Service existe solo dentro de ServiceSchedule
- Todas las operaciones sobre Service pasan por ServiceSchedule
- Simplifica transacciones (un solo agregado)
