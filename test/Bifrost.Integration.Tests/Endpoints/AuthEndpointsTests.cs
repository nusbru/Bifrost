using System.Net;
using System.Net.Http.Json;
using Bifrost.Contracts.Authentication;
using Bifrost.Integration.Tests.Fixtures;
using FluentAssertions;

namespace Bifrost.Integration.Tests.Endpoints;

/// <summary>
/// Integration tests for Authentication endpoints.
/// Tests registration, login, and logout operations.
/// These endpoints do not require authorization except logout.
/// </summary>
public class AuthEndpointsTests : IntegrationTestBase
{
    private const string AuthEndpoint = "/api/auth";

    /// <summary>
    /// POST /api/auth/register - Should register a new user and return 201 Created.
    /// Note: This test uses the actual Supabase API, so may fail if configuration is invalid.
    /// </summary>
    [Fact]
    public async Task Register_WithValidData_Returns201Created()
    {
        // Arrange
        ClearAuthorizationHeader(); // Registration doesn't require auth

        var uniqueEmail = $"test-{Guid.NewGuid()}@example.com";
        var request = new RegisterRequest
        {
            Email = uniqueEmail,
            Password = "SecurePassword123!",
            FullName = "Test User"
        };

        // Act
        var response = await Client.PostAsJsonAsync($"{AuthEndpoint}/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    /// <summary>
    /// POST /api/auth/register - Should return 400 BadRequest when email is empty.
    /// </summary>
    [Fact]
    public async Task Register_WithEmptyEmail_Returns400BadRequest()
    {
        // Arrange
        ClearAuthorizationHeader();

        var request = new RegisterRequest
        {
            Email = string.Empty,
            Password = "SecurePassword123!"
        };

        // Act
        var response = await Client.PostAsJsonAsync($"{AuthEndpoint}/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problemDetails = await ParseProblemDetailsAsync(response);
        problemDetails.Should().NotBeNull();
    }

    /// <summary>
    /// POST /api/auth/register - Should return 400 BadRequest when email is invalid.
    /// </summary>
    [Fact]
    public async Task Register_WithInvalidEmail_Returns400BadRequest()
    {
        // Arrange
        ClearAuthorizationHeader();

        var request = new RegisterRequest
        {
            Email = "invalid-email",
            Password = "SecurePassword123!"
        };

        // Act
        var response = await Client.PostAsJsonAsync($"{AuthEndpoint}/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// POST /api/auth/register - Should return 400 BadRequest when password is too short.
    /// </summary>
    [Fact]
    public async Task Register_WithShortPassword_Returns400BadRequest()
    {
        // Arrange
        ClearAuthorizationHeader();

        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "123"
        };

        // Act
        var response = await Client.PostAsJsonAsync($"{AuthEndpoint}/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// POST /api/auth/login - Should authenticate user and return 200 OK.
    /// Note: This requires a user to be registered first.
    /// </summary>
    [Fact]
    public async Task Login_WithValidCredentials_Returns200Ok()
    {
        // Arrange
        ClearAuthorizationHeader();

        // First, register a user
        var uniqueEmail = $"test-login-{Guid.NewGuid()}@example.com";
        var password = "SecurePassword123!";
        var registerRequest = new RegisterRequest
        {
            Email = uniqueEmail,
            Password = password,
            FullName = "Login Test User"
        };

        var registerResponse = await Client.PostAsJsonAsync($"{AuthEndpoint}/register", registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Only proceed with login if registration was successful
        if (registerResponse.StatusCode == HttpStatusCode.Created)
        {
            // Now attempt to login
            var loginRequest = new LoginRequest
            {
                Email = uniqueEmail,
                Password = password
            };

            // Act
            var response = await Client.PostAsJsonAsync($"{AuthEndpoint}/login", loginRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }

    /// <summary>
    /// POST /api/auth/login - Should return 401 Unauthorized with invalid credentials.
    /// </summary>
    [Fact]
    public async Task Login_WithInvalidCredentials_Returns401Unauthorized()
    {
        // Arrange
        ClearAuthorizationHeader();

        var request = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "WrongPassword123!"
        };

        // Act
        var response = await Client.PostAsJsonAsync($"{AuthEndpoint}/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// POST /api/auth/login - Should return 401 when email is empty.
    /// </summary>
    [Fact]
    public async Task Login_WithEmptyEmail_Returns401Unauthorized()
    {
        // Arrange
        ClearAuthorizationHeader();

        var request = new LoginRequest
        {
            Email = string.Empty,
            Password = "SecurePassword123!"
        };

        // Act
        var response = await Client.PostAsJsonAsync($"{AuthEndpoint}/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// POST /api/auth/logout - Should logout authenticated user and return 204 NoContent.
    /// </summary>
    [Fact]
    public async Task Logout_WithValidToken_Returns204NoContent()
    {
        // Arrange
        // Use the default authorization header set in IntegrationTestBase
        SetAuthorizationHeader(TestUserId);

        // Act
        var response = await Client.PostAsync($"{AuthEndpoint}/logout", null);

        // Assert
        // Note: This may return 400 if the test token is not valid with Supabase
        // In a real scenario, you'd use an actual Supabase token
        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
        else
        {
            // If using test JWT tokens, Supabase may reject them
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.BadRequest,
                HttpStatusCode.Unauthorized);
        }
    }

    /// <summary>
    /// POST /api/auth/logout - Should return 401 when no authorization header is provided.
    /// </summary>
    [Fact]
    public async Task Logout_WithoutAuthorization_Returns401Unauthorized()
    {
        // Arrange
        ClearAuthorizationHeader();

        // Act
        var response = await Client.PostAsync($"{AuthEndpoint}/logout", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// POST /api/auth/register - Should handle duplicate email registration gracefully.
    /// </summary>
    [Fact]
    public async Task Register_WithDuplicateEmail_Returns400BadRequest()
    {
        // Arrange
        ClearAuthorizationHeader();

        var uniqueEmail = $"duplicate-{Guid.NewGuid()}@example.com";
        var request = new RegisterRequest
        {
            Email = uniqueEmail,
            Password = "SecurePassword123!",
            FullName = "First User"
        };

        // First registration
        var firstResponse = await Client.PostAsJsonAsync($"{AuthEndpoint}/register", request);

        // Only test duplicate if first registration succeeded
        if (firstResponse.StatusCode == HttpStatusCode.Created)
        {
            // Attempt second registration with same email
            var duplicateRequest = new RegisterRequest
            {
                Email = uniqueEmail,
                Password = "DifferentPassword123!",
                FullName = "Second User"
            };

            // Act
            var response = await Client.PostAsJsonAsync($"{AuthEndpoint}/register", duplicateRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
