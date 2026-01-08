namespace Bifrost.Contracts.Authentication;

/// <summary>
/// Request model for user registration.
/// </summary>
public sealed record RegisterRequest
{
    /// <summary>
    /// User's email address.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// User's password.
    /// </summary>
    public required string Password { get; init; }

    /// <summary>
    /// User's full name (optional metadata).
    /// </summary>
    public string? FullName { get; init; }
}
