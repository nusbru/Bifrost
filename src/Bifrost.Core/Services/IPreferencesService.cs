using Bifrost.Core.Enums;
using Bifrost.Core.Models;

namespace Bifrost.Core.Services;

/// <summary>
/// Service interface for managing Preferences operations.
/// Handles CRUD operations and business logic for user preferences.
/// </summary>
public interface IPreferencesService
{
    /// <summary>
    /// Creates user preferences.
    /// </summary>
    Task<Preferences> CreatePreferencesAsync(Guid userId, decimal minSalary,
        decimal maxSalary, int preferredJobType, bool needSponsorship, bool needRelocation);

    /// <summary>
    /// Updates user preferences.
    /// </summary>
    Task<Preferences> UpdatePreferencesAsync(long preferenceId, decimal minSalary,
        decimal maxSalary, int? jobType = 0, bool? needSponsorship = false, bool? needRelocation = false);

    /// <summary>
    /// Deletes user preferences.
    /// </summary>
    Task DeletePreferencesAsync(long preferenceId);

    /// <summary>
    /// Gets user preferences by user ID.
    /// </summary>
    Task<Preferences?> GetUserPreferencesAsync(Guid userId);
}

