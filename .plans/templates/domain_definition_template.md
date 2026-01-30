# Domain Specification: {AggregateName}

---

## 1. Enums

### {EnumName}

```csharp
public enum {EnumName}
{
    Value1,
    Value2,
    Value3
}
```

---

## 2. Value Objects

### 2.1 {ValueObjectName}

#### Estructura

| Propiedad | Tipo |
|-----------|------|
| {Property1} | {Type} |
| {Property2} | {Type} |

#### Invariantes (Validator)

> Estas reglas se implementan en `{ValueObjectName}Validator : AbstractValidator<{ValueObjectName}>`

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| {Property1} | NotEmpty | "{Property1} is required" |
| {Property1} | MaxLength({n}) | "{Property1} cannot exceed {n} characters" |
| {Property2} | > 0 | "{Property2} must be greater than zero" |
| {Property2} | NotNull when {Condition} | "{Property2} is required when {Condition}" |
| {Property2} | Null when !{Condition} | "{Property2} only applies when {Condition}" |

#### Propiedades Calculadas

| Propiedad | Tipo | Fórmula |
|-----------|------|---------|
| {Property} | {Type} | `{Formula}` |

#### Métodos

- `{MethodName}({params})` → {ReturnType}

#### Comando: {ValueObjectName}.Create

**Input**

| Campo | Tipo | Default |
|-------|------|---------|
| {Field1} | {Type} | |
| {Field2} | {Type} | {DefaultValue} |

**Inyecta**: `IValidator<{ValueObjectName}>`

**Lógica**
```csharp
var {instance} = new {ValueObjectName}(
    command.{Field1},
    command.{Field2});

return {instance}Validator.ValidateOrThrow({instance});
```

**Estáticos** *(opcional)*: `{ValueObjectName}.{Static1}`, `{ValueObjectName}.{Static2}`

#### Tests Unitarios

✅ {Caso válido}
- Input: {Field1}={value1}, {Field2}={value2}
- Resultado: {ValueObjectName} creado

❌ {Caso inválido - regla 1}
- Input: {Field1}=""
- Resultado: ValidationException "{Property1} is required"

❌ {Caso inválido - regla 2}
- Input: {Property2}=-1
- Resultado: ValidationException "{Property2} must be greater than zero"

---

## 3. Entidades *(si aplica)*

### 3.1 {EntityName} (Entity)

#### Estructura

| Propiedad | Tipo | Modificador |
|-----------|------|-------------|
| Id | Guid | init |
| {Property1} | {Type} | protected set |
| {Property2} | {Type} | protected set |

#### Colecciones *(si aplica)*

```csharp
protected HashSet<{ChildValueObject}> _{collection} = [];
public IReadOnlyCollection<{ChildValueObject}> {Collection} => _{collection}.ToList().AsReadOnly();
```

#### Invariantes (Validator)

> Estas reglas se implementan en `{EntityName}Validator : AbstractValidator<{EntityName}>`

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| Id | NotEmpty | "Id is required" |
| {Property1} | NotEmpty | "{Property1} is required" |
| {Property1} | MaxLength({n}) | "{Property1} cannot exceed {n} characters" |
| {Property2} | >= 0 | "{Property2} must be greater than or equal to 0" |

#### Comando: {EntityName}.Create

**Input**

| Campo | Tipo | Default |
|-------|------|---------|
| {Field1} | {Type} | |
| {Field2} | {Type} | {DefaultValue} |

**Inyecta**: `IValidator<{EntityName}>`

**Lógica**
```csharp
var entity = new {EntityName}(Guid.NewGuid())
{
    {Property1} = command.{Property1},
    {Property2} = command.{Property2},
    IsActive = true
};

return entityValidator.ValidateOrThrow(entity);
```

#### Comando: {EntityName}.Update *(si aplica)*

**Input**

| Campo | Tipo |
|-------|------|
| {Field1} | {Type} |
| {Field2} | {Type} |

**Inyecta**: `IValidator<{EntityName}>`

**Lógica**
```csharp
entity.{Property1} = command.{Property1};
entity.{Property2} = command.{Property2};

return entityValidator.ValidateOrThrow(entity);
```

