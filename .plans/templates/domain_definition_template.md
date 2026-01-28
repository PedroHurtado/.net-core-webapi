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

#### Validaciones

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| {Property1} | {Rule} | "{Message}" |
| {Property2} | {Rule} | "{Message}" |

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

❌ {Caso inválido}
- Input: {Field1}={invalidValue}
- Resultado: ValidationException "{Message}"

---

## 3. Aggregate: {AggregateName}

### Estructura

```
{AggregateName} (Aggregate Root)
├─ Id: Guid
├─ {Property1}: {Type}
├─ {Property2}: {Type}
├─ {Collection1}: IReadOnlyCollection<{ValueObject1}>
└─ {Collection2}: IReadOnlyCollection<{ValueObject2}>
```

#### Propiedades

| Propiedad | Tipo | Modificador |
|-----------|------|-------------|
| Id | Guid | init |
| {Property1} | {Type} | protected set |
| {Property2} | {Type} | protected set |

#### Colecciones

```csharp
protected HashSet<{ValueObject1}> _{collection1} = [];
public IReadOnlyCollection<{ValueObject1}> {Collection1} => _{collection1}.ToList().AsReadOnly();

protected HashSet<{ValueObject2}> _{collection2} = [];
public IReadOnlyCollection<{ValueObject2}> {Collection2} => _{collection2}.ToList().AsReadOnly();
```

#### Propiedades Calculadas

| Propiedad | Tipo | Fórmula |
|-----------|------|---------|
| {Property} | bool | `_{collection}.Any(x => x.{Condition})` |

### Validaciones

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| Id | NotEmpty | "Id is required" |
| {Property1} | NotEmpty | "{Property1} is required" |
| {Property1} | Max({n}) | "{Property1} cannot exceed {n} characters" |

---

## 4. Response

```csharp
public record {AggregateName}Response(
    Guid Id,
    {Type1} {Property1},
    {Type2} {Property2},
    IReadOnlyCollection<{ValueObject1}Response> {Collection1},
    IReadOnlyCollection<{ValueObject2}Response> {Collection2}
);

public record {ValueObject1}Response(
    {Type} {Property1},
    {Type} {Property2}
);

public record {ValueObject2}Response(
    {Type} {Property1},
    {Type} {Property2}
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

## 6. Comandos y Slices (Orden de Implementación)

> ⚠️ **IMPORTANTE**: El orden de los comandos respeta las dependencias.
> - Las Queries (Get, List) van después de Create porque son necesarias para verificar persistencia
> - Activate/Deactivate van al final porque dependen de Add{ValueObject}

---

### 6.1 {AggregateName}.Create

#### Event Storming
```
🟡[{Actor}] → 🔵(Create{AggregateName}) → 🟤[[{AggregateName}]] → 🟠<{AggregateName}Created>
```

#### Dominio

**Input**

| Campo | Tipo |
|-------|------|
| {Field1} | {Type} |
| {Field2} | {Type} |

**Inyecta**
- `{ValueObject}.Create`
- `IValidator<{AggregateName}>`

**Guards**

Ninguno.

**Lógica**
```csharp
var {valueObject} = {valueObject}Create.Execute(new Create{ValueObject}Command(command.{Field1}, command.{Field2}));

var {aggregate} = new {AggregateName}(Guid.NewGuid())
{
    {Property1} = command.{Property1},
    {Property2} = {valueObject},
    IsActive = false
};

