namespace Bifrost.Contracts.Preferences;

/// <summary>
/// Response contract for user preferences.
/// </summary>
public record PreferencesResponse(
    long Id,
    Guid UserId,
    decimal MinSalary,
    decimal MaxSalary,
    int JobType,
    bool NeedSponsorship,
    bool NeedRelocation,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
