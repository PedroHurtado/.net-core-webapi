// Fudie.Validation/UnauthorizedGuard.cs
namespace Fudie.Validation;

using Fudie.Domain;

public static class UnauthorizedGuard
{
    public static void ThrowIf(bool condition, string message)
    {
        if (condition)
            throw new UnauthorizedException(message);
    }
}