return {aggregate}Validator.ValidateOrThrow({aggregate});
```

**Tests Unitarios Dominio**

✅ Crear {aggregate} con datos válidos
- Input: {Field1}={value1}, {Field2}={value2}
- Resultado: {AggregateName} creado con IsActive=false

❌ {Field1} vacío
- Input: {Field1}=""
- Resultado: ValidationException "{Field1} is required"

#### Slice: POST /{aggregates}

**Request**
```csharp
public record Create{AggregateName}Request(
    {Type} {Field1},
    {Type} {Field2}
);
```

**Response**: 201 Created → `{AggregateName}Response`

**Tests Unitarios Servicio**

✅ Llama a {AggregateName}.Create con los parámetros correctos
- Verifica que se invoca {aggregate}Create.Execute con el command correcto

✅ Añade el aggregate al repositorio
- Verifica que repository.Add es llamado con el {aggregate} creado

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

✅ Retorna Response mapeado correctamente
- Verifica que el Response contiene los datos del {aggregate}

**Tests Integración**

✅ 201 Created → {AggregateName}Response

❌ 422 → Validación fallida

---

### 6.2 Get{AggregateName}

#### Event Storming
```
🟡[{Actor}] → 🔵(Get{AggregateName}) → 🟤[[{AggregateName}]] → 📊 {AggregateName}Response
```

#### Slice: GET /{aggregates}/{id}

**Response**: 200 OK → `{AggregateName}Response`

**Tests Unitarios Servicio**

✅ Obtiene el {aggregate} del repositorio con el id correcto
- Verifica que repository.Get es llamado con el id

✅ Retorna Response mapeado correctamente
- Verifica que el Response contiene los datos del {aggregate}

**Tests Integración**

✅ 200 OK → {AggregateName}Response

❌ 404 → No encontrado

---

### 6.3 List{AggregateName}s

#### Event Storming
```
🟡[{Actor}] → 🔵(List{AggregateName}s) → 🟤[[{AggregateName}]] → 📊 {AggregateName}Response[]
```

#### Slice: GET /{aggregates}

**QueryParams**: `?isActive=true` (opcional)

**Response**: 200 OK → `{AggregateName}Response[]`

**Tests Unitarios Servicio**

✅ Retorna lista de {aggregate}s mapeados correctamente
- Verifica que el Response contiene los datos de los {aggregate}s

✅ Filtra por isActive cuando se proporciona
- Verifica que solo retorna {aggregate}s con el estado indicado

**Tests Integración**

✅ 200 OK → Array de {AggregateName}Response

✅ 200 OK → Array vacío si no hay {aggregateName}s

---

### 6.4 {AggregateName}.Update

#### Event Storming
```
🟡[{Actor}] → 🔵(Update{AggregateName}) → 🟤[[{AggregateName}]] → 🟠<{AggregateName}Updated>
```

#### Dominio

**Input**

| Campo | Tipo |
|-------|------|
| {Field1} | {Type} |
| {Field2} | {Type} |

**Inyecta**
- `{ValueObject}.Create`
- `IValidator<{AggregateName}>`

**Guards**

Ninguno.

**Lógica**
```csharp
var {valueObject} = {valueObject}Create.Execute(new Create{ValueObject}Command(command.{Field1}, command.{Field2}));

{aggregate}.{Property1} = command.{Property1};
{aggregate}.{Property2} = {valueObject};

return {aggregate}Validator.ValidateOrThrow({aggregate});
```

**Tests Unitarios Dominio**

✅ Actualizar {aggregate} existente
- Precondición: {AggregateName} existe
- Input: {Field1}={newValue}
- Resultado: {AggregateName} actualizado

❌ {Field1} vacío
- Input: {Field1}=""
- Resultado: ValidationException "{Field1} is required"

#### Slice: PUT /{aggregates}/{id}

**Request**
```csharp
public record Update{AggregateName}Request(
    {Type} {Field1},
    {Type} {Field2}
);
```

**Response**: 204 No Content

**Tests Unitarios Servicio**

✅ Obtiene el {aggregate} del repositorio
- Verifica que repository.Get es llamado con el id correcto

✅ Llama a {AggregateName}.Update con los parámetros correctos
- Verifica que se invoca {aggregate}Update.Execute con el command correcto

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

**Tests Integración**

✅ 204 No Content

✅ Persistencia: PUT → GET → verificar datos actualizados

❌ 404 → {AggregateName} no encontrado

❌ 422 → Validación fallida

---

### 6.5 {AggregateName}.Add{ValueObject1}

#### Event Storming
```
🟡[{Actor}] → 🔵(Add{ValueObject1}) → 🟤[[{AggregateName}]] → 🟠<{ValueObject1}Added>
                                          │
                                🟣{{Key}Único}