#### Tests Unitarios

✅ Crear {EntityName} válido
- Input: {Field1}={value1}
- Resultado: {EntityName} creado con IsActive=true

❌ {Property1} vacío
- Input: {Property1}=""
- Resultado: ValidationException "{Property1} is required"

---

## 4. Aggregate: {AggregateName}

### Estructura

```
{AggregateName} (Aggregate Root)
├─ Id: Guid
├─ TenantId: Guid *(si multi-tenant)*
├─ {Property1}: {Type}
├─ {Property2}: {Type}
├─ {ValueObject}: {ValueObjectType}? *(ComplexType)*
├─ {Collection1}: IReadOnlyCollection<{Entity1}>
└─ {Collection2}: IReadOnlyCollection<{ValueObject2}>
```

#### Propiedades

| Propiedad | Tipo | Modificador |
|-----------|------|-------------|
| Id | Guid | init |
| TenantId | Guid | protected set |
| {Property1} | {Type} | protected set |
| {Property2} | {Type} | protected set |
| IsActive | bool | protected set |

#### Colecciones

```csharp
protected HashSet<{Entity1}> _{collection1} = [];
public IReadOnlyCollection<{Entity1}> {Collection1} => _{collection1}.ToList().AsReadOnly();

protected HashSet<{ValueObject2}> _{collection2} = [];
public IReadOnlyCollection<{ValueObject2}> {Collection2} => _{collection2}.ToList().AsReadOnly();
```

#### Propiedades Calculadas

| Propiedad | Tipo | Fórmula |
|-----------|------|---------|
| {ComputedProperty1} | bool | `_{collection1}.Any(x => x.{Condition})` |
| {ComputedProperty2} | bool | `{ValueObject} != null` |

### Invariantes (Validator)

> Estas reglas se implementan en `{AggregateName}Validator : AbstractValidator<{AggregateName}>`

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| Id | NotEmpty | "Id is required" |
| TenantId | NotEmpty | "TenantId is required" |
| {Property1} | NotEmpty | "{Property1} is required" |
| {Property1} | MaxLength({n}) | "{Property1} cannot exceed {n} characters" |
| {Property2} | MaxLength({n}) | "{Property2} cannot exceed {n} characters" |
| {DateFrom}/{DateUntil} | {DateFrom} < {DateUntil} (si ambos presentes) | "Start date must be earlier than end date" |

---

## 5. Response

```csharp
public record {AggregateName}Response(
    Guid Id,
    Guid TenantId,
    {Type1} {Property1},
    {Type2} {Property2},
    bool IsActive,
    {ValueObjectType}Response? {ValueObject},
    IReadOnlyCollection<{Entity1}Response> {Collection1},
    IReadOnlyCollection<{ValueObject2}Response> {Collection2}
);

public record {ValueObjectType}Response(
    {Type} {Property1},
    {Type} {Property2}
);

public record {Entity1}Response(
    Guid Id,
    {Type} {Property1},
    {Type} {Property2},
    bool IsActive,
    IReadOnlyCollection<{ChildValueObject}Response> {ChildCollection}
);

public record {ValueObject2}Response(
    {Type} {Property1},
    {Type} {Property2}
);
```

---

## 6. Event Storming - Leyenda

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

## 7. Comandos

> ⚠️ **IMPORTANTE**: El orden de los comandos respeta las dependencias.
> - Las Queries (Get, List) van después de Create porque son necesarias para verificar persistencia
> - Activate/Deactivate van al final porque dependen de Add{Entity}/Add{ValueObject}

---

### 7.1 {AggregateName}.Create

#### Event Storming
```
🟡[{Actor}] → 🔵(Create{AggregateName}) → 🟤[[{AggregateName}]] → 🟠<{AggregateName}Created>
```

#### Input

| Campo | Tipo | Default |
|-------|------|---------|
| TenantId | Guid | |
| {Field1} | {Type} | |
| {Field2} | {Type}? | null |

#### Inyecta
- `IValidator<{AggregateName}>`

#### Guards

Ninguno.

