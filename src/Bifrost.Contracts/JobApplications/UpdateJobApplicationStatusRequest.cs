namespace Bifrost.Contracts.JobApplications;

/// <summary>
/// Request contract for updating a job application status.
/// </summary>
public record UpdateJobApplicationStatusRequest(
    int Status);