```

#### Dominio

**Input**

| Campo | Tipo |
|-------|------|
| {Key} | {Type} |
| {Field1} | {Type} |
| {Field2} | {Type}? |

**Inyecta**
- `{ValueObject1}.Create`
- `IValidator<{AggregateName}>`

**Guards**

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| {Key} ya existe | 409 | ConflictGuard | "{ValueObject1} with {key} '{Key}' already exists" |

**Lógica**
```csharp
ConflictGuard.ThrowIf(
    {aggregate}.{Collection1}.Any(x => x.{Key} == command.{Key}),
    $"{ValueObject1} with {key} '{command.{Key}}' already exists");

var {valueObject1} = {valueObject1}Create.Execute(new Create{ValueObject1}Command(
    command.{Key},
    command.{Field1},
    command.{Field2}));

{aggregate}._{collection1}.Add({valueObject1});

return {aggregate}Validator.ValidateOrThrow({aggregate});
```

**Tests Unitarios Dominio**

✅ Añadir {valueObject1} válido
- Precondición: {AggregateName} sin {ValueObject1} con {Key}={value}
- Input: {Key}={value}, {Field1}={value1}
- Resultado: {ValueObject1} añadido

❌ {Key} duplicado
- Precondición: {AggregateName} ya tiene {ValueObject1} con {Key}={value}
- Input: {Key}={value}
- Resultado: ConflictException "{ValueObject1} with {key} '{value}' already exists"

❌ Validación de {ValueObject1} falla
- Input: {Field1}=""
- Resultado: ValidationException "{Field1} is required"

#### Slice: POST /{aggregates}/{id}/{collection1}

**Request**
```csharp
public record Add{ValueObject1}Request(
    {Type} {Key},
    {Type} {Field1},
    {Type}? {Field2}
);
```

**Response**: 201 Created → `{AggregateName}Response`

**Tests Unitarios Servicio**

✅ Obtiene el {aggregate} del repositorio
- Verifica que repository.Get es llamado con el id correcto

✅ Llama a {AggregateName}.Add{ValueObject1} con los parámetros correctos
- Verifica que se invoca add{ValueObject1}.Execute con el command correcto

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

✅ Retorna Response mapeado correctamente
- Verifica que el Response contiene el {ValueObject1} añadido

**Tests Integración**

✅ 201 Created → {AggregateName}Response con {ValueObject1} añadido

✅ Persistencia: POST → GET → verificar {ValueObject1} añadido

❌ 404 → {AggregateName} no encontrado

❌ 409 → {Key} duplicado

❌ 422 → Validación fallida

---

### 6.6 {AggregateName}.Update{ValueObject1}

#### Event Storming
```
🟡[{Actor}] → 🔵(Update{ValueObject1}) → 🟤[[{AggregateName}]] → 🟠<{ValueObject1}Updated>
                                             │
                                   🟣{{ValueObject1}Existe}
```

#### Dominio

**Input**

| Campo | Tipo |
|-------|------|
| {Field1} | {Type} |
| {Field2} | {Type}? |

*{Key} viene en la ruta*

**Inyecta**
- `{ValueObject1}.Create`
- `IValidator<{AggregateName}>`

**Guards**

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| {ValueObject1} no existe | 404 | NotFoundGuard | "{ValueObject1} with {key} '{Key}' not found" |

**Lógica**
```csharp
var existing = {aggregate}.{Collection1}.FirstOrDefault(x => x.{Key} == {key});
NotFoundGuard.ThrowIfNull(existing, $"{ValueObject1} with {key} '{{key}}' not found");

var updated = {valueObject1}Create.Execute(new Create{ValueObject1}Command(
    {key},
    command.{Field1},
    command.{Field2}));

{aggregate}._{collection1}.Remove(existing);
{aggregate}._{collection1}.Add(updated);

