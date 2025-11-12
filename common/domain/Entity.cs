namespace webapi.common.domain;

public abstract class Entity(Guid id)
{
    public Guid Id { get; protected set; } = id;
    public override bool Equals(object? obj)
    {
        if (obj is Entity entiy)
        {
            return entiy.Id == Id;
        }
        return false;
    }
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}