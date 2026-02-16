using Microsoft.AspNetCore.Builder;

namespace Fudie.Features;

public static class EndpointAuthExtensions
{
    public static TBuilder RequirePlatform<TBuilder>(
        this TBuilder builder) where TBuilder : IEndpointConventionBuilder
        => builder.WithMetadata(new PlatformRequirement());

    public static TBuilder RequireInternal<TBuilder>(
        this TBuilder builder) where TBuilder : IEndpointConventionBuilder
        => builder.WithMetadata(new InternalRequirement());

    public static TBuilder RequireGroup<TBuilder>(
        this TBuilder builder, string group) where TBuilder : IEndpointConventionBuilder
        => builder.WithMetadata(new GroupRequirement(group));
}
