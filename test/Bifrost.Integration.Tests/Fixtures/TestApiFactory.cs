using Bifrost.Api;
using Bifrost.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace Bifrost.Integration.Tests.Fixtures;

/// <summary>
/// WebApplicationFactory for integration testing the Bifrost API.
/// Configures a PostgreSQL Testcontainer for realistic database testing with proper isolation.
/// </summary>
public class TestApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private PostgreSqlContainer? _postgreSqlContainer;
    private bool _initialized = false;

    /// <summary>
    /// Initializes the test container before any tests run.
    /// This must be called before using the factory.
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_initialized)
            return;

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
    /// Cleans up the test container after all tests complete.
    /// </summary>
    public new async Task DisposeAsync()
    {
        if (_postgreSqlContainer != null)
        {
            await _postgreSqlContainer.DisposeAsync();
        }
        await base.DisposeAsync();
    }

    /// <summary>
    /// Configures the WebHost to use PostgreSQL Testcontainer for testing.
    /// </summary>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
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
