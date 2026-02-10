# Domain Specification: TenantRole

---

## 1. Enums

*No hay enums en este agregado.*

---

## 2. Value Objects

*No hay value objects en este agregado. Todos los campos son tipos primitivos o arrays de strings.*

---

## 3. Aggregate: TenantRole

### Estructura

```
TenantRole (Aggregate Root)
├─ Id: Guid
├─ TenantId: Guid
├─ Name: string
├─ Description: string
├─ IsSystem: bool
├─ IsEditable: bool
├─ IsDeletable: bool
├─ Groups: IReadOnlyCollection<string>
├─ AdditionalScopes: IReadOnlyCollection<string>
└─ ExcludedScopes: IReadOnlyCollection<string>
```

#### Propiedades

| Propiedad | Tipo | Modificador |
|-----------|------|-------------|
| Id | Guid | init |
| TenantId | Guid | init |
| Name | string | protected set |
| Description | string | protected set |
| IsSystem | bool | init |
| IsEditable | bool | init |
| IsDeletable | bool | init |

#### Colecciones

```csharp
protected HashSet<string> _groups = [];
public IReadOnlyCollection<string> Groups => _groups.ToList().AsReadOnly();

protected HashSet<string> _additionalScopes = [];
public IReadOnlyCollection<string> AdditionalScopes => _additionalScopes.ToList().AsReadOnly();

protected HashSet<string> _excludedScopes = [];
public IReadOnlyCollection<string> ExcludedScopes => _excludedScopes.ToList().AsReadOnly();
```

### Invariantes (Validator)

> Estas reglas se implementan en `TenantRoleValidator : AbstractValidator<TenantRole>`

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| Id | NotEmpty | "Id is required" |
| TenantId | NotEmpty | "TenantId is required" |
| Name | NotEmpty | "Name is required" |
| Name | MaxLength(100) | "Name cannot exceed 100 characters" |
| Description | MaxLength(500) | "Description cannot exceed 500 characters" |

---

## 4. Response

```csharp
public record TenantRoleResponse(
    Guid Id,
    string Name,
    string Description,
    bool IsSystem,
    bool IsEditable,
    bool IsDeletable,
    List<string> Groups,
    List<string> AdditionalScopes,
    List<string> ExcludedScopes
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
> - TenantRole.SeedSystemRoles se ejecuta al crear el tenant (post-compra)
> - TenantRole.Create es solo para roles custom
> - Las Queries van después de Create
> - Update y UpdatePermissions van después
> - Delete va al final

> **Tests de dominio**: Usar `TestableTenantRole` para preparar estado previo. Usar `DomainFixture` para resolver comandos y validators. **NO encadenar comandos** para crear estado.
>
> **Tests de slice**: Usar `TestableTenantRole` para el estado que devuelve el repository mock. Usar `DomainFixture` para resolver el comando que la slice inyecta. Mock de `IRepository` e `IUnitOfWork`.

---

### 6.1 TenantRole.SeedSystemRoles

> Comando interno que se ejecuta al crear un tenant. Genera los roles de sistema predefinidos.

#### Event Storming
```
🟠<TenantCreated> → 🟣{SeedSystemRoles} → 🔵(SeedSystemRoles) → 🟤[[TenantRole]] → 🟠<SystemRolesSeeded>
```

#### Input

| Campo | Tipo |
|-------|------|
| TenantId | Guid |

#### Inyecta
- `IValidator<TenantRole>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Ya existen roles para este tenant | 409 | ConflictGuard | "System roles already exist for this tenant" |