#### Lógica
```csharp
var {aggregate} = new {AggregateName}(Guid.NewGuid())
{
    TenantId = command.TenantId,
    {Property1} = command.{Property1},
    {Property2} = command.{Property2},
    IsActive = false
};

return {aggregate}Validator.ValidateOrThrow({aggregate});
```

#### Slice: POST /{aggregates}

**Request**
```csharp
public record Create{AggregateName}Request(
    {Type} {Field1},
    {Type}? {Field2}
);
```

**Response**: 201 Created → `{AggregateName}Response`

#### Tests Unitarios (Dominio)

✅ Crear {aggregate} con datos válidos
- Input: TenantId=valid, {Field1}={value1}
- Resultado: {AggregateName} creado con IsActive=false

❌ {Field1} vacío
- Input: {Field1}=""
- Resultado: ValidationException "{Field1} is required"

❌ TenantId vacío
- Input: TenantId=Guid.Empty
- Resultado: ValidationException "TenantId is required"

#### Tests Integración

✅ 201 Created → {AggregateName}Response

❌ 422 → Validación fallida

---

### 7.2 Get{AggregateName}

#### Event Storming
```
🟡[{Actor}] → 🔵(Get{AggregateName}) → 🟤[[{AggregateName}]] → 📊 {AggregateName}Response
```

#### Slice: GET /{aggregates}/{id}

**Response**: 200 OK → `{AggregateName}Response`

#### Tests Unitarios (Servicio)

✅ Obtiene el {aggregate} del repositorio con el id correcto
- Verifica que repository.GetByIdAsync es llamado con el id

✅ Retorna Response mapeado correctamente
- Verifica que el Response contiene los datos del {aggregate}

#### Tests Integración

✅ 200 OK → {AggregateName}Response

❌ 404 → No encontrado

---

### 7.3 List{AggregateName}s

#### Event Storming
```
🟡[{Actor}] → 🔵(List{AggregateName}s) → 🟤[[{AggregateName}]] → 📊 {AggregateName}Response[]
```

#### Slice: GET /{aggregates}?isActive=true

**QueryParams**: `?isActive=true` (opcional)

**Response**: 200 OK → `{AggregateName}Response[]`

#### Tests Unitarios (Servicio)

✅ Retorna lista de {aggregate}s mapeados correctamente
- Verifica que el Response contiene los datos de los {aggregate}s

✅ Filtra por isActive cuando se proporciona
- Verifica que solo retorna {aggregate}s con el estado indicado

#### Tests Integración

✅ 200 OK → Array de {AggregateName}Response

✅ 200 OK → Array vacío si no hay {aggregateName}s

---

### 7.4 {AggregateName}.Update

#### Event Storming
```
🟡[{Actor}] → 🔵(Update{AggregateName}) → 🟤[[{AggregateName}]] → 🟠<{AggregateName}Updated>
```

#### Input

| Campo | Tipo |
|-------|------|
| {Field1} | {Type} |
| {Field2} | {Type}? |
| DisplayOrder | int |

#### Inyecta
- `IValidator<{AggregateName}>`

#### Guards

Ninguno.

#### Lógica
```csharp
{aggregate}.{Property1} = command.{Property1};
{aggregate}.{Property2} = command.{Property2};
{aggregate}.DisplayOrder = command.DisplayOrder;

return {aggregate}Validator.ValidateOrThrow({aggregate});
```

#### Slice: PUT /{aggregates}/{id}

**Request**
```csharp
public record Update{AggregateName}Request(
    {Type} {Field1},
    {Type}? {Field2},
    int DisplayOrder
);
```

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

✅ Actualizar {aggregate} existente
- Precondición: {AggregateName} existe
- Input: {Field1}={newValue}
- Resultado: {AggregateName} actualizado

❌ {Field1} vacío
- Input: {Field1}=""
- Resultado: ValidationException "{Field1} is required"

#### Tests Integración

✅ 204 No Content

❌ 404 → {AggregateName} no encontrado

❌ 422 → Validación fallida

---

### 7.5 {AggregateName}.Set{ValueObject} *(si aplica ComplexType opcional)*

#### Event Storming
```
🟡[{Actor}] → 🔵(Set{ValueObject}) → 🟤[[{AggregateName}]] → 🟠<{ValueObject}Configured>
```

