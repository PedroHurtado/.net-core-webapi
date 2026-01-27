namespace Plans.UnitTests.Helpers;

public record TestableFeature : Feature
{
    public TestableFeature(
        string code,
        string name,
        string? description,
        FeatureType type,
        int? limit = null,
        string? unit = null)
        : base(code, name, description, type, limit, unit) { }
}
