# Domain Specification: User

---

## 1. Enums

### AuthProvider

```csharp
public enum AuthProvider
{
    Google,
    Local
}
```

**Notas:**
- `Google` — autenticación vía Google OAuth. Flujo principal para todos los usuarios.
- `Local` — usuario y contraseña. Exclusivo para superadministradores de Fudie creados por script.
- Preparado para extensión futura (Apple, etc.) sin cambios estructurales.

---

## 2. Value Objects

### 2.1 HashedPassword

#### Estructura (Positional Record)

```csharp
public partial record HashedPassword(
    string Hash,
    string Salt
);
```

#### Invariantes (Validator)

> Estas reglas se implementan en `HashedPasswordValidator : AbstractValidator<HashedPassword>`

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| Hash | NotEmpty | "Hash is required" |
| Salt | NotEmpty | "Salt is required" |

#### Comando: HashedPassword.Create

**Input**

| Campo | Tipo | Default |
|-------|------|---------|
| PlainPassword | string | |

**Inyecta**: `IValidator<HashedPassword>`, `IPasswordHasher`

**Lógica**
```csharp
var salt = passwordHasher.GenerateSalt();
var hash = passwordHasher.Hash(command.PlainPassword, salt);

var hashedPassword = new HashedPassword(hash, salt);

return hashedPasswordValidator.ValidateOrThrow(hashedPassword);
```

**Notas:**
- `IPasswordHasher` abstrae el algoritmo de hashing (BCrypt, Argon2, etc.).
- El password en claro nunca se almacena ni viaja más allá del comando.
- Este Value Object solo se usa para usuarios con `AuthProvider.Local` (superadmin vía script).

#### Tests Unitarios

**Create:**

✅ Crear HashedPassword con password válido
- Input: PlainPassword="SecureP@ss123"
- Resultado: HashedPassword creado con Hash y Salt no vacíos

❌ Password vacío
- Input: PlainPassword=""
- Resultado: ValidationException (falla en la lógica previa al hashing, no en el validator del VO)

---

## 3. Entidades

No aplica. El agregado User no tiene entidades hijas.

---

## 4. Aggregate: User

### Estructura

```
User (Aggregate Root)
├─ Id: Guid
├─ ProviderId: string
├─ Provider: AuthProvider
├─ Email: string
├─ Name: string
├─ Phone: string?
├─ AvatarUrl: string?
├─ Password: HashedPassword? (ComplexType)
├─ LastLoginAt: DateTime?
└─ IsActive: bool
```

#### Propiedades

| Propiedad | Tipo | Modificador |
|-----------|------|-------------|
| Id | Guid | init |
| ProviderId | string | protected set |
| Provider | AuthProvider | protected set |
| Email | string | protected set |
| Name | string | protected set |
| Phone | string? | protected set |
| AvatarUrl | string? | protected set |
| Password | HashedPassword? | protected set |
| LastLoginAt | DateTime? | protected set |
| IsActive | bool | protected set |

**Notas:**
- `ProviderId` — identificador del usuario en el proveedor externo (ej: Google sub claim). Para `AuthProvider.Local`, es un identificador generado internamente.
- `Password` — solo se rellena cuando `Provider == AuthProvider.Local`. Es null para usuarios OAuth.
- `Phone` — nullable. Google no siempre lo devuelve. Se completará antes de comprar suscripción.
- `AvatarUrl` — URL del avatar de Google. Null para usuarios Local.
- No es multi-tenant. El User es transversal — puede pertenecer a múltiples restaurantes vía Membership.

#### Propiedades Calculadas

| Propiedad | Tipo | Fórmula |
|-----------|------|---------|
| IsOAuth | bool | `Provider != AuthProvider.Local` |
| HasPassword | bool | `Password != null` |

### Invariantes (Validator)

