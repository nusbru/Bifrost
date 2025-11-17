using Bifrost.Core.Models;

namespace Bifrost.Core.Services;

/// <summary>
/// Service interface for managing JobApplication operations.
/// Handles CRUD operations and business logic for job applications.
/// </summary>
public interface IJobApplicationService
{
    /// <summary>
    /// Creates a new job application.
    /// </summary>
    Task<JobApplication> CreateApplicationAsync(long jobId, Guid userId);

    /// <summary>
    /// Updates the status of a job application.
    /// </summary>
    Task<JobApplication> UpdateApplicationStatusAsync(long applicationId, JobApplicationStatus newStatus);

    /// <summary>
    /// Deletes a job application.
    /// </summary>
    Task DeleteApplicationAsync(long applicationId);

    /// <summary>
    /// Gets a job application by ID.
    /// </summary>
    Task<JobApplication?> GetApplicationByIdAsync(long applicationId);

    /// <summary>
    /// Gets all job applications for a user.
    /// </summary>
    Task<IEnumerable<JobApplication>> GetUserApplicationsAsync(Guid userId);

    /// <summary>
    /// Gets all applications for a specific job.
    /// </summary>
    Task<IEnumerable<JobApplication>> GetJobApplicationsAsync(long jobId);
}
