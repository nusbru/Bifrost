namespace Bifrost.Contracts.Preferences;

/// <summary>
/// Response contract for user preferences.
/// </summary>
public record PreferencesResponse(
    long Id,
    Guid UserId,
    decimal MinSalary,
    decimal MaxSalary,
    string PreferredJobTypes,
    string PreferredLocations,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