#### Lógica
```csharp
var systemRoles = await tenantRoleRepository.ExistsByTenantAsync(command.TenantId);

ConflictGuard.ThrowIf(systemRoles, "System roles already exist for this tenant");

var roles = new List<TenantRole>
{
    new TenantRole(Guid.NewGuid())
    {
        TenantId = command.TenantId,
        Name = "Owner",
        Description = "Propietario. Acceso completo.",
        IsSystem = true,
        IsEditable = false,
        IsDeletable = false,
        Groups = [],
        AdditionalScopes = [],
        ExcludedScopes = []
    },
    new TenantRole(Guid.NewGuid())
    {
        TenantId = command.TenantId,
        Name = "Manager",
        Description = "Encargado. Gestiona operativa diaria.",
        IsSystem = true,
        IsEditable = true,
        IsDeletable = false,
        Groups = [],
        AdditionalScopes = [],
        ExcludedScopes = []
    },
    new TenantRole(Guid.NewGuid())
    {
        TenantId = command.TenantId,
        Name = "Waiter",
        Description = "Camarero. Acceso limitado a operaciones del día a día.",
        IsSystem = true,
        IsEditable = true,
        IsDeletable = false,
        Groups = [],
        AdditionalScopes = [],
        ExcludedScopes = []
    },
    new TenantRole(Guid.NewGuid())
    {
        TenantId = command.TenantId,
        Name = "ExternalApp",
        Description = "Aplicación externa. Acceso programático a la API.",
        IsSystem = true,
        IsEditable = true,
        IsDeletable = false,
        Groups = [],
        AdditionalScopes = [],
        ExcludedScopes = []
    },
    new TenantRole(Guid.NewGuid())
    {
        TenantId = command.TenantId,
        Name = "Customer",
        Description = "Comensal registrado.",
        IsSystem = true,
        IsEditable = true,
        IsDeletable = false,
        Groups = [],
        AdditionalScopes = [],
        ExcludedScopes = []
    }
};

foreach (var role in roles)
    tenantRoleValidator.ValidateOrThrow(role);

return roles;
```

#### Slice: Interno (NotificationHandler)

> No expuesto como endpoint público. Se ejecuta como reacción al evento TenantCreated.

#### Tests Unitarios (Dominio)

✅ Genera 5 roles de sistema
- Input: TenantId=valid
- Resultado: 5 TenantRole con IsSystem=true

✅ Owner no es editable ni eliminable
- Resultado: IsEditable=false, IsDeletable=false

✅ Manager, Waiter, ExternalApp, Customer son editables pero no eliminables
- Resultado: IsEditable=true, IsDeletable=false

✅ Todos los roles se crean sin permisos
- Resultado: Groups=[], AdditionalScopes=[], ExcludedScopes=[]

❌ Roles ya existen para el tenant
- Precondición: Ya existen roles para TenantId=X
- Resultado: ConflictException "System roles already exist for this tenant"

#### Tests Unitarios (Servicio)

✅ Verifica existencia antes de crear
- Verifica que tenantRoleRepository.ExistsByTenantAsync es llamado con tenantId

✅ Añade todos los roles al repositorio
- Verifica que repository.AddRange es llamado con 5 roles

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

---

### 6.2 TenantRole.Create

> Solo para roles custom. Los roles de sistema se crean con SeedSystemRoles.

#### Event Storming
```
🟡[Owner] → 🔵(CreateTenantRole) → 🟤[[TenantRole]] → 🟠<TenantRoleCreated>
                                        │
                                  🟣{UniqueName}
```

#### Input

| Campo | Tipo |
|-------|------|
| Name | string |
| Description | string |

#### Inyecta
- `IValidator<TenantRole>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Ya existe un rol con ese nombre en el tenant | 409 | ConflictGuard | "A role with this name already exists" |

#### Lógica
```csharp
var duplicate = await tenantRoleRepository.ExistsByNameAsync(
    command.TenantId, command.Name);

ConflictGuard.ThrowIf(duplicate, "A role with this name already exists");

var role = new TenantRole(Guid.NewGuid())
{
    TenantId = command.TenantId,
    Name = command.Name,
    Description = command.Description,
    IsSystem = false,
    IsEditable = true,
    IsDeletable = true,
    Groups = [],
    AdditionalScopes = [],
    ExcludedScopes = []
};

