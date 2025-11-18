namespace Bifrost.Contracts.Jobs;

/// <summary>
/// Request contract for updating an existing job.
/// </summary>
public record UpdateJobRequest(
    string? Title,
    string? Company,
    string? Location,
    string? Description,
    bool? OfferSponsorship,
    bool? OfferRelocation);
