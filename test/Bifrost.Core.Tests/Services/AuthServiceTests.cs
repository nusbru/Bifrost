using System.Net;
using System.Text;
using System.Text.Json;
using Bifrost.Infrastructure.Authentication;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace Bifrost.Core.Tests.Services;

public class AuthServiceTests
{
    private readonly IConfiguration _configuration;
    private readonly string _supabaseUrl = "https://test.supabase.co";
    private readonly string _supabaseKey = "test-key";

    public AuthServiceTests()
    {
        var inMemorySettings = new Dictionary<string, string>
        {
            { "Supabase:Url", _supabaseUrl },
            { "Supabase:Key", _supabaseKey }
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_ReturnsAuthResult()
    {
        // Arrange
        var mockHttpMessageHandler = new MockHttpMessageHandler(
            HttpStatusCode.OK,
            CreateSuccessfulAuthResponse());
        var httpClient = new HttpClient(mockHttpMessageHandler);
        var authService = new AuthService(httpClient, _configuration);

        // Act
        var act = async () => await authService.RegisterAsync("test@example.com", "password123", "Test User");

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RegisterAsync_WithEmptyEmail_ThrowsArgumentException()
    {
        // Arrange
        var httpClient = new HttpClient(new MockHttpMessageHandler(HttpStatusCode.OK, "{}"));
        var authService = new AuthService(httpClient, _configuration);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            authService.RegisterAsync("", "password123"));
    }

    [Fact]
    public async Task RegisterAsync_WithInvalidEmail_ThrowsArgumentException()
    {
        // Arrange
        var httpClient = new HttpClient(new MockHttpMessageHandler(HttpStatusCode.OK, "{}"));
        var authService = new AuthService(httpClient, _configuration);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            authService.RegisterAsync("invalid-email", "password123"));
    }

    [Fact]
    public async Task RegisterAsync_WithShortPassword_ThrowsArgumentException()
    {
        // Arrange
        var httpClient = new HttpClient(new MockHttpMessageHandler(HttpStatusCode.OK, "{}"));
        var authService = new AuthService(httpClient, _configuration);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            authService.RegisterAsync("test@example.com", "123"));
    }

    [Fact]
    public async Task RegisterAsync_WithSupabaseError_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockHttpMessageHandler = new MockHttpMessageHandler(
            HttpStatusCode.BadRequest,
            "{\"error\": \"User already registered\"}");
        var httpClient = new HttpClient(mockHttpMessageHandler);
        var authService = new AuthService(httpClient, _configuration);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            authService.RegisterAsync("test@example.com", "password123"));
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsAuthResult()
    {
        // Arrange
        var mockHttpMessageHandler = new MockHttpMessageHandler(
            HttpStatusCode.OK,
            CreateSuccessfulAuthResponse());
        var httpClient = new HttpClient(mockHttpMessageHandler);
        var authService = new AuthService(httpClient, _configuration);

        // Act
        var result = await authService.LoginAsync("test@example.com", "password123");

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be("test@example.com");
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task LoginAsync_WithEmptyEmail_ThrowsArgumentException()
    {
        // Arrange
        var httpClient = new HttpClient(new MockHttpMessageHandler(HttpStatusCode.OK, "{}"));
        var authService = new AuthService(httpClient, _configuration);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            authService.LoginAsync("", "password123"));
    }

    [Fact]
    public async Task LoginAsync_WithInvalidCredentials_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockHttpMessageHandler = new MockHttpMessageHandler(
            HttpStatusCode.Unauthorized,
            "{\"error\": \"Invalid credentials\"}");
        var httpClient = new HttpClient(mockHttpMessageHandler);
        var authService = new AuthService(httpClient, _configuration);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            authService.LoginAsync("test@example.com", "wrongpassword"));
    }

    [Fact]
    public async Task LogoutAsync_WithValidToken_CompletesSuccessfully()
    {
        // Arrange
        var mockHttpMessageHandler = new MockHttpMessageHandler(HttpStatusCode.OK, "{}");
        var httpClient = new HttpClient(mockHttpMessageHandler);
        var authService = new AuthService(httpClient, _configuration);

        // Act
        await authService.LogoutAsync("valid-token");

        // Assert - No exception thrown
    }

    [Fact]
    public async Task LogoutAsync_WithEmptyToken_ThrowsArgumentException()
    {
        // Arrange
        var httpClient = new HttpClient(new MockHttpMessageHandler(HttpStatusCode.OK, "{}"));
        var authService = new AuthService(httpClient, _configuration);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            authService.LogoutAsync(""));
    }

    [Fact]
    public async Task LogoutAsync_WithSupabaseError_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockHttpMessageHandler = new MockHttpMessageHandler(
            HttpStatusCode.BadRequest,
            "{\"error\": \"Invalid token\"}");
        var httpClient = new HttpClient(mockHttpMessageHandler);
        var authService = new AuthService(httpClient, _configuration);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            authService.LogoutAsync("invalid-token"));
    }

    private static string CreateSuccessfulAuthResponse()
    {
        var response = new
        {
            access_token = "test-access-token",
            refresh_token = "test-refresh-token",
            expires_in = 3600,
            token_type = "bearer",
            user = new
            {
                id = Guid.NewGuid().ToString(),
                email = "test@example.com",
                created_at = DateTime.UtcNow.ToString("o")
            }
        };

        return JsonSerializer.Serialize(response);
    }
}

/// <summary>
/// Mock HttpMessageHandler for testing HttpClient calls.
/// </summary>
public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly string _responseContent;

    public MockHttpMessageHandler(HttpStatusCode statusCode, string responseContent)
    {
        _statusCode = statusCode;
        _responseContent = responseContent;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var response = new HttpResponseMessage(_statusCode)
        {
            Content = new StringContent(_responseContent, Encoding.UTF8, "application/json")
        };

        return Task.FromResult(response);
    }
}
