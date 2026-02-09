namespace Auth.Infrastructure.Google;

public record GoogleIdTokenClaims(
    string Sub,
    string Email,
    string Name,
    string? Picture
);

public interface IGoogleIdTokenValidator
{
    Task<GoogleIdTokenClaims> ValidateAsync(string idToken);
}
