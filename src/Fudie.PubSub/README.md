# Fudie.PubSub

Core de mensajeria para la plataforma Fudie. Define contratos, abstracciones y la capa de hosting. **No contiene implementacion de ningun proveedor**.

## Estructura

```
Fudie.PubSub/
  Transport/        Contratos de transporte (publish, subscribe, admin)
  Serialization/    Contrato y default de serializacion
  Messaging/        Envelope, contexto de mensaje y publisher de alto nivel
  Hosting/          Handler, MessageHost y configuracion de DI
```

## Transport

Interfaces segregadas para operaciones de transporte:

| Interfaz | Responsabilidad |
|----------|-----------------|
| `IPublisher` | Publicar mensajes en un topic |
| `ISubscriber` | Suscribirse a mensajes de una subscription |
| `ITopicAdmin` | Crear, eliminar y verificar topics |
| `ISubscriptionAdmin` | Crear, eliminar y verificar subscriptions |
| `IPubSubClient` | Interfaz compuesta que agrupa todas las anteriores |

`PubSubClient` es la clase base abstracta que implementa `IPubSubClient` con validacion de argumentos. Los proveedores heredan de ella.

## Serialization

```csharp
public interface ISerializer
{
    byte[] Serialize<T>(T value);
    T Deserialize<T>(byte[] data);
}
```

`JsonPubSubSerializer` es la implementacion por defecto usando `System.Text.Json`. Si no se registra un `ISerializer` en DI, el proveedor usa esta implementacion automaticamente.

## Messaging

### Envelope

Cada mensaje viaja envuelto en un `Envelope<T>`:

```csharp
public record Envelope<T>(
    string MessageId,
    string? CorrelationId,
    string Type,
    DateTime OccurredAt,
    IDictionary<string, string>? Claims,
    T Payload
);
```

El desarrollador nunca interactua con el envelope directamente. `MessagePublisher` lo construye al publicar y `MessageHost` lo desenvuelve al recibir.

### IMessageContext

Contexto scoped que expone los datos del mensaje actual:

```csharp
public interface IMessageContext
{
    string? TenantId { get; }
    string? UserId { get; }
    string? CorrelationId { get; }
    IDictionary<string, string> Claims { get; }
}
```

Se inyecta en handlers, DbContexts (para QueryFilters multi-tenant) y cualquier servicio scoped. Se puebla automaticamente desde:

- **HTTP middleware** (claims del JWT)
- **Outbox worker** (claims almacenados)
- **MessageHost** (claims del envelope al recibir un mensaje)

### MessagePublisher

`IMessagePublisher` es la interfaz de alto nivel para publicar mensajes:

```csharp
public interface IMessagePublisher
{
    Task PublishAsync<T>(string topicId, T message);
}
```

Envuelve `T` en `Envelope<T>` usando los claims del `IMessageContext` actual.

## Hosting

### IMessageHandler\<T\>

Contrato para consumir mensajes:

```csharp
public interface IMessageHandler<in T>
{
    Task Handle(T message, CancellationToken ct);
}
```

### MessageHost

Orquesta la recepcion de mensajes. Por cada mensaje:

1. Crea un scope de DI
2. Puebla `MessageContext` con los claims y correlationId del envelope
3. Resuelve `IMessageHandler<T>` dentro del scope
4. Ejecuta el handler

Esto garantiza que cada mensaje tiene su propio scope, con `IMessageContext` y `DbContext` independientes.

### Configuracion

```csharp
builder.Services.AddPubSubMessaging(pubsub =>
{
    pubsub.UseGcp(builder.Configuration);  // extension del proveedor
});
```

`AddPubSubMessaging` registra:

| Servicio | Lifetime |
|----------|----------|
| `MessageContext` / `IMessageContext` | Scoped |
| `IMessagePublisher` | Scoped |
| `MessageHost` | Singleton |

El proveedor (ej. `UseGcp`) registra `IPubSubClient` como Singleton.

## Dependencias

Este proyecto **no tiene dependencias de proveedor**. Solo depende de:

- `Microsoft.Extensions.DependencyInjection.Abstractions`
