namespace Bifrost.Contracts.JobApplications;

/// <summary>
/// Request contract for creating a new job application.
/// </summary>
public record CreateJobApplicationRequest(
    Guid UserId,
    long JobId);
