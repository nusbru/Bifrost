using Bifrost.Core.Enums;
using Bifrost.Core.Models;
using Bifrost.Core.Repositories;

namespace Bifrost.Core.Services;

/// <summary>
/// Service for managing Job operations.
/// </summary>
public class JobService : IJobService
{
    private readonly IJobRepository _jobRepository;

    public JobService(IJobRepository jobRepository)
    {
        _jobRepository = jobRepository ?? throw new ArgumentNullException(nameof(jobRepository));
    }

    public async Task<Job> CreateJobAsync(Guid userId, string title, string company, string location,
        int jobType, string description, bool offerSponsorship, bool offerRelocation)
    {
        ValidateUserId(userId);
        ValidateTitle(title);
        ValidateCompany(company);

        var job = new Job
        {
            Title = title.Trim(),
            Company = company.Trim(),
            Location = location?.Trim() ?? string.Empty,
            JobType = (JobType)jobType,
            Description = description?.Trim() ?? string.Empty,
            OfferSponsorship = offerSponsorship,
            OfferRelocation = offerRelocation,
            SupabaseUserId = userId
        };

        await _jobRepository.Add(job);
        return job;
    }

    public async Task<Job> UpdateJobAsync(long jobId, string title, string company, string location, string description)
    {
        ValidateJobId(jobId);

        var job = await _jobRepository.GetById(jobId)
            ?? throw new InvalidOperationException($"Job with ID {jobId} not found.");

        if (!string.IsNullOrWhiteSpace(title))
            job.Title = title.Trim();

        if (!string.IsNullOrWhiteSpace(company))
            job.Company = company.Trim();

        if (location != null)
            job.Location = location.Trim();

        if (description != null)
            job.Description = description.Trim();

        return job;
    }

    public async Task DeleteJobAsync(long jobId)
    {
        ValidateJobId(jobId);

        var job = await _jobRepository.GetById(jobId)
            ?? throw new InvalidOperationException($"Job with ID {jobId} not found.");

        _jobRepository.Remove(job);
    }

    public async Task<Job?> GetJobByIdAsync(long jobId)
    {
        ValidateJobId(jobId);
        return await _jobRepository.GetById(jobId);
    }

    public async Task<IEnumerable<Job>> GetUserJobsAsync(Guid userId)
    {
        ValidateUserId(userId);
        return await _jobRepository.Find(j => j.SupabaseUserId == userId);
    }

    private static void ValidateUserId(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));
    }

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Job title cannot be empty.", nameof(title));
    }

    private static void ValidateCompany(string company)
    {
        if (string.IsNullOrWhiteSpace(company))
            throw new ArgumentException("Company name cannot be empty.", nameof(company));
    }

    private static void ValidateJobId(long jobId)
    {
        if (jobId <= 0)
            throw new ArgumentException("Job ID must be greater than zero.", nameof(jobId));
    }
}
