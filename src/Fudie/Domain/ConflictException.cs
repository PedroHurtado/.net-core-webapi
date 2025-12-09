// Fudie.Domain/ConflictException.cs
namespace Fudie.Domain;

public class ConflictException(string message) : Exception(message);