#### Input

| Campo | Tipo | Default |
|-------|------|---------|
| {Field1} | {Type} | |
| {Field2} | {Type}? | null |

#### Inyecta
- `{ValueObject}.Create`
- `IValidator<{AggregateName}>`

#### Guards

Ninguno.

#### Lógica
```csharp
var {valueObject} = create{ValueObject}.Execute(new Create{ValueObject}Command(
    command.{Field1},
    command.{Field2}));

{aggregate}.{ValueObject} = {valueObject};

return {aggregate}Validator.ValidateOrThrow({aggregate});
```

#### Slice: PUT /{aggregates}/{id}/{value-object}

**Request**
```csharp
public record Set{ValueObject}Request(
    {Type} {Field1},
    {Type}? {Field2}
);
```

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

✅ Configurar {valueObject} válido
- Input: {Field1}={value1}
- Resultado: {ValueObject} configurado

❌ Validación de {ValueObject} falla
- Input: {Field1}=""
- Resultado: ValidationException "{Field1} is required"

#### Tests Integración

✅ 204 No Content

❌ 404 → {AggregateName} no encontrado

❌ 422 → Validación fallida

---

### 7.6 {AggregateName}.Remove{ValueObject} *(si aplica ComplexType opcional)*

#### Event Storming
```
🟡[{Actor}] → 🔵(Remove{ValueObject}) → 🟤[[{AggregateName}]] → 🟠<{ValueObject}Removed>
```

#### Input

Ninguno

#### Inyecta
- `IValidator<{AggregateName}>`

#### Guards

Ninguno.

#### Lógica
```csharp
{aggregate}.{ValueObject} = null;

return {aggregate}Validator.ValidateOrThrow({aggregate});
```

#### Slice: DELETE /{aggregates}/{id}/{value-object}

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

✅ Eliminar {valueObject} existente
- Precondición: {AggregateName} con {ValueObject} configurado
- Resultado: {ValueObject}=null

✅ Eliminar {valueObject} inexistente (idempotente)
- Precondición: {AggregateName} sin {ValueObject}
- Resultado: Sin cambios

#### Tests Integración

✅ 204 No Content

❌ 404 → {AggregateName} no encontrado

---

### 7.7 {AggregateName}.Add{Entity1}

#### Event Storming
```
🟡[{Actor}] → 🔵(Add{Entity1}) → 🟤[[{AggregateName}]] → 🟠<{Entity1}Added>
                                       │
                             🟣{Unique{Key}}
```

#### Input

| Campo | Tipo | Default |
|-------|------|---------|
| {Key} | {Type} | |
| {Field1} | {Type} | |
| {Field2} | {Type}? | null |

#### Inyecta
- `{Entity1}.Create`
- `IValidator<{AggregateName}>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| {Key} ya existe (case-insensitive si string) | 409 | ConflictGuard | "A {entity1} with this {key} already exists" |

#### Lógica
```csharp
var duplicate = {aggregate}._{collection1}.Any(e => 
    e.{Key}.Equals(command.{Key}, StringComparison.OrdinalIgnoreCase));

ConflictGuard.ThrowIf(duplicate, "A {entity1} with this {key} already exists");

var entity = create{Entity1}.Execute(new Create{Entity1}Command(
    command.{Key},
    command.{Field1},
    command.{Field2}));

{aggregate}._{collection1}.Add(entity);

