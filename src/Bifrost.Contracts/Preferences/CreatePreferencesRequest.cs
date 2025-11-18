namespace Bifrost.Contracts.Preferences;

/// <summary>
/// Request contract for creating user preferences.
/// </summary>
public record CreatePreferencesRequest(
    Guid UserId,
    decimal MinSalary,
    decimal MaxSalary,
    JobType PreferredJobType,
    bool NeedSponsorship,
    bool NeedRelocation);


public enum JobType
{
    None,
    FullTime,
    PartTime,
    Contract,
    Remote
}
