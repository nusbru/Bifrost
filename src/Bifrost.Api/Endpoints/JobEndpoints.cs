using Bifrost.Contracts.Jobs;
using Bifrost.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Bifrost.Api.Endpoints;

/// <summary>
/// Endpoints for managing jobs.
/// </summary>
public static class JobEndpoints
{
    public static void MapJobEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/jobs")
            .WithTags("Jobs")
            .WithOpenApi();

        group.MapPost("/", CreateJob)
            .WithName("CreateJob")
            .WithSummary("Create a new job")
            .Produces<JobResponse>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapGet("/{jobId}", GetJob)
            .WithName("GetJob")
            .WithSummary("Get a job by ID")
            .Produces<JobResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapGet("/user/{userId}", GetUserJobs)
            .WithName("GetUserJobs")
            .WithSummary("Get all jobs for a user")
            .Produces<List<JobResponse>>(StatusCodes.Status200OK);

        group.MapPut("/{jobId}", UpdateJob)
            .WithName("UpdateJob")
            .WithSummary("Update an existing job")
            .Produces<JobResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapDelete("/{jobId}", DeleteJob)
            .WithName("DeleteJob")
            .WithSummary("Delete a job")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> CreateJob(
        CreateJobRequest request,
        IJobService jobService,
        HttpContext httpContext)
    {
        try
        {
            var job = await jobService.CreateJobAsync(
                request.UserId,
                request.Title,
                request.Company,
                request.Location,
                request.JobType,
                request.Description,
                request.OfferSponsorship,
                request.OfferRelocation);

            var response = new JobResponse(
                job.Id,
                job.Title,
                job.Company,
                job.Location,
                (int)job.JobType,
                job.Description,
                job.OfferSponsorship,
                job.OfferRelocation,
                job.SupabaseUserId,
                job.CreatedAt ?? DateTime.UtcNow,
                job.UpdatedAt);

            return Results.Created($"/api/jobs/{job.Id}", response);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Validation Error",
                Status = StatusCodes.Status400BadRequest,
                Detail = ex.Message
            });
        }
    }

    private static async Task<IResult> GetJob(
        long jobId,
        IJobService jobService)
    {
        try
        {
            var job = await jobService.GetJobByIdAsync(jobId);
            if (job == null)
            {
                return Results.NotFound(new ProblemDetails
                {
                    Title = "Job Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = $"Job with ID {jobId} was not found."
                });
            }

            var response = new JobResponse(
                job.Id,
                job.Title,
                job.Company,
                job.Location,
                (int)job.JobType,
                job.Description,
                job.OfferSponsorship,
                job.OfferRelocation,
                job.SupabaseUserId,
                job.CreatedAt ?? DateTime.UtcNow,
                job.UpdatedAt);

            return Results.Ok(response);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Validation Error",
                Status = StatusCodes.Status400BadRequest,
                Detail = ex.Message
            });
        }
    }

    private static async Task<IResult> GetUserJobs(
        Guid userId,
        IJobService jobService)
    {
        try
        {
            var jobs = await jobService.GetUserJobsAsync(userId);

            var responses = jobs.Select(job => new JobResponse(
                job.Id,
                job.Title,
                job.Company,
                job.Location,
                (int)job.JobType,
                job.Description,
                job.OfferSponsorship,
                job.OfferRelocation,
                job.SupabaseUserId,
                job.CreatedAt ?? DateTime.UtcNow,
                job.UpdatedAt)).ToList();

            return Results.Ok(responses);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Validation Error",
                Status = StatusCodes.Status400BadRequest,
                Detail = ex.Message
            });
        }
    }

    private static async Task<IResult> UpdateJob(
        long jobId,
        UpdateJobRequest request,
        IJobService jobService)
    {
        try
        {
            var job = await jobService.UpdateJobAsync(
                jobId,
                request.Title,
                request.Company,
                request.Location,
                request.Description,
                request.OfferSponsorship,
                request.OfferRelocation);

            var response = new JobResponse(
                job.Id,
                job.Title,
                job.Company,
                job.Location,
                (int)job.JobType,
                job.Description,
                job.OfferSponsorship,
                job.OfferRelocation,
                job.SupabaseUserId,
                job.CreatedAt ?? DateTime.UtcNow,
                job.UpdatedAt);

            return Results.Ok(response);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Validation Error",
                Status = StatusCodes.Status400BadRequest,
                Detail = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return Results.NotFound(new ProblemDetails
            {
                Title = "Job Not Found",
                Status = StatusCodes.Status404NotFound,
                Detail = ex.Message
            });
        }
    }

    private static async Task<IResult> DeleteJob(
        long jobId,
        IJobService jobService)
    {
        try
        {
            await jobService.DeleteJobAsync(jobId);
            return Results.NoContent();
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Validation Error",
                Status = StatusCodes.Status400BadRequest,
                Detail = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return Results.NotFound(new ProblemDetails
            {
                Title = "Job Not Found",
                Status = StatusCodes.Status404NotFound,
                Detail = ex.Message
            });
        }
    }
}
