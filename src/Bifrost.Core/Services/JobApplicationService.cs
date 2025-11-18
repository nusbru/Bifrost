using Bifrost.Core.Models;
using Bifrost.Core.Repositories;

namespace Bifrost.Core.Services;

/// <summary>
/// Service for managing JobApplication operations.
/// </summary>
public class JobApplicationService : IJobApplicationService
{
    private readonly IJobApplicationRepository _applicationRepository;
    private readonly IJobRepository _jobRepository;

    public JobApplicationService(IJobApplicationRepository applicationRepository, IJobRepository jobRepository)
    {
        _applicationRepository = applicationRepository ?? throw new ArgumentNullException(nameof(applicationRepository));
        _jobRepository = jobRepository ?? throw new ArgumentNullException(nameof(jobRepository));
    }

    public async Task<JobApplication> CreateApplicationAsync(long jobId, Guid userId)
    {
        ValidateJobId(jobId);
        ValidateUserId(userId);

        var job = await _jobRepository.GetById(jobId)
            ?? throw new InvalidOperationException($"Job with ID {jobId} not found.");

        var application = new JobApplication
        {
            JobId = jobId,
            Status = JobApplicationStatus.Applied,
            Created = DateTime.UtcNow,
            Updated = DateTime.UtcNow,
            SupabaseUserId = userId
        };

        await _applicationRepository.Add(application);
        return application;
    }

    public async Task<JobApplication> UpdateApplicationStatusAsync(long applicationId, JobApplicationStatus newStatus)
    {
        ValidateApplicationId(applicationId);

        var application = await _applicationRepository.GetById(applicationId)
            ?? throw new InvalidOperationException($"Job application with ID {applicationId} not found.");

        application.Status = newStatus;
        application.Updated = DateTime.UtcNow;

        await _applicationRepository.Update(application);
        return application;
    }

    public async Task DeleteApplicationAsync(long applicationId)
    {
        ValidateApplicationId(applicationId);

        var application = await _applicationRepository.GetById(applicationId)
            ?? throw new InvalidOperationException($"Job application with ID {applicationId} not found.");

        await _applicationRepository.Remove(application);
    }

    public async Task<JobApplication?> GetApplicationByIdAsync(long applicationId)
    {
        ValidateApplicationId(applicationId);
        return await _applicationRepository.GetById(applicationId);
    }

    public async Task<IEnumerable<JobApplication>> GetUserApplicationsAsync(Guid userId)
    {
        ValidateUserId(userId);
        return await _applicationRepository.Find(a => a.SupabaseUserId == userId);
    }

    public async Task<IEnumerable<JobApplication>> GetJobApplicationsAsync(long jobId)
    {
        ValidateJobId(jobId);
        return await _applicationRepository.Find(a => a.JobId == jobId);
    }

    private static void ValidateJobId(long jobId)
    {
        if (jobId <= 0)
            throw new ArgumentException("Job ID must be greater than zero.", nameof(jobId));
    }

    private static void ValidateUserId(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));
    }

    private static void ValidateApplicationId(long applicationId)
    {
        if (applicationId <= 0)
            throw new ArgumentException("Application ID must be greater than zero.", nameof(applicationId));
    }
}