return tenantRoleValidator.ValidateOrThrow(role);
```

#### Slice: POST /tenant-roles

**Request**
```csharp
public record CreateTenantRoleRequest(
    string Name,
    string Description
);
```

**Response**: 201 Created → `TenantRoleResponse`

#### Tests Unitarios (Dominio)

✅ Crear rol custom con datos válidos
- Input: Name="Sommelier", Description="Carta de vinos"
- Resultado: TenantRole con IsSystem=false, IsEditable=true, IsDeletable=true, Groups=[]

❌ Name vacío
- Input: Name=""
- Resultado: ValidationException "Name is required"

❌ Name duplicado
- Precondición: Ya existe rol "Sommelier" en el tenant
- Input: Name="Sommelier"
- Resultado: ConflictException "A role with this name already exists"

#### Tests Unitarios (Servicio)

✅ Verifica unicidad antes de crear
- Verifica que tenantRoleRepository.ExistsByNameAsync es llamado con tenantId y name

✅ Llama a TenantRole.Create con los parámetros correctos
- Verifica que se invoca tenantRoleCreate.Execute con el command correcto

✅ Añade el rol al repositorio
- Verifica que repository.Add es llamado con el rol creado

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

✅ Retorna Response mapeado correctamente
- Verifica que el Response contiene los datos del rol

#### Tests Integración

✅ 201 Created → TenantRoleResponse con IsSystem=false

❌ 409 → Nombre duplicado

❌ 422 → Validación fallida

---

### 6.3 GetTenantRole

#### Event Storming
```
🟡[Owner] → 🔵(GetTenantRole) → 🟤[[TenantRole]] → 📊 TenantRoleResponse
```

#### Slice: GET /tenant-roles/{id}

**Response**: 200 OK → `TenantRoleResponse`

#### Tests Unitarios (Servicio)

✅ Obtiene el rol del repositorio con el id correcto
- Verifica que repository.GetByIdAsync es llamado con el id

✅ Retorna Response mapeado correctamente
- Verifica que el Response contiene los datos del rol

#### Tests Integración

✅ 200 OK → TenantRoleResponse

❌ 404 → No encontrado

---

### 6.4 ListTenantRoles

#### Event Storming
```
🟡[Owner] → 🔵(ListTenantRoles) → 🟤[[TenantRole]] → 📊 TenantRoleResponse[]
```

#### Slice: GET /tenant-roles

> El tenantId se obtiene del JWT.

**Response**: 200 OK → `TenantRoleResponse[]`

#### Tests Unitarios (Servicio)

✅ Obtiene roles del tenantId del JWT
- Verifica que repository.GetByTenantIdAsync es llamado con el tenantId del token

✅ Retorna lista mapeada correctamente
- Verifica que el Response contiene todos los roles del tenant

#### Tests Integración

✅ 200 OK → Lista de TenantRoleResponse

✅ 200 OK → Lista vacía si no hay roles

---

### 6.5 TenantRole.Update

> Solo permite cambiar Name y Description. No permite modificar roles con IsEditable=false (Owner).

#### Event Storming
```
🟡[Owner] → 🔵(UpdateTenantRole) → 🟤[[TenantRole]] → 🟠<TenantRoleUpdated>
                                        │
                                  🟣{IsEditable}
                                  🟣{UniqueName}
```

#### Input

| Campo | Tipo |
|-------|------|
| Name | string |
| Description | string |

#### Inyecta
- `IValidator<TenantRole>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Rol no es editable | 409 | ConflictGuard | "This role cannot be edited" |
| Nombre duplicado en el tenant (otro rol) | 409 | ConflictGuard | "A role with this name already exists" |

#### Lógica
```csharp
ConflictGuard.ThrowIf(!role.IsEditable, "This role cannot be edited");

var duplicate = await tenantRoleRepository.ExistsByNameExcludingAsync(
    role.TenantId, command.Name, role.Id);

ConflictGuard.ThrowIf(duplicate, "A role with this name already exists");

role.Name = command.Name;
role.Description = command.Description;

return tenantRoleValidator.ValidateOrThrow(role);
```

#### Slice: PUT /tenant-roles/{id}

**Request**
```csharp
public record UpdateTenantRoleRequest(
    string Name,
    string Description
);
```

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

✅ Actualizar nombre y descripción de rol editable
- Precondición: TenantRole con IsEditable=true
- Input: Name="Jefe de Sala", Description="Coordina servicio"
- Resultado: TenantRole actualizado

✅ Actualizar rol de sistema editable (Manager)
- Precondición: TenantRole con IsSystem=true, IsEditable=true
- Input: Name="Encargado Senior"
- Resultado: TenantRole actualizado

❌ Rol no editable (Owner)
- Precondición: TenantRole con IsEditable=false
- Resultado: ConflictException "This role cannot be edited"

❌ Nombre duplicado
- Precondición: Ya existe otro rol con Name="Camarero"
- Input: Name="Camarero"
- Resultado: ConflictException "A role with this name already exists"