> Estas reglas se implementan en `UserValidator : AbstractValidator<User>`

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| Id | NotEmpty | "Id is required" |
| ProviderId | NotEmpty | "ProviderId is required" |
| Provider | IsInEnum | "Provider is not valid" |
| Email | NotEmpty | "Email is required" |
| Email | MaxLength(256) | "Email cannot exceed 256 characters" |
| Email | EmailAddress | "Email is not valid" |
| Name | NotEmpty | "Name is required" |
| Name | MaxLength(200) | "Name cannot exceed 200 characters" |
| Phone | MaxLength(20) when not null | "Phone cannot exceed 20 characters" |
| AvatarUrl | MaxLength(500) when not null | "AvatarUrl cannot exceed 500 characters" |
| Password | NotNull when Provider == Local | "Password is required for local users" |
| Password | Null when Provider != Local | "Password only applies to local users" |

---

## 5. Response

```csharp
public record UserResponse(
    Guid Id,
    string ProviderId,
    AuthProvider Provider,
    string Email,
    string Name,
    string? Phone,
    string? AvatarUrl,
    DateTime? LastLoginAt,
    bool IsActive
);
```

**Notas:**
- `Password` nunca se expone en la respuesta.
- `ProviderId` se expone para identificación visual (ej: en panel de administración de Fudie).

---

## 6. Event Storming – Leyenda

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
> - LoginWithGoogle y LoginWithPassword son los puntos de entrada.
> - RegisterWithGoogle crea el User si no existe (primer login).
> - Get/GetCurrentUser van después para verificar persistencia.
> - Update, Deactivate, Activate, ChangePassword van al final.

> **Tests de dominio**: Usar `TestableUser` para preparar estado previo. Usar `DomainFixture` para resolver comandos y validators. **NO encadenar comandos** para crear estado.
>
> **Tests de slice**: Usar `TestableUser` para el estado que devuelve el repository mock. Usar `DomainFixture` para resolver el comando que la slice inyecta. Mock de `IRepository` e `IUnitOfWork`.

---

### 7.1 User.RegisterWithGoogle

> Este comando lo ejecuta el sistema internamente durante el flujo OAuth cuando el usuario no existe. No es un endpoint público directo — se invoca desde la slice de LoginWithGoogle.

#### Event Storming
```
🟡[Sistema] → 🔵(RegisterWithGoogle) → 🟤[[User]] → 🟠<UserRegistered>
                                          │
                                🟣{ProviderIdUnique}
```

#### Input

| Campo | Tipo | Default |
|-------|------|---------|
| ProviderId | string | |
| Email | string | |
| Name | string | |
| Phone | string? | null |
| AvatarUrl | string? | null |

#### Inyecta
- `IValidator<User>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| ProviderId ya existe con mismo Provider | 409 | ConflictGuard | "A user with this provider already exists" |

**Nota:** El guard de duplicado se evalúa a nivel de slice consultando el repositorio, no dentro del comando de dominio.

#### Lógica
```csharp
var user = new User(Guid.NewGuid())
{
    ProviderId = command.ProviderId,
    Provider = AuthProvider.Google,
    Email = command.Email,
    Name = command.Name,
    Phone = command.Phone,
    AvatarUrl = command.AvatarUrl,
    Password = null,
    LastLoginAt = DateTime.UtcNow,
    IsActive = true
};

return userValidator.ValidateOrThrow(user);
```

**Notas:**
- El User se crea activo inmediatamente — ya pasó la autenticación de Google.
- `LastLoginAt` se inicializa al momento del registro porque el registro es simultáneo al primer login.

#### Tests Unitarios (Dominio)

✅ Crear user con datos válidos de Google
- Input: ProviderId="google|123", Email="pedro@test.com", Name="Pedro", Phone=null, AvatarUrl="https://..."
- Resultado: User creado con Provider=Google, IsActive=true, Password=null

❌ ProviderId vacío
- Input: ProviderId=""
- Resultado: ValidationException "ProviderId is required"

❌ Email vacío
- Input: Email=""
- Resultado: ValidationException "Email is required"

❌ Email inválido
- Input: Email="not-an-email"
- Resultado: ValidationException "Email is not valid"

❌ Name vacío
- Input: Name=""
- Resultado: ValidationException "Name is required"

---

### 7.2 LoginWithGoogle

> Son **dos slices** que implementan el flujo completo de Google OAuth. El POST inicia el flujo (redirect a Google). El GET recibe el callback de Google con el authorization code.

#### Event Storming
```
🟡[Usuario] → 🔵(LoginWithGoogle) → ⚡Google OAuth → 🟤[[User]] → 🟠<UserLoggedIn>
                                                          │
                                                🟣{Si no existe → RegisterWithGoogle}
