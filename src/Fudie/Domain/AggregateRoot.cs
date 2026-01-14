// Fudie.Domain/AggregateRoot.cs
namespace Fudie.Domain;

public abstract class AggregateRoot<TId>(TId id) : Entity<TId>(id) where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
}
