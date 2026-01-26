# Domain Specification: Allergen

---

## 1. Enums

*No se requieren enums para este agregado.*

---

## 2. Value Objects

*No se requieren value objects para este agregado.*

---

## 3. Aggregate: Allergen

### Estructura

```
Allergen (Aggregate Root)
├─ Id: string (Code)
├─ Name: string
├─ IconUrl: string?
├─ IsActive: bool
└─ DisplayOrder: int
```

#### Propiedades

| Propiedad | Tipo | Modificador |
|-----------|------|-------------|
| Id | string | init |
| Name | string | protected set |
| IconUrl | string? | protected set |
| IsActive | bool | protected set |
| DisplayOrder | int | protected set |

### Validaciones

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| Id (Code) | NotEmpty | "Code is required" |
| Id (Code) | Max(20) | "Code cannot exceed 20 characters" |
| Id (Code) | Matches(`^[A-Z0-9_]+$`) | "Code must be uppercase letters, numbers, and underscores only" |
| Name | NotEmpty | "Name is required" |
| Name | Max(100) | "Name cannot exceed 100 characters" |
| IconUrl | Max(500) | "IconUrl cannot exceed 500 characters" |
| IconUrl | ValidUrl when NotEmpty | "IconUrl must be a valid URL" |
| DisplayOrder | >= 0 | "DisplayOrder must be greater than or equal to 0" |

---

## 4. Response

```csharp
public record AllergenResponse(
    string Id,
    string Name,
    string? IconUrl,
    bool IsActive,
    int DisplayOrder
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

### 6.1 Allergen.Create

#### Event Storming
```
🟡[Admin] → 🔵(CreateAllergen) → 🟤[[Allergen]] → 🟠<AllergenCreated>
```

#### Input

| Campo | Tipo | Default |
|-------|------|---------|
| Code | string | |
| Name | string | |
| IconUrl | string? | null |
| IsActive | bool | true |
| DisplayOrder | int | 0 |

#### Inyecta
- `IValidator<Allergen>`

#### Guards
Ninguno.

#### Lógica
```csharp
var allergen = new Allergen(command.Code)
{
    Name = command.Name,
    IconUrl = command.IconUrl,
    IsActive = command.IsActive,
    DisplayOrder = command.DisplayOrder
};

return allergenValidator.ValidateOrThrow(allergen);
```

#### Slice: POST /allergens

**Request**
```csharp
public record CreateAllergenRequest(
    string Code,
    string Name,
    string? IconUrl = null,
    bool IsActive = true,
    int DisplayOrder = 0
);
```

**Response**: 201 Created → `AllergenResponse`

#### Tests Unitarios (Dominio)

✅ Crear alérgeno con datos válidos
- Input: Code="GLUTEN", Name="Gluten", IsActive=true, DisplayOrder=0
- Resultado: Allergen creado

✅ Crear alérgeno con IconUrl
- Input: Code="LACTEOS", Name="Lácteos", IconUrl="https://cdn.fudie.com/icons/lacteos.svg"
- Resultado: Allergen creado con IconUrl

✅ Crear alérgeno inactivo
- Input: Code="SULFITOS", Name="Sulfitos", IsActive=false
- Resultado: Allergen creado con IsActive=false

❌ Code vacío
- Input: Code=""
- Resultado: ValidationException "Code is required"

❌ Code demasiado largo
- Input: Code="ESTE_CODE_ES_DEMASIADO_LARGO"
- Resultado: ValidationException "Code cannot exceed 20 characters"

❌ Code con formato inválido (minúsculas)
- Input: Code="gluten"
- Resultado: ValidationException "Code must be uppercase letters, numbers, and underscores only"

❌ Code con formato inválido (espacios)
- Input: Code="GLUTEN FREE"
- Resultado: ValidationException "Code must be uppercase letters, numbers, and underscores only"

❌ Name vacío
- Input: Name=""
- Resultado: ValidationException "Name is required"

❌ Name demasiado largo
- Input: Name="A".repeat(101)
- Resultado: ValidationException "Name cannot exceed 100 characters"

❌ IconUrl inválido
- Input: IconUrl="not-a-valid-url"
- Resultado: ValidationException "IconUrl must be a valid URL"

❌ DisplayOrder negativo
- Input: DisplayOrder=-1
- Resultado: ValidationException "DisplayOrder must be greater than or equal to 0"

#### Tests Unitarios (Servicio)

✅ Llama a Allergen.Create con los parámetros correctos
- Verifica que se invoca allergenCreate.Execute con el command correcto

✅ Añade el allergen al repositorio
- Verifica que repository.Add es llamado con el allergen creado

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

✅ Retorna Response mapeado correctamente
- Verifica que el Response contiene los datos del allergen

#### Tests Integración

✅ 201 Created → AllergenResponse

❌ 422 → Validación fallida

---

### 6.2 Allergen.Update

#### Event Storming
```
🟡[Admin] → 🔵(UpdateAllergen) → 🟤[[Allergen]] → 🟠<AllergenUpdated>
```

#### Input

| Campo | Tipo |
|-------|------|
| Name | string |
| IconUrl | string? |
| IsActive | bool |
| DisplayOrder | int |

#### Inyecta
- `IValidator<Allergen>`

#### Guards
Ninguno.

#### Lógica
```csharp
allergen.Name = command.Name;
allergen.IconUrl = command.IconUrl;
allergen.IsActive = command.IsActive;
allergen.DisplayOrder = command.DisplayOrder;

