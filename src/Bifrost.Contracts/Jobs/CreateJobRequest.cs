namespace Bifrost.Contracts.Jobs;

/// <summary>
/// Request contract for creating a new job.
/// </summary>
public record CreateJobRequest(
    string Title,
    string Company,
    string Location,
    int JobType,
    string Description,
    bool OfferSponsorship,
    bool OfferRelocation);
