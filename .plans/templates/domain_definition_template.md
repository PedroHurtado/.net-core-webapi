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

## 6. Comandos

---

### 6.1 {AggregateName}.Create

#### Event Storming
```
🟡[{Actor}] → 🔵(Create{AggregateName}) → 🟤[[{AggregateName}]] → 🟠<{AggregateName}Created>
```

#### Input

| Campo | Tipo |
|-------|------|
| {Field1} | {Type} |
| {Field2} | {Type} |

#### Inyecta
- `{ValueObject}.Create`
- `IValidator<{AggregateName}>`

#### Guards
Ninguno.

#### Lógica
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

#### Slice: POST /{aggregates}

**Request**
```csharp
public record Create{AggregateName}Request(
    {Type} {Field1},
    {Type} {Field2}
);
```

**Response**: 201 Created → `{AggregateName}Response`

#### Tests Unitarios (Dominio)

✅ Crear {aggregate} con datos válidos
- Input: {Field1}={value1}, {Field2}={value2}
- Resultado: {AggregateName} creado con IsActive=false

❌ {Field1} vacío
- Input: {Field1}=""
- Resultado: ValidationException "{Field1} is required"

#### Tests Unitarios (Servicio)

✅ Llama a {AggregateName}.Create con los parámetros correctos
- Verifica que se invoca {aggregate}Create.Execute con el command correcto

✅ Añade el aggregate al repositorio
- Verifica que repository.Add es llamado con el {aggregate} creado

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

✅ Retorna Response mapeado correctamente
- Verifica que el Response contiene los datos del {aggregate}

#### Tests Integración

✅ 201 Created → {AggregateName}Response

❌ 422 → Validación fallida

---

### 6.2 {AggregateName}.Update

#### Event Storming
```
🟡[{Actor}] → 🔵(Update{AggregateName}) → 🟤[[{AggregateName}]] → 🟠<{AggregateName}Updated>
```

#### Input

| Campo | Tipo |
|-------|------|
| {Field1} | {Type} |
| {Field2} | {Type} |

#### Inyecta
- `{ValueObject}.Create`
- `IValidator<{AggregateName}>`

#### Guards
Ninguno.

#### Lógica
```csharp
var {valueObject} = {valueObject}Create.Execute(new Create{ValueObject}Command(command.{Field1}, command.{Field2}));

{aggregate}.{Property1} = command.{Property1};
{aggregate}.{Property2} = {valueObject};

return {aggregate}Validator.ValidateOrThrow({aggregate});
```

#### Slice: PUT /{aggregates}/{id}

**Request**
```csharp
public record Update{AggregateName}Request(
    {Type} {Field1},
    {Type} {Field2}
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

#### Tests Unitarios (Servicio)

✅ Obtiene el {aggregate} del repositorio
- Verifica que repository.GetByIdAsync es llamado con el id correcto

✅ Llama a {AggregateName}.Update con los parámetros correctos
- Verifica que se invoca {aggregate}Update.Execute con el command correcto

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

#### Tests Integración

✅ 204 No Content

❌ 404 → {AggregateName} no encontrado

❌ 422 → Validación fallida

---

### 6.3 {AggregateName}.Activate

#### Event Storming
```
🟡[{Actor}] → 🔵(Activate{AggregateName}) → 🟤[[{AggregateName}]] → 🟠<{AggregateName}Activated>
                                                │
                                      🟣{TieneRequisito1}
                                      🟣{TieneRequisito2}
```

#### Input
Ninguno

#### Inyecta
- `IValidator<{AggregateName}>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Ya está activo | 409 | ConflictGuard | "{AggregateName} is already active" |
| No tiene {Requisito1} | 422 | ValidationGuard | "{AggregateName} must have at least one {requisito1}" |
| No tiene {Requisito2} | 422 | ValidationGuard | "{AggregateName} must have at least one {requisito2}" |

#### Lógica
```csharp
ConflictGuard.ThrowIf({aggregate}.IsActive, "{AggregateName} is already active");
ValidationGuard.ThrowIf(!{aggregate}.{Collection1}.Any(), "{AggregateName} must have at least one {requisito1}", nameof({aggregate}.{Collection1}));
ValidationGuard.ThrowIf(!{aggregate}.{HasRequisito2}, "{AggregateName} must have at least one {requisito2}", nameof({aggregate}.{Collection2}));

{aggregate}.IsActive = true;

return {aggregate}Validator.ValidateOrThrow({aggregate});
```

