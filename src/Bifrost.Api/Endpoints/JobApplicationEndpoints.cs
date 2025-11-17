using Bifrost.Contracts.JobApplications;
using Bifrost.Core.Models;
using Bifrost.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Bifrost.Api.Endpoints;

/// <summary>
/// Endpoints for managing job applications.
/// </summary>
public static class JobApplicationEndpoints
{
    public static void MapJobApplicationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/applications")
            .WithTags("Job Applications")
            .WithOpenApi();

        group.MapPost("/", CreateApplication)
            .WithName("CreateJobApplication")
            .WithSummary("Create a new job application")
            .Produces<JobApplicationResponse>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapGet("/{applicationId}", GetApplication)
            .WithName("GetJobApplication")
            .WithSummary("Get a job application by ID")
            .Produces<JobApplicationResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapGet("/user/{userId}", GetUserApplications)
            .WithName("GetUserApplications")
            .WithSummary("Get all applications for a user")
            .Produces<List<JobApplicationResponse>>(StatusCodes.Status200OK);

        group.MapGet("/job/{jobId}", GetJobApplications)
            .WithName("GetJobApplications")
            .WithSummary("Get all applications for a job")
            .Produces<List<JobApplicationResponse>>(StatusCodes.Status200OK);

        group.MapPut("/{applicationId}/status", UpdateApplicationStatus)
            .WithName("UpdateApplicationStatus")
            .WithSummary("Update an application status")
            .Produces<JobApplicationResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapDelete("/{applicationId}", DeleteApplication)
            .WithName("DeleteJobApplication")
            .WithSummary("Delete a job application")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> CreateApplication(
        CreateJobApplicationRequest request,
        IJobApplicationService applicationService)
    {
        try
        {
            // TODO: Get userId from authenticated user context
            var userId = Guid.NewGuid();

            var application = await applicationService.CreateApplicationAsync(
                request.JobId,
                userId);

            var response = new JobApplicationResponse(
                application.Id,
                application.JobId,
                application.SupabaseUserId,
                (int)application.Status,
                application.Created,
                application.Updated);

            return Results.Created($"/api/applications/{application.Id}", response);
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
                Title = "Resource Not Found",
                Status = StatusCodes.Status404NotFound,
                Detail = ex.Message
            });
        }
    }

    private static async Task<IResult> GetApplication(
        long applicationId,
        IJobApplicationService applicationService)
    {
        try
        {
            var application = await applicationService.GetApplicationByIdAsync(applicationId);
            if (application == null)
            {
                return Results.NotFound(new ProblemDetails
                {
                    Title = "Application Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = $"Application with ID {applicationId} was not found."
                });
            }

            var response = new JobApplicationResponse(
                application.Id,
                application.JobId,
                application.SupabaseUserId,
                (int)application.Status,
                application.Created,
                application.Updated);

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

    private static async Task<IResult> GetUserApplications(
        Guid userId,
        IJobApplicationService applicationService)
    {
        try
        {
            var applications = await applicationService.GetUserApplicationsAsync(userId);

            var responses = applications.Select(app => new JobApplicationResponse(
                app.Id,
                app.JobId,
                app.SupabaseUserId,
                (int)app.Status,
                app.Created,
                app.Updated)).ToList();

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

    private static async Task<IResult> GetJobApplications(
        long jobId,
        IJobApplicationService applicationService)
    {
        try
        {
            var applications = await applicationService.GetJobApplicationsAsync(jobId);

            var responses = applications.Select(app => new JobApplicationResponse(
                app.Id,
                app.JobId,
                app.SupabaseUserId,
                (int)app.Status,
                app.Created,
                app.Updated)).ToList();

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

    private static async Task<IResult> UpdateApplicationStatus(
        long applicationId,
        UpdateJobApplicationStatusRequest request,
        IJobApplicationService applicationService)
    {
        try
        {
            var application = await applicationService.UpdateApplicationStatusAsync(
                applicationId,
                (JobApplicationStatus)request.Status);

            var response = new JobApplicationResponse(
                application.Id,
                application.JobId,
                application.SupabaseUserId,
                (int)application.Status,
                application.Created,
                application.Updated);

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
                Title = "Application Not Found",
                Status = StatusCodes.Status404NotFound,
                Detail = ex.Message
            });
        }
    }

    private static async Task<IResult> DeleteApplication(
        long applicationId,
        IJobApplicationService applicationService)
    {
        try
        {
            await applicationService.DeleteApplicationAsync(applicationId);
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
                Title = "Application Not Found",
                Status = StatusCodes.Status404NotFound,
                Detail = ex.Message
            });
        }
    }
}
