using Bifrost.Core.Models;
using Bifrost.Core.Repositories;

namespace Bifrost.Core.Services;

/// <summary>
/// Service for managing Preferences operations.
/// </summary>
public class PreferencesService : IPreferencesService
{
    private readonly IPreferencesRepository _preferencesRepository;

    public PreferencesService(IPreferencesRepository preferencesRepository)
    {
        _preferencesRepository = preferencesRepository ?? throw new ArgumentNullException(nameof(preferencesRepository));
    }

    public async Task<Preferences> CreatePreferencesAsync(Guid userId, decimal minSalary,
        decimal maxSalary, string preferredJobTypes, string preferredLocations)
    {
        ValidateUserId(userId);
        ValidateSalaryRange(minSalary, maxSalary);

        var preferences = new Preferences
        {
            SalaryRange = new SalaryRange { Min = minSalary, Max = maxSalary },
            SupabaseUserId = userId
        };

        await _preferencesRepository.Add(preferences);
        return preferences;
    }

    public async Task<Preferences> UpdatePreferencesAsync(long preferenceId, decimal minSalary,
        decimal maxSalary, string preferredJobTypes, string preferredLocations)
    {
        ValidatePreferenceId(preferenceId);
        ValidateSalaryRange(minSalary, maxSalary);

        var preferences = await _preferencesRepository.GetById(preferenceId)
            ?? throw new InvalidOperationException($"Preferences with ID {preferenceId} not found.");

        preferences.SalaryRange = new SalaryRange { Min = minSalary, Max = maxSalary };

        return preferences;
    }

    public async Task DeletePreferencesAsync(long preferenceId)
    {
        ValidatePreferenceId(preferenceId);

        var preferences = await _preferencesRepository.GetById(preferenceId)
            ?? throw new InvalidOperationException($"Preferences with ID {preferenceId} not found.");

        _preferencesRepository.Remove(preferences);
    }

    public async Task<Preferences?> GetUserPreferencesAsync(Guid userId)
    {
        ValidateUserId(userId);

        var allPreferences = await _preferencesRepository.Find(p => p.SupabaseUserId == userId);
        return allPreferences.FirstOrDefault();
    }

    private static void ValidateUserId(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));
    }

    private static void ValidateSalaryRange(decimal minSalary, decimal maxSalary)
    {
        if (minSalary < 0)
            throw new ArgumentException("Minimum salary cannot be negative.", nameof(minSalary));

        if (maxSalary < 0)
            throw new ArgumentException("Maximum salary cannot be negative.", nameof(maxSalary));

        if (minSalary > maxSalary)
            throw new ArgumentException("Minimum salary cannot be greater than maximum salary.");
    }

    private static void ValidatePreferenceId(long preferenceId)
    {
        if (preferenceId <= 0)
            throw new ArgumentException("Preference ID must be greater than zero.", nameof(preferenceId));
    }
}