return {aggregate}Validator.ValidateOrThrow({aggregate});
```

#### Slice: POST /{aggregates}/{id}/{collection1}

**Request**
```csharp
public record Add{Entity1}Request(
    {Type} {Key},
    {Type} {Field1},
    {Type}? {Field2}
);
```

**Response**: 201 Created → `{AggregateName}Response`

#### Tests Unitarios (Dominio)

✅ Añadir {entity1} válido
- Precondición: {AggregateName} sin {Entity1} con {Key}={value}
- Input: {Key}={value}, {Field1}={value1}
- Resultado: {Entity1} añadido con IsActive=true

❌ {Key} duplicado
- Precondición: {AggregateName} ya tiene {Entity1} con {Key}={value}
- Input: {Key}={value} (case-insensitive)
- Resultado: ConflictException "A {entity1} with this {key} already exists"

❌ Validación de {Entity1} falla
- Input: {Field1}=""
- Resultado: ValidationException "{Field1} is required"

#### Tests Integración

✅ 201 Created → {AggregateName}Response con {entity1} añadido

❌ 404 → {AggregateName} no encontrado

❌ 409 → {Key} duplicado

❌ 422 → Validación fallida

---

### 7.8 {AggregateName}.Update{Entity1}

#### Event Storming
```
🟡[{Actor}] → 🔵(Update{Entity1}) → 🟤[[{AggregateName}]] → 🟠<{Entity1}Updated>
                                          │
                                🟣{{Entity1}Exists}
                                🟣{Unique{Key}}
```

#### Input

| Campo | Tipo |
|-------|------|
| {Entity1}Id | Guid |
| {Key} | {Type} |
| {Field1} | {Type} |
| {Field2} | {Type}? |

#### Inyecta
- `{Entity1}.Update`
- `IValidator<{AggregateName}>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| {Entity1} no existe | 404 | NotFoundGuard | "{Entity1} not found" |
| {Key} duplicado (otra entidad) | 409 | ConflictGuard | "A {entity1} with this {key} already exists" |

#### Lógica
```csharp
var entity = {aggregate}._{collection1}.FirstOrDefault(e => e.Id == command.{Entity1}Id);

NotFoundGuard.ThrowIfNull(entity, command.{Entity1}Id);

var duplicate = {aggregate}._{collection1}.Any(e =>
    e.Id != command.{Entity1}Id &&
    e.{Key}.Equals(command.{Key}, StringComparison.OrdinalIgnoreCase));

ConflictGuard.ThrowIf(duplicate, "A {entity1} with this {key} already exists");

update{Entity1}.Execute(entity!, new Update{Entity1}Command(
    command.{Key},
    command.{Field1},
    command.{Field2}));

return {aggregate}Validator.ValidateOrThrow({aggregate});
```

#### Slice: PUT /{aggregates}/{id}/{collection1}/{entity1Id}

**Request**
```csharp
public record Update{Entity1}Request(
    {Type} {Key},
    {Type} {Field1},
    {Type}? {Field2}
);
```

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

✅ Actualizar {entity1} existente
- Precondición: {AggregateName} tiene {Entity1} con Id=X
- Input: {Entity1}Id=X, {Key}={newValue}
- Resultado: {Entity1} actualizado

✅ Renombrar con mismo {key} (sin cambio)
- Precondición: {Entity1} con {Key}={value}
- Input: {Key}={value}
- Resultado: Sin error

❌ {Entity1} no existe
- Input: {Entity1}Id=inexistente
- Resultado: NotFoundException "{Entity1} not found"

❌ {Key} duplicado con otra entidad
- Precondición: {AggregateName} tiene "{Key}1" y "{Key}2"
- Input: {Entity1}Id de "{Key}2", {Key}="{Key}1"
- Resultado: ConflictException "A {entity1} with this {key} already exists"

#### Tests Integración

✅ 204 No Content

❌ 404 → {AggregateName} o {Entity1} no encontrada

❌ 409 → {Key} duplicado

❌ 422 → Validación fallida

---

### 7.9 {AggregateName}.Remove{Entity1}

#### Event Storming
```
🟡[{Actor}] → 🔵(Remove{Entity1}) → 🟤[[{AggregateName}]] → 🟠<{Entity1}Removed>
                                          │
                                🟣{{Entity1}Exists}
                                🟣{{Entity1}Empty}
```

#### Input

| Campo | Tipo |
|-------|------|
| {Entity1}Id | Guid |

#### Inyecta
- `IValidator<{AggregateName}>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| {Entity1} no existe | 404 | NotFoundGuard | "{Entity1} not found" |
| {Entity1} tiene items | 422 | ValidationGuard | "Cannot remove a {entity1} that contains items" |

#### Lógica
```csharp
var entity = {aggregate}._{collection1}.FirstOrDefault(e => e.Id == command.{Entity1}Id);

NotFoundGuard.ThrowIfNull(entity, command.{Entity1}Id);