```

#### Flujo completo

```
Usuario hace POST /auth/login/google (clic en "Entrar con Google")
        │
        ▼
Backend genera state (valor aleatorio criptográfico)
        │
        ▼
Backend setea cookie temporal con el state:
  Set-Cookie: fudie_oauth_state={state}; Path=/auth/login/google; HttpOnly; Secure; SameSite=Lax; Max-Age=300
        │
        ▼
Backend construye URL de Google OAuth:
  https://accounts.google.com/o/oauth2/v2/auth?
    client_id={clientId}
    &redirect_uri={baseUrl}/auth/login/google
    &response_type=code
    &scope=openid email profile
    &access_type=online
    &state={state}
        │
        ▼
Backend devuelve 302 Redirect a la URL de Google
        │
        ▼
Google autentica al usuario (pantalla de login/consentimiento)
        │
        ▼
Google redirige al callback: GET /auth/login/google?code={code}&state={state}
        │
        ▼
Backend lee cookie fudie_oauth_state y compara con query param state
  → Si no coincide → 401 (CSRF detectado)
  → Si coincide → continúa
        │
        ▼
Backend elimina cookie fudie_oauth_state (ya no se necesita)
        │
        ▼
Refit llama a https://oauth2.googleapis.com/token (server-to-server)
  → envía: code, client_id, client_secret, redirect_uri, grant_type=authorization_code
  → recibe: id_token, access_token
        │
        ▼
Valida id_token contra JWKS de Google (firma, expiración, audience)
        │
        ▼
Extrae del id_token: sub (ProviderId), email, name, picture (AvatarUrl)
        │
        ▼
Busca User por ProviderId + Provider=Google
        │
        ├─ No existe → RegisterWithGoogle (crea User)
        │
        └─ Existe → Actualiza LastLoginAt, Name, Email, AvatarUrl si cambiaron en Google
        │
        ▼
Crea sesión en Firestore (sessions/{sessionId})
        │
        ▼
Setea cookie fudie_session={sessionId}
        │
        ▼
Redirige al frontend (302 a la URL de la app)
```

#### Protección CSRF con cookie temporal (state)

El parámetro `state` de OAuth protege contra CSRF en el callback. Sin él, un atacante podría fabricar una URL de callback con un code válido de otra cuenta y forzar al usuario a iniciar sesión con una cuenta que no es suya.

**Mecanismo:**

1. El POST genera un valor aleatorio criptográfico (`state`).
2. Lo guarda en una cookie temporal `fudie_oauth_state` (HttpOnly, Secure, SameSite=Lax, Max-Age=300 segundos).
3. Lo envía a Google como parámetro `state` en la URL de redirect.
4. Google devuelve el mismo `state` en el callback GET.
5. El GET compara el `state` del query string con el de la cookie. Si no coincide, rechaza.
6. La cookie se elimina después de la validación.

**¿Por qué la cookie llega al GET callback?** Porque Google hace un redirect 302, que es una navegación GET del navegador al mismo dominio. `SameSite=Lax` permite el envío de cookies en navegaciones GET de nivel superior.

**Cookie temporal:**

```
Set-Cookie: fudie_oauth_state={state};
  Path=/auth/login/google;
  Max-Age=300;
  Secure;
  HttpOnly;
  SameSite=Lax
```

| Atributo | Justificación |
|----------|---------------|
| `Path=/auth/login/google` | Solo se envía al callback, no a toda la app |
| `Max-Age=300` | 5 minutos para completar el flujo con Google. Después muere sola. |
| `HttpOnly` | JavaScript no puede leerla |
| `Secure` | Solo HTTPS |
| `SameSite=Lax` | Llega en el redirect de Google (navegación GET top-level) |

#### Interfaz Refit para Google OAuth

```csharp
public interface IGoogleOAuthApi
{
    [Post("/token")]
    Task<GoogleTokenResponse> ExchangeCodeAsync(
        [Body(BodySerializationMethod.UrlEncoded)] GoogleTokenRequest request);
}