#### Slice: POST /{aggregates}/{id}/activate

**Response**: 200 OK → `{AggregateName}Response`

#### Tests Unitarios (Dominio)

✅ Activar {aggregate} completo
- Precondición: {AggregateName} con {Collection1} y {Collection2}, IsActive=false
- Resultado: {AggregateName} con IsActive=true

❌ Ya activo
- Precondición: {AggregateName} con IsActive=true
- Resultado: ConflictException "{AggregateName} is already active"

❌ Sin {Collection1}
- Precondición: {AggregateName} sin {Collection1}
- Resultado: ValidationException "{AggregateName} must have at least one {requisito1}"

#### Tests Unitarios (Servicio)

✅ Obtiene el {aggregate} del repositorio
- Verifica que repository.GetByIdAsync es llamado con el id correcto

✅ Llama a {AggregateName}.Activate
- Verifica que se invoca {aggregate}Activate.Execute

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

✅ Retorna Response mapeado correctamente
- Verifica que el Response contiene IsActive=true

#### Tests Integración

✅ 200 OK → {AggregateName}Response con IsActive=true

❌ 404 → {AggregateName} no encontrado

❌ 409 → Ya estaba activo

❌ 422 → Falta requisito

---

### 6.4 {AggregateName}.Deactivate

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

#### Tests Unitarios (Servicio)

✅ Obtiene el {aggregate} del repositorio
- Verifica que repository.GetByIdAsync es llamado con el id correcto

✅ Llama a {AggregateName}.Deactivate
- Verifica que se invoca {aggregate}Deactivate.Execute

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

✅ Retorna Response mapeado correctamente
- Verifica que el Response contiene IsActive=false

#### Tests Integración

✅ 200 OK → {AggregateName}Response con IsActive=false

❌ 404 → {AggregateName} no encontrado

❌ 409 → Ya estaba inactivo

---

### 6.5 {AggregateName}.Add{ValueObject}

#### Event Storming
```
🟡[{Actor}] → 🔵(Add{ValueObject}) → 🟤[[{AggregateName}]] → 🟠<{ValueObject}Added>
                                          │
                                🟣{{Key}Único}
```

#### Input

| Campo | Tipo |
|-------|------|
| {Key} | {Type} |
| {Field1} | {Type} |
| {Field2} | {Type}? |

#### Inyecta
- `{ValueObject}.Create`
- `IValidator<{AggregateName}>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| {Key} ya existe | 409 | ConflictGuard | "{ValueObject} with {key} '{Key}' already exists" |

#### Lógica
```csharp
ConflictGuard.ThrowIf(
    {aggregate}.{Collection}.Any(x => x.{Key} == command.{Key}),
    $"{ValueObject} with {key} '{command.{Key}}' already exists");

var {valueObject} = {valueObject}Create.Execute(new Create{ValueObject}Command(
    command.{Key},
    command.{Field1},
    command.{Field2}));

{aggregate}._{collection}.Add({valueObject});

return {aggregate}Validator.ValidateOrThrow({aggregate});
```

#### Slice: POST /{aggregates}/{id}/{collection}

**Request**
```csharp
public record Add{ValueObject}Request(
    {Type} {Key},
    {Type} {Field1},
    {Type}? {Field2}
);
```

**Response**: 201 Created → `{AggregateName}Response`

#### Tests Unitarios (Dominio)

✅ Añadir {valueObject} válido
- Precondición: {AggregateName} sin {ValueObject} con {Key}={value}
- Input: {Key}={value}, {Field1}={value1}
- Resultado: {ValueObject} añadido

❌ {Key} duplicado
- Precondición: {AggregateName} ya tiene {ValueObject} con {Key}={value}
- Input: {Key}={value}
- Resultado: ConflictException "{ValueObject} with {key} '{value}' already exists"

❌ Validación de {ValueObject} falla
- Input: {Field1}=""
- Resultado: ValidationException "{Field1} is required"

#### Tests Unitarios (Servicio)

✅ Obtiene el {aggregate} del repositorio
- Verifica que repository.GetByIdAsync es llamado con el id correcto

✅ Llama a {AggregateName}.Add{ValueObject} con los parámetros correctos
- Verifica que se invoca add{ValueObject}.Execute con el command correcto

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

✅ Retorna Response mapeado correctamente
- Verifica que el Response contiene el {ValueObject} añadido

#### Tests Integración

✅ 201 Created → {AggregateName}Response con {ValueObject} añadido

❌ 404 → {AggregateName} no encontrado

❌ 409 → {Key} duplicado

❌ 422 → Validación fallida

---

### 6.6 {AggregateName}.Update{ValueObject}

#### Event Storming
```
🟡[{Actor}] → 🔵(Update{ValueObject}) → 🟤[[{AggregateName}]] → 🟠<{ValueObject}Updated>
                                             │
                                   🟣{{ValueObject}Existe}
