# Fudie — Escenarios de creación de Owner

## 1. Seed de plataforma

El seed crea el super admin de Fudie y el tenant de plataforma. Se ejecuta una sola vez en producción. En desarrollo se ejecuta tantas veces como haga falta (si ya existe, destruye todo y vuelve a crear).

### Protección

El endpoint `POST /auth/seed` se protege con `SeedKey`: un GUID configurado como variable de entorno (`FUDIE_SEED_KEY`). Sin esa clave en el header, 401. En producción se elimina la variable tras el primer uso. En desarrollo se deja fija en `appsettings.Development.json`.

### Flujo

| Paso | Acción | Dónde |
|------|--------|-------|
| 1 | Pipeline llama a `POST /auth/seed` con `SeedKey` + datos del super admin (email, nombre, contraseña) | Auth |
| 2 | Auth valida la `SeedKey`. Si ya existe el super admin, destruye todo (Membership, TenantRoles, Customer, User) y empieza de cero | Auth |
| 3 | Auth crea el User con `AuthProvider.Local` y contraseña | Auth |
| 4 | Auth genera JWT interno (tiene la clave privada) | Auth |
| 5 | Auth llama a `POST /customers` con JWT interno para crear el tenant de plataforma con el GUID fijo de `Fudie:PlatformTenantId` | Customer |
| 6 | Customer crea el tenant y devuelve el Id | Customer |
| 7 | Auth ejecuta SeedSystemRoles (5 roles para ese tenant) | Auth |
| 8 | Auth crea Membership Owner sin invitación, vinculando el User con el TenantRole Owner | Auth |

### Request

```
POST /auth/seed
Header: X-Seed-Key: {GUID}

{
  "email": "admin@fudie.app",
  "name": "Pedro",
  "password": "SecureP@ss123"
}
```

---

## 2. Suscripción externa

Un usuario compra un plan para su restaurante. El tenant, los roles y la Membership Owner se crean después de que Stripe confirme el pago.

### Flujo

| Paso | Acción | Dónde |
|------|--------|-------|
| 1 | Usuario autenticado (JWT con `sub`) elige plan en el frontend | Frontend |
| 2 | Frontend llama a `POST /checkout` con el PriceId del plan | Subscription |
| 3 | Subscription crea la sesión de Stripe Checkout con PriceId y email del usuario. Devuelve la URL del formulario de Stripe | Subscription |
| 4 | Usuario completa el pago en el formulario de Stripe | Stripe |
| 5 | Stripe envía webhook a Subscription con `checkout.session.completed` | Subscription |
| 6 | Subscription valida la firma del webhook (secreto de Stripe) | Subscription |
| 7 | Subscription llama a `POST /auth/internal/token` para obtener JWT interno | Auth |
| 8 | Auth genera JWT interno y lo devuelve | Auth |
| 9 | Subscription llama a `POST /customers` con JWT interno para crear el tenant del restaurante | Customer |
| 10 | Customer crea el tenant y devuelve el Id | Customer |
| 11 | Subscription llama a Auth con JWT interno para ejecutar SeedSystemRoles y crear Membership Owner para el UserId que pagó | Auth |
| 12 | Subscription crea el registro de Subscription vinculando el tenant con el plan de Stripe | Subscription |
| 13 | Usuario refresca, hace login, ve el nuevo tenant en su selector | Frontend |

### Endpoints involucrados

| Endpoint | Extensión | Microservicio |
|----------|-----------|---------------|
| `POST /checkout` | `.RequireAuthenticated()` | Subscription |
| `POST /auth/internal/token` | `.RequireInternal()` | Auth |
| `POST /customers` | `.RequireInternal()` | Customer |
| SeedSystemRoles | `.RequireInternal()` | Auth |
| Crear Membership Owner | `.RequireInternal()` | Auth |

### Protección de `POST /auth/internal/token`

`.RequireInternal()` — la autenticación de la llamada depende del entorno:

- **Cloud Run**: service-to-service authentication con service accounts de Google Cloud
- **On-premise**: secreto compartido (GUID) en variable de entorno, enviado en header
- **Desarrollo**: user-secrets de .NET

---

## 3. Clasificación de endpoints

| Extensión | Qué exige | Ejemplo |
|-----------|-----------|---------|
| `.AllowAnonymous()` | Nada | `POST /auth/login` |
| `.RequireAuthenticated()` | JWT con `sub` válido | `POST /checkout` |
| Sin nada (defecto) | JWT con `tid` + permisos | `GET /menus`, `POST /reservations` |
| `.RequirePlatform()` | JWT con `tid` = tenant de plataforma + permisos | Escritura en catálogo maestro |
| `.RequireInternal()` | Autenticación entre microservicios | `POST /customers`, `POST /auth/internal/token` |

### Orden de evaluación en el middleware

1. `.AllowAnonymous()` → pasa
2. `.RequireInternal()` → valida autenticación de infraestructura/secreto
3. ¿JWT válido? → si no, 401
4. `.RequireAuthenticated()` → tiene `sub`, pasa
5. `.RequirePlatform()` → `tid` debe ser el de plataforma, si no 403
6. `owner: true` → bypass de permisos dentro de su tenant
7. Validación de scopes (groups, additional, excluded)

---

## 4. Secretos

Un solo almacén en todos los entornos. En desarrollo, todos los proyectos comparten el mismo `UserSecretsId`. En producción, un solo Cloud Secret Manager.

```xml
<!-- Mismo UserSecretsId en todos los csproj -->
<UserSecretsId>569f94d2-6a9b-4b8c-b190-7525f739671c</UserSecretsId>
```

### Catálogo de secretos

| Clave | Descripción | Quién lo usa |
|-------|-------------|--------------|
| `Google:OAuth` | Credenciales OAuth de Google | Auth |
| `Jwt:PrivateKey` | Clave privada ES256 para firmar JWTs | Auth |
| `Jwt:Kid` | Key ID del par de claves | Auth |
| `Fudie:SeedKey` | GUID para proteger el endpoint del seed | Auth |
| `Fudie:PlatformTenantId` | GUID fijo del tenant de plataforma | Todos |
| `Fudie:InternalSecret` | GUID compartido para `.RequireInternal()` | Todos |
| `Stripe:SecretKey` | Clave secreta de Stripe | Subscription |
| `Stripe:WebhookSecret` | Secreto para validar firma de webhooks de Stripe | Subscription |
