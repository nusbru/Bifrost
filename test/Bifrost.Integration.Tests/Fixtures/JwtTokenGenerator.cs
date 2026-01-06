using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Bifrost.Integration.Tests.Fixtures;

/// <summary>
/// Helper class to generate JWT tokens for integration testing.
/// </summary>
public static class JwtTokenGenerator
{
    /// <summary>
    /// Generates a valid JWT token for integration testing.
    /// </summary>
    /// <param name="userId">The user ID to include in the token claims.</param>
    /// <param name="expiresInMinutes">Token expiration time in minutes. Defaults to 60.</param>
    /// <returns>A valid JWT token string.</returns>
    public static string GenerateToken(Guid userId, int expiresInMinutes = 60)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(TestApiFactory.TestJwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: TestApiFactory.TestJwtIssuer,
            audience: TestApiFactory.TestJwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Generates an expired JWT token for testing authorization failures.
    /// </summary>
    /// <param name="userId">The user ID to include in the token claims.</param>
    /// <returns>An expired JWT token string.</returns>
    public static string GenerateExpiredToken(Guid userId)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(TestApiFactory.TestJwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: TestApiFactory.TestJwtIssuer,
            audience: TestApiFactory.TestJwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(-10), // Already expired
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