return {aggregate}Validator.ValidateOrThrow({aggregate});
```

**Tests Unitarios Dominio**

✅ Actualizar {valueObject1} existente
- Precondición: {AggregateName} tiene {ValueObject1} con {Key}={value}
- Input: {Field1}={newValue}
- Resultado: {ValueObject1} actualizado

❌ {ValueObject1} no existe
- Precondición: {AggregateName} no tiene {ValueObject1} con {Key}={value}
- Resultado: NotFoundException "{ValueObject1} with {key} '{value}' not found"

#### Slice: PUT /{aggregates}/{id}/{collection1}/{key}

**Request**
```csharp
public record Update{ValueObject1}Request(
    {Type} {Field1},
    {Type}? {Field2}
);
```

**Response**: 204 No Content

**Tests Unitarios Servicio**

✅ Obtiene el {aggregate} del repositorio
- Verifica que repository.Get es llamado con el id correcto

✅ Llama a {AggregateName}.Update{ValueObject1} con los parámetros correctos
- Verifica que se invoca update{ValueObject1}.Execute con el {key} y command correctos

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

**Tests Integración**

✅ 204 No Content

✅ Persistencia: PUT → GET → verificar {ValueObject1} actualizado

❌ 404 → {AggregateName} o {ValueObject1} no encontrado

❌ 422 → Validación fallida

---

### 6.7 {AggregateName}.Remove{ValueObject1}

#### Event Storming
```
🟡[{Actor}] → 🔵(Remove{ValueObject1}) → 🟤[[{AggregateName}]] → 🟠<{ValueObject1}Removed>
                                             │
                                   🟣{{ValueObject1}Existe}
                                   🟣{NoEsElÚltimo}
```

#### Dominio

**Input**

*{Key} viene en la ruta*

**Inyecta**
- `IValidator<{AggregateName}>`

**Guards**

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| {ValueObject1} no existe | 404 | NotFoundGuard | "{ValueObject1} with {key} '{Key}' not found" |
| Es el último y {AggregateName} activo | 422 | ValidationGuard | "Cannot remove last {valueObject1} from active {aggregateName}" |

**Lógica**
```csharp
var existing = {aggregate}.{Collection1}.FirstOrDefault(x => x.{Key} == {key});
NotFoundGuard.ThrowIfNull(existing, $"{ValueObject1} with {key} '{{key}}' not found");

ValidationGuard.ThrowIf(
    {aggregate}.IsActive && {aggregate}.{Collection1}.Count <= 1,
    "Cannot remove last {valueObject1} from active {aggregateName}",
    nameof({aggregate}.{Collection1}));

{aggregate}._{collection1}.Remove(existing);

return {aggregate}Validator.ValidateOrThrow({aggregate});
```

**Tests Unitarios Dominio**

✅ Eliminar {valueObject1} (hay varios)
- Precondición: {AggregateName} con múltiples {ValueObject1}s
- Resultado: {ValueObject1} eliminado

✅ Eliminar último {valueObject1} ({aggregateName} inactivo)
- Precondición: {AggregateName} inactivo con 1 {ValueObject1}
- Resultado: {ValueObject1} eliminado

❌ {ValueObject1} no existe
- Resultado: NotFoundException "{ValueObject1} with {key} '{value}' not found"

❌ Último {valueObject1} en {aggregateName} activo
- Precondición: {AggregateName} activo con 1 {ValueObject1}
- Resultado: ValidationException "Cannot remove last {valueObject1} from active {aggregateName}"

#### Slice: DELETE /{aggregates}/{id}/{collection1}/{key}

**Response**: 204 No Content

**Tests Unitarios Servicio**

✅ Obtiene el {aggregate} del repositorio
- Verifica que repository.Get es llamado con el id correcto

✅ Llama a {AggregateName}.Remove{ValueObject1} con el {key} correcto
- Verifica que se invoca remove{ValueObject1}.Execute con el {key}

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

**Tests Integración**

✅ 204 No Content

✅ Persistencia: DELETE → GET → verificar {ValueObject1} eliminado

❌ 404 → {AggregateName} o {ValueObject1} no encontrado

❌ 422 → Es el último en {aggregateName} activo

---

### 6.8 {AggregateName}.Add{ValueObject2}

*(Repetir patrón 6.5 para {ValueObject2})*

---

### 6.9 {AggregateName}.Update{ValueObject2}

*(Repetir patrón 6.6 para {ValueObject2})*

---

### 6.10 {AggregateName}.Remove{ValueObject2}

*(Repetir patrón 6.7 para {ValueObject2})*

---

### 6.11 {AggregateName}.Activate

> ⚠️ **Dependencias**: Requiere que existan {Collection1} y {Collection2} activos.
> Por eso este comando va después de Add{ValueObject1} y Add{ValueObject2}.

#### Event Storming
```
🟡[{Actor}] → 🔵(Activate{AggregateName}) → 🟤[[{AggregateName}]] → 🟠<{AggregateName}Activated>
                                                │
                                      🟣{Tiene{Collection1}}
                                      🟣{Tiene{Collection2}Activo}
