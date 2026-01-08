using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Bifrost.Core.Services;
using Microsoft.Extensions.Configuration;

namespace Bifrost.Infrastructure.Authentication;

/// <summary>
/// Service for authentication operations using Supabase API.
/// </summary>
public sealed class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly string _supabaseUrl;
    private readonly string _supabaseKey;

    public AuthService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _supabaseUrl = configuration["Supabase:Url"] ?? throw new InvalidOperationException("Supabase URL is not configured");
        _supabaseKey = configuration["Supabase:Key"] ?? throw new InvalidOperationException("Supabase Key is not configured");

        // Configure HttpClient with Supabase API key
        _httpClient.DefaultRequestHeaders.Add("apikey", _supabaseKey);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task RegisterAsync(string email, string password, string? fullName = null)
    {
        ValidateEmail(email);
        ValidatePassword(password);

        var requestBody = new
        {
            email,
            password,
            data = fullName != null ? new { full_name = fullName } : null
        };

        var content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync(
            $"{_supabaseUrl}/auth/v1/signup",
            content);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Registration failed: {errorContent}");
        }
    }

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        ValidateEmail(email);
        ValidatePassword(password);

        var requestBody = new
        {
            email,
            password
        };

        var content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync(
            $"{_supabaseUrl}/auth/v1/token?grant_type=password",
            content);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Login failed: {errorContent}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var supabaseResponse = JsonSerializer.Deserialize<SupabaseAuthResponse>(responseContent)
            ?? throw new InvalidOperationException("Failed to deserialize Supabase response");

        return MapToAuthResult(supabaseResponse);
    }

    public async Task LogoutAsync(string accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new ArgumentException("Access token cannot be null or empty", nameof(accessToken));
        }

        var request = new HttpRequestMessage(HttpMethod.Post, $"{_supabaseUrl}/auth/v1/logout");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Logout failed: {errorContent}");
        }
    }

    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be null or empty", nameof(email));
        }

        if (!email.Contains('@'))
        {
            throw new ArgumentException("Email must be a valid email address", nameof(email));
        }
    }

    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password cannot be null or empty", nameof(password));
        }

        if (password.Length < 6)
        {
            throw new ArgumentException("Password must be at least 6 characters long", nameof(password));
        }
    }

    private static AuthResult MapToAuthResult(SupabaseAuthResponse response)
    {
        if (response.User == null)
        {
            throw new InvalidOperationException("Invalid authentication response from Supabase");
        }

        return new AuthResult
        {
            AccessToken = response.AccessToken,
            RefreshToken = response.RefreshToken,
            ExpiresIn = response.ExpiresIn,
            TokenType = response.TokenType,
            UserId = Guid.Parse(response.User.Id),
            Email = response.User.Email,
            CreatedAt = DateTime.Parse(response.User.CreatedAt)
        };
    }
}

#region Supabase Response Models

internal sealed class SupabaseAuthResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }

    [JsonPropertyName("user")]
    public SupabaseUser? User { get; set; }
}


internal sealed class SupabaseUser
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("email")]
    public required string Email { get; set; }

    [JsonPropertyName("created_at")]
    public required string CreatedAt { get; set; }
}

#endregion
