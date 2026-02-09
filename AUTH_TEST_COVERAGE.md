# Auth - Cobertura de Tests

## Tests existentes

| Fuente | Test | Tipo |
|--------|------|------|
| `User.cs` | `UserTests.cs` | Contrato |
| `UserValidator` | `UserValidatorTests.cs` | Validador |
| `AuthProvider.cs` | `AuthProviderTests.cs` | Contrato |
| `HashedPassword.cs` | `HashedPasswordTests.cs` | Contrato |
| `HashedPasswordValidator` | `HashedPasswordValidatorTests.cs` | Validador |
| `HashedPassword_Create.cs` | `HashedPasswordCreateTests.cs` | Comando |
| `User_Create.cs` | `UserCreateTests.cs` | Comando |
| `User_UpdateFromOAuth.cs` | `UserUpdateFromOAuthTests.cs` | Comando |
| `IPasswordHasher.cs` | `IPasswordHasherTests.cs` | Contrato |
| `BcryptPasswordHasher.cs` | `BcryptPasswordHasherTests.cs` | Implementacion |
| `IGoogleOAuthSettings.cs` | `GoogleOAuthSettingsTests.cs` | Contrato |
| `DevGoogleOAuthSettings.cs` | `DevGoogleOAuthSettingsTests.cs` | Implementacion |
| `IGoogleOAuthUrlBuilder.cs` | `GoogleOAuthUrlTests.cs` | Contrato |
| `GoogleOAuthUrlBuilder.cs` | `GoogleOAuthUrlBuilderTests.cs` | Implementacion |
| `IGoogleOAuthApi.cs` | `GoogleTokenRequestTests.cs` + `GoogleTokenResponseTests.cs` | Contrato |
| `IGoogleIdTokenValidator.cs` | `GoogleIdTokenClaimsTests.cs` | Contrato |
| `GoogleIdTokenValidator.cs` | `GoogleIdTokenValidatorTests.cs` | Implementacion |

## Tests unitarios faltantes

| Fuente | Test faltante | Descripcion |
|--------|---------------|-------------|
| `IGoogleCertificateProvider.cs` | `GoogleCertificateProviderTests.cs` | Cache con TTL, parsing PEM a SecurityKey, thread-safety |
| `InitiateGoogleLogin.cs` | `InitiateGoogleLoginTests.cs` | Handler: cookie state + redirect. Service: llama urlBuilder |
| `GoogleLoginCallback.cs` | `GoogleLoginCallbackTests.cs` | Handler: validacion state, cookie session, redirect. Service: exchange code, validar token, crear/actualizar user |

## Tests de integracion faltantes

| Slice | Escenario | Resultado esperado |
|-------|-----------|--------------------|
| POST `/auth/login/google` | Flujo normal | Redirect a Google + cookie `fudie_oauth_state` seteada |
| GET `/auth/login/google` | Usuario nuevo, code valido | Se registra, cookie `fudie_session`, redirect |
| GET `/auth/login/google` | Usuario existente, code valido | Login, cookie `fudie_session`, redirect |
| GET `/auth/login/google` | State no coincide con cookie | 401 |
| GET `/auth/login/google` | Sin cookie `fudie_oauth_state` | 401 |
| GET `/auth/login/google` | Code invalido (Google rechaza) | 401 |
| GET `/auth/login/google` | id_token invalido (firma no valida) | 401 |
