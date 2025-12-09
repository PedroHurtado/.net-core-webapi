// Fudie.Domain/IDomainEvent.cs
namespace Fudie.Domain;

public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}