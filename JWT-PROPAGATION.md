# JWT Propagation — Pendiente para después de separación en librerías

## Contexto

Tras implementar `IFudieUser` y `ClaimsPrincipal` en el middleware, queda pendiente la propagación del JWT entre microservicios, tanto en comunicación síncrona (HTTP/Refit) como asíncrona (PubSub).

## Estado actual

- El middleware valida el JWT contra JWKS (`JwksUrl` cacheada 60 min) y genera `FudieTokenContext` + `ClaimsPrincipal`.
- **No se guarda el JWT raw** en ningún sitio después de la validación.
- `IMessageContext` (PubSub) no se puebla desde HTTP — los claims del Envelope viajan vacíos.
- No existe un `DelegatingHandler` genérico para propagar el JWT en llamadas Refit entre microservicios.

## Cambios necesarios

### 1. Guardar el JWT raw en el middleware

En `FudieAuthorizationMiddleware.SetTokenContext`, guardar el token en `HttpContext.Items`:

```csharp
context.Items["FudieJwt"] = token; // el string raw del Bearer
```

Exponer en `IFudieUser`:

```csharp
public string? Token => httpContextAccessor.HttpContext?.Items["FudieJwt"] as string;
```

### 2. Poblar IMessageContext desde HTTP

Crear un puente para que cuando un servicio HTTP publique un mensaje PubSub, los claims del JWT viajen en el Envelope. Opciones:

- Middleware que pueble `MessageContext` desde `ClaimsPrincipal` (si PubSub está registrado).
- Factory en `MessagePublisher` que lea de `IFudieUser` / `HttpContext.User` cuando `IMessageContext.Claims` está vacío.

Claims a propagar en el Envelope:

| Claim     | Descripción          |
|-----------|----------------------|
| `sub`     | UserId               |
| `tid`     | TenantId (opcional)  |
| `owner`   | IsOwner (si aplica)  |
| `groups`  | Grupos de permisos   |
| `add`     | Scopes adicionales   |
| `exc`     | Scopes excluidos     |

### 3. DelegatingHandler para Refit (propagación HTTP→HTTP)

Un handler genérico que reenvíe el JWT en llamadas entre microservicios:

```csharp
public class JwtPropagationHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = httpContextAccessor.HttpContext?.Items["FudieJwt"] as string;
        if (token is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }
}
```

Cada microservicio lo registra en su Refit client:

```csharp
.AddHttpMessageHandler<JwtPropagationHandler>();
```

### 4. Propagación PubSub→HTTP (subscriber llama a otro microservicio)

Cuando un subscriber recoge un mensaje y necesita llamar a otro servicio vía Refit:

- El JWT viaja como metadata en el Envelope (campo Claims o un campo `Token` dedicado).
- El subscriber restaura el token en el contexto del scope.
- El `DelegatingHandler` de Refit lo propaga igual que en HTTP.
- **No se revalida el JWT** en el subscriber: el PubSub es infraestructura interna y el token ya fue validado por el middleware en el origen.

## Decisión: no revalidar JWT en PubSub

El JWT que viaja en el Envelope ya fue validado por `FudieAuthorizationMiddleware` antes de publicar el mensaje. El PubSub es infraestructura interna (no superficie de ataque externa). Revalidar sería redundante.

Riesgo aceptado: si el token expira entre publicación y consumo, el subscriber lo usará igual. Esto es aceptable porque los mensajes se procesan en segundos y los tokens tienen TTL de minutos.

## Librerías afectadas

| Librería        | Cambio                                                    |
|-----------------|-----------------------------------------------------------|
| Fudie           | Guardar JWT raw, exponer `Token` en `IFudieUser`         |
| Fudie.Security  | Posible `JwtPropagationHandler` genérico                  |
| Fudie.PubSub    | Poblar `MessageContext` desde HTTP, campo Token en Envelope |

## Notas

- `InternalAuthHandler` (Auth→Customers) sigue usando `X-Internal-Key` porque es una llamada sin contexto de usuario (Seed).
- El `JwtPropagationHandler` es para llamadas **con** contexto de usuario autenticado.
- Ambos handlers pueden coexistir en el mismo Refit client si se necesita fallback.
