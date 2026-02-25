namespace Fudie.Features;

public interface IAggregateDescription
{
    string Id { get; }
    string DisplayName { get; }
    string? Icon { get; }
    string ReadDescription { get; }
    string WriteDescription { get; }
}
