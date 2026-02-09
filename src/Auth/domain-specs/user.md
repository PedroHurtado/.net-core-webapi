# Fudie – Domain Specification: User – Corrección Sección 7.2 y Endpoints

## Cambios respecto a la versión anterior

La sección 7.2 LoginWithGoogle y la tabla de endpoints estaban mal diseñadas. El flujo OAuth no funciona como estaba descrito. Este documento corrige esas secciones.

---

## 7.2 LoginWithGoogle

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

## 9. Resumen de Endpoints (Orden de Implementación) — CORREGIDO

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

---

## Página de desarrollo para testing

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

El navegador sigue el redirect a Google, Google callback → cookie seteada → vuelves a `/swagger` y todo funciona.

---

**Fecha**: 2026-02-09
