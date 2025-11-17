namespace Bifrost.Contracts.Jobs;

/// <summary>
/// Response contract for job information.
/// </summary>
public record JobResponse(
    long Id,
    string Title,
    string Company,
    string Location,
    int JobType,
    string Description,
    bool OfferSponsorship,
    bool OfferRelocation,
    Guid UserId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
