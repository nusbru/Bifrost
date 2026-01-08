using Bifrost.Api;
using Bifrost.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using WireMock.Server;

namespace Bifrost.Integration.Tests.Fixtures;

/// <summary>
/// WebApplicationFactory for integration testing the Bifrost API.
/// Configures PostgreSQL Testcontainer and WireMock server for realistic testing with proper isolation.
/// WireMock mocks Supabase authentication endpoints.
/// </summary>
public class TestApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private PostgreSqlContainer? _postgreSqlContainer;
    private WireMockServer? _wireMockServer;
    private bool _initialized = false;

    /// <summary>
    /// Test JWT configuration values.
    /// </summary>
    public const string TestJwtKey = "ThisIsATestSecretKeyForIntegrationTestsMinimum32Chars!";
    public const string TestJwtIssuer = "Bifrost";
    public const string TestJwtAudience = "BifrostApp";
    
    /// <summary>
    /// Test Supabase configuration values.
    /// </summary>
    public const string TestSupabaseKey = "test-supabase-key-for-integration-tests";
    
    /// <summary>
    /// Gets the WireMock server URL for Supabase API mocking.
    /// </summary>
    public string WireMockUrl => _wireMockServer?.Urls.FirstOrDefault() ?? "http://localhost:0";

    /// <summary>
    /// Initializes the test containers before any tests run.
    /// This must be called before using the factory.
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_initialized)
            return;

        // Initialize WireMock server for Supabase API mocking
        _wireMockServer = WireMockServer.Start();
        WireMockSupabaseHelper.ConfigureSupabaseAuthEndpoints(_wireMockServer, TestSupabaseKey);

        // Initialize PostgreSQL container with default settings
        _postgreSqlContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16")  // Match the version used in production
            .WithDatabase("bifrost_test")
            .WithUsername("test_user")
            .WithPassword("test_password")
            .WithCleanUp(true)
            .Build();

        await _postgreSqlContainer.StartAsync();
        _initialized = true;
    }

    /// <summary>
    /// Cleans up the test containers after all tests complete.
    /// </summary>
    public new async Task DisposeAsync()
    {
        if (_wireMockServer != null)
        {
            _wireMockServer.Stop();
            _wireMockServer.Dispose();
        }
        
        if (_postgreSqlContainer != null)
        {
            await _postgreSqlContainer.DisposeAsync();
        }
        
        await base.DisposeAsync();
    }

    /// <summary>
    /// Configures the WebHost to use PostgreSQL Testcontainer and WireMock for testing.
    /// </summary>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        if (_wireMockServer == null)
        {
            throw new InvalidOperationException(
                "WireMock server not initialized. Call InitializeAsync() before using the factory.");
        }

        // Add JWT and Supabase configuration for testing
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = TestJwtKey,
                ["Jwt:Issuer"] = TestJwtIssuer,
                ["Jwt:Audience"] = TestJwtAudience,
                ["Supabase:Url"] = WireMockUrl,  // Point to WireMock instead of real Supabase
                ["Supabase:Key"] = TestSupabaseKey
            });
        });

        builder.ConfigureServices(services =>
        {
            if (_postgreSqlContainer == null)
            {
                throw new InvalidOperationException(
                    "PostgreSQL container not initialized. Call InitializeAsync() before using the factory.");
            }

            // Remove the existing DbContext registration from Infrastructure
            var dbContextDescriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<BifrostDbContext>));

            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            // Remove existing EF Core and Npgsql service registrations
            var efProviders = services.Where(d =>
                d.ServiceType.FullName?.StartsWith("Microsoft.EntityFrameworkCore") == true ||
                d.ServiceType.FullName?.StartsWith("Npgsql") == true).ToList();

            foreach (var provider in efProviders)
            {
                services.Remove(provider);
            }

            // Add PostgreSQL DbContext using Testcontainer connection string
            services.AddDbContext<BifrostDbContext>(options =>
                options.UseNpgsql(_postgreSqlContainer.GetConnectionString()));

            // Apply migrations and seed database
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<BifrostDbContext>();

            // Apply EF Core migrations to create database schema
            dbContext.Database.Migrate();
        });

        builder.UseEnvironment("Test");
    }

    /// <summary>
    /// Gets a database context for direct database manipulation in tests.
    /// </summary>
    /// <returns>A new instance of BifrostDbContext scoped to the factory.</returns>
    public BifrostDbContext GetDbContext()
    {
        var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<BifrostDbContext>();
    }

    /// <summary>
    /// Clears all data from the database to ensure test isolation.
    /// Uses TRUNCATE for PostgreSQL for efficient cleanup while respecting foreign key constraints.
    /// </summary>
    public async Task ClearDatabaseAsync()
    {
        var context = GetDbContext();
        try
        {
            // Use raw SQL for efficient cleanup with proper CASCADE handling
            // TRUNCATE is much faster than DELETE for clearing tables
            await context.Database.ExecuteSqlRawAsync(
                @"TRUNCATE TABLE ""ApplicationNotes"", ""JobApplications"", ""Jobs"", ""Preferences"" RESTART IDENTITY CASCADE");
        }
        finally
        {
            await context.DisposeAsync();
        }
    }
}
