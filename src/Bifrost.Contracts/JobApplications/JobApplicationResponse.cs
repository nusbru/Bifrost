namespace Bifrost.Contracts.JobApplications;

/// <summary>
/// Response contract for job application information.
/// </summary>
public record JobApplicationResponse(
    long Id,
    long JobId,
    Guid UserId,
    int Status,
    DateTime Created,
    DateTime Updated);
