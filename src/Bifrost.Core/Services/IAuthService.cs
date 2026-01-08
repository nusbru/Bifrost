namespace Bifrost.Core.Services;

/// <summary>
/// Service interface for authentication operations.
/// Handles user registration, login, and logout via Supabase.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user with email and password.
    /// </summary>
    /// <param name="email">User's email address.</param>
    /// <param name="password">User's password.</param>
    /// <param name="fullName">Optional full name.</param>
    /// <returns>Authentication response with tokens and user info.</returns>
    Task RegisterAsync(string email, string password, string? fullName = null);

    /// <summary>
    /// Authenticates a user with email and password.
    /// </summary>
    /// <param name="email">User's email address.</param>
    /// <param name="password">User's password.</param>
    /// <returns>Authentication response with tokens and user info.</returns>
    Task<AuthResult> LoginAsync(string email, string password);

    /// <summary>
    /// Signs out the current user.
    /// </summary>
    /// <param name="accessToken">User's access token to invalidate.</param>
    Task LogoutAsync(string accessToken);
}

/// <summary>
/// Result of authentication operations.
/// </summary>
public sealed record AuthResult
{
    /// <summary>
    /// JWT access token.
    /// </summary>
    public required string AccessToken { get; init; }

    /// <summary>
    /// Refresh token.
    /// </summary>
    public required string RefreshToken { get; init; }

    /// <summary>
    /// Token expiration in seconds.
    /// </summary>
    public required int ExpiresIn { get; init; }

    /// <summary>
    /// Token type.
    /// </summary>
    public required string TokenType { get; init; }

    /// <summary>
    /// User's unique identifier.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// User's email.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// User creation timestamp.
    /// </summary>
    public required DateTime CreatedAt { get; init; }
}