ValidationGuard.ThrowIf(
    entity!.{ChildCollection}.Count != 0,
    "Cannot remove a {entity1} that contains items",
    "{Entity1}Id");

{aggregate}._{collection1}.Remove(entity);

return {aggregate}Validator.ValidateOrThrow({aggregate});
```

#### Slice: DELETE /{aggregates}/{id}/{collection1}/{entity1Id}

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

✅ Eliminar {entity1} vacío
- Precondición: {Entity1} sin items
- Resultado: {Entity1} eliminado

❌ {Entity1} no existe
- Resultado: NotFoundException "{Entity1} not found"

❌ {Entity1} con items
- Precondición: {Entity1} tiene items
- Resultado: ValidationException "Cannot remove a {entity1} that contains items"

#### Tests Integración

✅ 204 No Content

❌ 404 → {AggregateName} o {Entity1} no encontrada

❌ 422 → {Entity1} tiene items

---

### 7.10 {AggregateName}.Add{ChildItem}To{Entity1}

#### Event Storming
```
🟡[{Actor}] → 🔵(Add{ChildItem}To{Entity1}) → 🟤[[{AggregateName}]] → 🟠<{ChildItem}AddedTo{Entity1}>
                                                    │
                                          🟣{{Entity1}Exists}
                                          🟣{{ChildItem}NotDuplicate}
```

#### Input

| Campo | Tipo | Default |
|-------|------|---------|
| {Entity1}Id | Guid | |
| {ChildItemRef}Id | Guid | |
| DisplayOrder | int | 0 |

#### Inyecta
- `{Entity1}.Add{ChildItem}`
- `IValidator<{AggregateName}>`
- `I{ChildItemRef}Repository` (para obtener el {ChildItemRef})

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| {Entity1} no existe | 404 | NotFoundGuard | "{Entity1} not found" |
| {ChildItemRef} no existe | 404 | NotFoundGuard | "{ChildItemRef} not found" |
| {ChildItem} ya existe en {Entity1} | 409 | ConflictGuard | "This item already exists in the {entity1}" |

#### Lógica
```csharp
var entity = {aggregate}._{collection1}.FirstOrDefault(e => e.Id == command.{Entity1}Id);
NotFoundGuard.ThrowIfNull(entity, command.{Entity1}Id);

var childRef = await {childItemRef}Repository.GetByIdAsync(command.{ChildItemRef}Id);
NotFoundGuard.ThrowIfNull(childRef, command.{ChildItemRef}Id);

add{ChildItem}.Execute(entity!, new Add{ChildItem}Command(
    childRef!,
    command.DisplayOrder));

return {aggregate}Validator.ValidateOrThrow({aggregate});
```

#### Slice: POST /{aggregates}/{id}/{collection1}/{entity1Id}/items

**Request**
```csharp
public record Add{ChildItem}Request(
    Guid {ChildItemRef}Id,
    int DisplayOrder = 0
);
```

**Response**: 201 Created → `{AggregateName}Response`

#### Tests Unitarios (Dominio)

✅ Añadir item a {entity1}
- Precondición: {Entity1} existe, {ChildItemRef} existe
- Input: {Entity1}Id=X, {ChildItemRef}Id=Y
- Resultado: {ChildItem} añadido

❌ {Entity1} no existe
- Resultado: NotFoundException "{Entity1} not found"

❌ {ChildItemRef} no existe
- Resultado: NotFoundException "{ChildItemRef} not found"

❌ {ChildItem} duplicado
- Precondición: {ChildItem} ya está en la {entity1}
- Resultado: ConflictException "This item already exists in the {entity1}"

#### Tests Integración

✅ 201 Created → {AggregateName}Response con item añadido

❌ 404 → {AggregateName}, {Entity1} o {ChildItemRef} no encontrado

❌ 409 → Item duplicado

❌ 422 → Validación fallida

---

### 7.11 {AggregateName}.Update{ChildItem}In{Entity1}

*(Patrón similar a 7.10 pero con PUT)*

---

### 7.12 {AggregateName}.Remove{ChildItem}From{Entity1}

*(Patrón similar a 7.9 pero para items dentro de la entidad)*

---

### 7.13 {AggregateName}.Activate

> ⚠️ **Dependencias**: Requiere que existan {Collection1} con items.
> Por eso este comando va después de Add{Entity1} y Add{ChildItem}.

#### Event Storming
```
🟡[{Actor}] → 🔵(Activate{AggregateName}) → 🟤[[{AggregateName}]] → 🟠<{AggregateName}Activated>
                                                 │
                                       🟣{Tiene{Collection1}}
                                       🟣{TieneItemsEn{Collection1}}