public record GoogleTokenRequest(
    [AliasAs("code")] string Code,
    [AliasAs("client_id")] string ClientId,
    [AliasAs("client_secret")] string ClientSecret,
    [AliasAs("redirect_uri")] string RedirectUri,
    [AliasAs("grant_type")] string GrantType = "authorization_code"
);

public record GoogleTokenResponse(
    [property: JsonPropertyName("id_token")] string IdToken,
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("token_type")] string TokenType,
    [property: JsonPropertyName("expires_in")] int ExpiresIn
);
```

**Configuración Refit en Program.cs:**
```csharp
builder.Services
    .AddRefitClient<IGoogleOAuthApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://oauth2.googleapis.com"));
```

#### Slice 1: POST /auth/login/google — Inicia flujo OAuth

Inicia el flujo OAuth redirigiendo al usuario a Google.

**Request**: Sin body.

**Response**: 302 Redirect a Google OAuth + cookie `fudie_oauth_state`

**Endpoint**: `.AllowAnonymous()`

**Nota**: Este endpoint no aparece en Swagger. No es una API consumible — es una navegación del navegador.

#### Slice 2: GET /auth/login/google — Callback de Google

Recibe el callback de Google con el authorization code, intercambia el code por el id_token vía Refit (server-to-server), crea o actualiza el User, crea sesión y setea la cookie de sesión.

**Query params** (enviados por Google):

| Param | Tipo | Descripción |
|-------|------|-------------|
| code | string | Authorization code de Google |
| state | string | Valor anti-CSRF para validar contra la cookie |

**Response**: 302 Redirect al frontend + cookie `fudie_session`

**Endpoint**: `.AllowAnonymous()`

**Nota**: Este endpoint no aparece en Swagger. Google lo llama directamente como redirect.

#### Tests Integración

✅ Flujo completo: POST redirige a Google → GET callback con code válido, usuario nuevo → se registra, cookie seteada, redirect al frontend

✅ Flujo completo: POST redirige a Google → GET callback con code válido, usuario existente → login, cookie seteada, redirect al frontend

❌ GET callback con state inválido (no coincide con cookie) → 401

❌ GET callback sin cookie fudie_oauth_state → 401

❌ GET callback con code inválido (Google rechaza el intercambio) → 401

❌ GET callback con id_token inválido (firma no válida) → 401

---

### 7.3 LoginWithPassword

> Slice exclusiva para superadministradores de Fudie con `AuthProvider.Local`.

#### Event Storming
```
🟡[SuperAdmin] → 🔵(LoginWithPassword) → 🟤[[User]] → 🟠<UserLoggedIn>
                                             │
                                   🟣{ValidateCredentials}
```

#### Slice: POST /auth/login

**Request**
```csharp
public record LoginWithPasswordRequest(
    string Email,
    string Password
);
```

**Endpoint**: `.AllowAnonymous()`

#### Lógica de la slice
```csharp
var user = await userRepository.GetByEmailAndProviderAsync(command.Email, AuthProvider.Local);

UnauthorizedGuard.ThrowIfNull(user, "Invalid credentials");

var isValid = passwordHasher.Verify(command.Password, user!.Password!.Hash, user.Password.Salt);

UnauthorizedGuard.ThrowIf(!isValid, "Invalid credentials");

user.LastLoginAt = DateTime.UtcNow;

// Crear sesión en Firestore
var session = await sessionService.CreateAsync(user);

