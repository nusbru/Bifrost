namespace Bifrost.Contracts.Authentication;

/// <summary>
/// Request model for user login.
/// </summary>
public sealed record LoginRequest
{
    /// <summary>
    /// User's email address.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// User's password.
    /// </summary>
    public required string Password { get; init; }
}
