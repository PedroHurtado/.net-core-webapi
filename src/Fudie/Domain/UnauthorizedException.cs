// Fudie.Domain/UnauthorizedException.cs
namespace Fudie.Domain;

public class UnauthorizedException(string message) : Exception(message);
