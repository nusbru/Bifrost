namespace Bifrost.Contracts.Preferences;

/// <summary>
/// Request contract for updating user preferences.
/// </summary>
public record UpdatePreferencesRequest(
    decimal MinSalary,
    decimal MaxSalary,
    int? JobType,
    bool? NeedSponsorship,
    bool? NeedRelocation);