```

#### Input

| Campo | Tipo |
|-------|------|
| {Field1} | {Type} |
| {Field2} | {Type}? |

*{Key} viene en la ruta*

#### Inyecta
- `{ValueObject}.Create`
- `IValidator<{AggregateName}>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| {ValueObject} no existe | 404 | NotFoundGuard | "{ValueObject} with {key} '{Key}' not found" |

#### Lógica
```csharp
var existing = {aggregate}.{Collection}.FirstOrDefault(x => x.{Key} == {key});
NotFoundGuard.ThrowIfNull(existing, $"{ValueObject} with {key} '{{key}}' not found");

var updated = {valueObject}Create.Execute(new Create{ValueObject}Command(
    {key},
    command.{Field1},
    command.{Field2}));

{aggregate}._{collection}.Remove(existing);
{aggregate}._{collection}.Add(updated);

return {aggregate}Validator.ValidateOrThrow({aggregate});
```

#### Slice: PUT /{aggregates}/{id}/{collection}/{{key}}

**Request**
```csharp
public record Update{ValueObject}Request(
    {Type} {Field1},
    {Type}? {Field2}
);
```

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

✅ Actualizar {valueObject} existente
- Precondición: {AggregateName} tiene {ValueObject} con {Key}={value}
- Input: {Field1}={newValue}
- Resultado: {ValueObject} actualizado

❌ {ValueObject} no existe
- Precondición: {AggregateName} no tiene {ValueObject} con {Key}={value}
- Resultado: NotFoundException "{ValueObject} with {key} '{value}' not found"

#### Tests Unitarios (Servicio)

✅ Obtiene el {aggregate} del repositorio
- Verifica que repository.GetByIdAsync es llamado con el id correcto

✅ Llama a {AggregateName}.Update{ValueObject} con los parámetros correctos
- Verifica que se invoca update{ValueObject}.Execute con el {key} y command correctos

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

#### Tests Integración

✅ 204 No Content

❌ 404 → {AggregateName} o {ValueObject} no encontrado

❌ 422 → Validación fallida

---

### 6.7 {AggregateName}.Remove{ValueObject}

#### Event Storming
```
🟡[{Actor}] → 🔵(Remove{ValueObject}) → 🟤[[{AggregateName}]] → 🟠<{ValueObject}Removed>
                                             │
                                   🟣{{ValueObject}Existe}
                                   🟣{NoEsElÚltimo}
```

#### Input
*{Key} viene en la ruta*

#### Inyecta
- `IValidator<{AggregateName}>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| {ValueObject} no existe | 404 | NotFoundGuard | "{ValueObject} with {key} '{Key}' not found" |
| Es el último y {AggregateName} activo | 422 | ValidationGuard | "Cannot remove last {valueObject} from active {aggregateName}" |

#### Lógica
```csharp
var existing = {aggregate}.{Collection}.FirstOrDefault(x => x.{Key} == {key});
NotFoundGuard.ThrowIfNull(existing, $"{ValueObject} with {key} '{{key}}' not found");

ValidationGuard.ThrowIf(
    {aggregate}.IsActive && {aggregate}.{Collection}.Count <= 1,
    "Cannot remove last {valueObject} from active {aggregateName}",
    nameof({aggregate}.{Collection}));

{aggregate}._{collection}.Remove(existing);