❌ Name vacío
- Input: Name=""
- Resultado: ValidationException "Name is required"

#### Tests Integración

✅ 204 No Content

❌ 404 → No encontrado

❌ 409 → No editable o nombre duplicado

❌ 422 → Validación fallida

---

### 6.6 TenantRole.UpdatePermissions

> Actualiza groups, additionalScopes y excludedScopes. No permite modificar roles con IsEditable=false (Owner). Al guardar se invalidan las sesiones de los memberships con este rol.

#### Event Storming
```
🟡[Owner] → 🔵(UpdatePermissions) → 🟤[[TenantRole]] → 🟠<TenantRolePermissionsUpdated>
                                         │
                                   🟣{IsEditable}
```

#### Input

| Campo | Tipo |
|-------|------|
| Groups | List\<string\> |
| AdditionalScopes | List\<string\> |
| ExcludedScopes | List\<string\> |

#### Inyecta
- `IValidator<TenantRole>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Rol no es editable | 409 | ConflictGuard | "This role cannot be edited" |

#### Lógica
```csharp
ConflictGuard.ThrowIf(!role.IsEditable, "This role cannot be edited");

role.Groups = command.Groups;
role.AdditionalScopes = command.AdditionalScopes;
role.ExcludedScopes = command.ExcludedScopes;

return tenantRoleValidator.ValidateOrThrow(role);
```

#### Slice: PUT /tenant-roles/{id}/permissions

**Request**
```csharp
public record UpdatePermissionsRequest(
    List<string> Groups,
    List<string> AdditionalScopes,
    List<string> ExcludedScopes
);
```

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

✅ Actualizar permisos de rol editable
- Precondición: TenantRole con IsEditable=true, Groups=[]
- Input: Groups=["menu:read", "menu:write"], AdditionalScopes=[], ExcludedScopes=[]
- Resultado: TenantRole con Groups=["menu:read", "menu:write"]

✅ Actualizar con exclusiones
- Input: Groups=["menu:write"], ExcludedScopes=["menu-svc:SetMenuDepositPolicy"]
- Resultado: TenantRole con exclusión aplicada

✅ Actualizar con scopes adicionales
- Input: AdditionalScopes=["res-svc:CancelReservation"]
- Resultado: TenantRole con scope adicional

❌ Rol no editable (Owner)
- Precondición: TenantRole con IsEditable=false
- Resultado: ConflictException "This role cannot be edited"

#### Tests Unitarios (Servicio)

✅ Obtiene el rol del repositorio
- Verifica que repository.GetByIdAsync es llamado con roleId

✅ Llama a TenantRole.UpdatePermissions
- Verifica que se invoca con el command correcto

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

#### Tests Integración

✅ 204 No Content

❌ 404 → No encontrado

❌ 409 → No editable

---

### 6.7 TenantRole.Delete

> Solo permite eliminar roles con IsDeletable=true (roles custom). No permite eliminar si hay memberships asignados a este rol.

#### Event Storming
```
🟡[Owner] → 🔵(DeleteTenantRole) → 🟤[[TenantRole]] → 🟠<TenantRoleDeleted>
                                        │
                                  🟣{IsDeletable}
                                  🟣{NoMemberships}
```

#### Input

*Sin input adicional*

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Rol no es eliminable | 409 | ConflictGuard | "This role cannot be deleted" |
| Existen memberships con este rol | 409 | ConflictGuard | "Cannot delete a role that has members assigned" |

#### Lógica
```csharp
ConflictGuard.ThrowIf(!role.IsDeletable, "This role cannot be deleted");

var hasMemberships = await membershipRepository.ExistsByRoleIdAsync(role.Id);

ConflictGuard.ThrowIf(hasMemberships, "Cannot delete a role that has members assigned");

