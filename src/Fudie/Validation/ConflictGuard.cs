// Fudie.Validation/ConflictGuard.cs
namespace Fudie.Validation;

using Fudie.Domain;

public static class ConflictGuard
{
    public static void ThrowIf(bool condition, string message)
    {
        if (condition)
            throw new ConflictException(message);
    }
}