```

#### Input

Ninguno

#### Inyecta
- `IValidator<{AggregateName}>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Ya está activo | 409 | ConflictGuard | "{AggregateName} is already active" |
| No tiene {Collection1} | 422 | ValidationGuard | "{AggregateName} must have at least one {entity1}" |
| Ninguna {Entity1} tiene items | 422 | ValidationGuard | "{AggregateName} must have at least one {entity1} with items" |

#### Lógica
```csharp
ConflictGuard.ThrowIf({aggregate}.IsActive, "{AggregateName} is already active");

ValidationGuard.ThrowIf(
    !{aggregate}.{Collection1}.Any(),
    "{AggregateName} must have at least one {entity1}",
    nameof({aggregate}.{Collection1}));

ValidationGuard.ThrowIf(
    !{aggregate}.{Collection1}.Any(e => e.{ChildCollection}.Any()),
    "{AggregateName} must have at least one {entity1} with items",
    nameof({aggregate}.{Collection1}));

{aggregate}.IsActive = true;

return {aggregate}Validator.ValidateOrThrow({aggregate});
```

#### Slice: POST /{aggregates}/{id}/activate

**Response**: 200 OK → `{AggregateName}Response`

#### Tests Unitarios (Dominio)

✅ Activar {aggregate} con {collection1} e items
- Precondición: {AggregateName} con IsActive=false, tiene {entity1} con items
- Resultado: {AggregateName} con IsActive=true

❌ Ya activo
- Precondición: {AggregateName} con IsActive=true
- Resultado: ConflictException "{AggregateName} is already active"

❌ Sin {collection1}
- Precondición: {AggregateName} sin {collection1}
- Resultado: ValidationException "{AggregateName} must have at least one {entity1}"

❌ {Collection1} vacíos
- Precondición: {AggregateName} con {collection1} pero sin items en ninguna
- Resultado: ValidationException "{AggregateName} must have at least one {entity1} with items"

#### Tests Integración

✅ 200 OK → {AggregateName}Response con IsActive=true

❌ 404 → {AggregateName} no encontrado

❌ 409 → Ya estaba activo

❌ 422 → Falta {entity1} o items

---

### 7.14 {AggregateName}.Deactivate

#### Event Storming
```
🟡[{Actor}] → 🔵(Deactivate{AggregateName}) → 🟤[[{AggregateName}]] → 🟠<{AggregateName}Deactivated>
```

#### Input

Ninguno

#### Inyecta
- `IValidator<{AggregateName}>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Ya está inactivo | 409 | ConflictGuard | "{AggregateName} is already inactive" |

#### Lógica
```csharp
ConflictGuard.ThrowIf(!{aggregate}.IsActive, "{AggregateName} is already inactive");

{aggregate}.IsActive = false;