tenantRoleRepository.Delete(role);
```

#### Slice: DELETE /tenant-roles/{id}

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

✅ Eliminar rol custom sin memberships
- Precondición: TenantRole con IsDeletable=true, sin memberships asignados
- Resultado: Rol eliminado

❌ Rol no eliminable (sistema)
- Precondición: TenantRole con IsDeletable=false
- Resultado: ConflictException "This role cannot be deleted"

❌ Rol con memberships asignados
- Precondición: TenantRole con IsDeletable=true, tiene memberships
- Resultado: ConflictException "Cannot delete a role that has members assigned"

#### Tests Unitarios (Servicio)

✅ Obtiene el rol del repositorio
- Verifica que repository.GetByIdAsync es llamado con roleId

✅ Verifica que no hay memberships asignados
- Verifica que membershipRepository.ExistsByRoleIdAsync es llamado con roleId

✅ Elimina el rol
- Verifica que repository.Delete es llamado

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

#### Tests Integración

✅ 204 No Content

❌ 404 → No encontrado

❌ 409 → No eliminable o tiene memberships

---

## 7. Descripciones de Permisos

> Las descripciones son **responsabilidad de producto**. Se definen en español durante la sesión de diseño. Claude Code genera el archivo de descripciones del microservicio con el español como base y traduce automáticamente al resto de idiomas necesarios.
>
> Deben ser claras, concisas y comprensibles para alguien sin conocimientos técnicos — es lo que el administrador ve cuando configura roles.

### Scopes atómicos

| Scope (nombre de clase) | Descripción (es) |
|--------------------------|-------------------|
| `CreateTenantRole` | Crear un nuevo rol personalizado |
| `GetTenantRole` | Ver los detalles de un rol |
| `ListTenantRoles` | Ver la lista de roles |
| `UpdateTenantRole` | Modificar nombre y descripción de un rol |
| `UpdatePermissions` | Configurar los permisos de un rol |
| `DeleteTenantRole` | Eliminar un rol personalizado |

---

## 8. Resumen de Endpoints (Orden de Implementación)

| # | Método | Ruta | Comando/Query | Response |
|---|--------|------|---------------|----------|
| 1 | POST | /tenant-roles | TenantRole.Create | 201 → `TenantRoleResponse` |
| 2 | GET | /tenant-roles/{id} | GetTenantRole | 200 → `TenantRoleResponse` |
| 3 | GET | /tenant-roles | ListTenantRoles | 200 → `TenantRoleResponse[]` |
| 4 | PUT | /tenant-roles/{id} | TenantRole.Update | 204 |
| 5 | PUT | /tenant-roles/{id}/permissions | TenantRole.UpdatePermissions | 204 |
| 6 | DELETE | /tenant-roles/{id} | TenantRole.Delete | 204 |

---

## 9. Persistencia (Firestore)

### Colección

`/tenant_roles/{tenantRoleId}`

### Configuración DbContext

```csharp
modelBuilder.Entity<TenantRole>(entity =>
{
    // QueryFilter: multi-tenancy by TenantId
    entity.HasQueryFilter(x => x.TenantId == tenantId);

    // Backing fields: HashSet<string> para evitar duplicados
    entity.Property(x => x.Groups)
        .HasBackingField("_groups");

    entity.Property(x => x.AdditionalScopes)
        .HasBackingField("_additionalScopes");

    entity.Property(x => x.ExcludedScopes)
        .HasBackingField("_excludedScopes");
});
```

### Documento Ejemplo

```json
{
  "id": "role-001-guid",
  "tenantId": "tenant-001-guid",
  "name": "Manager",
  "description": "Encargado. Gestiona operativa diaria.",
  "isSystem": true,
  "isEditable": true,
  "isDeletable": false,
  "groups": ["menu:read", "menu:write", "reservation:read", "reservation:write"],
  "additionalScopes": ["res-svc:CancelReservation"],
  "excludedScopes": ["menu-svc:SetMenuDepositPolicy", "menu-svc:RemoveMenuDepositPolicy"]
}
```

---

## 10. Hot Spots ⚠️

| # | Pregunta | Estado |
|---|----------|--------|
| 1 | ¿Se necesita validar que los groups y scopes existen en algún catálogo al asignarlos? | Pendiente: El Auth no conoce el catálogo — solo copia permisos al JWT |
| 2 | ¿Se necesita auditoría de cambios en permisos de roles? | Pendiente |
| 3 | ¿Los roles de sistema deben tener permisos por defecto al crearse o se configuran después? | Decidido: Se crean sin permisos — el Owner los configura |
| 4 | ¿Se debe invalidar sesiones al actualizar permisos de un rol? | Decidido: Sí — el wireframe lo muestra explícitamente |

---

**Fecha**: 2026-02-10
**Autor**: Equipo Fudie
