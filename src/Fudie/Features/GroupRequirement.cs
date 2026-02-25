namespace Fudie.Features;

public class GroupRequirement(string group, string description)
{
    public string Group { get; } = group;
    public string Description { get; } = description;
}