return {aggregate}Validator.ValidateOrThrow({aggregate});
```

#### Slice: POST /{aggregates}/{id}/deactivate

**Response**: 200 OK → `{AggregateName}Response`

#### Tests Unitarios (Dominio)

✅ Desactivar {aggregate} activo
- Precondición: {AggregateName} con IsActive=true
- Resultado: {AggregateName} con IsActive=false

❌ Ya inactivo
- Precondición: {AggregateName} con IsActive=false
- Resultado: ConflictException "{AggregateName} is already inactive"

#### Tests Integración

✅ 200 OK → {AggregateName}Response con IsActive=false

❌ 404 → {AggregateName} no encontrado

❌ 409 → Ya estaba inactivo

---

## 8. Resumen de Endpoints (Orden de Implementación)

| # | Método | Ruta | Comando/Query | Response |
|---|--------|------|---------------|----------|
| 1 | POST | /{aggregates} | {AggregateName}.Create | 201 → `{AggregateName}Response` |
| 2 | GET | /{aggregates}/{id} | Get{AggregateName} | 200 → `{AggregateName}Response` |
| 3 | GET | /{aggregates} | List{AggregateName}s | 200 → `{AggregateName}Response[]` |
| 4 | PUT | /{aggregates}/{id} | {AggregateName}.Update | 204 |
| 5 | PUT | /{aggregates}/{id}/{value-object} | {AggregateName}.Set{ValueObject} | 204 |
| 6 | DELETE | /{aggregates}/{id}/{value-object} | {AggregateName}.Remove{ValueObject} | 204 |
| 7 | POST | /{aggregates}/{id}/{collection1} | {AggregateName}.Add{Entity1} | 201 → `{AggregateName}Response` |
| 8 | PUT | /{aggregates}/{id}/{collection1}/{entity1Id} | {AggregateName}.Update{Entity1} | 204 |
| 9 | DELETE | /{aggregates}/{id}/{collection1}/{entity1Id} | {AggregateName}.Remove{Entity1} | 204 |
| 10 | POST | /{aggregates}/{id}/{collection1}/{entity1Id}/items | {AggregateName}.Add{ChildItem} | 201 → `{AggregateName}Response` |
| 11 | PUT | /{aggregates}/{id}/{collection1}/{entity1Id}/items/{itemId} | {AggregateName}.Update{ChildItem} | 204 |
| 12 | DELETE | /{aggregates}/{id}/{collection1}/{entity1Id}/items/{itemId} | {AggregateName}.Remove{ChildItem} | 204 |
| 13 | POST | /{aggregates}/{id}/activate | {AggregateName}.Activate | 200 → `{AggregateName}Response` |
| 14 | POST | /{aggregates}/{id}/deactivate | {AggregateName}.Deactivate | 200 → `{AggregateName}Response` |

---

## 9. Persistencia (Firestore)

### Colección

`/{aggregates}/{aggregateId}`

### Configuración DbContext

```csharp
modelBuilder.Entity<{AggregateName}>(entity =>
{
    // QueryFilter: multi-tenancy by TenantId
    entity.HasQueryFilter(x => x.TenantId == tenantId);

    // Ignore: propiedades computed (no backing fields)
    entity.Ignore(x => x.{ComputedProperty1});
    entity.Ignore(x => x.{ComputedProperty2});

    // ComplexType: {ValueObject} (nullable)
    entity.ComplexProperty(x => x.{ValueObject}, vo =>
    {
        // Ignore: propiedades computed de {ValueObject}
        vo.Ignore(v => v.{ComputedProperty});
    });

    // ArrayOf: {Collection1} (usa backing field _{collection1})
    entity.ArrayOf(x => x.{Collection1}, item =>
    {
        // ArrayOf anidado: {ChildCollection} dentro de {Entity1}
        item.ArrayOf(i => i.{ChildCollection}, child =>
        {
            // AsReference: referencia a otro aggregate
            child.AsReference(c => c.{Reference});
            
            // Ignore: propiedades computed
            child.Ignore(c => c.{ComputedProperty});
        });
    });
});
```

### Documento Ejemplo

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "tenantId": "tenant-001-guid",
  "{property1}": "{value1}",
  "{property2}": "{value2}",
  "isActive": true,
  "{valueObject}": {
    "{field1}": "{value}",
    "{field2}": "{value}"
  },
  "{collection1}": [
    {
      "id": "{entity1-guid}",
      "{key}": "{keyValue}",
      "{field1}": "{value1}",
      "isActive": true,
      "{childCollection}": [
        {
          "{referenceId}": "{reference-guid}",
          "displayOrder": 1
        }
      ]
    }
  ]
}
```

---

## 10. Hot Spots ⚠️

| # | Pregunta | Estado |
|---|----------|--------|
| 1 | {Pregunta pendiente 1} | Pendiente |
| 2 | {Pregunta pendiente 2} | Decidido: {decisión} |

---

**Fecha**: {YYYY-MM-DD}
**Autor**: {Equipo}
