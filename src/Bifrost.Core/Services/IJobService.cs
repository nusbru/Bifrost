using Bifrost.Core.Models;

namespace Bifrost.Core.Services;

/// <summary>
/// Service interface for managing Job operations.
/// Handles CRUD operations and business logic for jobs.
/// </summary>
public interface IJobService
{
    /// <summary>
    /// Creates a new job.
    /// </summary>
    Task<Job> CreateJobAsync(Guid userId, string title, string company, string location, 
        int jobType, string description, bool offerSponsorship, bool offerRelocation);

    /// <summary>
    /// Updates an existing job.
    /// </summary>
    Task<Job> UpdateJobAsync(long jobId, string title, string company, string location, string description);

    /// <summary>
    /// Deletes a job by ID.
    /// </summary>
    Task DeleteJobAsync(long jobId);

    /// <summary>
    /// Gets a job by ID.
    /// </summary>
    Task<Job?> GetJobByIdAsync(long jobId);

    /// <summary>
    /// Gets all jobs for a specific user.
    /// </summary>
    Task<IEnumerable<Job>> GetUserJobsAsync(Guid userId);
}