return (user, session);
```

**Response**: 200 OK → cookie `fudie_session` + `UserResponse`

**Notas:**
- El mensaje de error es genérico ("Invalid credentials") tanto si el email no existe como si el password es incorrecto. No se filtra información.
- Solo busca usuarios con `Provider == AuthProvider.Local`.

#### Tests Integración

✅ 200 OK → UserResponse + cookie fudie_session

❌ 401 → Email no existe

❌ 401 → Password incorrecto

❌ 401 → Usuario existe pero es OAuth (Provider != Local)

---

### 7.4 Logout

#### Event Storming
```
🟡[Usuario] → 🔵(Logout) → 🟤[[Session]] → 🟠<SessionDestroyed>
```

#### Slice: POST /auth/logout

**Endpoint**: RequireAuthorization (usuario autenticado)

#### Lógica
```csharp
var sessionId = httpContext.Request.Cookies["fudie_session"];

await sessionRepository.DeleteAsync(sessionId);

httpContext.Response.Cookies.Delete("fudie_session");
```

**Response**: 204 No Content

#### Tests Integración

✅ 204 No Content → cookie eliminada, sesión borrada de Firestore

❌ 401 → No autenticado

---

### 7.5 GetUser

#### Event Storming
```
🟡[SuperAdmin] → 🔵(GetUser) → 🟤[[User]] → 📊 UserResponse
```

#### Slice: GET /users/{id}

**Response**: 200 OK → `UserResponse`

#### Tests Unitarios (Servicio)

✅ Obtiene el user del repositorio con el id correcto

✅ Retorna Response mapeado correctamente (sin Password)

#### Tests Integración

✅ 200 OK → UserResponse

❌ 401 → No autenticado

❌ 403 → Sin permiso

❌ 404 → No encontrado

---

### 7.6 GetCurrentUser

> Devuelve el usuario autenticado actual a partir de la sesión.

#### Event Storming
```
🟡[Usuario] → 🔵(GetCurrentUser) → 🟤[[User]] → 📊 UserResponse
```

#### Slice: GET /auth/me

**Response**: 200 OK → `UserResponse`

**Endpoint**: RequireAuthorization (no requiere permiso específico, solo estar autenticado)

#### Tests Integración

✅ 200 OK → UserResponse del usuario autenticado

❌ 401 → No autenticado

---

### 7.7 User.Update

#### Event Storming
```
🟡[Usuario] → 🔵(UpdateUser) → 🟤[[User]] → 🟠<UserUpdated>
```

#### Input

| Campo | Tipo |
|-------|------|
| Name | string |
| Phone | string? |

#### Inyecta
- `IValidator<User>`

#### Guards

Ninguno.

#### Lógica
```csharp
user.Name = command.Name;
user.Phone = command.Phone;

return userValidator.ValidateOrThrow(user);
```

**Notas:**
- Email no se actualiza por este comando. El email viene del proveedor OAuth o fue definido por script.

#### Slice: PUT /users/{id}

**Request**
```csharp
public record UpdateUserRequest(
    string Name,
    string? Phone
);
```

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

> Estado previo: `TestableUser` con propiedades iniciales.

✅ Actualizar user existente
- Precondición: User existe
- Input: Name="Nuevo Nombre", Phone="+34666999888"
- Resultado: User actualizado

❌ Name vacío
- Input: Name=""
- Resultado: ValidationException "Name is required"

#### Tests Integración

✅ 204 No Content

❌ 401 → No autenticado

❌ 403 → Sin permiso

❌ 404 → User no encontrado

❌ 422 → Validación fallida

---

### 7.8 User.Deactivate

#### Event Storming
```
🟡[SuperAdmin] → 🔵(DeactivateUser) → 🟤[[User]] → 🟠<UserDeactivated>
                                           │
                                 🟣{InvalidateSessions}
```

#### Input

Ninguno

#### Inyecta
- `IValidator<User>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Ya está inactivo | 409 | ConflictGuard | "User is already inactive" |

#### Lógica
```csharp
ConflictGuard.ThrowIf(!user.IsActive, "User is already inactive");

user.IsActive = false;

return userValidator.ValidateOrThrow(user);
```

**Efecto adicional en la slice:** Se eliminan todas las sesiones del usuario en Firestore. La siguiente request con cualquier cookie de este usuario → 401.

#### Slice: POST /users/{id}/deactivate

**Response**: 200 OK → `UserResponse`

