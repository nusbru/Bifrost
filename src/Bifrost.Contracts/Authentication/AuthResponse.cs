namespace Bifrost.Contracts.Authentication;

/// <summary>
/// Response model for authentication operations.
/// </summary>
public sealed record AuthResponse
{
    /// <summary>
    /// JWT access token for authenticated requests.
    /// </summary>
    public required string AccessToken { get; init; }

    /// <summary>
    /// Refresh token for obtaining new access tokens.
    /// </summary>
    public required string RefreshToken { get; init; }

    /// <summary>
    /// Token expiration time in seconds.
    /// </summary>
    public required int ExpiresIn { get; init; }

    /// <summary>
    /// Token type (typically "bearer").
    /// </summary>
    public required string TokenType { get; init; }

    /// <summary>
    /// User information.
    /// </summary>
    public required UserInfo User { get; init; }
}

/// <summary>
/// User information included in authentication response.
/// </summary>
public sealed record UserInfo
{
    /// <summary>
    /// User's unique identifier.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// User's email address.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// User's full name (if provided).
    /// </summary>
    public string? FullName { get; init; }

    /// <summary>
    /// Timestamp when user was created.
    /// </summary>
    public required DateTime CreatedAt { get; init; }
}
