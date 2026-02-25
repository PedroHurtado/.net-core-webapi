namespace Fudie.Security;

public interface IJwtValidator
{
    Task<FudieTokenContext?> ValidateTokenAsync(string token);
}