return allergenValidator.ValidateOrThrow(allergen);
```

#### Slice: PUT /allergens/{code}

**Request**
```csharp
public record UpdateAllergenRequest(
    string Name,
    string? IconUrl,
    bool IsActive,
    int DisplayOrder
);
```

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

✅ Actualizar nombre
- Precondición: Allergen existe con Name="Gluten"
- Input: Name="Gluten (trigo, centeno, cebada)"
- Resultado: Allergen actualizado

✅ Actualizar IconUrl
- Precondición: Allergen existe sin IconUrl
- Input: IconUrl="https://cdn.fudie.com/icons/gluten-new.svg"
- Resultado: Allergen actualizado con IconUrl

✅ Eliminar IconUrl
- Precondición: Allergen existe con IconUrl
- Input: IconUrl=null
- Resultado: Allergen actualizado con IconUrl=null

✅ Desactivar alérgeno
- Precondición: Allergen con IsActive=true
- Input: IsActive=false
- Resultado: Allergen con IsActive=false

✅ Activar alérgeno
- Precondición: Allergen con IsActive=false
- Input: IsActive=true
- Resultado: Allergen con IsActive=true

✅ Cambiar DisplayOrder
- Precondición: Allergen con DisplayOrder=0
- Input: DisplayOrder=5
- Resultado: Allergen con DisplayOrder=5

❌ Name vacío
- Input: Name=""
- Resultado: ValidationException "Name is required"

❌ IconUrl inválido
- Input: IconUrl="invalid-url"
- Resultado: ValidationException "IconUrl must be a valid URL"

❌ DisplayOrder negativo
- Input: DisplayOrder=-1
- Resultado: ValidationException "DisplayOrder must be greater than or equal to 0"

#### Tests Unitarios (Servicio)

✅ Obtiene el allergen del repositorio
- Verifica que repository.GetByIdAsync es llamado con el code correcto

✅ Llama a Allergen.Update con los parámetros correctos
- Verifica que se invoca allergenUpdate.Execute con el command correcto

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

#### Tests Integración

✅ 204 No Content

❌ 404 → Allergen no encontrado

❌ 422 → Validación fallida

---

## 7. Queries

### GetAllergen

**Slice**: GET /allergens/{code}

**Response**: 200 OK → `AllergenResponse`

#### Tests Integración

✅ 200 OK → AllergenResponse

❌ 404 → No encontrado

---

### ListAllergens

**Slice**: GET /allergens?isActive=true

**Response**: 200 OK → `AllergenResponse[]`

#### Tests Integración

✅ 200 OK → Array de AllergenResponse ordenado por DisplayOrder

✅ 200 OK → Array vacío si no hay allergens

✅ 200 OK → Filtra por isActive cuando se especifica

---

## 8. Resumen de Endpoints

| Método | Ruta | Comando/Query | Response |
|--------|------|---------------|----------|
| POST | /allergens | Allergen.Create | 201 → `AllergenResponse` |
| GET | /allergens | ListAllergens | 200 → `AllergenResponse[]` |
| GET | /allergens/{code} | GetAllergen | 200 → `AllergenResponse` |
| PUT | /allergens/{code} | Allergen.Update | 204 |

---

## 9. Persistencia (Firestore)

### Colección

`/allergens/{code}`

### Configuración DbContext

```csharp
modelBuilder.Entity<Allergen>(entity =>
{
    // Id es el Code (string)
    entity.HasKey(a => a.Id);
});
```

### Documento Ejemplo

```json
{
  "id": "GLUTEN",
  "name": "Gluten",
  "iconUrl": "https://cdn.fudie.com/icons/allergens/gluten.svg",
  "isActive": true,
  "displayOrder": 1
}
```

### Datos Iniciales (Seed)

```json
[
  { "id": "GLUTEN", "name": "Gluten", "displayOrder": 1 },
  { "id": "CRUSTACEOS", "name": "Crustáceos", "displayOrder": 2 },
  { "id": "HUEVOS", "name": "Huevos", "displayOrder": 3 },
  { "id": "PESCADO", "name": "Pescado", "displayOrder": 4 },
  { "id": "CACAHUETES", "name": "Cacahuetes", "displayOrder": 5 },
  { "id": "SOJA", "name": "Soja", "displayOrder": 6 },
  { "id": "LACTEOS", "name": "Lácteos", "displayOrder": 7 },
  { "id": "FRUTOS_SECOS", "name": "Frutos de cáscara", "displayOrder": 8 },
  { "id": "APIO", "name": "Apio", "displayOrder": 9 },
  { "id": "MOSTAZA", "name": "Mostaza", "displayOrder": 10 },
  { "id": "SESAMO", "name": "Sésamo", "displayOrder": 11 },
  { "id": "SULFITOS", "name": "Sulfitos", "displayOrder": 12 },
  { "id": "ALTRAMUCES", "name": "Altramuces", "displayOrder": 13 },
  { "id": "MOLUSCOS", "name": "Moluscos", "displayOrder": 14 }
]
```

---

## 10. Hot Spots ⚠️

| # | Pregunta | Estado |
|---|----------|--------|
| 1 | ¿Se puede eliminar un Allergen? | Decidido: No, solo desactivar para mantener integridad referencial con MenuItems |
| 2 | ¿El Code debe ser único globalmente? | Decidido: Sí, es el Id natural del agregado |
| 3 | ¿Qué pasa si un Allergen se desactiva y hay MenuItems que lo referencian? | Decidido: MenuItem mantiene la referencia, frontend puede mostrar advertencia o filtrar |
| 4 | ¿Debería haber validación de unicidad de Code en Create? | Pendiente: Depende de si Firestore lanza excepción por clave duplicada o si se necesita guard explícito |

---

**Fecha**: 2025-01-26
**Autor**: Equipo Fudie
