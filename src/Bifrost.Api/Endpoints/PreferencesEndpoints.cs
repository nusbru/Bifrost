using Bifrost.Contracts.Preferences;
using Bifrost.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Bifrost.Api.Endpoints;

/// <summary>
/// Endpoints for managing user preferences.
/// </summary>
public static class PreferencesEndpoints
{
    public static void MapPreferencesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/preferences")
            .WithTags("Preferences")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapPost("/", CreatePreferences)
            .WithName("CreatePreferences")
            .WithSummary("Create user preferences")
            .Produces<PreferencesResponse>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapGet("/user/{userId}", GetUserPreferences)
            .WithName("GetUserPreferences")
            .WithSummary("Get user preferences")
            .Produces<PreferencesResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPut("/{preferenceId}", UpdatePreferences)
            .WithName("UpdatePreferences")
            .WithSummary("Update user preferences")
            .Produces<PreferencesResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapDelete("/{preferenceId}", DeletePreferences)
            .WithName("DeletePreferences")
            .WithSummary("Delete user preferences")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> CreatePreferences(
        CreatePreferencesRequest request,
        IPreferencesService preferencesService)
    {
        try
        {
            var preferences = await preferencesService.CreatePreferencesAsync(
                request.UserId,
                request.MinSalary,
                request.MaxSalary,
                (int)request.PreferredJobType,
                request.NeedSponsorship,
                request.NeedRelocation);

            var response = new PreferencesResponse(
                preferences.Id,
                preferences.SupabaseUserId,
                preferences.SalaryRange.Min,
                preferences.SalaryRange.Max,
                (int)preferences.JobType,
                preferences.NeedSponsorship,
                preferences.NeedRelocation,
                preferences.CreatedAt ?? DateTime.UtcNow,
                preferences.UpdatedAt);

            return Results.Created($"/api/preferences/{preferences.Id}", response);
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

    private static async Task<IResult> GetUserPreferences(
        Guid userId,
        IPreferencesService preferencesService)
    {
        try
        {
            var preferences = await preferencesService.GetUserPreferencesAsync(userId);
            if (preferences == null)
            {
                return Results.NotFound(new ProblemDetails
                {
                    Title = "Preferences Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = $"Preferences for user {userId} were not found."
                });
            }

            var response = new PreferencesResponse(
                preferences.Id,
                preferences.SupabaseUserId,
                preferences.SalaryRange.Min,
                preferences.SalaryRange.Max,
                (int)preferences.JobType,
                preferences.NeedSponsorship,
                preferences.NeedRelocation,
                preferences.CreatedAt ?? DateTime.UtcNow,
                preferences.UpdatedAt);

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

    private static async Task<IResult> UpdatePreferences(
        long preferenceId,
        UpdatePreferencesRequest request,
        IPreferencesService preferencesService)
    {
        try
        {
            var preferences = await preferencesService.UpdatePreferencesAsync(
                preferenceId,
                request.MinSalary,
                request.MaxSalary,
                request.JobType,
                request.NeedSponsorship,
                request.NeedRelocation);

            var response = new PreferencesResponse(
                preferences.Id,
                preferences.SupabaseUserId,
                preferences.SalaryRange.Min,
                preferences.SalaryRange.Max,
                (int)preferences.JobType,
                preferences.NeedSponsorship,
                preferences.NeedRelocation,
                preferences.CreatedAt ?? DateTime.UtcNow,
                preferences.UpdatedAt);

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
                Title = "Preferences Not Found",
                Status = StatusCodes.Status404NotFound,
                Detail = ex.Message
            });
        }
    }

    private static async Task<IResult> DeletePreferences(
        long preferenceId,
        IPreferencesService preferencesService)
    {
        try
        {
            await preferencesService.DeletePreferencesAsync(preferenceId);
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
                Title = "Preferences Not Found",
                Status = StatusCodes.Status404NotFound,
                Detail = ex.Message
            });
        }
    }
}
