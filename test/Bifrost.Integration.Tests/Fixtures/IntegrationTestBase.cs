using System.Net.Http.Json;
using System.Text.Json;
using Bifrost.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace Bifrost.Integration.Tests.Fixtures;

/// <summary>
/// Base class for integration tests providing common setup, teardown, and helper methods.
/// Implements IAsyncLifetime for proper Xunit async test lifecycle management.
/// </summary>
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly TestApiFactory Factory;
    protected HttpClient Client = null!;
    protected BifrostDbContext DbContext = null!;

    /// <summary>
    /// Test user ID used across test data.
    /// </summary>
    protected static readonly Guid TestUserId = Guid.NewGuid();

    /// <summary>
    /// Alternative test user ID for multi-user scenarios.
    /// </summary>
    protected static readonly Guid AnotherTestUserId = Guid.NewGuid();

    protected IntegrationTestBase()
    {
        Factory = new TestApiFactory();
    }

    /// <summary>
    /// Initializes the test by ensuring a clean database state.
    /// Called automatically by Xunit before each test.
    /// </summary>
    public async Task InitializeAsync()
    {
        // Ensure the factory (and PostgreSQL container) is initialized
        await Factory.InitializeAsync();

        // Now we can safely create the client and DbContext
        Client = Factory.CreateClient();
        DbContext = Factory.GetDbContext();

        // Clear database for test isolation
        await Factory.ClearDatabaseAsync();
    }

    /// <summary>
    /// Cleans up resources after each test.
    /// Called automatically by Xunit after each test.
    /// </summary>
    public async Task DisposeAsync()
    {
        if (DbContext != null)
            await DbContext.DisposeAsync();

        if (Client != null)
            Client.Dispose();

        await Factory.DisposeAsync();
    }

    /// <summary>
    /// Helper method to parse JSON responses into typed objects.
    /// </summary>
    protected async Task<T?> DeserializeResponseAsync<T>(HttpResponseMessage response)
    {
        var jsonContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(jsonContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    /// <summary>
    /// Helper method to extract a single item from a list response.
    /// </summary>
    protected async Task<T?> GetFirstFromListResponseAsync<T>(HttpResponseMessage response) where T : class
    {
        var jsonContent = await response.Content.ReadAsStringAsync();
        var list = JsonSerializer.Deserialize<List<T>>(jsonContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        return list?.FirstOrDefault();
    }

    /// <summary>
    /// Helper method to parse problem details from error responses.
    /// </summary>
    protected async Task<ProblemDetails?> ParseProblemDetailsAsync(HttpResponseMessage response)
    {
        var jsonContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ProblemDetails>(jsonContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }
}