```

#### Dominio

**Input**

Ninguno

**Inyecta**
- `IValidator<{AggregateName}>`

**Guards**

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Ya está activo | 409 | ConflictGuard | "{AggregateName} is already active" |
| No tiene {Collection1} | 422 | ValidationGuard | "{AggregateName} must have at least one {valueObject1}" |
| No tiene {Collection2} activo | 422 | ValidationGuard | "{AggregateName} must have at least one active {valueObject2}" |

**Lógica**
```csharp
ConflictGuard.ThrowIf({aggregate}.IsActive, "{AggregateName} is already active");
ValidationGuard.ThrowIf(!{aggregate}.{Collection1}.Any(), "{AggregateName} must have at least one {valueObject1}", nameof({aggregate}.{Collection1}));
ValidationGuard.ThrowIf(!{aggregate}.HasActive{ValueObject2}, "{AggregateName} must have at least one active {valueObject2}", nameof({aggregate}.{Collection2}));

{aggregate}.IsActive = true;

return {aggregate}Validator.ValidateOrThrow({aggregate});
```

**Tests Unitarios Dominio**

✅ Activar {aggregate} completo
- Precondición: {AggregateName} con {Collection1} y {Collection2} activo, IsActive=false
- Resultado: {AggregateName} con IsActive=true

❌ Ya activo
- Precondición: {AggregateName} con IsActive=true
- Resultado: ConflictException "{AggregateName} is already active"

❌ Sin {Collection1}
- Precondición: {AggregateName} sin {Collection1}
- Resultado: ValidationException "{AggregateName} must have at least one {valueObject1}"

❌ Sin {Collection2} activo
- Precondición: {AggregateName} con {Collection1}, sin {Collection2} activo
- Resultado: ValidationException "{AggregateName} must have at least one active {valueObject2}"

#### Slice: POST /{aggregates}/{id}/activate

**Response**: 200 OK → `{AggregateName}Response`

**Tests Unitarios Servicio**

✅ Obtiene el {aggregate} del repositorio
- Verifica que repository.Get es llamado con el id correcto

✅ Llama a {AggregateName}.Activate
- Verifica que se invoca {aggregate}Activate.Execute

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

✅ Retorna Response mapeado correctamente
- Verifica que el Response contiene IsActive=true

**Tests Integración**

✅ 200 OK → {AggregateName}Response con IsActive=true

✅ Persistencia: POST → GET → verificar IsActive=true

❌ 404 → {AggregateName} no encontrado

❌ 409 → Ya estaba activo

❌ 422 → Falta {Collection1} o {Collection2} activo

---

### 6.12 {AggregateName}.Deactivate

#### Event Storming
```
🟡[{Actor}] → 🔵(Deactivate{AggregateName}) → 🟤[[{AggregateName}]] → 🟠<{AggregateName}Deactivated>
```

#### Dominio

**Input**

Ninguno

**Inyecta**
- `IValidator<{AggregateName}>`

**Guards**

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Ya está inactivo | 409 | ConflictGuard | "{AggregateName} is already inactive" |

**Lógica**
```csharp
ConflictGuard.ThrowIf(!{aggregate}.IsActive, "{AggregateName} is already inactive");

{aggregate}.IsActive = false;