#### Tests Unitarios (Dominio)

> Estado previo: `TestableUser` con IsActive=true/false.

✅ Desactivar user activo
- Precondición: User con IsActive=true
- Resultado: User con IsActive=false

❌ Ya inactivo
- Precondición: User con IsActive=false
- Resultado: ConflictException "User is already inactive"

#### Tests Integración

✅ 200 OK → UserResponse con IsActive=false

❌ 401 → No autenticado

❌ 403 → Sin permiso

❌ 404 → User no encontrado

❌ 409 → Ya estaba inactivo

---

### 7.9 User.Activate

#### Event Storming
```
🟡[SuperAdmin] → 🔵(ActivateUser) → 🟤[[User]] → 🟠<UserActivated>
```

#### Input

Ninguno

#### Inyecta
- `IValidator<User>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Ya está activo | 409 | ConflictGuard | "User is already active" |

#### Lógica
```csharp
ConflictGuard.ThrowIf(user.IsActive, "User is already active");

user.IsActive = true;

return userValidator.ValidateOrThrow(user);
```

#### Slice: POST /users/{id}/activate

**Response**: 200 OK → `UserResponse`

#### Tests Unitarios (Dominio)

✅ Activar user inactivo
- Precondición: User con IsActive=false
- Resultado: User con IsActive=true

❌ Ya activo
- Precondición: User con IsActive=true
- Resultado: ConflictException "User is already active"

#### Tests Integración

✅ 200 OK → UserResponse con IsActive=true

❌ 401 → No autenticado

❌ 403 → Sin permiso

❌ 404 → User no encontrado

❌ 409 → Ya estaba activo

---

### 7.10 User.ChangePassword

> Exclusivo para usuarios con `AuthProvider.Local` (superadmin).

#### Event Storming
```
🟡[SuperAdmin] → 🔵(ChangePassword) → 🟤[[User]] → 🟠<PasswordChanged>
                                          │
                                🟣{ValidateCurrentPassword}
                                🟣{OnlyLocalUsers}
```

#### Input

| Campo | Tipo |
|-------|------|
| CurrentPassword | string |
| NewPassword | string |

#### Inyecta
- `HashedPassword.Create`
- `IPasswordHasher`
- `IValidator<User>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| User no es Local | 409 | ConflictGuard | "Password change is only available for local users" |
| CurrentPassword no coincide | 401 | UnauthorizedGuard | "Invalid credentials" |

#### Lógica
```csharp
ConflictGuard.ThrowIf(user.Provider != AuthProvider.Local,
    "Password change is only available for local users");

var isValid = passwordHasher.Verify(command.CurrentPassword, user.Password!.Hash, user.Password.Salt);

UnauthorizedGuard.ThrowIf(!isValid, "Invalid credentials");

user.Password = createHashedPassword.Execute(
    new CreateHashedPasswordCommand(command.NewPassword));

return userValidator.ValidateOrThrow(user);
```

**Efecto adicional en la slice:** Se eliminan todas las sesiones del usuario excepto la actual. Obliga a re-login en otros dispositivos.

#### Slice: POST /users/{id}/change-password

**Request**
```csharp
public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword
);
```

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

> Estado previo: `TestableUser` con Provider=Local y Password configurado.

✅ Cambiar password con credenciales válidas
- Precondición: User con Provider=Local, Password conocido
- Input: CurrentPassword=correcto, NewPassword="NuevoP@ss456"
- Resultado: Password actualizado

❌ User no es Local
- Precondición: User con Provider=Google
- Resultado: ConflictException "Password change is only available for local users"

❌ CurrentPassword incorrecto
- Precondición: User con Provider=Local
- Input: CurrentPassword=incorrecto
- Resultado: UnauthorizedException "Invalid credentials"

#### Tests Integración

✅ 204 No Content

❌ 401 → No autenticado o contraseña actual incorrecta

❌ 403 → Sin permiso

❌ 404 → User no encontrado

❌ 409 → User no es Local

---

## 8. Descripciones de Permisos

### Scopes atómicos

