using Bifrost.Contracts.Authentication;
using Bifrost.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Bifrost.Api.Endpoints;

/// <summary>
/// Endpoints for authentication operations.
/// </summary>
public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication")
            .WithOpenApi();

        group.MapPost("/register", Register)
            .WithName("Register")
            .WithSummary("Register a new user account")
            .Produces<AuthResponse>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .AllowAnonymous();

        group.MapPost("/login", Login)
            .WithName("Login")
            .WithSummary("Login with email and password")
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .AllowAnonymous();

        group.MapPost("/logout", Logout)
            .WithName("Logout")
            .WithSummary("Logout the current user")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();
    }

    private static async Task<IResult> Register(
        [FromBody] RegisterRequest request,
        [FromServices] IAuthService authService)
    {
        try
        {
            await authService.RegisterAsync(
                request.Email,
                request.Password,
                request.FullName);

            return Results.Created();
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Invalid registration request",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Registration failed",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    private static async Task<IResult> Login(
        [FromBody] LoginRequest request,
        [FromServices] IAuthService authService)
    {
        try
        {
            var result = await authService.LoginAsync(
                request.Email,
                request.Password);

            var response = new AuthResponse
            {
                AccessToken = result.AccessToken,
                RefreshToken = result.RefreshToken,
                ExpiresIn = result.ExpiresIn,
                TokenType = result.TokenType,
                User = new UserInfo
                {
                    Id = result.UserId,
                    Email = result.Email,
                    CreatedAt = result.CreatedAt
                }
            };

            return Results.Ok(response);
        }
        catch (ArgumentException)
        {
            return Results.Unauthorized();
        }
        catch (InvalidOperationException)
        {
            return Results.Unauthorized();
        }
    }

    private static async Task<IResult> Logout(
        HttpContext context,
        [FromServices] IAuthService authService)
    {
        try
        {
            // Extract Bearer token from Authorization header
            var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Results.Unauthorized();
            }

            var accessToken = authHeader["Bearer ".Length..].Trim();
            await authService.LogoutAsync(accessToken);

            return Results.NoContent();
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Invalid logout request",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Logout failed",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }
}