return {aggregate}Validator.ValidateOrThrow({aggregate});
```

**Tests Unitarios Dominio**

✅ Desactivar {aggregate} activo
- Precondición: {AggregateName} con IsActive=true
- Resultado: {AggregateName} con IsActive=false

❌ Ya inactivo
- Precondición: {AggregateName} con IsActive=false
- Resultado: ConflictException "{AggregateName} is already inactive"

#### Slice: POST /{aggregates}/{id}/deactivate

**Response**: 200 OK → `{AggregateName}Response`

**Tests Unitarios Servicio**

✅ Obtiene el {aggregate} del repositorio
- Verifica que repository.Get es llamado con el id correcto

✅ Llama a {AggregateName}.Deactivate
- Verifica que se invoca {aggregate}Deactivate.Execute

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

✅ Retorna Response mapeado correctamente
- Verifica que el Response contiene IsActive=false

**Tests Integración**

✅ 200 OK → {AggregateName}Response con IsActive=false

✅ Persistencia: POST → GET → verificar IsActive=false

❌ 404 → {AggregateName} no encontrado

❌ 409 → Ya estaba inactivo

---

## 7. Resumen de Endpoints (Orden de Implementación)

| # | Método | Ruta | Comando/Query | Response |
|---|--------|------|---------------|----------|
| 1 | POST | /{aggregates} | {AggregateName}.Create | 201 → `{AggregateName}Response` |
| 2 | GET | /{aggregates}/{id} | Get{AggregateName} | 200 → `{AggregateName}Response` |
| 3 | GET | /{aggregates} | List{AggregateName}s | 200 → `{AggregateName}Response[]` |
| 4 | PUT | /{aggregates}/{id} | {AggregateName}.Update | 204 |
| 5 | POST | /{aggregates}/{id}/{collection1} | {AggregateName}.Add{ValueObject1} | 201 → `{AggregateName}Response` |
| 6 | PUT | /{aggregates}/{id}/{collection1}/{key} | {AggregateName}.Update{ValueObject1} | 204 |
| 7 | DELETE | /{aggregates}/{id}/{collection1}/{key} | {AggregateName}.Remove{ValueObject1} | 204 |
| 8 | POST | /{aggregates}/{id}/{collection2} | {AggregateName}.Add{ValueObject2} | 201 → `{AggregateName}Response` |
| 9 | PUT | /{aggregates}/{id}/{collection2}/{key} | {AggregateName}.Update{ValueObject2} | 204 |
| 10 | DELETE | /{aggregates}/{id}/{collection2}/{key} | {AggregateName}.Remove{ValueObject2} | 204 |
| 11 | POST | /{aggregates}/{id}/activate | {AggregateName}.Activate | 200 → `{AggregateName}Response` |
| 12 | POST | /{aggregates}/{id}/deactivate | {AggregateName}.Deactivate | 200 → `{AggregateName}Response` |

---

## 8. Persistencia (Firestore)

### Colección

`/{aggregates}/{aggregateId}`

### Configuración DbContext

```csharp
modelBuilder.Entity<{AggregateName}Agg>(entity =>
{
    // Ignore: propiedades computed (no backing fields)
    entity.Ignore(x => x.{ComputedProperty});            

    // ComplexType: {ValueObject} (con nested {NestedValueObject})
    entity.ComplexProperty(x => x.{ValueObject}, vo =>
    {
        // Ignore: propiedades computed de {ValueObject}
        vo.Ignore(v => v.{ComputedProperty1});
        vo.Ignore(v => v.{ComputedProperty2});

        vo.ComplexProperty(v => v.{NestedValueObject});
    });

    // ArrayOf: {Collection1} (usa backing field _{collection1})
    entity.ArrayOf(x => x.{Collection1}, item =>
    {
        // Ignore: propiedades computed de {ItemType}
        item.Ignore(i => i.{ComputedProperty});
    });

    // ArrayOf: {Collection2} (usa backing field _{collection2})
    entity.ArrayOf(x => x.{Collection2});
});
```

### Documento Ejemplo

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "{property1}": "{value1}",
  "{property2}": "{value2}",
  "{valueObject}": {
    "{field1}": "{value}",
    "{nestedValueObject}": {
      "{nestedField}": "{nestedValue}"
    }
  },
  "{collection1}": [
    {
      "{key}": "{keyValue}",
      "{field1}": "{value1}"
    }
  ],
  "{collection2}": [
    {
      "{key}": "{keyValue}",
      "{field1}": "{value1}"
    }
  ]
}
```

---

## 9. Hot Spots ⚠️

| # | Pregunta | Estado |
|---|----------|--------|
| 1 | {Pregunta pendiente 1} | Pendiente |
| 2 | {Pregunta pendiente 2} | Pendiente |
| 3 | {Pregunta pendiente 3} | Decidido: {decisión} |

---

**Fecha**: {YYYY-MM-DD}
**Autor**: {Equipo}