| Scope (nombre de clase) | Descripción (es) |
|--------------------------|-------------------|
| `LoginWithGoogle` (POST) | *AllowAnonymous — no participa en el catálogo de permisos* |
| `LoginWithGoogle` (GET callback) | *AllowAnonymous — no participa en el catálogo de permisos* |
| `LoginWithPassword` | *AllowAnonymous — no participa en el catálogo de permisos* |
| `Logout` | *Requiere autenticación, pero no permiso específico* |
| `GetCurrentUser` | *Requiere autenticación, pero no permiso específico* |
| `GetUser` | Ver los datos de un usuario |
| `UpdateUser` | Modificar nombre y teléfono de un usuario |
| `DeactivateUser` | Desactivar un usuario para revocar su acceso |
| `ActivateUser` | Reactivar un usuario previamente desactivado |
| `ChangePassword` | Cambiar la contraseña de un usuario local |

**Notas:**
- Los endpoints de login, logout y "me" son infraestructura de Auth — no participan en el catálogo de permisos.
- Los endpoints de gestión de usuarios (Get, Update, Activate, Deactivate, ChangePassword) sí participan en el catálogo — solo el superadmin o roles con permiso explícito pueden gestionar usuarios de otros.

### Agrupaciones automáticas

| Agrupación | Scopes que incluye |
|------------|-------------------|
| `user:read` | `GetUser` |
| `user:write` | `UpdateUser`, `DeactivateUser`, `ActivateUser`, `ChangePassword` |

---

## 9. Resumen de Endpoints (Orden de Implementación)

| # | Método | Ruta | Comando/Query | Response | Auth | Swagger |
|---|--------|------|---------------|----------|------|---------|
| 1 | POST | /auth/login/google | LoginWithGoogle (inicia flujo) | 302 Redirect a Google + cookie `fudie_oauth_state` | AllowAnonymous | No |
| 2 | GET | /auth/login/google | LoginWithGoogle (callback) | 302 Redirect al frontend + cookie `fudie_session` | AllowAnonymous | No |
| 3 | POST | /auth/login | LoginWithPassword | 200 → `UserResponse` + cookie | AllowAnonymous | Sí |
| 4 | POST | /auth/logout | Logout | 204 | Authenticated | Sí |
| 5 | GET | /auth/me | GetCurrentUser | 200 → `UserResponse` | Authenticated | Sí |
| 6 | GET | /users/{id} | GetUser | 200 → `UserResponse` | RequirePermission | Sí |
| 7 | PUT | /users/{id} | User.Update | 204 | RequirePermission | Sí |
| 8 | POST | /users/{id}/deactivate | User.Deactivate | 200 → `UserResponse` | RequirePermission | Sí |
| 9 | POST | /users/{id}/activate | User.Activate | 200 → `UserResponse` | RequirePermission | Sí |
| 10 | POST | /users/{id}/change-password | User.ChangePassword | 204 | RequirePermission | Sí |

---

## 10. Persistencia (Firestore)

### Colección

`/users/{userId}`

### Configuración DbContext

```csharp
modelBuilder.Entity<User>(entity =>
{
    // Ignore: propiedades computed
    entity.Ignore(x => x.IsOAuth);
    entity.Ignore(x => x.HasPassword);

    // ComplexType: Password (nullable)
    entity.ComplexProperty(x => x.Password, pwd =>
    {
        // No hay propiedades computed en HashedPassword
    });
});
```

