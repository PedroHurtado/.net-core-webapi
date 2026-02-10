using FluentAssertions;
using Fudie.Domain;
using Fudie.Validation;

namespace Fudie.UnitTests.Validation;

public class UnauthorizedGuardTests
{
    [Fact]
    public void ThrowIf_WhenConditionIsTrue_ShouldThrowUnauthorizedException()
    {
        // Arrange & Act
        var act = () => UnauthorizedGuard.ThrowIf(true, "Session expired");

        // Assert
        act.Should().Throw<UnauthorizedException>().WithMessage("Session expired");
    }

    [Fact]
    public void ThrowIf_WhenConditionIsFalse_ShouldNotThrow()
    {
        // Arrange & Act
        var act = () => UnauthorizedGuard.ThrowIf(false, "Session expired");

        // Assert
        act.Should().NotThrow();
    }
}
