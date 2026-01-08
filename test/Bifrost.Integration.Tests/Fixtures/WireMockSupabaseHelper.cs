using System.Text.Json;
using System.Collections.Concurrent;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Matchers;

namespace Bifrost.Integration.Tests.Fixtures;

/// <summary>
/// Helper class to configure WireMock server with Supabase authentication API mocks.
/// Provides mock responses for register, login, and logout endpoints matching Supabase API structure.
/// </summary>
public static class WireMockSupabaseHelper
{
    private static readonly ConcurrentDictionary<string, string> RegisteredUsers = new();

    /// <summary>
    /// Configures WireMock server with all Supabase authentication endpoints.
    /// </summary>
    /// <param name="server">The WireMock server instance to configure.</param>
    /// <param name="supabaseKey">The API key to validate in requests.</param>
    public static void ConfigureSupabaseAuthEndpoints(WireMockServer server, string supabaseKey)
    {
        // Clear registered users for fresh start
        RegisteredUsers.Clear();

        ConfigureSignupEndpoint(server, supabaseKey);
        ConfigureLoginEndpoint(server, supabaseKey);
        ConfigureLogoutEndpoint(server, supabaseKey);
    }

    /// <summary>
    /// Configures the /auth/v1/signup endpoint for user registration.
    /// POST /auth/v1/signup - Creates a new user account.
    /// Tracks registered emails to prevent duplicates.
    /// </summary>
    private static void ConfigureSignupEndpoint(WireMockServer server, string supabaseKey)
    {
        server
            .Given(Request.Create()
                .WithPath("/auth/v1/signup")
                .WithHeader("apikey", supabaseKey)
                .UsingPost())
            .RespondWith(Response.Create()
                .WithCallback(req =>
                {
                    // Parse the request body to extract email and password
                    var requestBody = JsonSerializer.Deserialize<JsonElement>(req.Body ?? "{}");
                    var email = requestBody.TryGetProperty("email", out var emailProp)
                        ? emailProp.GetString() ?? "test@example.com"
                        : "test@example.com";

                    var password = requestBody.TryGetProperty("password", out var passProp)
                        ? passProp.GetString() ?? "password"
                        : "password";

                    string? fullName = null;
                    if (requestBody.TryGetProperty("data", out var data) &&
                        data.TryGetProperty("full_name", out var nameElement))
                    {
                        fullName = nameElement.GetString();
                    }

                    // Check for duplicate email
                    if (RegisteredUsers.ContainsKey(email))
                    {
                        // Return 400 for duplicate registration
                        var errorResponse = new
                        {
                            error = "User already registered",
                            error_description = "A user with this email already exists"
                        };

                        return new WireMock.ResponseMessage
                        {
                            StatusCode = 400,
                            Headers = new Dictionary<string, WireMock.Types.WireMockList<string>>
                            {
                                ["Content-Type"] = new WireMock.Types.WireMockList<string>("application/json")
                            },
                            BodyData = new WireMock.Util.BodyData
                            {
                                BodyAsString = JsonSerializer.Serialize(errorResponse)
                            }
                        };
                    }

                    // Store the registered user
                    RegisteredUsers.TryAdd(email, password);

                    var userId = Guid.NewGuid();
                    var now = DateTime.UtcNow;
                    var accessToken = GenerateMockJwt(userId, email);
                    var refreshToken = GenerateMockRefreshToken();

                    var response = new
                    {
                        access_token = accessToken,
                        token_type = "bearer",
                        expires_in = 3600,
                        refresh_token = refreshToken,
                        user = new
                        {
                            id = userId.ToString(),
                            email = email,
                            created_at = now.ToString("O")
                        }
                    };

                    return new WireMock.ResponseMessage
                    {
                        StatusCode = 200,
                        Headers = new Dictionary<string, WireMock.Types.WireMockList<string>>
                        {
                            ["Content-Type"] = new WireMock.Types.WireMockList<string>("application/json")
                        },
                        BodyData = new WireMock.Util.BodyData
                        {
                            BodyAsString = JsonSerializer.Serialize(response)
                        }
                    };
                }));
    }

    /// <summary>
    /// Configures the /auth/v1/token endpoint for user login.
    /// POST /auth/v1/token?grant_type=password - Authenticates a user.
    /// Validates credentials against registered users.
    /// </summary>
    private static void ConfigureLoginEndpoint(WireMockServer server, string supabaseKey)
    {
        server
            .Given(Request.Create()
                .WithPath("/auth/v1/token")
                .WithParam("grant_type", "password")
                .WithHeader("apikey", supabaseKey)
                .UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(req =>
                {
                    var requestBody = JsonSerializer.Deserialize<JsonElement>(req.Body ?? "{}");
                    var email = requestBody.TryGetProperty("email", out var emailProp)
                        ? emailProp.GetString() ?? "test@example.com"
                        : "test@example.com";

                    var password = requestBody.TryGetProperty("password", out var passProp)
                        ? passProp.GetString() ?? "password"
                        : "password";

                    // Validate credentials against registered users
                    if (!RegisteredUsers.TryGetValue(email, out var storedPassword) || storedPassword != password)
                    {
                        return JsonSerializer.Serialize(new
                        {
                            error = "Invalid login credentials",
                            error_description = "Invalid email or password"
                        });
                    }

                    var userId = Guid.NewGuid();
                    var now = DateTime.UtcNow;
                    var accessToken = GenerateMockJwt(userId, email);
                    var refreshToken = GenerateMockRefreshToken();

                    var response = new
                    {
                        access_token = accessToken,
                        token_type = "bearer",
                        expires_in = 3600,
                        refresh_token = refreshToken,
                        user = new
                        {
                            id = userId.ToString(),
                            email = email,
                            created_at = now.ToString("O")
                        }
                    };

                    return JsonSerializer.Serialize(response);
                }));
    }

    /// <summary>
    /// Configures the /auth/v1/logout endpoint for user logout.
    /// POST /auth/v1/logout - Signs out the current user.
    /// </summary>
    private static void ConfigureLogoutEndpoint(WireMockServer server, string supabaseKey)
    {
        server
            .Given(Request.Create()
                .WithPath("/auth/v1/logout")
                .WithHeader("apikey", supabaseKey)
                .WithHeader("Authorization", new RegexMatcher("Bearer .+"))
                .UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(204)); // No Content - successful logout
    }

    /// <summary>
    /// Generates a mock JWT token for testing.
    /// Note: This is a simplified mock token and not a valid JWT.
    /// </summary>
    private static string GenerateMockJwt(Guid userId, string email)
    {
        var header = Convert.ToBase64String(
            System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { alg = "HS256", typ = "JWT" })));
        var payload = Convert.ToBase64String(
            System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new
            {
                sub = userId.ToString(),
                email = email,
                exp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds(),
                iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            })));
        var signature = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("mock-signature"));

        return $"{header}.{payload}.{signature}";
    }

    /// <summary>
    /// Generates a mock refresh token for testing.
    /// </summary>
    private static string GenerateMockRefreshToken()
    {
        return $"refresh_{Guid.NewGuid():N}";
    }
}