return {aggregate}Validator.ValidateOrThrow({aggregate});
```

#### Slice: DELETE /{aggregates}/{id}/{collection}/{{key}}

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

✅ Eliminar {valueObject} (hay varios)
- Precondición: {AggregateName} con múltiples {ValueObject}s
- Resultado: {ValueObject} eliminado

✅ Eliminar último {valueObject} ({aggregateName} inactivo)
- Precondición: {AggregateName} inactivo con 1 {ValueObject}
- Resultado: {ValueObject} eliminado

❌ {ValueObject} no existe
- Resultado: NotFoundException "{ValueObject} with {key} '{value}' not found"

❌ Último {valueObject} en {aggregateName} activo
- Precondición: {AggregateName} activo con 1 {ValueObject}
- Resultado: ValidationException "Cannot remove last {valueObject} from active {aggregateName}"

#### Tests Unitarios (Servicio)

✅ Obtiene el {aggregate} del repositorio
- Verifica que repository.GetByIdAsync es llamado con el id correcto

✅ Llama a {AggregateName}.Remove{ValueObject} con el {key} correcto
- Verifica que se invoca remove{ValueObject}.Execute con el {key}

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

#### Tests Integración

✅ 204 No Content

❌ 404 → {AggregateName} o {ValueObject} no encontrado

❌ 422 → Es el último en {aggregateName} activo

---

## 7. Queries

### Get{AggregateName}

**Slice**: GET /{aggregates}/{id}

**Response**: 200 OK → `{AggregateName}Response`

#### Tests Unitarios (Handler)

✅ Obtiene el {aggregate} del repositorio
- Verifica que repository.GetByIdAsync es llamado con el id correcto

✅ Lanza NotFoundException si no existe
- Verifica que se lanza excepción cuando repository devuelve null

✅ Retorna Response mapeado correctamente
- Verifica que el Response contiene los datos del {aggregate}

#### Tests Integración

✅ 200 OK → {AggregateName}Response

❌ 404 → No encontrado

---

### List{AggregateName}s

**Slice**: GET /{aggregates}?isActive=true

**Response**: 200 OK → `{AggregateName}Response[]`

#### Tests Unitarios (Handler)

✅ Obtiene los {aggregate}s del repositorio
- Verifica que repository.GetAllAsync es llamado

✅ Filtra por isActive cuando se proporciona
- Verifica que se aplica el filtro correctamente

✅ Retorna array vacío si no hay resultados
- Verifica que devuelve colección vacía, no null

✅ Retorna Responses mapeados correctamente
- Verifica que cada Response contiene los datos del {aggregate}

#### Tests Integración

✅ 200 OK → Array de {AggregateName}Response

✅ 200 OK → Array vacío si no hay {aggregateName}s

---

### List{AggregateName}s

**Slice**: GET /{aggregates}?isActive=true

**Response**: 200 OK → `{AggregateName}Response[]`

#### Tests Integración

✅ 200 OK → Array de {AggregateName}Response

✅ 200 OK → Array vacío si no hay {aggregateName}s

---

## 8. Resumen de Endpoints

| Método | Ruta | Comando/Query | Response |
|--------|------|---------------|----------|
| POST | /{aggregates} | {AggregateName}.Create | 201 → `{AggregateName}Response` |
| GET | /{aggregates} | List{AggregateName}s | 200 → `{AggregateName}Response[]` |
| GET | /{aggregates}/{id} | Get{AggregateName} | 200 → `{AggregateName}Response` |
| PUT | /{aggregates}/{id} | {AggregateName}.Update | 204 |
| POST | /{aggregates}/{id}/activate | {AggregateName}.Activate | 200 → `{AggregateName}Response` |
| POST | /{aggregates}/{id}/deactivate | {AggregateName}.Deactivate | 200 → `{AggregateName}Response` |
| POST | /{aggregates}/{id}/{collection} | {AggregateName}.Add{ValueObject} | 201 → `{AggregateName}Response` |
| PUT | /{aggregates}/{id}/{collection}/{{key}} | {AggregateName}.Update{ValueObject} | 204 |
| DELETE | /{aggregates}/{id}/{collection}/{{key}} | {AggregateName}.Remove{ValueObject} | 204 |

---

## 9. Persistencia (Firestore)

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

    // SubCollection: {SubCollection} (colección separada en Firestore)
    entity.SubCollection(x => x.{SubCollection}, sub =>
    {
        // ArrayOf embedded con Reference
        sub.ArrayOf(s => s.{Items}, item =>
        {
            item.Reference(i => i.{ReferencedEntity});
        });
    });
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

## 10. Hot Spots ⚠️

| # | Pregunta | Estado |
|---|----------|--------|
| 1 | {Pregunta pendiente 1} | Pendiente |
| 2 | {Pregunta pendiente 2} | Pendiente |
| 3 | {Pregunta pendiente 3} | Decidido: {decisión} |

---

**Fecha**: {YYYY-MM-DD}
**Autor**: {Equipo}