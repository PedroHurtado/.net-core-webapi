# Fudie.PubSub.Gcp

Proveedor de Google Cloud Pub/Sub para `Fudie.PubSub`.

## Configuracion

### Con fluent API (recomendado)

```csharp
builder.Services.AddPubSubMessaging(pubsub =>
{
    pubsub.UseGcp(builder.Configuration);
});
```

### Standalone (solo transporte, sin messaging)

```csharp
builder.Services.AddPubSubGcp(builder.Configuration);
```

### appsettings.json

```json
{
  "PubSub": {
    "ProjectId": "mi-proyecto-gcp"
  }
}
```

`ProjectId` es obligatorio. Si no esta configurado, lanza `InvalidOperationException`.

## Que registra

| Metodo | Servicio | Lifetime |
|--------|----------|----------|
| `UseGcp` | `IPubSubClient` (via `GcpPubSubClient`) | Singleton |
| `AddPubSubGcp` | `IPubSubClient` (via `GcpPubSubClient`) | Singleton |

`UseGcp` se usa dentro de `AddPubSubMessaging` para que el core registre tambien `MessageHost`, `IMessagePublisher` e `IMessageContext`.

`AddPubSubGcp` registra solo el cliente de transporte, sin la capa de messaging.

## GcpPubSubClient

Clase `internal sealed` que hereda de `PubSubClient`. Implementa todas las operaciones contra la API de Google Cloud Pub/Sub:

- **Topics**: crear, eliminar, verificar existencia, publicar
- **Subscriptions**: crear, eliminar, verificar existencia, suscribirse (pull streaming)

### Serializer

Si se registra un `ISerializer` en DI, lo usa. Si no, usa `JsonPubSubSerializer` por defecto.

## Dependencias

- `Fudie.PubSub` (core)
- `Google.Cloud.PubSub.V1`
- `Microsoft.Extensions.Configuration.Abstractions`
- `Microsoft.Extensions.DependencyInjection.Abstractions`

## Emulador local

Para desarrollo y tests, usar el emulador de Pub/Sub:

```bash
gcloud beta emulators pubsub start --project=demo-project
```

Y configurar la variable de entorno:

```bash
export PUBSUB_EMULATOR_HOST=localhost:8085
```