### Documento Ejemplo (Google OAuth)

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "providerId": "google|117234856923",
  "provider": "Google",
  "email": "pedro@example.com",
  "name": "Pedro García",
  "phone": "+34666123456",
  "avatarUrl": "https://lh3.googleusercontent.com/...",
  "password": null,
  "lastLoginAt": "2026-02-09T10:30:00Z",
  "isActive": true
}
```

### Documento Ejemplo (SuperAdmin Local)

```json
{
  "id": "a1b2c3d4-5678-9012-3456-789012345678",
  "providerId": "local|superadmin-001",
  "provider": "Local",
  "email": "admin@fudie.app",
  "name": "Fudie Admin",
  "phone": "+34900000000",
  "avatarUrl": null,
  "password": {
    "hash": "$2a$12$LJ3m1...",
    "salt": "random-salt-value"
  },
  "lastLoginAt": "2026-02-09T09:00:00Z",
  "isActive": true
}
```

### Índices requeridos

| Campo(s) | Tipo | Justificación |
|----------|------|---------------|
| `providerId` + `provider` | Composite | Buscar usuario por proveedor en login OAuth |
| `email` + `provider` | Composite | Buscar usuario por email en login local |
| `email` | Simple | Unicidad global del email (guard por lectura + ConflictGuard en slice) |

---

## 11. Script de Seed: SuperAdmin

> El superadmin se crea por script, nunca por endpoint. Ejemplo de estructura del script:

```csharp
// seed-superadmin.csx o como migration/startup task
var hashedPassword = passwordHasher.Hash("initial-secure-password", passwordHasher.GenerateSalt());

var superAdmin = new User(Guid.NewGuid())
{
    ProviderId = "local|superadmin-001",
    Provider = AuthProvider.Local,
    Email = "admin@fudie.app",
    Name = "Fudie Admin",
    Phone = "+34900000000",
    AvatarUrl = null,
    Password = new HashedPassword(hashedPassword.Hash, hashedPassword.Salt),
    LastLoginAt = null,
    IsActive = true
};

await userRepository.AddAsync(superAdmin);
await unitOfWork.SaveChangesAsync();
```

**Notas:**
- El password inicial se cambia en el primer login (o se inyecta por variable de entorno).
- El `ProviderId` para usuarios locales sigue el formato `local|{identifier}` para mantener consistencia con el campo.

---

## 12. Validación del id_token de Google

El `id_token` que devuelve Google tras el intercambio del authorization code es un JWT firmado por Google. Se valida contra las claves públicas JWKS de Google:

```
https://www.googleapis.com/oauth2/v3/certs
```

Mismo patrón que los microservicios usan para validar el JWT efímero del servicio de Auth vía `/.well-known/jwks.json`. Se usa `Microsoft.IdentityModel.Tokens` con `TokenValidationParameters`:

- **IssuerSigningKeys**: descargadas de la URL JWKS de Google.
- **ValidAudience**: el `client_id` de la aplicación Fudie en Google Cloud Console.
- **ValidIssuers**: `https://accounts.google.com` y `accounts.google.com`.
- **ValidateLifetime**: `true`.

Las claves se cachean al arrancar y se refrescan periódicamente (Google las rota).

---

## 13. Página de Desarrollo para Testing

En desarrollo, Swagger no puede iniciar el flujo OAuth (es una navegación del navegador, no una API call). Se sirve una página estática en `/dev` con un formulario que hace POST a `/auth/login/google`:

```csharp
if (app.Environment.IsDevelopment())
{
    app.MapGet("/dev", () => Results.Content(DevLoginPage.Html, "text/html"))
        .AllowAnonymous();
}
```

La página contiene:

```html
<form method="POST" action="/auth/login/google">
    <button type="submit">Entrar con Google</button>
</form>
```

El navegador sigue el redirect a Google → Google callback → cookie seteada → vuelves a `/swagger` y todo funciona.

No se necesita protección CSRF en el formulario: no hay cookie ni sesión previa, y el POST solo dispara un redirect a Google sin efectos secundarios.

---

## 14. Hot Spots ⚠️

| # | Pregunta | Estado |
|---|----------|--------|
| 1 | Phone nullable — Google puede no devolverlo | Decidido: phone es nullable. Se completará antes de comprar suscripción. |
| 2 | Email único globalmente o por Provider | Decidido: único global. Guard por lectura + ConflictGuard en slice. |
| 3 | Validación id_token de Google | Decidido: JWKS v3 de Google con Microsoft.IdentityModel.Tokens. |
| 4 | Endpoint para cambiar password del superadmin | Decidido: sí, POST /users/{id}/change-password |
| 5 | Rate limiting específico en endpoints de login | Pendiente |

---

**Fecha**: 2026-02-09
**Autor**: Equipo Fudie
