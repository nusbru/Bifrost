using Bifrost.Core.Repositories;
using Bifrost.Core.Services;
using Bifrost.Infrastructure.Authentication;
using Bifrost.Infrastructure.Persistence;
using Bifrost.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bifrost.Infrastructure;

/// <summary>
/// Dependency injection extension methods for infrastructure services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds infrastructure services to the dependency injection container.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add DbContext
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<BifrostDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Register repositories
        services.AddScoped<IJobRepository, JobRepository>();
        services.AddScoped<IJobApplicationRepository, JobApplicationRepository>();
        services.AddScoped<IApplicationNoteRepository, ApplicationNoteRepository>();
        services.AddScoped<IPreferencesRepository, PreferencesRepository>();

        // Register authentication service
        services.AddHttpClient<IAuthService, AuthService>();

        return services;
    }
}
