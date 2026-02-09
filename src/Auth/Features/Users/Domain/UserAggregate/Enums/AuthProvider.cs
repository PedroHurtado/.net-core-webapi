namespace Auth.Features.Users.Domain.UserAggregate.Enums;

/// <summary>
/// Defines the authentication provider used to create a user account.
/// </summary>
/// <remarks>
/// Determines how the user authenticates: via external OAuth provider or local credentials.
/// </remarks>
public enum AuthProvider
{
    /// <summary>
    /// Authentication via Google OAuth.
    /// </summary>
    /// <remarks>
    /// Primary authentication flow for all users.
    /// </remarks>
    Google = 1,

    /// <summary>
    /// Local authentication with email and password.
    /// </summary>
    /// <remarks>
    /// Exclusive for Fudie super administrators created by script.
    /// </remarks>
    Local = 2
}