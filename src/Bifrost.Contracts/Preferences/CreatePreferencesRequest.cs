namespace Bifrost.Contracts.Preferences;

/// <summary>
/// Request contract for creating user preferences.
/// </summary>
public record CreatePreferencesRequest(
    Guid UserId,
    decimal MinSalary,
    decimal MaxSalary,
    string PreferredJobTypes,
    string PreferredLocations);
