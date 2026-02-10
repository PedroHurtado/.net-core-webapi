using FluentAssertions;
using Fudie.Domain;

namespace Fudie.UnitTests.Domain;

public class UnauthorizedExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_ShouldSetMessage()
    {
        // Arrange
        var message = "Session expired";

        // Act
        var exception = new UnauthorizedException(message);

        // Assert
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void UnauthorizedException_ShouldInheritFromException()
    {
        // Arrange & Act
        var exception = new UnauthorizedException("test");

        // Assert
        exception.Should().BeAssignableTo<Exception>();
    }
}
