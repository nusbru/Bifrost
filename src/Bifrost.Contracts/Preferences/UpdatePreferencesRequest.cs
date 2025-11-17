namespace Bifrost.Contracts.Preferences;

/// <summary>
/// Request contract for updating user preferences.
/// </summary>
public record UpdatePreferencesRequest(
    decimal MinSalary,
    decimal MaxSalary,
    string PreferredJobTypes,
    string PreferredLocations);
